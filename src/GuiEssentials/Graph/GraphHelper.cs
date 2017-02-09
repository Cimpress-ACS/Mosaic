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


using QuickGraph.Objects;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.Entities.DTOs;
using VP.FF.PT.Common.WpfInfrastructure.Screens.Model;

namespace VP.FF.PT.Common.GuiEssentials.Graph
{
    public static class GraphHelper
    {
        public static void UpdateVertex(ModuleVertexViewModel vertex, PlatformModuleDTO platformModule)
        {
            if (vertex == null || platformModule == null)
                return;

            vertex.Capacity = platformModule.MaxCapacity;
            vertex.Counter = platformModule.PlatformItems.Count;

            if (platformModule.State == PlatformModuleState.Run)
                vertex.State = ModuleState.Run;
            else if (platformModule.State == PlatformModuleState.RunBusy)
                vertex.State = ModuleState.RunBusy;
            else if (platformModule.State == PlatformModuleState.Standby)
                vertex.State = ModuleState.Standby;
            else if (platformModule.State == PlatformModuleState.StandbyBusy)
                vertex.State = ModuleState.StandbyBusy;
            else if (platformModule.State == PlatformModuleState.OffBusy)
                vertex.State = ModuleState.OffBusy;
            else
                vertex.State = ModuleState.Off;

            vertex.IsBusy = platformModule.State == PlatformModuleState.RunBusy ||
                            platformModule.State == PlatformModuleState.OffBusy;

            vertex.HasWarnings = platformModule.HasWarnings;
            vertex.HasErrors = platformModule.HasErrors;
            vertex.AlertText = platformModule.MostImportantAlarmText;
        }
    }
}
