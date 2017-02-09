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


using System.Windows;
using System.Windows.Controls;

namespace VP.FF.PT.Common.GuiEssentials
{
    /// <summary>
    /// Interaction logic for ModuleScreenView.xaml
    /// </summary>
    public partial class ModuleScreenView
    {
        public static readonly DependencyProperty ModuleSpecificPageProperty = DependencyProperty.Register(
            "ModuleSpecificPage", typeof(ContentControl), typeof(ModuleScreenView), new PropertyMetadata(default(ContentControl)));

        public ContentControl ModuleSpecificPage
        {
            get { return (ContentControl)GetValue(ModuleSpecificPageProperty); }
            set { SetValue(ModuleSpecificPageProperty, value); }
        }

        public static readonly DependencyProperty ModuleDetailsPageProperty = DependencyProperty.Register(
            "ModuleDetailsPage", typeof(ContentControl), typeof(ModuleScreenView), new PropertyMetadata(default(ContentControl)));

        public ContentControl ModuleDetailsPage
        {
            get { return (ContentControl)GetValue(ModuleDetailsPageProperty); }
            set { SetValue(ModuleDetailsPageProperty, value); }
        }

        public ModuleScreenView()
        {
            InitializeComponent();
        }

    }
}
