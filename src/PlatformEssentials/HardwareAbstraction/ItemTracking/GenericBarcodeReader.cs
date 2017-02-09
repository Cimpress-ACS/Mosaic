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

namespace VP.FF.PT.Common.PlatformEssentials.HardwareAbstraction.ItemTracking
{
    /// <summary>
    /// Dummy implementation which can be replaced by some real hardware readers
    /// </summary>
    [Export(typeof(IBarcodeReader))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GenericBarcodeReader : ModuleEquipment, IBarcodeReader
    {
        private readonly ILogger _logger;
        public event EventHandler<BarcodeEventArgs> BarcodeReceivedEvent;
        public event EventHandler<BarcodeEventArgs> InvalidBarcodeEvent;

        [ImportingConstructor]
        public GenericBarcodeReader(ILogger logger)
        {
            _logger = logger;
            _logger.Init(GetType());
            EquipmentName = GetType().Name;
        }

        public void FireBarcodeRead(string scannedBarcode)
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
