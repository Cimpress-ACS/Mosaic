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
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow
{
    public class ForcingRouteCalculator : RouteCalculator, IRouteForcing<IPlatformModule>
    {
        protected readonly IDictionary<IPlatformModule, HashSet<int>> CurrentForcedPorts;
        protected readonly ISet<String> IgnoreTargetState = new HashSet<String>();
        protected readonly ILogger Logger;
        protected readonly IPlatformScheduler Scheduler = new SingleActionScheduler();

        public ForcingRouteCalculator(IEnumerable<IPlatformModule> modules, ModuleGraph graph, ILogger logger) 
            : base(modules, graph, logger)
        {
            Logger = logger;
            CurrentForcedPorts = new Dictionary<IPlatformModule, HashSet<int>>();
            Initialize();
        }

        private void SetModules()
        {
            foreach (var platformModule in Modules)
            {
                CurrentForcedPorts.Add(platformModule, new HashSet<int>());
            }
        }

        private void Initialize()
        {
            SetModules();

            foreach (var edge in Graph.Edges.Where(edge => edge.IsForcingEnabled))
            {
                ForcePath(edge.Source, edge.Target, edge.OriginPort, edge.TargetPort);
            }
        }

        public void ForcePath(IPlatformModule sourceModule, IPlatformModule targetModule, int sourcePortIndex, int targetPortIndex)
        {
            CurrentForcedPorts[sourceModule].Add(sourcePortIndex);

            RecalculateRoute();
        }

        public void ReleaseForcePath(IPlatformModule sourceModule, IPlatformModule targetModule, int sourcePortIndex, int targetPortIndex)
        {
            CurrentForcedPorts[sourceModule].Remove(sourcePortIndex);

            RecalculateRoute();
        }

        public void SetIgnoreDownstreamModule(string moduleName, bool ignore)
        {
            if (!ignore && IgnoreTargetState.Contains(moduleName))
            {
                IgnoreTargetState.Remove(moduleName);
            }

            if (ignore && !IgnoreTargetState.Contains(moduleName))
            {
                IgnoreTargetState.Add(moduleName);
            }

            RecalculateRoute();
        }

        public bool GetIgnoreDownstreamModule(string moduleName)
        {
            if (IgnoreTargetState.Contains(moduleName))
            {
                return true;
            }

            return false;
        }

        public void RecalculateRoute()
        {
            Scheduler.Schedule(DoRecalculateRoute);
        }

        virtual protected void DoRecalculateRoute()
        {
            foreach (var currentForcedPort in CurrentForcedPorts)
            {
                var module = currentForcedPort.Key;

                if (!module.IsInitialized)
                    return;

                var ports = currentForcedPort.Value;

                foreach (var port in ports)
                {
                    var edge = (from e in Graph.Edges where e.Source == module && e.OriginPort == port select e).First();

                    bool isPortFree = !edge.Target.IsFull(edge.TargetPort);

                    int limit = edge.Target.LimitItemCount;
                    bool hasCapacity = limit == 0 || edge.Target.CurrentItemCount < limit;

                    bool targetStateOk =
                        IgnoreTargetState.Contains(edge.Source.Name) ||
                        IsRunStandbyOrInTransition(edge.Target);

                    if (targetStateOk && isPortFree && hasCapacity)
                    {
                        edge.Source.AddPortRouting(port);
                    }
                    else
                    {
                        edge.Source.RemovePortRouting(port);
                    }
                }
            }
        }

        protected bool IsRunStandbyOrInTransition(IPlatformModule target)
        {
            if (target.State == PlatformModuleState.Run ||
                target.State == PlatformModuleState.Standby ||
                (target.OldState == PlatformModuleState.Standby && target.State == PlatformModuleState.RunBusy) ||
                (target.OldState == PlatformModuleState.Run && target.State == PlatformModuleState.StandbyBusy))
            {
                return true;
            }

            return false;
        }
    }
}
