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
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;
using VP.FF.PT.Common.PlcCommunicationSimulator.Behavior;

namespace VP.FF.PT.Common.PlcCommunicationSimulator
{
    /// <summary>
    /// TagController mock for simulation. No real PLC required.
    /// </summary>
    [Export(typeof(ITagController))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [DebuggerDisplay("IsConnected={IsConnected}")]
    public class SimulatedTagController : ITagController, IPartImportsSatisfiedNotification
    {
        internal event EventHandler<Tag> TagWrittenEvent; 

        [Import]
        internal ISimulatedBehaviorManagerInternal SimulatedBehaviorManager = null;

        private readonly LooseTagStorage _looseTagStorage;

        [ImportingConstructor]
        public SimulatedTagController(LooseTagStorage looseTagStorage)
        {
            _looseTagStorage = looseTagStorage;
        }

        public void Dispose()
        {
            IsConnected = false;
        }

        public void AddUdtHandler(string dataType, ConvertToLogixTagValueFunction convertToLogixTagValue)
        {
        }

        public void StartConnection()
        {
            IsConnected = true;
        }

        public void StartConnection(string address, int path)
        {
            IsConnected = true;
        }

        public bool IsConnected { get; private set; }

        public Task WriteTag(Tag tag, object value)
        {
            StartConnection();

            var existingTag = _looseTagStorage.GetOrCreateTag(tag);

            try
            {
                value = Convert.ChangeType(value, IEC61131_3_DataTypes.NetDataTypes[tag.DataType]);
            }
            catch
            {
                var boolArray = value as bool[];

                if (boolArray != null)
                {
                    value = boolArray.Cast<object>().ToArray();
                }
            }

            tag.Value = value;
            existingTag.Value = value;

            if (TagWrittenEvent != null)
                TagWrittenEvent(this, tag);

            return Task.FromResult(true);
        }

        public Task WriteTag(Tag tag, object[] values)
        {
            StartConnection();

            if (TagWrittenEvent != null)
                TagWrittenEvent(this, tag);

            tag.Value = values;

            return Task.FromResult(true);
        }

        public void OnImportsSatisfied()
        {
            SimulatedBehaviorManager.AddTagController(this);
        }
    }
}
