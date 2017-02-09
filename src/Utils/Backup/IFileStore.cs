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


namespace VP.FF.PT.Common.Utils.Backup
{
    public interface IFileStore
    {
        /// <summary>
        /// Refresh the Vcs Repository status to the most up to date state
        /// </summary>
        void Refresh();

        /// <summary>
        /// LoadFile retrieves configuration file for a module from the VCS
        /// </summary>
        /// <param name="identifier">relative path (including filename) identifier</param>
        /// <returns>filepath where the file can be retrieved</returns>
        string LoadFile(string identifier);

        /// <summary>
        /// SaveFile take a file from a path and persist it into VCS>
        /// </summary>
        /// <param name="identifier">module identifier</param>
        /// <param name="filepath">path to the file to be persistedd</param>
        void SaveFile(string identifier, string filepath);
    }
}
