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
using System.Text.RegularExpressions;

namespace VP.FF.PT.Common.PlcCommunication
{
    public class TagMetaDataParser : ITagMetaDataParser
    {
        private static readonly Regex PointerRegex = new Regex(@"\{readptr\}");

        // e.g. {enum 0=Start;1=Pause;2=Stop}, multiline and whitespaces are supported
        private static readonly Regex EnumRegex = new Regex(@"\{enum[ \n\r\t]+([ \n\r\t]*(-?\d+)[ \n\r\t]*=[ \n\r\t]*([\w ]+)[ \n\r\t]*[;\}])+", RegexOptions.Multiline | RegexOptions.Compiled);

        private static readonly List<Regex> MetaDataKeywords = new List<Regex>
        {
            PointerRegex,
            EnumRegex
        };

        public TagMetaData Parse(string rawString)
        {
            if (rawString.IsNullOrEmpty())
            {
                return new TagMetaData();
            }

            var ret = new TagMetaData
            {
                Comment = GetComment(rawString),
                ReadPointer = TryParsePointer(rawString),
                EnumerationMembers = TryParseEnum(rawString)
            };

            if (!ret.EnumerationMembers.IsNullOrEmpty())
            {
                ret.Minimum = ret.EnumerationMembers.Min(m => m.Value);
                ret.Maximum = ret.EnumerationMembers.Max(m => m.Value);
            }

            return ret;
        }

        private static bool TryParsePointer(string rawString)
        {
            Match match = PointerRegex.Match(rawString);
            return match.Success;
        }

        private static IList<EnumerationMember> TryParseEnum(string rawString)
        {
            Match match = EnumRegex.Match(rawString);

            // must have 4 groups: 1st group is whole string e.g. {enum 1=COMMENT;2=COMMENT2}, 2nd group is 2=COMMENT2}, 3rd group is 1 and 4th group is COMMENT
            if (match.Groups.Count != 4)
            {
                return null;
            }

            var ret = new List<EnumerationMember>();

            for (int i = 0; i < match.Groups[2].Captures.Count; i++)
            {
                ret.Add(new EnumerationMember
                {
                    Value = short.Parse(match.Groups[2].Captures[i].Value), // will not throw because Regex captures only numerics here
                    Comment = match.Groups[3].Captures[i].Value.Trim()
                });
            }

            return ret;
        }

        private static string GetComment(string rawString)
        {
            foreach (var metaDataKeyword in MetaDataKeywords)
            {
                var match = metaDataKeyword.Match(rawString);

                if (match.Success)
                {
                    rawString = rawString.Replace(match.Groups[0].Value, string.Empty);
                }
            }

            return rawString.Trim();
        }
    }
}
