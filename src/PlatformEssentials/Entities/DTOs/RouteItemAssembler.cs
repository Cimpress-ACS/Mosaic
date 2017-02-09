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
    /// Assembler for <see cref="RouteItem"/> and <see cref="RouteItemDTO"/>.
    /// </summary>
    public static class RouteItemAssembler
    {
        /// <summary>
        /// Converts this instance of <see cref="RouteItemDTO"/> to an instance of <see cref="RouteItem"/>.
        /// </summary>
        /// <param name="dto"><see cref="RouteItemDTO"/> to convert.</param>
        public static RouteItem ToEntity(this RouteItemDTO dto)
        {
            if (dto == null) return null;

            var entity = new RouteItem();

            entity.Id = dto.Id;
            entity.ModuleType = dto.ModuleType;
            entity.OverrideModuleType = dto.OverrideModuleType;
            entity.Index = dto.Index;
            entity.ForceModuleInstance = dto.ForceModuleInstance;
            entity.ForbiddenModuleType = dto.ForbiddenModuleType;

            return entity;
        }

        /// <summary>
        /// Converts this instance of <see cref="RouteItem"/> to an instance of <see cref="RouteItemDTO"/>.
        /// </summary>
        /// <param name="entity"><see cref="RouteItem"/> to convert.</param>
        public static RouteItemDTO ToDTO(this RouteItem entity)
        {
            if (entity == null) return null;

            var dto = new RouteItemDTO();

            dto.Id = entity.Id;
            dto.ModuleType = entity.ModuleType;
            dto.OverrideModuleType = entity.OverrideModuleType;
            dto.Index = entity.Index;
            dto.ForceModuleInstance = entity.ForceModuleInstance;
            dto.ForbiddenModuleType = entity.ForbiddenModuleType;

            return dto;
        }

        /// <summary>
        /// Converts each instance of <see cref="RouteItemDTO"/> to an instance of <see cref="RouteItem"/>.
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public static List<RouteItem> ToEntities(this IEnumerable<RouteItemDTO> dtos)
        {
            if (dtos == null) return null;

            return dtos.Select(e => e.ToEntity()).ToList();
        }

        /// <summary>
        /// Converts each instance of <see cref="RouteItem"/> to an instance of <see cref="RouteItemDTO"/>.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static List<RouteItemDTO> ToDTOs(this IEnumerable<RouteItem> entities)
        {
            if (entities == null) return null;

            return entities.Select(e => e.ToDTO()).ToList();
        }
    }
}
