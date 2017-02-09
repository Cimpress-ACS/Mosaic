/* Copyright 2017 Cimpress

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License. */


using System;
using System.Diagnostics;
using System.Threading;
using TwinCAT.Ads;
using VP.FF.PT.Common.Infrastructure.Concurrency;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff
{
    /// <summary>
    /// Triggers a RefreshAll for TagListeners and handles error recovery in case the PLC is down with a timeout.
    /// </summary>
    public class PollingManagerForTagListener
    {
        private TimeSpan _timeout = TimeSpan.MaxValue;

        private Thread _thread;
        private bool _shutdownThread;
        private static readonly TimeSpan _pollTimeoutForLoggerWarning = TimeSpan.FromSeconds(2);
        private readonly BeckhoffPollingTagListener _tagListener;
        private readonly TcAdsClient _twinCatClient;
        private readonly IGlobalLock _globalLock;
        private readonly ILogger _logger;
        private PlcPerformanceLogBuilder _performanceLogBuilderRead;
        private PlcPerformanceLogBuilder _performanceLogBuilderApply;

        /// <summary>
        /// Initializes a new instance of the <see cref="PollingManagerForTagListener"/> class.
        /// </summary>
        /// <param name="tagListener">The tag listener.</param>
        /// <param name="twinCatClient">The twin cat client.</param>
        /// <param name="pollingRate">The polling rate.</param>
        /// <param name="globalLock">A class managing locking globally within the application.</param>
        /// <param name="logger">A logger for standard message logging</param>
        public PollingManagerForTagListener(BeckhoffPollingTagListener tagListener, TcAdsClient twinCatClient, int pollingRate, IGlobalLock globalLock, ILogger logger = null)
        {
            _tagListener = tagListener;
            _twinCatClient = twinCatClient;
            _globalLock = globalLock;
            _logger = logger ?? new NullLogger();
            PollingRate = pollingRate;
        }

        /// <summary>
        /// Gets or sets the polling rate.
        /// </summary>
        /// <value>
        /// The polling rate.
        /// </value>
        public int PollingRate { get; set; }

        /// <summary>
        /// Gets or sets the timeout for reestablish a lostconnection to PLC.
        /// </summary>
        /// <value>
        /// The timeout in milliseconds.
        /// </value>
        public int Timeout
        {
            get { return (int)_timeout.TotalMilliseconds; }
            set { _timeout = TimeSpan.FromMilliseconds(value); }
        }

        public void Start()
        {
            _shutdownThread = false;

            if (_thread == null)
            {
                _thread = new Thread(TagListenerThread) {Name = "TagListener-" + _tagListener.Name + "-" + PollingRate};
                _thread.IsBackground = true;

                _performanceLogBuilderRead = new PlcPerformanceLogBuilder(_tagListener.Name + " (R-" + PollingRate + ")");
                _performanceLogBuilderApply = new PlcPerformanceLogBuilder(_tagListener.Name + " (A-" + PollingRate + ")");

                // assign higher priority
                const string msg = "Start thread '{0}' (Id={1}) with priority '{2}'. Refresh rate: {3}";
                if (PollingRate <= 100)
                {
                    _thread.Priority = ThreadPriority.AboveNormal;
                }
                _logger.InfoFormat(msg, _thread.Name, _thread.ManagedThreadId, _thread.Priority, PollingRate);

                _thread.Start();
            }
        }

        public void Stop()
        {
            if (_thread != null)
            {
                _shutdownThread = true;
                _thread.Join();
                _thread = null;
            }
        }

        private void TagListenerThread()
        {
            var performanceStopWatch = Stopwatch.StartNew();

            while (!_shutdownThread)
            {
                // always sleep for the time of the specified polling rate
                Thread.Sleep(PollingRate);

                // verify connection exists
                if (ProbeConnectionLostAndReconnect())
                {
                    if (performanceStopWatch.Elapsed > _timeout)
                    {
                        Stop();
                        throw new PlcCommunicationException(
                            "Communication to PLC is down. Was not able to recover after a timeout of " + Timeout +
                            "ms !", _tagListener.AddressAndPath, "Timeout");
                    }

                    continue;
                }

                // main functionality of reading/writing values
                try
                {
                    // start measuring time for reading and writing
                    performanceStopWatch.Restart();
                    
                    // read all values
                    _tagListener.RefreshAllForPollingManager();

                    // log performance of reading data
                    _performanceLogBuilderRead.AddPerformanceData(performanceStopWatch.ElapsedMilliseconds);

                    // restart stopwatch for write performance log
                    performanceStopWatch.Restart();

                    // write all values within a global lock to avoid problems with event chain
                    _globalLock.Execute(() =>
                        {
                            // start measuring time for just writing the events
                            var beforePumpEventTimestamp = Stopwatch.StartNew();

                            // write the events
                            _tagListener.AssignValuesAndPumpEvents();

                            // Execute the Polling Event
                            _tagListener.ThrowPollingEvent();

                            // log if writing the events took a long time
                            beforePumpEventTimestamp.Stop();
                            if (beforePumpEventTimestamp.Elapsed > TimeSpan.FromMilliseconds(300))
                            {
                                _logger.WarnFormat("Pump events for {0} (ThreadId: {1}) took {2}ms.", _tagListener.Name, Thread.CurrentThread.ManagedThreadId,
                                                   beforePumpEventTimestamp.ElapsedMilliseconds);
                            }
                        });

                    // log performance of writing data, including waiting for the lock
                    _performanceLogBuilderApply.AddPerformanceData(performanceStopWatch.ElapsedMilliseconds);
                }
                catch (PlcCommunicationException e)
                {
                    if (IsAdsException(e))
                    {
                        // delete address and try to read again
                        if (e.Tag != null)
                        {
                            _logger.Error("Exception caught for " + e.Tag.FullName() + ". Delete address now and try again", e);
                            e.Tag.IndexGroup = 0;
                            e.Tag.IndexOffset = 0;
                        }

                        // ignore exception, cancel foreach and try StartListening for next RefreshAll again
                    }            
                }
                
                // log if the performance of reading and writing the values took a long time
                performanceStopWatch.Stop();
                if (performanceStopWatch.Elapsed > _pollTimeoutForLoggerWarning)
                {
                    _logger.WarnFormat("TagListener poll took {0}ms.  (Thread: {1}, Listener: {2})", performanceStopWatch.ElapsedMilliseconds,
                                       Thread.CurrentThread.ManagedThreadId, _tagListener.Name);
                }
            }
        }

        private bool ProbeConnectionLostAndReconnect()
        {
            try
            {
                _twinCatClient.ReadState();
            }
            catch (AdsErrorException e)
            {
                _logger.Error("Error while probing for connection.", e);

                if (e.ErrorCode == AdsErrorCode.DeviceInvalidOffset || e.ErrorCode == AdsErrorCode.DeviceInvalidGroup)
                {
                    var isPortErrorCode = e.ErrorCode == AdsErrorCode.PortDisabled || e.ErrorCode == AdsErrorCode.TargetPortNotFound;
                    try
                    {
                        if (e.ErrorCode == AdsErrorCode.DeviceInvalidOffset || e.ErrorCode == AdsErrorCode.DeviceInvalidGroup || isPortErrorCode)
                        {
                            // clear the address cache, because it might be that a PLC download occured and the addresses might be different!
                            _tagListener.ClearAddressCache();
                            _logger.Warn("Tag cache cleared, because of invalid address (maybe a new program was downloaded to PLC). In " + _tagListener.Name);

                            if (isPortErrorCode)
                            {
                                // try to reconnect
                                _twinCatClient.Connect(_twinCatClient.ClientNetID, _twinCatClient.ClientPort);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // ignore, it will fail after a timeout anyway
                        _logger.Error("Error while clearing address cache.", ex);
                    }

                    return isPortErrorCode;
                }
            }

            return false;
        }

        private bool IsAdsException(PlcCommunicationException exception)
        {
            var innerException = exception.InnerException;

            while (innerException != null)
            {
                if (innerException is AdsErrorException)
                    return true;

                innerException = innerException.InnerException;
            }

            return false;
        }
    }
}
