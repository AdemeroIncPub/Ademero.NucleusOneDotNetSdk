namespace NucleusOneDotNetSdk.Common
{
    /// <summary>
    /// This class is directly translated from the Nucleus One client's project.
    /// </summary>
    public static class PathHelper
    {
        public static string GetOrganizationLink(string organizationId, string path)
        {
            if (path[0] != '/')
            {
                path = '/' + path;
            }
            return $"/organizations/{organizationId}/link{path}";
        }

        public static string GetHomePath()
        {
            return "/home";
        }
    }
}
