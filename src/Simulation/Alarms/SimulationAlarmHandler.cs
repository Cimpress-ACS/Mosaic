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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.Simulation.Alarms
{
    /// <summary>
    /// This is a meta level alarm handler when something happens on the simulated "hardware" level.
    /// E.g. when a paper jam was detected or any kind of item collision.
    /// Will log every alarm to the usual logger.
    /// </summary>
    [Export]
    public class SimulationAlarmHandler
    {
        private readonly List<Alarm> _alarms = new List<Alarm>();

        [ImportingConstructor]
        public SimulationAlarmHandler([ImportMany]IEnumerable<IAlarmSource> alarmSources, ILogger logger)
        {
            logger.Init(GetType());

            foreach (var alarmSource in alarmSources)
            {
                alarmSource.Alarms.Subscribe(alarm =>
                {
                    logger.Warn("Simulation Alarm: Source=" + alarm.Source + " Message=" + alarm.Message);
                    _alarms.Add(alarm);
                });
            }
        }

        public IList<Alarm> CurrentAlarms
        {
            get { return _alarms; }
        }
    }
}
