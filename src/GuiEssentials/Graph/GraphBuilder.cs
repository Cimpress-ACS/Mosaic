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
using System.Windows;
using QuickGraph.Objects;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow.DTOs;

namespace VP.FF.PT.Common.GuiEssentials.Graph
{
    public static class GraphBuilder
    {
        private const double EdgeDistance = 32;
        private const string FallbackIconKey = "type_missing";

        public static ModuleGraph BuildGraph(ILogger logger, ModuleGraphDTO moduleGraphDto, IList<ModuleVertexViewModelBase> positions, bool smallFallbackIcons = false)
        {
            var graph = new ModuleGraph();

            try
            {
                foreach (var platformModuleDto in moduleGraphDto.Vertices)
                {
                    var res = (from p in positions
                               where p.ID == platformModuleDto.Name
                               select p).FirstOrDefault();

                    Point pos = res == null ? new Point(50, 50) : res.Position;

                    var imagepath = @"Images\" + platformModuleDto.Type + ".png";
                    graph.AddVertex(new ModuleVertexViewModel(platformModuleDto.Name, imagepath, pos)
                    {
                        Capacity = platformModuleDto.MaxCapacity,
                        Counter = platformModuleDto.PlatformItems.Count,
                        Position = pos,
                        DefaultVertexHeight = smallFallbackIcons ? 100 : 700
                    });
                }

                foreach (var edgeDto in moduleGraphDto.Edges)
                {
                    var edge = new ModuleEdgeViewModel(
                         edgeDto.Id,
                         FindModuleVertex(graph, edgeDto.Source),
                         FindModuleVertex(graph, edgeDto.Target))
                    {
                        OriginPortIndex = edgeDto.OriginPort,
                        IsForcingEnabled = edgeDto.IsForcingEnabled
                    };

                    DetectCyclicEdge(graph, edge);

                    graph.AddEdge(edge);
                }

                foreach (var edge in graph.Edges)
                {
                    DetectMultipleEdges(graph, edge);
                }
            }
            catch (Exception e)
            {
                logger.Error("Could not load and create Module PaperStreamGraph for Overview.", e);
                throw;
            }

            return graph;
        }

        private static void DetectMultipleEdges(ModuleGraph graph, ModuleEdgeViewModel edge)
        {
            var multiEdges = from e in graph.Edges
                             where e.Source == edge.Source &&
                                     e.Target == edge.Target
                             select e;

            int numEdges = multiEdges.Count();

            if (numEdges > 1)
            {
                int edgeIndex = 0;
                foreach (var multiEdge in multiEdges)
                {
                    if (multiEdge.EdgeMargin.Top.Equals(0))
                    {
                        multiEdge.EdgeMargin = new Thickness(
                            0,
                            (EdgeDistance * edgeIndex++) - ((EdgeDistance * numEdges) / 2) + 1,
                            0,
                            0);
                    }
                }
            }
        }

        private static void DetectCyclicEdge(ModuleGraph graph, ModuleEdgeViewModel edge)
        {
            var cyclicEdge = (from e in graph.Edges
                              where e.Source == edge.Target &&
                                      e.Target == edge.Source
                              select e).FirstOrDefault();

            if (cyclicEdge != null)
            {
                cyclicEdge.FirstEdge = true;
                edge.SecondEdge = true;
            }
        }

        private static ModuleVertexViewModelBase FindModuleVertex(ModuleGraph graph, string moduleName)
        {
            return graph.Vertices.FirstOrDefault(moduleVertex => moduleVertex.ID == moduleName);
        }
    }
}
