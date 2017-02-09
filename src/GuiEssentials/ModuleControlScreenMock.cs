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


using System.Threading.Tasks;
using VP.FF.PT.Common.WpfInfrastructure.Screens.ViewModels;

namespace VP.FF.PT.Common.GuiEssentials
{
    public class ModuleControlScreenMock : ModuleControlScreen
    {
        public ModuleControlScreenMock()
        {
            Throughput = 1187.4;
            ThroughputMin = 0;
            ThroughputMax = 3000;
            CurrentItemCount = 5449;
            UpTime = 250;
            DownTime = 43;
            State = "Run";
            SubState = "Wait Operator";
            ModuleState = WpfInfrastructure.Screens.Model.ModuleState.Run;
            AlarmSummaryViewModel = new AlarmSummaryViewModel();
        }

        public override Task Initialize()
        {
            return Task.FromResult(true);
        }

        public override Task Shutdown()
        {
            return Task.FromResult(true);
        }

        public override int SortOrder
        {
            get { return 0; }
        }

        public override string IconKey
        {
            get { return "N/A"; }
        }

        public override GenericPlcViewModel DetailViewModel
        {
            get { return new GenericPlcViewModel(this); }
        }
    }
}
