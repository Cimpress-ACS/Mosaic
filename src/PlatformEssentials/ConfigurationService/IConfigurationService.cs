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


using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace VP.FF.PT.Common.PlatformEssentials.ConfigurationService
{
    [ServiceContract]
    public interface IConfigurationService
    {
        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        bool KeepAlive();

        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        string GetValue(string key);

        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        bool GetBoolValue(string key);

        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        void SetValue(string key, string value);

        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        void SetBoolValue(string key, bool value);

        [OperationContract]
        [FaultContract(typeof(ConfigurationFault))]
        IEnumerable<string> GetAllKeys();

        bool IsInitialized
        {
            [OperationContract]
            [FaultContract(typeof(ConfigurationFault))]
            get;
        }
    }

    [DataContract]
    public class ConfigurationFault
    {
    }
}
