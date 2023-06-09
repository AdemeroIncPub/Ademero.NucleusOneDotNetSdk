namespace Ademero.NucleusOneDotNetSdk.Common.Strings
{
    public static class StringExtensions
    {
        /// <summary>
        /// If the <see cref="string"/>'s lower-case value equals "true", true is returned; otherwise, false.
        /// </summary>
        public static bool ParseBool(this string str)
        {
            return str.ToLowerInvariant() == "true";
        }

        ///// <summary>
        ///// Replaces the placeholder value with an actual value.
        ///// </summary>
        ///// <param name="project">The project that the replacement values should be sourced from.</param>
        //public static string ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(this string str, NucleusOneAppProject project)
        //{
        //    return str.ReplaceOrgIdPlaceholder(project.organization.id).ReplaceProjectIdPlaceholder(project.id);
        //}

        /// <summary>
        /// Replaces the placeholder value with an actual value.
        /// </summary>
        /// <param name="organizationId">The organization ID to replace the placeholder with.</param>
        /// <param name="projectId">The project ID to replace the placeholder with.</param>
        public static string ReplaceOrgIdAndProjectIdPlaceholders(this string str, string organizationId, string projectId)
        {
            return str.ReplaceOrgIdPlaceholder(organizationId).ReplaceProjectIdPlaceholder(projectId);
        }

        /// <summary>
        /// Replaces the placeholder value with an actual value.
        /// </summary>
        /// <param name="organizationId">The organization ID to replace the placeholder with.</param>
        public static string ReplaceOrgIdPlaceholder(this string str, string organizationId)
        {
            return str.Replace("<organizationId>", organizationId);
        }

        /// <summary>
        /// Replaces the placeholder value with an actual value.
        /// </summary>
        /// <param name="projectId">The project ID to replace the placeholder with.</param>
        public static string ReplaceProjectIdPlaceholder(this string str, string projectId)
        {
            return str.Replace("<projectId>", projectId);
        }

        /// <summary>
        /// Replaces the placeholder value with an actual value.
        /// </summary>
        /// <param name="documentId">The document ID to replace the placeholder with.</param>
        public static string ReplaceDocumentIdPlaceholder(this string str, string documentId)
        {
            return str.Replace("<documentId>", documentId);
        }

        /// <summary>
        /// Replaces the placeholder value with an actual value.
        /// </summary>
        /// <param name="documentFolderId">The document folder ID to replace the placeholder with.</param>
        public static string ReplaceDocumentFolderIdPlaceholder(this string str, string documentFolderId)
        {
            return str.Replace("<documentFolderId>", documentFolderId);
        }

        /// <summary>
        /// Replaces the placeholder value with an actual value.
        /// </summary>
        /// <param name="signatureFormId">The signature form ID to replace the placeholder with.</param>
        public static string ReplaceDocumentSignatureFormIdPlaceholder(this string str, string signatureFormId)
        {
            return str.Replace("<documentSignatureFormId>", signatureFormId);
        }

        /// <summary>
        /// Replaces the placeholder value with an actual value.
        /// </summary>
        /// <param name="signatureFormFieldId">The signature form field ID to replace the placeholder with.</param>
        public static string ReplaceDocumentSignatureFormFieldIdPlaceholder(this string str, string signatureFormFieldId)
        {
            return str.Replace("<documentSignatureFormFieldId>", signatureFormFieldId);
        }

        /// <summary>
        /// Replaces the placeholder value with an actual value.
        /// </summary>
        /// <param name="signatureSessionId">The signature session ID to replace the placeholder with.</param>
        public static string ReplaceDocumentSignatureSessionIdPlaceholder(this string str, string signatureSessionId)
        {
            return str.Replace("<documentSignatureSessionId>", signatureSessionId);
        }

        /// <summary>
        /// Replaces the placeholder value with an actual value.
        /// </summary>
        /// <param name="signatureSessionRecipientId">The signature session recipient ID to replace the placeholder with.</param>
        public static string ReplaceDocumentSignatureSessionRecipientIdPlaceholder(this string str, string signatureSessionRecipientId)
        {
            return str.Replace("<documentSignatureSessionRecipientId>", signatureSessionRecipientId);
        }

        /// <summary>
        /// Replaces the placeholder value with an actual value.
        /// </summary>
        /// <param name="templateId">The template ID to replace the placeholder with.</param>
        public static string ReplaceSignatureFormTemplateIdPlaceholder(this string str, string templateId)
        {
            return str.Replace("<signatureFormTemplateId>", templateId);
        }

        /// <summary>
        /// Replaces the placeholder value with an actual value.
        /// </summary>
        /// <param name="templateId">The template ID to replace the placeholder with.</param>
        public static string ReplaceFormTemplateIdPlaceholder(this string str, string templateId)
        {
            return str.Replace("<formTemplateId>", templateId);
        }

        /// <summary>
        /// Replaces the placeholder value with an actual value.
        /// </summary>
        /// <param name="fieldId">The field ID to replace the placeholder with.</param>
        public static string ReplaceFormTemplateFieldIdPlaceholder(this string str, string fieldId)
        {
            return str.Replace("<formTemplateFieldId>", fieldId);
        }

        /// <summary>
        /// Replaces the placeholder value with an actual value.
        /// </summary>
        /// <param name="taskId">The task ID to replace the placeholder with.</param>
        public static string ReplaceTaskIdPlaceholder(this string str, string taskId)
        {
            return str.Replace("<taskId>", taskId);
        }
    }
}
