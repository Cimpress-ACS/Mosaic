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
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcEssentials.ControllerImporting;

namespace VP.FF.PT.Common.PlcEssentials.Impl
{
    public class RootController : Controller
    {
        private readonly ICommand _autoModeCommand;

        public RootController(
            ITagController tagController,
            IControllerTag controllerTag)
            : base(tagController, controllerTag)
        {
            _autoModeCommand = new SetAll2AutoModeCommand(this);
        }

        public override IEnumerable<ICommand> Commands
        {
            get { return base.Commands.Concat(new[] { _autoModeCommand }); }
        }

        public class SetAll2AutoModeCommand : ICommand
        {
            public const string COMMAND_NAME = "SetAll2AutoMode";
            private readonly RootController _controller;

            public SetAll2AutoModeCommand(RootController controller)
            {
                _controller = controller;
            }

            public IEnumerable<Tag> CmdValues
            {
                get { yield break; }
            }

            public int CommandId
            {
                get { return -1; }
            }

            public void Fire()
            {
                if (_controller == null)
                    return;

                _controller.SetAllController2AutoMode();
            }

            public string Name
            {
                get { return COMMAND_NAME; }
            }

            public string Comment { get; set; }

            public IController Controller
            {
                get { return _controller; }
            }

            public bool Equals(Command other)
            {
                if (ReferenceEquals(null, other)) 
                    return false;

                return CommandId == other.CommandId;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Command)obj);
            }

            public int CompareTo(Command other)
            {
                return CommandId - other.CommandId;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = CommandId;
                    hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                    return hashCode;
                }
            }

            public bool IsAvailable
            {
                get { return true; }
            }


            public Tag IsAvailableTag
            {
                get 
                { 
                    return null; 
                }
            }

            public Tag ModeTag
            {
                get { return null; }
            }

            public Tag CommandTag
            {
                get { return null; }
            }

            public bool HasCommandAndModeTag
            {
                get { return false; }
            }
        }
    }
}
