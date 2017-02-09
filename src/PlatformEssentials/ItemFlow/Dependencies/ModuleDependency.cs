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
using System.Linq;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow.Dependencies
{
    public class ModuleDependency : IDependency
    {
        private readonly ILogger _logger;
        private readonly List<ICondition> _conditions = new List<ICondition>();
        private readonly List<ITrigger> _triggers = new List<ITrigger>();
        private readonly List<IAction> _actions = new List<IAction>();

        public ModuleDependency(ILogger logger)
        {
            _logger = logger;
            IsEnabled = true;
        }

        public bool IsEnabled { get; set; }

        public void AddCondition(ICondition condition)
        {
            _conditions.Add(condition);
        }

        public void AddTrigger(ITrigger trigger)
        {
            trigger.TriggerOccurred += (s, e) =>
            {
                _logger.DebugFormat("trigger " + trigger + " occurred");
                OnTriggerOccurred(s, e);
            };

            _triggers.Add(trigger);
        }

        public void AddAction(IAction action)
        {
            _actions.Add(action);
        }

        private void OnTriggerOccurred(object sender, EventArgs eventArgs)
        {
            if (!IsEnabled)
                return;

            if (_conditions.Any(condition => !condition.IsTrue()))
                return;

            foreach (var action in _actions)
            {
                action.Execute();
                _logger.DebugFormat("executed dependency action \"{0}\"", action);
            }
        }
    }
}
