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

namespace VP.FF.PT.Common.PlcCommunication
{
    public static class BeckhoffTagExtensions
    {
        /// <summary>
        /// The full name also contains the scope (when available) and is unique.
        /// Therefore is can be used as a primary key.
        /// </summary>
        public static string FullName(this Tag tag)
        {
            string name;

            if (!string.IsNullOrEmpty(tag.Scope))
                name = tag.Scope + "." + tag.Name;
            else
                name = tag.Name;

            return name;
        }

        public static string GetPointerlessFullName(this Tag tag)
        {
            string fullName = tag.FullName();

            int idxPointerSign = -1;
            int idxPointBefore = -1;

            var determineIndex = new Func<string, bool>(value =>
                {
                    idxPointerSign = value.IndexOf('^');

                    if (idxPointerSign != -1)
                        idxPointBefore = value.LastIndexOf(".", idxPointerSign);
                    else
                        idxPointBefore = -1;

                    return idxPointBefore != -1 && idxPointerSign != -1;
                });

            while (determineIndex(fullName))
            {
                var pointerName = fullName.Substring(idxPointBefore, idxPointerSign - idxPointBefore + 1);
                fullName = fullName.Replace(pointerName, string.Empty);
            }
            
            return fullName;
        }
    }
}
