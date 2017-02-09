using System;

namespace NCrunch.Framework
{
	public abstract class ResourceUsageAttribute: Attribute
	{
		private readonly string[] _resourceNames;

		protected ResourceUsageAttribute(params string[] resourceName)
		{
			_resourceNames = resourceName;
		}

        protected ResourceUsageAttribute(string resourceName)
        {
            _resourceNames = new [] {resourceName};
        }

        public string[] ResourceNames
		{
			get { return _resourceNames; }
		}
	}
}
