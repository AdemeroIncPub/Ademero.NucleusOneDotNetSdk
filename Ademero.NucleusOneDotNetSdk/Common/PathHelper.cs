﻿namespace Ademero.NucleusOneDotNetSdk.Common
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

        public static string GetProjectsPath()
        {
            return "/projects";
        }

        public static string GetProjectPath(string projectId)
        {
            return $"{GetProjectsPath()}/{projectId}";
        }

        public static string GetHomePath()
        {
            return "/home";
        }

        public static string GetWorkspacePath()
        {
            return "/workspace";
        }

        public static string GetWorkspaceDocumentFoldersPath(string projectId)
        {
            return $"{GetWorkspacePath()}/documents/projects/{projectId}/documentFolders";
        }
    }
}
