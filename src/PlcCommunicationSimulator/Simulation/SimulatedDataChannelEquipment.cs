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
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunicationSimulator.Behavior;
using VP.FF.PT.Common.Simulation;
using VP.FF.PT.Common.Simulation.Equipment;

namespace VP.FF.PT.Common.PlcCommunicationSimulator.Simulation
{
    public class SimulatedDataChannelEquipment : ISimulatedEquipment
    {
        private readonly Tag _dataChannelTag;
        private readonly Func<ISimulatedItem, object> _getDataChannelValue;
        private readonly Tag _dataChannelStateTag;

        private const string UdtHandshakeFieldName = "intDataState";

        public SimulatedDataChannelEquipment(int position, ISimulatedPlcBehaviorManager plcBehaviorManager, Tag dataChannelTag, Func<ISimulatedItem, object> getDataChannelValue)
        {
            IsActive = true;
            _dataChannelTag = dataChannelTag;
            _getDataChannelValue = getDataChannelValue;
            Position = position;

            _dataChannelStateTag = plcBehaviorManager.SearchTag(dataChannelTag.Scope + "." + dataChannelTag.Name + "." + UdtHandshakeFieldName);
        }

        public bool IsActive { get; set; }
        public int Position { get; private set; }
        public ulong ItemPassedCount { get; private set; }

        public void ItemPassed(ISimulatedItem item)
        {
            if (IsActive)
            {
                ItemPassedCount++;
                _dataChannelTag.Value = _getDataChannelValue(item);
                _dataChannelStateTag.Value = (short) DataStateEnum.DataWritten;
            }
        }
    }
}
