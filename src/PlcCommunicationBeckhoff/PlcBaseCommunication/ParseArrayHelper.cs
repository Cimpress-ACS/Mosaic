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
using System.Linq;
using TwinCAT.Ads;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcEssentials;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.PlcBaseCommunication
{
    public static class ParseArrayHelper
    {
        /// <summary>
        /// Parses an PLC array of type T. Supports pointered arrays (POINTER TO ...).
        /// </summary>
        /// <typeparam name="T">Marshall type representation in .NET. See Beckhoff TwinCat 3 manual for an example.</typeparam>
        /// <param name="plcPath">The path in PLC to the array.</param>
        /// <param name="twinCatClient">The adsClient instance.</param>
        /// <param name="arraySize">Size of the array.</param>
        /// <returns>
        /// List of imported and converted (.NET type) array elements.
        /// </returns>
        /// <remarks>
        /// This implementation does not use an AdsSymbol loader and fixes some pointer dereferencing issues (11.11.2013).
        /// It seems the AdsSymbol loader cannot resolve complicated paths using .^ character.
        /// </remarks>
        public static IList<T> ParseArray<T>(this TcAdsClient twinCatClient, string plcPath, int arraySize)
        {
            return ParseArray(twinCatClient, plcPath, arraySize, typeof (T)).OfType<T>().ToList();
        }

        /// <summary>
        /// Parses an PLC array of type T. Supports pointered arrays (POINTER TO ...).
        /// </summary>
        /// <param name="twinCatClient">The adsClient instance.</param>
        /// <param name="plcPath">The path in PLC to the array.</param>
        /// <param name="arraySize">Size of the array.</param>
        /// <param name="type">Marshall type representation in .NET. See Beckhoff TwinCat 3 manual for an example.</param>
        /// <param name="array">the new instance where the values will be stored in</param>
        /// <param name="stopReadingPositions">Whether it should continue to read additional values.</param>
        /// <returns>
        /// List of imported and converted (.NET type) array elements.
        /// </returns>
        /// <remarks>
        /// This implementation does not use an AdsSymbol loader and fixes some pointer dereferencing issues (11.11.2013).
        /// It seems the AdsSymbol loader cannot resolve complicated paths using .^ character.
        /// </remarks>
        public static IEnumerable ParseArray(this TcAdsClient twinCatClient, string plcPath, int arraySize, Type type, IList array, Func<object, bool> stopReadingPositions = null)
        {
            for (int i = 0; i < arraySize; i++)
            {
                var handle = twinCatClient.CreateVariableHandle(plcPath + "[" + i + "]");

                try
                {
                    var value = twinCatClient.ReadAny(handle, type);
                    if (stopReadingPositions != null && stopReadingPositions(value))
                    {
                        break;
                    }
                    array.Add(value);
                }
                finally
                {
                    twinCatClient.DeleteVariableHandle(handle);
                }
            }

            return array;
        }

        /// <summary>
        /// Parses an PLC array of type T. Supports pointered arrays (POINTER TO ...).
        /// </summary>
        /// <param name="plcPath">The path in PLC to the array.</param>
        /// <param name="twinCatClient">The adsClient instance.</param>
        /// <param name="arraySize">Size of the array.</param>
        /// <param name="type">Marshall type representation in .NET. See Beckhoff TwinCat 3 manual for an example.</param>
        /// <returns>
        /// List of imported and converted (.NET type) array elements.
        /// </returns>
        /// <remarks>
        /// This implementation does not use an AdsSymbol loader and fixes some pointer dereferencing issues (11.11.2013).
        /// It seems the AdsSymbol loader cannot resolve complicated paths using .^ character.
        /// </remarks>
        public static IEnumerable ParseArray(this TcAdsClient twinCatClient, string plcPath, int arraySize, Type type)
        {
            return ParseArray(twinCatClient, plcPath, arraySize, type, new ArrayList());
        }

        /// <summary>
        /// Parses an PLC array of type T. Supports pointered arrays (POINTER TO ...).
        /// In case of pointered array it will skipp NULL pointers and import only valid instances.
        /// </summary>
        /// <param name="plcPath">The path in PLC to the array.</param>
        /// <param name="twinCatClient">The adsClient instance.</param>
        /// <param name="typeOfElements">Marshall type representation in .NET. See Beckhoff TwinCat 3 manual for an example.</param>
        /// <returns>
        /// Dictionary of imported and converted (.NET type) array elements and their pathes.
        /// </returns>
        public static IEnumerable GetArrayElements(this TcAdsClient twinCatClient, string plcPath, Type typeOfElements)
        {
            var elements = new ArrayList();
            TcAdsSymbolInfoLoader symbolLoader = twinCatClient.CreateSymbolInfoLoader();
            TcAdsSymbolInfo arraySymbol = symbolLoader.FindSymbol(plcPath);
            if (arraySymbol == null)
                return elements;

            var stream = new AdsStream(8);
            var reader = new AdsBinaryReader(stream);

            TcAdsSymbolInfo elementSymbol = arraySymbol.FirstSubSymbol;
            while (elementSymbol != null)
            {
                stream.Position = 0;
                twinCatClient.Read(elementSymbol.IndexGroup, elementSymbol.IndexOffset, stream);

                var pointerValue = PlcSystem.IsX64Mode ? reader.ReadInt64() : reader.ReadInt32();

                if (pointerValue != 0)
                {
                    string plcArrayElementPath = elementSymbol.Name;

                    if (elementSymbol.IsPointer)
                        plcArrayElementPath = string.Format("{0}^", plcArrayElementPath);

                    var handle = twinCatClient.CreateVariableHandle(plcArrayElementPath);

                    try
                    {
                        object element = twinCatClient.ReadAny(handle, typeOfElements);
                        elements.Add(element);
                    }
                    finally
                    {
                        twinCatClient.DeleteVariableHandle(handle);
                    }
                }
                elementSymbol = elementSymbol.NextSymbol;
            }

            return elements;
        }

        /// <summary>
        /// Parses an PLC array of type T. Supports pointered arrays (POINTER TO ...).
        /// In case of pointered array it will skipp NULL pointers and import only valid instances.
        /// </summary>
        /// <typeparam name="T">
        /// Marshall type representation in .NET. See Beckhoff TwinCat 3 manual for an example.
        /// </typeparam>
        /// <param name="plcPath">The path in PLC to the array.</param>
        /// <param name="symbolLoader">The symbol loader instance.</param>
        /// <param name="twinCatClient">The adsClient instance.</param>
        /// <returns>
        /// Dictionary of imported and converted (.NET type) array elements and their pathes.
        /// </returns>
        public static IDictionary<string, T> GetArrayElementsWithPathes<T>(this TcAdsClient twinCatClient, TcAdsSymbolInfoLoader symbolLoader, string plcPath)
        {
            IDictionary<string, T> array = new Dictionary<string, T>();

            TcAdsSymbolInfo arraySymbol = symbolLoader.FindSymbol(plcPath);
            if (arraySymbol == null)
                return array;

            var stream = new AdsStream(8);
            var reader = new AdsBinaryReader(stream);

            TcAdsSymbolInfo elementSymbol = arraySymbol.FirstSubSymbol;
            while (elementSymbol != null)
            {
                stream.Position = 0;
                twinCatClient.Read(elementSymbol.IndexGroup, elementSymbol.IndexOffset, stream);

                var pointerValue = PlcSystem.IsX64Mode ? reader.ReadInt64() : reader.ReadInt32();

                if (pointerValue != 0)
                {
                    string plcArrayElementPath = elementSymbol.Name;

                    if (elementSymbol.IsPointer)
                        plcArrayElementPath = string.Format("{0}^", plcArrayElementPath);

                    var handle = twinCatClient.CreateVariableHandle(plcArrayElementPath);

                    try
                    {
                        var element = (T) twinCatClient.ReadAny(handle, typeof (T));
                        array.Add(plcArrayElementPath, element);
                    }
                    finally
                    {
                        twinCatClient.DeleteVariableHandle(handle);
                    }
                }
                elementSymbol = elementSymbol.NextSymbol;
            }

            return array;
        }
    }
}
