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


using System.Collections.ObjectModel;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow;

// Module PaperStreamGraph overview:
//
//      ModuleB
//      |    /|\               --> ModuleD --
//     \|/    |0              /0             \0       /-0------------\0
//      ModuleA --1--> ModuleC                 ModuleF                ModuleG
//                            \1             /1       \-1--ModuleX---/1
//                             --> ModuleE --
//
// ModuleA [1] (Source) simulates an item source
// ModuleB [2] (ProcessA) simulates circular routing
// ModuleC [3] (ProcessB) simulates multiple ways to ModuleF, because ModuleD and ModuleE are same type
// ModuleD [4] simulates same type
// ModuleE [4] simulates same type
// ModuleF [6] (Consolidation) simulates multiple inputs and multiple output to single ModuleG
// ModuleG [7] (Sink) simulates an item sink
// ModuleX [8] simulates a module in between
namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.ItemFlow
{
    /// <summary>
    /// Tests for ModuleBusManager implementation base on a sample graph which should cover the most cases of routing.
    /// Heavy unit test because it uses MEF container and a real EventAggregator and PlatformModule base class 
    /// instead of mock to simplify setup (it's tested anyway in another unit test).
    /// </summary>
    [TestFixture]
    public class ItemFlowTests : ItemFlowTestBase<ModuleMock>
    {
        [SetUp]
        public void Setup()
        {
            SetUp();

            var moduleBusManagers = Container.GetExportedValues<IModuleBusManager>().ToArray();

            foreach (var moduleBusManager in moduleBusManagers.OfType<IPlatformModuleRouteForcing>())
            {
                moduleBusManager.ReleaseForcePath(ModuleA, ModuleC, 1, 0);
                moduleBusManager.ReleaseForcePath(ModuleA, ModuleB, 0, 0);
                moduleBusManager.ReleaseForcePath(ModuleB, ModuleA, 0, 0);
                moduleBusManager.ReleaseForcePath(ModuleD, ModuleF, 0, 0);
                moduleBusManager.ReleaseForcePath(ModuleE, ModuleF, 0, 1);
                moduleBusManager.ReleaseForcePath(ModuleX, ModuleG, 0, 1);
            }

            StartAllModules();
        }

        [TearDown]
        public void TearDown()
        {
            base.TearDown();
        }

        // tests if settings from App.config readed correctly
        [Test]
        public void InitializeValidationTest()
        {
            ModuleBusManager.GraphDto.Vertices.Should().HaveCount(8, "8 modules (A-G) were exported");
            ModuleBusManager.GraphDto.Edges.Should().HaveCount(10, "10 edges defined in App.config file in <nextModules> section");
            //SecondModuleBusManager.GraphDto.Vertices.Should().BeEmpty("moduleWiringShirtStream is defined in App.config without modules");
            ModuleA.ModuleTypeId.Should().Be(1);
            ModuleB.ModuleTypeId.Should().Be(2);
            ModuleC.ModuleTypeId.Should().Be(3);
            ModuleD.ModuleTypeId.Should().Be(4);
            ModuleE.ModuleTypeId.Should().Be(4);
            ModuleF.ModuleTypeId.Should().Be(6);
            ModuleG.ModuleTypeId.Should().Be(7);
            ModuleX.ModuleTypeId.Should().Be(8);
        }

        [Test]
        public void WhenImportModuleBusManager_ShouldBeSameInstance()
        {
            var testee = Container.GetExportedValue<IModuleBusManager>();

            testee.GraphDto.Should().NotBeNull("same instace has already been initialized");
            testee.GraphDto.Vertices.Should().HaveCount(x => x > 0, "same instace has already been initialized");
        }

        [Test]
        public void IsRoutePossible_WhenRouteNotPossible_ShouldReturnFalse()
        {
            var result = ModuleBusManager.IsRoutePossible(ModuleG, ModuleF);

            result.Should().BeFalse();
        }

        [Test]
        public void IsRoutePossible_WhenRoutePossible_ShouldReturnTrue()
        {
            var result = ModuleBusManager.IsRoutePossible(ModuleE, ModuleF);

            result.Should().BeTrue();
        }

        [Test]
        public void IsRoutePossible_WhenLongRoutePossible_ShouldReturnTrue()
        {
            var result = ModuleBusManager.IsRoutePossible(ModuleB, ModuleG);

            result.Should().BeTrue();
        }

        [Test]
        public void IsRoutePossible_WhenLongRoutePossible_ShouldReturnTrue_ShouldDisregardModuleState()
        {
            ModuleC.SimulateEmergencyOff();
            ModuleF.SimulateIsFull(0);
            ModuleF.SimulateIsFull(1);
            ModuleA.LimitItemCount = 1;
            ModuleA.Entities.PlatformItems.Add(new PlatformItem());
            var result = ModuleBusManager.IsRoutePossible(ModuleB, ModuleG);

            result.Should().BeTrue();
        }

        [Test]
        [Ignore("Needs investigation")]
        public void WhenReleaseRouteAll_ShouldNotRoute()
        {
            ModuleA.Entities.PlatformItems.Add(new PlatformItem());

            ModuleBusManager.ForcePath(ModuleA, ModuleB, 0, 0);
            ModuleBusManager.ReleaseForcePath(ModuleA, ModuleB, 0, 0);

            ModuleA.TestCurrentAllPortRoutings.Should().HaveCount(0, "Module must route nothing because it was released by ModuleBusManager");
        }

        [Test]
        public void GivenTargetIsFull_WhenRouteAll_ShouldNotRoute()
        {
            ModuleA.Entities.PlatformItems.Add(new PlatformItem());
            ModuleB.SimulateIsFull(0);

            ModuleBusManager.ForcePath(ModuleA, ModuleB, 0, 0);

            ModuleA.TestCurrentAllPortRoutings.Should().HaveCount(0, "ModuleA must not route items because target is full");
        }

        [Test]
        public void GivenRouteAll_WhenDestinationGetsFull_ShouldStopRouting()
        {
            var item = new PlatformItem();
            ModuleA.Entities.PlatformItems.Add(item);
            ModuleBusManager.ForcePath(ModuleA, ModuleB, 0, 0);

            ModuleB.SimulateIsFull(0);

            ModuleA.TestCurrentAllPortRoutings.Should().HaveCount(0, "ModuleA must not route items because target is full now");
        }

        [Test]
        public void GivenRouteAll_WhenDestinationStops_ShouldStopRouting()
        {
            var item = new PlatformItem();
            ModuleA.Entities.PlatformItems.Add(item);
            ModuleBusManager.ForcePath(ModuleA, ModuleB, 0, 0);

            ModuleB.Stop();

            ModuleA.TestCurrentAllPortRoutings.Should().HaveCount(0, "ModuleA must not route items because target is full now");
        }

        [Test]
        public void GivenTargetIsOff_WhenRouteAll_ShouldNotRoute()
        {
            ModuleA.Entities.PlatformItems.Add(new PlatformItem());
            ModuleB.Stop();

            ModuleBusManager.ForcePath(ModuleA, ModuleB, 0, 0);

            ModuleA.TestCurrentAllPortRoutings.Should().HaveCount(0, "ModuleA must not route items because target is in OFF state");
        }

        [Test]
        public void GivenRouteAll_WhenModuleStops_ShouldNotRoute()
        {
            ModuleA.Entities.PlatformItems.Add(new PlatformItem());
            ModuleBusManager.ForcePath(ModuleA, ModuleB, 0, 0);

            ModuleB.Stop();

            ModuleA.TestCurrentAllPortRoutings.Should().HaveCount(0, "ModuleA must not route items because target is in OFF state now");
        }

        [Test]
        [Ignore]
        public void GivenItemsInModuleA_WhenRouteAllToModuleB_ShouldRoute()
        {
            ModuleA.Entities.PlatformItems.Add(new PlatformItem());

            ModuleBusManager.ForcePath(ModuleA, ModuleB, 0, 0);

            ModuleA.TestCurrentAllPortRoutings.Should().Contain(0, "forcing to ModuleB was enabled and port 0 is connected to ModuleB");
        }

        [Test]
        public void WhenReleaseItem_ShouldBeInNextModule()
        {
            var item = new PlatformItem();
            ModuleA.Entities.PlatformItems.Add(item);

            ModuleA.SimulateItemReleased(item, 0);

            ModuleB.Entities.PlatformItems.Should().Contain(item, "item should be in ModuleB because it comes next");
            ModuleA.Entities.PlatformItems.Should().HaveCount(0, "item has been released");
        }

        [Test]
        public void WhenNewItemCreated_ShouldAddItemToModule()
        {
            ModuleA.SimulateNewItemCreated(new PlatformItem());

            ModuleA.Entities.PlatformItems.Count.Should().Be(1);
        }

        [Test]
        public void GivenItemInModuleA_WhenDetectedInModuleB_ShouldMoveItem()
        {
            var item = new PlatformItem();
            ModuleA.Entities.PlatformItems.Add(item);

            ModuleB.SimulateItemDetected(item);

            ModuleA.Entities.PlatformItems.Count.Should().Be(0, "item is now in ModuleB and cannot exist in ModuleA at the same time");
            ModuleB.Entities.PlatformItems.Contains(item).Should().BeTrue("item was detected in ModuleB");
        }

        [Test]
        public void GivenItemCreatedInModuleA_WhenDetectedInModuleB_ShouldMoveItem()
        {
            var item = new PlatformItem();
            ModuleA.SimulateNewItemCreated(item);

            ModuleB.SimulateItemDetected(item);

            ModuleA.Entities.PlatformItems.Count.Should().Be(0, "item is now in ModuleB and cannot exist in ModuleA at the same time");
            ModuleB.Entities.PlatformItems.Contains(item).Should().BeTrue("item was detected in ModuleB");
        }

        [Test]
        public void GivenItemInModule_WhenDetectedAgain_NothingHappens()
        {
            var item = new PlatformItem();
            ModuleA.Entities.PlatformItems.Add(item);
            ModuleA.MonitorEvents();

            // simulate a circulating buffer within ModuleA, where items detected multiple times
            ModuleA.SimulateItemDetected(item);
            ModuleA.SimulateItemDetected(item);

            ModuleA.Entities.PlatformItems.Count.Should().Be(1, "only one item is in ModuleA even if detected multiple times");
            ModuleA.ShouldNotRaise("CurrentItemCountChangedEvent");
        }

        // Route is:  ModuleA [1] --> ModuleC [3] --> ModuleD [4]
        [Test]
        public void GivenItemWithRoute_WhenCreatedAndDetected_ShouldRouteAndUpdateIndex()
        {
            var item = new PlatformItem();
            item.Route = new Route
            {
                RouteItems = new Collection<RouteItem>
                {
                    new RouteItem {ModuleType = 1},
                    new RouteItem {ModuleType = 3},
                    new RouteItem {ModuleType = 4}
                }
            };

            ModuleA.SimulateNewItemCreated(item);
            item.Route.CurrentIndex.Should().Be(0);
            ModuleA.TestCurrentItemRoutings.Should().ContainValue(1, "port 1 is the path to ModuleC which is next in the route");

            ModuleC.SimulateItemDetected(item);
            item.Route.CurrentIndex.Should().Be(1);
            ModuleA.TestCurrentItemRoutings.Should().HaveCount(0, "old routing task is fulfilled");
            ModuleC.TestCurrentItemRoutings.Should().ContainValue(0, "port 0 is the path to ModuleD");

            ModuleD.SimulateItemDetected(item);
            item.Route.CurrentIndex.Should().Be(2);
            ModuleC.TestCurrentItemRoutings.Should().HaveCount(0, "old routing task is fulfilled");
            ModuleD.TestCurrentItemRoutings.Should().HaveCount(0, "item has no more route items");
        }

        // Route is:  ModuleA [1] --> ModuleC [3] --> ModuleD [4]
        [Test]
        public void GivenItemWithRoute_WhenReleased_ShouldRouteAndUpdateIndex()
        {
            var item = new PlatformItem();
            item.Route = new Route
            {
                RouteItems = new Collection<RouteItem>
                {
                    new RouteItem {ModuleType = 1},
                    new RouteItem {ModuleType = 3},
                    new RouteItem {ModuleType = 4}
                }
            };
            ModuleA.Entities.PlatformItems.Add(item);

            ModuleA.SimulateItemReleased(item, 1);

            item.Route.CurrentIndex.Should().Be(1);
            ModuleC.Entities.PlatformItems.Should().Contain(item);
            ModuleC.TestCurrentItemRoutings.Should().HaveCount(1);
            ModuleA.Entities.PlatformItems.Should().HaveCount(0);

            ModuleC.SimulateItemReleased(item, 0);

            ModuleC.Entities.PlatformItems.Should().HaveCount(0);
            ModuleD.Entities.PlatformItems.Should().HaveCount(1);
        }

        [Test]
        public void GivenItemWithRoute_WhenDetectedInWrongModule_ShouldNotUpdateIndexAndRouteBack()
        {
            var item = new PlatformItem();
            item.Route = new Route
            {
                RouteItems = new Collection<RouteItem>
                {
                    new RouteItem {ModuleType = 1},
                    new RouteItem {ModuleType = 3},   // ModuleC
                }
            };

            ModuleA.SimulateNewItemCreated(item);
            ModuleB.SimulateItemDetected(item);    // item detected in wrong module!

            ModuleB.TestCurrentItemRoutings.Should().ContainValue(0, "item was detected in wrong module and should go back to ModuleA");
            item.Route.CurrentIndex.Should().Be(0, "item has still to visit ModuleC");
        }

        // Route is:  ModuleA [1] --> ModuleB [2] --> ModuleA [1]
        [Test]
        public void GivenItemWithRoute_WhenDetectedPingPong_ShouldRouteAndUpdateIndex()
        {
            var item = new PlatformItem();
            item.Route = new Route
            {
                RouteItems = new Collection<RouteItem>
                {
                    new RouteItem {ModuleType = 1},
                    new RouteItem {ModuleType = 2},
                    new RouteItem {ModuleType = 1}
                }
            };

            ModuleA.SimulateNewItemCreated(item);
            item.Route.CurrentIndex.Should().Be(0);
            ModuleA.TestCurrentItemRoutings.Should().ContainValue(0, "port 0 is the path to ModuleB which is next in the route");

            ModuleB.SimulateItemDetected(item);
            item.Route.CurrentIndex.Should().Be(1);
            ModuleA.TestCurrentItemRoutings.Should().HaveCount(0, "old routing task is fulfilled");
            ModuleB.TestCurrentItemRoutings.Should().ContainValue(0, "port 0 is the path to ModuleA");

            ModuleA.SimulateItemDetected(item);
            item.Route.CurrentIndex.Should().Be(2);
            ModuleB.TestCurrentItemRoutings.Should().HaveCount(0, "old routing task is fulfilled");
            ModuleA.TestCurrentItemRoutings.Should().HaveCount(0, "item has no more route items");
        }

        // ModuleA -> ModuleB(limit full)
        [Test]
        public void GivenItemWithRoute_WhenTargetPortFull_ShouldNotRouteSingleItem()
        {
            ModuleB.SimulateIsFull(0);
            var item = new PlatformItem();
            item.Route = new Route
            {
                RouteItems = new Collection<RouteItem>
                {
                    new RouteItem {ModuleType = 1},
                    new RouteItem {ModuleType = 2},
                }
            };

            ModuleA.SimulateNewItemCreated(item);

            ModuleA.TestCurrentItemRoutings.Should().HaveCount(0, "must not route because target port is full");

            ModuleA.SimulateItemDetected(item);

            ModuleA.TestCurrentItemRoutings.Should().HaveCount(0, "must not route because target port is still full");
        }

        // ModuleA -> ModuleB(limit full)
        [Test]
        public void GivenItemWithRoute_WhenTargetFull_ShouldNotRouteSingleItem()
        {
            ModuleB.LimitItemCount = 1;
            ModuleB.Entities.PlatformItems.Add(new PlatformItem { ItemId = 99 });
            var item = new PlatformItem { ItemId = 1 };
            item.Route = new Route
            {
                RouteItems = new Collection<RouteItem>
                {
                    new RouteItem {ModuleType = 1},
                    new RouteItem {ModuleType = 2},
                }
            };

            ModuleA.TestCurrentItemRoutings.Should().HaveCount(0, "must not route because target port is full");

            ModuleA.SimulateItemDetected(item);

            ModuleA.TestCurrentItemRoutings.Should().HaveCount(0, "must not route because target port is still full");
        }

        // ModuleA -> ModuleB(limit full)
        [Test]
        public void GivenModuleHasItemAndFull_WhenRouteAll_ShouldNotRouteAll()
        {
            ModuleB.LimitItemCount = 1;
            ModuleB.Entities.PlatformItems.Add(new PlatformItem());

            ModuleBusManager.ForcePath(ModuleA, ModuleB, 0, 0);

            ModuleA.TestCurrentAllPortRoutings.Should().HaveCount(0);
        }

        // ModuleA -> ModuleB(not full anymore)
        [Test]
        [Ignore]
        public void GivenModuleFullAndRouteAll_WhenReleaseAndNotFullAnymore_ShouldRouteAll()
        {
            var item = new PlatformItem();
            ModuleB.LimitItemCount = 1;
            ModuleB.Entities.PlatformItems.Add(item);
            ModuleBusManager.ForcePath(ModuleA, ModuleB, 0, 0);

            ModuleB.SimulateItemReleased(item, 0);

            ModuleA.TestCurrentAllPortRoutings.Should().HaveCount(1);
        }

        // ModuleA -> ModuleB(not full anymore because item gots detected in ModuleA)
        [Test]
        [Ignore]
        public void GivenModuleFullAndRouteAll_WhenNotFullAnymore_ShouldRouteAll()
        {
            var item = new PlatformItem();
            ModuleB.LimitItemCount = 1;
            ModuleB.Entities.PlatformItems.Add(item);
            ModuleBusManager.ForcePath(ModuleA, ModuleB, 0, 0);

            ModuleA.SimulateItemDetected(item);

            ModuleA.TestCurrentAllPortRoutings.Should().HaveCount(1);
        }

        // ModuleA -> ModuleB(gets full by item detected)
        [Test]
        public void GivenModuleEmptyAndRouteAll_WhenGetsFull_ShouldReleaseRouteAll()
        {
            ModuleB.LimitItemCount = 1;
            ModuleBusManager.ForcePath(ModuleA, ModuleB, 0, 0);

            ModuleB.SimulateItemDetected(new PlatformItem());

            ModuleA.TestCurrentAllPortRoutings.Should().HaveCount(0);
        }

        [Test]
        public void GivenItemAlreadyCreated_WhenDetectCreatedAgain_ShouldLogWarnAndRemoveOldItem()
        {
            var item = new PlatformItem { Id = 1, ItemId = 1 };
            ModuleA.SimulateNewItemCreated(item);

            ModuleB.SimulateNewItemCreated(new PlatformItem { ItemId = 1 });

            Logger.Verify(l => l.Warn(It.IsAny<string>()), Times.AtLeastOnce);
            ModuleA.Entities.PlatformItems.Should().HaveCount(0);
            ModuleB.Entities.PlatformItems.Should().HaveCount(1);
        }

        [Test]
        public void GivenItemAlreadyCreated_WhenDetectedAgain_ShouldMoveShirt()
        {
            var item = new PlatformItem { ItemId = 1 };
            ModuleA.SimulateNewItemCreated(item);

            ModuleB.SimulateItemDetected(new PlatformItem { ItemId = 1 });

            ModuleA.Entities.PlatformItems.Should().HaveCount(0);
            ModuleB.ContainsItem(item.ItemId).Should().BeTrue();
            ModuleB.Entities.PlatformItems.First().Should().BeSameAs(item);
        }

        [Test]
        public void GivenItemWithRouteAndAlreadyCreated_WhenDetected_ShouldUpdateRoute()
        {
            var item = new PlatformItem { ItemId = 1 };
            item.Route = CreateRoute();
            ModuleA.SimulateNewItemCreated(item);

            ModuleB.SimulateItemDetected(item);

            var route = ModuleB.Entities.PlatformItems.First().Route;
            route.Should().NotBeNull();
            route.CurrentIndex.Should().Be(1);
        }

        [Test]
        public void GivenItemWithRoute_GivenFull_WhenNotFullAnymore_ShouldRouteSingleItem()
        {
            var item = new PlatformItem { ItemId = 1 };
            item.Route = CreateRoute();
            ModuleB.SimulateIsFull(0);
            ModuleA.SimulateNewItemCreated(item);
            ModuleA.SimulateItemDetected(item);

            ModuleA.TestCurrentItemRoutings.Should().BeEmpty();
            ModuleB.SimulateIsNotFull(0);
            ModuleA.SimulateItemDetected(item);

            ModuleA.TestCurrentItemRoutings.Should().HaveCount(1);
        }

        [Test]
        public void GivenItemWithRoute_WhenIndirectWayIsThereForbidden_ShouldRoute()
        {
            var item = new PlatformItem();
            item.Route = new Route
            {
                RouteItems = new Collection<RouteItem>{
                    new RouteItem {ModuleType = 6}, // ModuleF
                    new RouteItem {ModuleType = 7}, // ModuleG
                }
            };

            ModuleG.SimulateIsFull(0); // ...but port 0 is full!

            ModuleF.SimulateNewItemCreated(item);

            ModuleF.TestCurrentItemRoutings.Should().ContainValue(1, "Port 0 of ModuleG is full but there is still another way");
        }

        [Test]
        [Ignore]
        public void WhenItemReachesLastRouteModule_ShouldBeFulfilled()
        {
            var item = new PlatformItem
            {
                Route = new Route
                {
                    RouteItems = new Collection<RouteItem>{
                        new RouteItem {ModuleType = 6}, // ModuleF
                        new RouteItem {ModuleType = 7}, // ModuleG
                    }
                }
            };
            JobManager.ResetCalls();

            ModuleF.SimulateNewItemCreated(item);
            ModuleG.SimulateItemDetected(item);
            ModuleG.SimulateItemDetected(item);
            ModuleG.SimulateItemReleased(item, 0);

            JobManager.Verify(j => j.FulfillJobItem(item));
        }

        private Route CreateRoute()
        {
            return new Route
            {
                RouteItems = new Collection<RouteItem>
                {
                    new RouteItem {ModuleType = ModuleA.ModuleTypeId},
                    new RouteItem {ModuleType = ModuleB.ModuleTypeId}
                }
            };
        }
    }
}
