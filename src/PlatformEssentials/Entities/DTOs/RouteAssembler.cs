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
    /// Assembler for <see cref="Route"/> and <see cref="RouteDTO"/>.
    /// </summary>
    public static class RouteAssembler
    {
        /// <summary>
        /// Converts this instance of <see cref="RouteDTO"/> to an instance of <see cref="Route"/>.
        /// </summary>
        /// <param name="dto"><see cref="RouteDTO"/> to convert.</param>
        public static Route ToEntity(this RouteDTO dto)
        {
            if (dto == null) return null;

            var entity = new Route();

            entity.Id = dto.Id;
            entity.CurrentIndex = dto.CurrentIndex;

            entity.RouteItems = dto.RouteItems.ToEntities();

            return entity;
        }

        /// <summary>
        /// Converts this instance of <see cref="Route"/> to an instance of <see cref="RouteDTO"/>.
        /// </summary>
        /// <param name="entity"><see cref="Route"/> to convert.</param>
        public static RouteDTO ToDTO(this Route entity)
        {
            if (entity == null) return null;

            var dto = new RouteDTO();

            dto.Id = entity.Id;
            dto.CurrentIndex = entity.CurrentIndex;

            dto.RouteItems = entity.RouteItems.ToDTOs();

            return dto;
        }

        /// <summary>
        /// Converts each instance of <see cref="RouteDTO"/> to an instance of <see cref="Route"/>.
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        public static List<Route> ToEntities(this IEnumerable<RouteDTO> dtos)
        {
            if (dtos == null) return null;

            return dtos.Select(e => e.ToEntity()).ToList();
        }

        /// <summary>
        /// Converts each instance of <see cref="Route"/> to an instance of <see cref="RouteDTO"/>.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static List<RouteDTO> ToDTOs(this IEnumerable<Route> entities)
        {
            if (entities == null) return null;

            return entities.Select(e => e.ToDTO()).ToList();
        }
    }
}
