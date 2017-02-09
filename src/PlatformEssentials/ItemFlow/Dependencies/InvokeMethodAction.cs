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


using System.Reflection;

namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow.Dependencies
{
    public class InvokeMethodAction : IAction
    {
        private readonly MethodInfo _methodInfo;
        private readonly object _targetObjectInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeMethodAction"/> class.
        /// </summary>
        /// <param name="targetObjectInstance">The target object instance.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <exception cref="DependencyException">If the object or method does not exist.</exception>
        public InvokeMethodAction(object targetObjectInstance, string methodName)
        {
            if (targetObjectInstance == null)
                throw new DependencyException(
                    string.Format("the target object for method action \"{0}\" must not be null", methodName));

            _targetObjectInstance = targetObjectInstance;
            _methodInfo = targetObjectInstance.GetType()
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

            if (_methodInfo == null)
                throw new DependencyException(string.Format(
                    "the method \"{0}\" does not exist in object {1}", _methodInfo, _targetObjectInstance.GetType().FullName));
        }

        public void Execute()
        {
            _methodInfo.Invoke(_targetObjectInstance, null);
        }

        public override string ToString()
        {
            return string.Format("{0} ->Method: {1}", base.ToString(), _methodInfo.Name);
        }
    }
}
