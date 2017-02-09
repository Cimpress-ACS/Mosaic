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


using System.Collections.Generic;
using System.Linq;

namespace VP.FF.PT.Common.PlatformEssentials.Entities.DTOs
{
    /// <summary>
    /// Assembler for <see cref="Value"/> and <see cref="ValueDTO"/>.
    /// </summary>
    public static class ValueAssembler
    {
        /// <summary>
        /// Converts this instance of <see cref="ValueDTO"/> to an instance of <see cref="Value"/>.
        /// </summary>
        /// <param name="dto"><see cref="ValueDTO"/> to convert.</param>
        public static Value ToEntity(this ValueDTO dto)
        {
            if (dto == null) return null;

            var entity = new Value();

            entity.Id = dto.Id;
            entity.Parameter = dto.Parameter;
            entity.Key = dto.Key;

            return entity;
        }

        /// <summary>
        /// Converts this instance of <see cref="Value"/> to an instance of <see cref="ValueDTO"/>.
        /// </summary>
        /// <param name="entity"><see cref="Value"/> to convert.</param>
        public static ValueDTO ToDTO(this Value entity)
        {
            if (entity == null) return null;

            var dto = new ValueDTO();

            dto.Id = entity.Id;
            dto.Parameter = entity.Parameter;
            dto.Key = entity.Key;

            return dto;
        }

        /// <summary>
        /// Converts each instance of <see cref="ValueDTO"/> to an instance of <see cref="Value"/>.
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public static List<Value> ToEntities(this IEnumerable<ValueDTO> dtos)
        {
            if (dtos == null) return null;

            return dtos.Select(e => e.ToEntity()).ToList();
        }

        /// <summary>
        /// Converts each instance of <see cref="Value"/> to an instance of <see cref="ValueDTO"/>.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static List<ValueDTO> ToDTOs(this IEnumerable<Value> entities)
        {
            if (entities == null) return null;

            return entities.Select(e => e.ToDTO()).ToList();
        }
    }
}
