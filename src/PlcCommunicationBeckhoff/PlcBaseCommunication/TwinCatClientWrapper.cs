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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using TwinCAT.Ads;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.PlcBaseCommunication
{
    /// <summary>
    /// Wraps around the <see cref="TcAdsClient"/> and provides some easy access methods
    /// </summary>
    [Export(typeof(ITwinCatClient))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class TwinCatClientWrapper : ITwinCatClient
    {
        private readonly ILogger _logger;
        private TcAdsClient _twinCatClient;
        private TcAdsSymbolInfoLoader _symbolLoader;
        private string _path;
        private int _port;

        public TwinCatClientWrapper()
        {
            _logger = new Log4NetLogger();
        }

        [ImportingConstructor]
        public TwinCatClientWrapper(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates and initializes the <see cref="TcAdsClient"/>.
        /// </summary>
        /// <param name="path">The connection string or path (file path or IP address depending on implementation). In case of Beckhoff this is the AdsAddress.</param>
        /// <param name="port">The ADS Port in case of Beckhoff implementation.</param>
        public void Initialize(string path, int port)
        {
            _logger.Init(GetType());
            _path = path;
            _port = port;
            try
            {
                _twinCatClient = new TcAdsClient();
                _twinCatClient.Connect(path, port);
                _symbolLoader = _twinCatClient.CreateSymbolInfoLoader();
            }
            catch (AdsErrorException e)
            {
                throw new PlcCommunicationException("Can't establish connection to PLC", path,
                                    port.ToString(CultureInfo.InvariantCulture), string.Empty, e);
            }
        }

        /// <summary>
        /// Reads the element from the specified <paramref name="elementPath"/> and returns it.
        /// </summary>
        /// <param name="elementPath">The path to read from.</param>
        /// <param name="typeOfElement">The type of the desired element.</param>
        /// <returns>The read value.</returns>
        public object ReadElement(string elementPath, Type typeOfElement)
        {
            int handle = -1;
            try
            {
                handle = _twinCatClient.CreateVariableHandle(elementPath);
                return _twinCatClient.ReadAny(handle, typeOfElement);
            }
            catch (AdsErrorException e)
            {
                var exception = new PlcCommunicationException("Can't read tag " + elementPath, _path,
                    _port.ToString(CultureInfo.InvariantCulture), string.Empty, e);
                throw exception;
            }
            finally
            {
                if (handle != -1)
                    _twinCatClient.DeleteVariableHandle(handle);
            }
        }

        /// <summary>
        /// Reads the element from the specified <paramref name="elementPath"/> and returns it.
        /// </summary>
        /// <typeparam name="TElement">The type of the desired element.</typeparam>
        /// <param name="elementPath">The path to read from.</param>
        /// <returns>The read value.</returns>
        public TElement ReadElement<TElement>(string elementPath)
        {
            return (TElement) ReadElement(elementPath, typeof(TElement));
        }

        private void ReadElementToStream(string elementPath, AdsStream stream)
        {
            try
            {
                int handle = _twinCatClient.CreateVariableHandle(elementPath);
                try
                {
                    _twinCatClient.Read(handle, stream);
                }
                finally
                {
                    _twinCatClient.DeleteVariableHandle(handle);
                }
            }
            catch (AdsErrorException ex)
            {
                _logger.ErrorFormat("Could not read element on path '{0}' because of '{1}'.", elementPath, ex.ErrorCode);
                throw;
            }
        }

        /// <summary>
        /// Reads an elements array of the specified <paramref name="typeOfElements"/>. Supports pointered arrays (POINTER TO ...).
        /// </summary>
        /// <param name="elementsArrayPath">The path in PLC to the array.</param>
        /// <param name="arraySize">Size of the array.</param>
        /// <param name="typeOfElements">Marshall type representation in .NET. See Beckhoff TwinCat 3 manual for an example.</param>
        /// <returns>
        /// List of imported and converted (.NET type) array elements.
        /// </returns>
        /// <remarks>
        /// This implementation does not use an AdsSymbol loader and fixes some pointer dereferencing issues (11.11.2013).
        /// It seems the AdsSymbol loader cannot resolve complicated paths using .^ character.
        /// </remarks>
        public IEnumerable ReadElementsArray(string elementsArrayPath, int arraySize, Type typeOfElements)
        {
            IList array = new ArrayList(arraySize);

            for (int elementsArrayIndex = 0; elementsArrayIndex < arraySize; elementsArrayIndex++)
            {
                string concreteElementPath = string.Format("{0}[{1}]", elementsArrayPath, elementsArrayIndex);
                object elementValue = ReadElement(concreteElementPath, typeOfElements);
                array.Add(elementValue);
            }

            return array;
        }


        /// <summary>
        /// Reads an elements array of type TElement. Supports pointered arrays (POINTER TO ...).
        /// </summary>
        /// <typeparam name="TElement">Marshall type representation in .NET. See Beckhoff TwinCat 3 manual for an example.</typeparam>
        /// <param name="elementsArrayPath">The path in PLC to the array.</param>
        /// <param name="arraySize">Size of the array.</param>
        /// <returns>
        /// List of imported and converted (.NET type) array elements.
        /// </returns>
        /// <remarks>
        /// This implementation does not use an AdsSymbol loader and fixes some pointer dereferencing issues (11.11.2013).
        /// It seems the AdsSymbol loader cannot resolve complicated paths using .^ character.
        /// </remarks>
        public IEnumerable<TElement> ReadElementsArray<TElement>(string elementsArrayPath, int arraySize)
        {
            return ReadElementsArray(elementsArrayPath, arraySize, typeof (TElement)).OfType<TElement>();
        }

        /// <summary>
        /// Reads an elements array of type TElement. Supports pointered arrays (POINTER TO ...).
        /// </summary>
        /// <typeparam name="TElement">Marshall type representation in .NET. See Beckhoff TwinCat 3 manual for an example.</typeparam>
        /// <param name="elementsArrayPath">The path in PLC to the array.</param>
        /// <returns>
        /// A dictionary of imported and converted (.NET type) array elements (value) sorted by their pathes (key).
        /// </returns>
        public IDictionary<string, TElement> ReadElementsArrayWithPathes<TElement>(string elementsArrayPath)
        {
            _logger.Debug(string.Format("Read elements array of type '{0}' from path '{1}' on '{2}'.", typeof(TElement).Name, elementsArrayPath, this));
            IDictionary<string, TElement> array = new Dictionary<string, TElement>();

            var arraySymbol = _symbolLoader.FindSymbol(elementsArrayPath);
            if (arraySymbol == null)
                return array;

            var stream = new AdsStream(8);
            var reader = new AdsBinaryReader(stream);

            TcAdsSymbolInfo elementSymbol = arraySymbol.FirstSubSymbol;
            while (elementSymbol != null)
            {
                stream.Position = 0;
                _twinCatClient.Read(elementSymbol.IndexGroup, elementSymbol.IndexOffset, stream);

                var pointerValue = PlcSystem.IsX64Mode ? reader.ReadInt64() : reader.ReadInt32();

                if (pointerValue != 0)
                {
                    string plcArrayElementPath = elementSymbol.Name;

                    if (elementSymbol.IsPointer)
                        plcArrayElementPath = string.Format("{0}^", plcArrayElementPath);

                    var element = ReadElement<TElement>(plcArrayElementPath);

                    array.Add(plcArrayElementPath, element);
                }
                elementSymbol = elementSymbol.NextSymbol;
            }

            _logger.Debug(string.Format("Finished read of elements array of type '{0}' from path '{1}' on '{2}'.", typeof(TElement).Name, elementsArrayPath, this));
            return array;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("TwinCatClient for plc '{0}:{1}'", _path, _port);
        }
    }
}
