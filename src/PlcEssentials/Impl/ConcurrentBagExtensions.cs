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


using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VP.FF.PT.Common.PlcEssentials.Impl
{
    public static class ConcurrentBagExtensions
    {
        public static void TryRemoveAll<T>(this ConcurrentBag<T> bag)
        {
            while (!bag.IsEmpty)
            {
                T item;
                bag.TryTake(out item);
            }
        }

        public static IEnumerable<T> TryTakeAll<T>(this ConcurrentBag<T> bag)
        {
            var items = new List<T>();
            while (!bag.IsEmpty)
            {
                T item;
                bag.TryTake(out item);
                items.Add(item);
            }
            return items;
        }
    }
}
