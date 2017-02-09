using System;

namespace NCrunch.Framework
{
    /// <summary>
    /// Surfaces several useful methods for gaining information about the NCrunch runtime environment.
    /// </summary>
	public static class NCrunchEnvironment
	{
        /// <summary>
        /// Returns TRUE if NCrunch is responsible for executing this code
        /// </summary>
		public static bool NCrunchIsResident()
		{
			return Environment.GetEnvironmentVariable("NCrunch") == "1";
		}

        /// <summary>
        /// Returns the full file path of the solution file open in the IDE, as it exists in the foreground solution.
        /// </summary>
		public static string GetOriginalSolutionPath()
		{
			return Environment.GetEnvironmentVariable("NCrunch.OriginalSolutionPath");
		}

        /// <summary>
        /// Returns the full file path of the test project file, as it exists in the foreground solution.
        /// </summary>
		public static string GetOriginalProjectPath()
		{
			return Environment.GetEnvironmentVariable("NCrunch.OriginalProjectPath");
		}

        /// <summary>
        /// Returns an array of assemblies that have been declared as 'implicitly referenced' by this runtime environment, as declared in NCrunch's 'Implicit project references' project-level configuration setting.
        /// </summary>
	    public static string[] GetImplicitlyReferencedAssemblyLocations()
	    {
	        var dependencies = Environment.GetEnvironmentVariable("NCrunch.ImplicitlyReferencedAssemblyLocations");
	        if (dependencies == null)
	            return null;

	        return dependencies.Split(';');
	    }

        /// <summary>
        /// Returns all assemblies detected by NCrunch as being required or referenced by this runtime environment.
        /// </summary>
	    public static string[] GetAllAssemblyLocations()
	    {
            var dependencies = Environment.GetEnvironmentVariable("NCrunch.AllAssemblyLocations");
            if (dependencies == null)
                return null;

            return dependencies.Split(';');
        }
	}
}
