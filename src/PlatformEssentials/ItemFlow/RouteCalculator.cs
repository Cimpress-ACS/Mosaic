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
using QuickGraph;
using QuickGraph.Algorithms;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.Entities.DTOs;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow.DTOs;

namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow
{
    public class RouteCalculator : IRouteCalculator<IPlatformModule>
    {
        protected IEnumerable<IPlatformModule> Modules { get; private set; }

        internal protected ModuleGraph Graph { get; private set; }

        private readonly ILogger _logger;

        // cost for an item for transportation between 2 modules (TODO: allow individual transportation costs)
        private const int ItemTransitionCost = 1;

        public RouteCalculator(IEnumerable<IPlatformModule> modules, ModuleGraph graph, ILogger logger)
        {
            if (graph == null)
            {
                throw new ArgumentException("Graph cannot be null.", "graph");
            }

            Modules = modules;
            Graph = graph;
            _logger = logger;
        }

        public bool IsRoutePossible(IPlatformModule sourceModule, IPlatformModule targetModule)
        {
            Func<ModuleGraphEdge, double> edgeCost = x => 1;
            TryFunc<IPlatformModule, IEnumerable<ModuleGraphEdge>> tryGetPaths = Graph.ShortestPathsDijkstra(edgeCost, sourceModule);

            IEnumerable<ModuleGraphEdge> path;
            return tryGetPaths(targetModule, out path);
        }

        public bool GraphContainsModule(IPlatformModule module)
        {
            return Graph.Vertices.Contains(module);
        }

        public IPlatformModule GetTargetModule(int releasePort, IPlatformModule module)
        {
            var targetEdge = (from e in Graph.Edges where e.Source == module && e.OriginPort == releasePort select e.Target).SingleOrDefault();
            return targetEdge;
        }

        /// <summary>
        /// Calculates the single item routing based on Module states, CurrentItemCount, Limit, port-full, 
        /// and RouteItem flags like ForbiddenModuleType and ForceModuleInstance.
        /// </summary>
        /// <param name="item">The underlying item for path finding.</param>
        /// <param name="associatedModule">The current origin module of the item.</param>
        public bool CalculateSingleItemRouting(PlatformItem item, IPlatformModule associatedModule)
        {
            if (item == null || item.Route == null)
            {
                return false;
            }

            var nextRouteIndex = item.Route.CurrentIndex + 1;

            if (nextRouteIndex < item.Route.RouteItems.Count)
            {
                var nextRouteItem = item.Route.GetOrderedList()[nextRouteIndex];
                var potentialTargetModules = from m in Modules
                                             where m.ModuleTypeId == nextRouteItem.ModuleType
                                             orderby m.CurrentItemCount / (double)m.MaxCapacity
                                             select m;

                // compute shortest paths
                Func<ModuleGraphEdge, double> edgeCost = EdgeCostFunction;
                TryFunc<IPlatformModule, IEnumerable<ModuleGraphEdge>> tryGetPaths = Graph.ShortestPathsDijkstra(edgeCost, associatedModule);

                foreach (var potentialTargetModule in potentialTargetModules)
                {
                    if (!IsRoutePossible(associatedModule, potentialTargetModule))
                    {
                        continue;
                    }

                    // force specific Modules
                    if (!string.IsNullOrEmpty(nextRouteItem.ForceModuleInstance) &&
                        potentialTargetModule.Name != nextRouteItem.ForceModuleInstance)
                    {
                        continue;
                    }

                    // query path for given vertices
                    IEnumerable<ModuleGraphEdge> path;
                    if (tryGetPaths(potentialTargetModule, out path))
                    {
                        // handle forbidden modules
                        var edges = path.ToArray();

                        // verify there are no forbidden paths
                        var forbiddenEdge = from e in edges
                                            where e.Target.ModuleTypeId == nextRouteItem.ForbiddenModuleType
                                            select e;

                        if (forbiddenEdge.Any())
                        {
                            continue;
                        }

                        // verify the route is currently possible given states and capacity constraints
                        var route = edges.First();
                        if (potentialTargetModule.State != PlatformModuleState.Run || route.Target.IsFull(route.TargetPort) || !HasCapacity(route))
                        {
                            continue;
                        }

                        _logger.DebugFormat("AddItemRouting with ItemId={0} from module {1} to module {2}.", item.ItemId, associatedModule.Name, route.Target.Name);

                        // all restrictions passed; route item
                        route.Source.AddItemRouting(item, route.OriginPort);
                        return true;
                    }
                }
            }

            // no path could be found; cancel routing of the item
            return false;
        }

        private static bool HasCapacity(ModuleGraphEdge route)
        {
            if (route.Target.LimitItemCount > 0)
            {
                return route.Target.CurrentItemCount < route.Target.LimitItemCount;
            }
            return true;
        }

        private static double EdgeCostFunction(ModuleGraphEdge edge)
        {
            if (edge.Target.State != PlatformModuleState.Run)
            {
                return double.PositiveInfinity;
            }

            if (edge.Target.IsFull(edge.TargetPort))
            {
                return double.PositiveInfinity;
            }

            // TODO: respect throughput capability of individual modules as soon as the minimalistic statistic feature is implemented in Mosaic
            return (double)ItemTransitionCost + edge.Target.CurrentItemCount;
        }

        public ModuleGraphDTO GraphToDto()
        {
            var graphDto = new ModuleGraphDTO
            {
                Vertices = (from v in Graph.Vertices let pm = (PlatformModule)v select pm.ToDTO()).ToList(),
                Edges =
                    (from edge in Graph.Edges
                     select new ModuleGraphEdgeDTO(edge.Id, edge.Source.Name, edge.Target.Name, edge.OriginPort, edge.TargetPort, edge.IsForcingEnabled))
                        .ToList()
            };

            return graphDto;
        }
    }
}
