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

namespace VP.FF.PT.Common.PlatformEssentials.TCPManager
{
    public interface IDataConverter<TData>
    {
        /// <summary>
        /// Parses the received raw data and assembles the data to a object of type TData. All these
        /// objects will be returned as a list of that type TData.
        /// </summary>
        /// <param name="newRawData">the raw data that was just received from the client</param>
        List<TData> Parse(byte[] newRawData);

        /// <summary>
        /// Converts the item of type T to a Byte Array
        /// </summary>
        Byte[] GetBytes(TData item);

        /// <summary>
        /// This method is called after a error or on init
        /// </summary>
        void Init();
    }
}
