using System;

namespace NCrunch.Framework
{
    /// <summary>
    /// Declares a test as belonging to a specific category.  This category will only be applied for NCrunch.
    /// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Assembly, AllowMultiple = true)]
	public class CategoryAttribute: Attribute
	{
		public CategoryAttribute(string category)
		{
			Category = category;
		}

		public string Category { get; private set; }
	}
}
