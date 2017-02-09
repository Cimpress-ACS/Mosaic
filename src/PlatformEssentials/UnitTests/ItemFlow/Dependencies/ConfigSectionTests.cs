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
using System.Configuration;
using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.PlatformEssentials.ModuleWiringConfigSection;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.ItemFlow.Dependencies
{
    [TestFixture]
    public class ConfigSectionTests
    {
        [Test]
        public void GivenValidAppConfig_ShouldParseWithoutErrors()
        {
            Action readConfig = () => ConfigurationManager.GetSection("moduleWiring2");

            readConfig.ShouldNotThrow();
        }

        [Test]
        public void DeepConfigCheck()
        {
            var config = ConfigurationManager.GetSection("moduleWiring2") as ModuleWiringConfigSection.ModuleWiringConfigSection;

            foreach (ModuleConfig module in config.Modules)
            {
                if (module.Name == "JEM")
                {
                    module.ContractType.Should().Be("1");
                    module.ModuleTypeId.Should().Be(1);
                    module.NextModules.Should().HaveCount(1);
                }
                else if (module.Name == "GRM")
                {
                    module.ContractType.Should().Be("2");
                    module.ModuleTypeId.Should().Be(2);
                    module.NextModules.Should().HaveCount(0);

                    module.Dependencies.Should().HaveCount(2);
                    foreach (ModuleDependencyConfig dependency in module.Dependencies)
                    {
                        
                    }

                }
                else
                {
                    Assert.Fail("unknown module {0}", module.Name);
                }
            }
        }
    }
}
