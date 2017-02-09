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
using System.IO;
using SystemWrapper.IO;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationLogix
{
    public class CsvLogixTagImporter : ITagImporter
    {
        /// <summary>
        /// IO abstraction for Unit testing.
        /// </summary>
        public static IStreamReaderWrap StreamReader
        {
            set; 
            get;
        }

        public void Initialize(string address, int port = 0)
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyCollection<Tag> ImportAllTags()
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyCollection<Tag> ImportTags(string startTag)
        {
            throw new System.NotImplementedException();
        }

        public ICollection<Tag> ImportTags(string path, int dummy)
        {
            var csvList = ParseCSV(path);

            csvList = RemoveQuoteSigns(csvList);

            var tags = CreateTags(csvList);

            return tags;
        }

        public ICollection<Tag> ImportTags(string startTag, string path, int port)
        {
            throw new System.NotImplementedException();
        }

        public ICollection<Tag> ImportTagsFromCache(string startTag, IEnumerable<Tag> tagList)
        {
            throw new System.NotImplementedException();
        }

        public Tag ImportTagRecursive(string scopedPath)
        {
            throw new System.NotImplementedException();
        }

        public ICollection<Tag> ImportTagsFromCache(string startTag, ICollection<Tag> tagList)
        {
            throw new System.NotImplementedException();
        }

        public Tag ImportTag(string name)
        {
            throw new System.NotImplementedException();
        }

        public Tag ImportTag(string name, int port)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
        }

        private IList<string[]> RemoveQuoteSigns(IList<string[]> csvList)
        {
            foreach (var row in csvList)
            {
                for (int i = 0; i < row.Length; i++)
                {
                    var value = row[i];

                    if (value.StartsWith("\""))
                        value = value.Remove(0, 1);

                    if (value.EndsWith("\""))
                        value = value.Remove(value.Length - 1, 1);

                    row[i] = value.Trim();
                }
            }

            return csvList;
        }

        private IList<string[]> ParseCSV(string path)
        {
            var parsedData = new List<string[]>();

            try
            {
                if (StreamReader == null)
                {
                    StreamReader = new StreamReaderWrap(path);
                }

                StreamReader.Initialize(path);

                using (var readFile = StreamReader)
                {
                    string line;

                    while ((line = readFile.ReadLine()) != null)
                    {
                        //string[] row = line.Split(',');

                        var row = new List<string>();

                        // split line into values but not withing quote signs
                        int startIndex = 0;
                        bool withinQuotes = false;
                        for (int i = 0; i < line.Length; i++)
                        {
                            char character = line[i];
                            
                            if (character.Equals('\"'))
                            {
                                withinQuotes = !withinQuotes;
                            }

                            // split
                            if (character.Equals(',') && !withinQuotes)
                            {
                                row.Add(line.Substring(startIndex, i - startIndex));
                                startIndex = i + 1;
                            }
                        }
                        
                        // add last entry
                        row.Add(line.Substring(startIndex, line.Length - startIndex));

                        parsedData.Add(row.ToArray());
                    }
                }
            }
            catch (IOException e)
            {
                throw new TagReaderException("Cant read file " + path, path, e);
            }

            return parsedData;
        }

        private IList<Tag> CreateTags(IList<string[]> csvList)
        {
            var tags = new List<Tag>();
            for (int i = FirstTagRow(csvList); i < csvList.Count; i++)
            {
                var csvRow = csvList[i];

                Tag tag;

                try
                {
                    tag = new Tag
                                  {
                                      Type = GetTagType(csvRow[0]),
                                      Scope = csvRow[1],
                                      Name = csvRow[2],
                                      Description = csvRow[3],
                                      DataType = csvRow[4],
                                      Specifier = csvRow.Length > 5 ? csvRow[5] : string.Empty,
                                      Attributes = csvRow.Length > 6 ? csvRow[6] : string.Empty
                                  };
                }
                catch (TagReaderException e)
                {
                    // ignore some rows we know
                    if (e.FailedText.Equals("TYPE"))
                        continue;

                    // but throw Exception for unknown rows (to keep this software up to date)
                    throw e;
                }

                if (tag.Type != TagType.Comment)
                    tags.Add(tag);
            }

            return tags;
        }

        private int FirstTagRow(IEnumerable<string[]> csvList)
        {
            // first line after tag headers: TYPE,SCOPE,NAME,DESCRIPTION,DATATYPE,SPECIFIER,ATTRIBUTES
            int row = 0;
            foreach (var csvRow in csvList)
            {
                ++row;
                if (csvRow.Length == 7 && 
                    csvRow[0].Equals("TYPE") &&
                    csvRow[1].Equals("SCOPE") &&
                    csvRow[2].Equals("NAME"))
                {
                    return row;
                }
            }

            throw new TagReaderException("Parse error: No tags found!", string.Empty);
        }

        private TagType GetTagType(string typeString)
        {
            switch (typeString)
            {
                case "TAG":
                    return TagType.Tag;
                case "ALIAS":
                    return TagType.Alias;
                case "RCOMMENT":
                    return TagType.Comment;
                default:
                    throw new TagReaderException("Parse error: Unknown tag type " + typeString, typeString);
            }
        }
    }
}
