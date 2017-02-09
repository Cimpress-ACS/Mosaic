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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VP.FF.PT.Common.PlatformEssentials.Entities
{
    [Serializable]
    [DebuggerDisplay("Count: {RouteItems.Count}  CurrentIndex: {CurrentIndex}")]

    public class Route
    {
        private IList<RouteItem> _cachedOrderedList = new List<RouteItem>();

        public Route()
        {
            RouteItems = new HashSet<RouteItem>();
        }

        public long Id { get; set; }
        public int CurrentIndex { get; set; }
    
        public virtual ICollection<RouteItem> RouteItems { get; set; }

        public IList<RouteItem> GetOrderedList()
        {
            if (_cachedOrderedList.Count == 0)
            {
                _cachedOrderedList = (from l in RouteItems
                                      orderby l.Index ascending
                                      select l).ToList();
            }

            return _cachedOrderedList;
        }

        public bool ContainsRouteItem(int moduleType)
        {
            return RouteItems.Any(r => r.ModuleType == moduleType);
        }
    }
}
