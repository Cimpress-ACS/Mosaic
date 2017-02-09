using System;

namespace NCrunch.Framework
{
    /// <summary>
    /// Tests implementing this attribute will be cloned by NCrunch, with each clone marked as requiring one of the attribute's parameters as a 'Capability'.
    /// You can use this attribute to spread the execution of a test over multiple nodes when using NCrunch with a grid.
    /// </summary>
    public class DistributeByCapabilitiesAttribute: Attribute
    {
        private string[] _capabilities;

        public DistributeByCapabilitiesAttribute(params string[] capabilities)
        {
            _capabilities = capabilities;
        }

        public string[] Capabilities
        {
            get { return _capabilities; }
        }
    }
}
