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
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace VP.FF.PT.Common.GuiEssentials.Converters
{
    public class ModuleStateToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
            {
                return Binding.DoNothing;
            }

            var targetElement = values[0] as FrameworkElement;

            if(targetElement == null || values.Length < 2)
                return null;

            var state = values[1] as string;
            if (state == null)
            {
                return Binding.DoNothing;
            }

            string stateColorType;
            switch (state.ToLower())
            {
                case "run":
                case "standby":
                    stateColorType = "Run";
                    break;
                case "stop":
                    stateColorType = "Stop";
                    break;
                case "error":
                    stateColorType = "Error";
                    break;
                case "run_busy":
                case "standby_busy":
                    stateColorType = "Run.Busy";
                    break;
                case "stop_busy":
                    stateColorType = "Stop.Busy";
                    break;
                case "error_busy":
                    stateColorType = "Error.Busy";
                    break;
                case "off":
                default:
                    stateColorType = "Off";
                    break;
            }

            var styleName = "Brush.ModuleControl.State." + stateColorType;

            return (Brush)targetElement.TryFindResource(styleName);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
