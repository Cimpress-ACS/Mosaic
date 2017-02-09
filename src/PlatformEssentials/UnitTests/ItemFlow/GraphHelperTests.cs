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


using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.ItemFlow
{
    [TestFixture]
    public class GraphHelperTests
    {
        private GraphHelper _testee;

        private readonly Mock<IPlatformModule> _moduleA = new Mock<IPlatformModule>();
        private readonly Mock<IPlatformModule> _moduleB = new Mock<IPlatformModule>();
        private readonly Mock<IPlatformModule> _moduleC = new Mock<IPlatformModule>();
        private readonly Mock<IPlatformModule> _moduleD = new Mock<IPlatformModule>();

        [SetUp]
        public void SetUp()
        {
            _testee = new GraphHelper(CreateSampleModuleGraph());
        }

        [Test]
        public void Test_FindSourceModules()
        {
            var result = _testee.FindSourceModules();

            result.Count().Should().Be(2);
            result.Should().Contain(_moduleA.Object);
            result.Should().Contain(_moduleD.Object);
        }

        [Test]
        public void Test_FindSinkModules()
        {
            var result = _testee.FindSinkModules();

            result.Count().Should().Be(1);
            result.First().Should().Be(_moduleC.Object);
        }

        [Test]
        public void Test_FindUpStreamModules_OfModuleB()
        {
            var result = _testee.FindUpStreamModules(_moduleB.Object);

            result.Count().Should().Be(1);
            result.First().Should().Be(_moduleA.Object);
        }

        [Test]
        public void Test_FindUpStreamModules_OfModuleC()
        {
            var result = _testee.FindUpStreamModules(_moduleC.Object);

            result.Count().Should().Be(2);
            result.Should().Contain(_moduleB.Object);
            result.Should().Contain(_moduleD.Object);
        }

        [Test]
        public void Test_FindDownStreamModules_OfModuleB()
        {
            var result = _testee.FindDownStreamModules(_moduleB.Object);

            result.Count().Should().Be(1);
            result.First().Should().Be(_moduleC.Object);
        }

        //
        // ModuleA (source) --> ModuleB ------> ModuleC (sink)
        //                                /
        //                      ModuleD -/
        private ModuleGraph CreateSampleModuleGraph()
        {
            var graph = new ModuleGraph();

            graph.AddVertex(_moduleA.Object);
            graph.AddVertex(_moduleB.Object);
            graph.AddVertex(_moduleC.Object);
            graph.AddVertex(_moduleD.Object);

            graph.AddEdge(new ModuleGraphEdge("A->B", _moduleA.Object, _moduleB.Object, 0, 0));
            graph.AddEdge(new ModuleGraphEdge("B->C", _moduleB.Object, _moduleC.Object, 0, 0));
            graph.AddEdge(new ModuleGraphEdge("D->C", _moduleD.Object, _moduleC.Object, 0, 1));

            return graph;
        }
    }
}
