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
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials;
using VP.FF.PT.Common.PlatformEssentials.HardwareAbstraction.ItemTracking;

namespace VP.FF.PT.Common.Simulation.HardwareAbstraction
{
    /// <summary>
    /// This fakes a IBarcodeReader interface for mosaic, don't mismatch it with the simulated equipment.
    /// </summary>
    [Export("simulation", typeof(IBarcodeReader))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SimulatedBarcodeReader : ModuleEquipment, IBarcodeReader
    {
        private readonly ILogger _logger;
        public event EventHandler<BarcodeEventArgs> BarcodeReceivedEvent;
        public event EventHandler<BarcodeEventArgs> InvalidBarcodeEvent;

        [ImportingConstructor]
        public SimulatedBarcodeReader(ILogger logger)
        {
            _logger = logger;
            _logger.Init(GetType());
            EquipmentName = GetType().Name;
        }

        public void SimulateBarcodeRead(string scannedBarcode)
        {
            if (scannedBarcode.IsNullOrEmpty())
            {
                _logger.Warn("invalid empty barcode");

                if (InvalidBarcodeEvent != null)
                    InvalidBarcodeEvent(this, new BarcodeEventArgs(scannedBarcode));
            }

            if (BarcodeReceivedEvent != null)
                BarcodeReceivedEvent.Invoke(this, new BarcodeEventArgs(scannedBarcode));
        }
    }
}
