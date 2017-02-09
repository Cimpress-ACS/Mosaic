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
using TwinCAT.Ads;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.PlcBaseCommunication
{
    public static class TcAdsClientExtensions
    {
        /// <summary>
        /// Reads a pointer value of the specified <paramref name="typeOfValue"/>.
        /// </summary>
        /// <param name="twinCatClient">The twin cat client to read from.</param>
        /// <param name="plcPath">The path in the PC to the element.</param>
        /// <param name="typeOfValue">The expected type of the value.</param>
        /// <returns>
        /// The value of the element at the specified <paramref name="plcPath"/>.
        /// </returns>
        public static object GetPointerValue(this TcAdsClient twinCatClient, string plcPath, Type typeOfValue)
        {
            TcAdsSymbolInfoLoader symbolLoader = twinCatClient.CreateSymbolInfoLoader();
            TcAdsSymbolInfo symbol = symbolLoader.FindSymbol(plcPath);
            if (symbol == null)
                return null;

            var stream = new AdsStream(8);
            var reader = new AdsBinaryReader(stream);

            stream.Position = 0;
            twinCatClient.Read(symbol.IndexGroup, symbol.IndexOffset, stream);

            var pointerValue = PlcSystem.IsX64Mode ? reader.ReadInt64() : reader.ReadInt32();

            if (pointerValue != 0)
            {
                string plcArrayElementPath = symbol.Name;

                if (symbol.IsPointer)
                    plcArrayElementPath = string.Format("{0}^", plcArrayElementPath);

                var handle = twinCatClient.CreateVariableHandle(plcArrayElementPath);

                try
                {
                    return twinCatClient.ReadAny(handle, typeOfValue);
                }
                finally
                {
                    twinCatClient.DeleteVariableHandle(handle);
                }
            }

            return null;
        }
    }
}
