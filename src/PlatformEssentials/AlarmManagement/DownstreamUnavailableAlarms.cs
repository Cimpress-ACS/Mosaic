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
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Linq;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow;

namespace VP.FF.PT.Common.PlatformEssentials.AlarmManagement
{
    [Export(typeof(IDownstreamUnavailableAlarms))]
    public class DownstreamUnavailableAlarms : IDownstreamUnavailableAlarms
    {
        internal struct AlarmKey
        {
            public IPlatformModule Module;
            public string Message;
        }

        private readonly ConcurrentDictionary<AlarmKey, Alarm> _alarms;
        private readonly IModuleBusManager _moduleBusManager;

        [ImportingConstructor]
        public DownstreamUnavailableAlarms(IModuleBusManager moduleBusManager)
        {
            _alarms = new ConcurrentDictionary<AlarmKey, Alarm>();
            _moduleBusManager = moduleBusManager;
        }

        public Alarm GetAlarm(IPlatformModule module)
        {
            var message = string.Format("Stopped {0} because {1} is not available", module.Name, GetNextModuleName(module));
            var alarm = _alarms.GetOrAdd(new AlarmKey { Module = module, Message = message }, m => CreateAlarm(m.Module, m.Message));
            return alarm;
        }

        public Alarm GetAlarm(IPlatformModule module, Func<string> message)
        {
            var alarm = _alarms.GetOrAdd(new AlarmKey { Module = module, Message = message() }, m => CreateAlarm(m.Module, m.Message));
            return alarm;
        }

        private Alarm CreateAlarm(IPlatformModule module, string message)
        {
            return new Alarm
            {
                Message = message,
                Type = AlarmType.Warning,
                Source = module.Name,
                SourceType = AlarmSourceType.LineControl,
                Timestamp = DateTime.Now
            };
        }

        private string GetNextModuleName(IPlatformModule module)
        {
            try
            {
                var graph = _moduleBusManager.GraphDto;
                var edge = graph.Edges.LastOrDefault(e => e.Source == module.Name);
                return edge == null ? "the next module" : edge.Target;
            }
            catch (Exception)
            {
                return "the next module";
            }
        }
    }
}
