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


namespace VP.FF.PT.Common.PlcCommunication
{
    /// <summary>
    /// Will parse a raw string for metadata syntax and assign metadata to a tag..
    /// </summary>
    public interface ITagMetaDataParser
    {
        /// <summary>
        /// Sets the meta data to a tag. If the raw string doesn't contain any metadata it will at least set the raw string to the comment property.
        /// </summary>
        /// <param name="rawString">The raw meta data string which can contain metadata syntax (with curly brackets).</param>
        /// <returns>Extracted tag metadata including the comment.s</returns>
        TagMetaData Parse(string rawString);
    }
}
