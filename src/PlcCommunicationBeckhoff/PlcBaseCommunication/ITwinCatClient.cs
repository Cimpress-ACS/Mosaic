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
using TwinCAT.Ads;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.PlcBaseCommunication
{
    /// <summary>
    /// Wraps around the <see cref="TcAdsClient"/> and provides some easy access methods
    /// </summary>
    public interface ITwinCatClient
    {
        /// <summary>
        /// Creates and initializes the <see cref="TcAdsClient"/>.
        /// </summary>
        /// <param name="address">The connection string or path (file path or IP address depending on implementation). In case of Beckhoff this is the AdsAddress.</param>
        /// <param name="port">The ADS Port in case of Beckhoff implementation.</param>
        void Initialize(string address, int port);

        /// <summary>
        /// Reads the element from the specified <paramref name="elementPath"/> and returns it.
        /// </summary>
        /// <param name="elementPath">The path to read from.</param>
        /// <param name="typeOfElement">The type of the desired element.</param>
        /// <returns>The read value.</returns>
        object ReadElement(string elementPath, Type typeOfElement);

        /// <summary>
        /// Reads the element from the specified <paramref name="elementPath"/> and returns it.
        /// </summary>
        /// <typeparam name="TElement">The type of the desired element.</typeparam>
        /// <param name="elementPath">The path to read from.</param>
        /// <returns>The read value.</returns>
        TElement ReadElement<TElement>(string elementPath);

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
        IEnumerable ReadElementsArray(string elementsArrayPath, int arraySize, Type typeOfElements);

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
        IEnumerable<TElement> ReadElementsArray<TElement>(string elementsArrayPath, int arraySize);

        /// <summary>
        /// Reads an elements array of type TElement. Supports pointered arrays (POINTER TO ...).
        /// </summary>
        /// <typeparam name="TElement">Marshall type representation in .NET. See Beckhoff TwinCat 3 manual for an example.</typeparam>
        /// <param name="elementsArrayPath">The path in PLC to the array.</param>
        /// <returns>
        /// A dictionary of imported and converted (.NET type) array elements (value) sorted by their pathes (key).
        /// </returns>
        IDictionary<string, TElement> ReadElementsArrayWithPathes<TElement>(string elementsArrayPath);
    }
}
