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

namespace VP.FF.PT.Common.PlatformEssentials
{
    public class AcceptResult : IEquatable<AcceptResult>
    {
        private readonly bool _accepted;
        private readonly string _reason;

        public bool IsAccepted
        {
            get { return _accepted; }
        }

        public string Reason
        {
            get { return _reason; }
        }

        private AcceptResult(bool accepted, string reason)
        {
            _accepted = accepted;
            _reason = reason;
        }

        public static AcceptResult Accepted()
        {
            return new AcceptResult(true, string.Empty);
        }

        public static AcceptResult Rejected(string reason)
        {
            return new AcceptResult(false, reason);
        }

        public override bool Equals(object other)
        {
            return Equals(other as AcceptResult);
        }

        public override int GetHashCode()
        {
            return new { _accepted, _reason }.GetHashCode();
        }

        public bool Equals(AcceptResult other)
        {
            return other != null && _accepted == other._accepted && string.Equals(_reason, other._reason);
        }
    }
}
