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
using System.Diagnostics;
using QuickGraph;

namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow
{
    [DebuggerDisplay("{Source.Name} -> {Target.Name}")]
    [Serializable]
    public class ModuleGraphEdge : Edge<IPlatformModule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleGraphEdge" /> class.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="sourcePort">The source port is the output index number of origin module.</param>
        /// <param name="targetPort">The target port is the input index number of target module.</param>
        public ModuleGraphEdge(string id, IPlatformModule source, IPlatformModule target, int sourcePort, int targetPort)
            : base(source, target)
        {
            Id = id;
            OriginPort = sourcePort;
            TargetPort = targetPort;
        }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the origin port which is the index for output conveyer from origin module.
        /// </summary>
        /// <value>
        /// The origin port.
        /// </value>
        public int OriginPort { get; set; }

        /// <summary>
        /// Gets or sets the target port which is the index for input conveyor of target module.
        /// </summary>
        /// <value>
        /// The target port.
        /// </value>
        public int TargetPort { get; set; }

        public bool IsForcingEnabled { get; set; }
    }
}
