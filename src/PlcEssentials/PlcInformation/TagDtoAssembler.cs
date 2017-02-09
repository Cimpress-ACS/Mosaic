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
using System.Linq;
using VP.FF.PT.Common.Infrastructure.Assembling;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;
using VP.FF.PT.Common.PlcEssentials.PlcInformation.DTOs;

namespace VP.FF.PT.Common.PlcEssentials.PlcInformation
{
    /// <summary>
    /// Assembles <see cref="TagDTO"/> instances out of <see cref="Tag"/> instances.
    /// </summary>
    public class TagDtoAssembler : IAssemble<Tag, TagDTO>
    {
        private const string ArrayTypeIndicator = "ARRAY";
        private const string EnumTypeIndicator = "E_";
        private const string StringTypeIndicator = "STRING";

        private readonly IAssemble<IEnumerable<Tag>, IEnumerable<TagDTO>> _tagsAssembler;

        /// <summary>
        /// Initializes a new <see cref="TagDtoAssembler"/> instance using the default implementations
        /// of its dependencies.
        /// </summary>
        public TagDtoAssembler()
        {
            _tagsAssembler = new NullFilteringEnumerableAssembler<Tag, TagDTO>(this);
        }

        /// <summary>
        /// Initializes a new <see cref="TagDtoAssembler"/> instance.
        /// </summary>
        /// <param name="tagsAssembler">The enumerable assembler used to assemble the children of a tag.</param>
        public TagDtoAssembler(IAssemble<IEnumerable<Tag>, IEnumerable<TagDTO>> tagsAssembler)
        {
            _tagsAssembler = tagsAssembler;
        }

        /// <summary>
        /// Assembles a new <see cref="TagDTO"/> instance out of the specified <paramref name="fromItem"/>.
        /// </summary>
        /// <param name="fromItem">The original object.</param>
        /// <param name="assembleParameters">The method does not consider any parameters.</param>
        /// <returns>The resulting object.</returns>
        public TagDTO Assemble(Tag fromItem, dynamic assembleParameters = null)
        {
            if (fromItem == null)
                return null;

            string value = string.Empty;

            if (fromItem.Value != null)
                value = fromItem.Value.ToString();

            var dto = new TagDTO
            {
                Children = _tagsAssembler.Assemble(fromItem.Childs).ToList(),
                Comment = fromItem.MetaData.Comment,
                Key = fromItem.Name,
                Name = fromItem.NestedName,
                Type = GetDotNetType(fromItem),
                Unit = fromItem.MetaData.UnitForUser,
                Value = value
            };

            dto.EnumerationMembers = new List<EnumerationMemberDto>();
            if (fromItem.MetaData.EnumerationMembers != null)
            {
                foreach (var enumerationMember in fromItem.MetaData.EnumerationMembers)
                {
                    dto.EnumerationMembers.Add(new EnumerationMemberDto
                    {
                        Comment = enumerationMember.Comment,
                        Value = enumerationMember.Value
                    });
                }
            }

            return dto;
        }

        private static string GetDotNetType(Tag tag)
        {
            if (tag == null)
                return string.Empty;
            if (string.IsNullOrEmpty(tag.DataType))
                return string.Empty;
            if (tag.DataType.StartsWith(ArrayTypeIndicator))
                return typeof(Array).FullName;
            if (tag.DataType.Split('.').Last().StartsWith(EnumTypeIndicator))
                return typeof(short).FullName;
            if (tag.DataType.StartsWith(StringTypeIndicator))
                return typeof (string).FullName;
            Type dotNetType;
            if (IEC61131_3_DataTypes.NetDataTypes.TryGetValue(tag.DataType, out dotNetType))
                return dotNetType.FullName;
            return null;
        }
    }
}
