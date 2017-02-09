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


using System.Configuration;

namespace VP.FF.PT.Common.PlatformEssentials.ModuleWiringConfigSection
{
      //<moduleWiring>
      //  <modules>
      //    <module name="ItemInjectorModule">
      //      <config>
      //        <AdsPort>855</AdsPort>
      //      </config>
      //      <nextModules>
      //        <add name="TtsModule" port="0" targetPort="0"/>
      //      </nextModules>
      //    </module>
      //    <module name="TtsModule">
      //      <nextModules>
      //        <add name="DryerModule" port="0" targetPort="1"/>
      //      </nextModules>
      //    </module>
      // ...
      //</moduleWiring>
    public class ModuleWiringConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("modules")]
        public ModulesConfigCollection Modules
        {
            get { return (ModulesConfigCollection)base["modules"]; }
        }
    }
}
