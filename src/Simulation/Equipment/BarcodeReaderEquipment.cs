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


using System.ComponentModel.Composition;
using System.Diagnostics;
using VP.FF.PT.Common.Simulation.HardwareAbstraction;

namespace VP.FF.PT.Common.Simulation.Equipment
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [DebuggerDisplay("BarcodeReaderEquipment Position:{Position} ReadCount:{ItemPassedCount}")]
    public class BarcodeReaderEquipment : ISimulatedEquipment
    {
        private SimulatedBarcodeReader _mosaicBarcodeReader;

        public bool IsActive { get; set; }
        public int Position { get; private set; }
        public ulong ItemPassedCount { get; private set; }

        public BarcodeReaderEquipment()
        {
            IsActive = true;
        }

        public void Initialize(int position, SimulatedBarcodeReader mosaicBarcodeReader)
        {
            Position = position;
            _mosaicBarcodeReader = mosaicBarcodeReader;
        }

        public void ItemPassed(ISimulatedItem item)
        {
            if (IsActive)
            {
                ItemPassedCount++;

                object barcode;
                item.Metadata.TryGetValue("barcode", out barcode);

                _mosaicBarcodeReader.SimulateBarcodeRead(barcode != null ? barcode.ToString() : string.Empty);
            }
        }
    }
}
