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
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunicationSimulator.Behavior;
using VP.FF.PT.Common.PlcEssentials;
using VP.FF.PT.Common.PlcEssentials.Impl;

namespace VP.FF.PT.Common.PlcCommunicationSimulator
{
    [Export(typeof(IAlarmsImporter))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SimulatedAlarmsImporter : IAlarmsImporter, IPartImportsSatisfiedNotification
    {
        [Import]
        internal ISimulatedBehaviorManagerInternal SimulatedBehaviorManager = null;

        public SimulatedAlarmsImporter()
        {
            AlarmsChanged += a => { };
        }

        public event Action<IEnumerable<Alarm>> AlarmsChanged { add { } remove { } }

        public void Initialize(ITagListener tagListener)
        {
        }

        public void ImportAlarms()
        {
        }

        public IEnumerable<Alarm> GetAllImportedAlarms()
        {
            yield break;
        }

        public IEnumerable<Alarm> GetAlarms(int controllerId)
        {
            yield break;
        }

        public void OnImportsSatisfied()
        {
            SimulatedBehaviorManager.AddAlarmsImporter(this);
        }
    }
}
