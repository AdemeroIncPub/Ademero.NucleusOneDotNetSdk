using Ademero.NucleusOneDotNetSdk.Common.Strings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Ademero.NucleusOneDotNetSdk.Hierarchy
{
    public class NucleusOneAppField : NucleusOneAppDependent
    {
        /// <summary>
        /// The project to perform operations on.
        /// </summary>
        public NucleusOneAppProject Project
        {
            [DebuggerStepThrough]
            get;
        }

        /// <summary>
        /// The field's ID.
        /// </summary>
        public string Id
        {
            [DebuggerStepThrough]
            get;
        }

        /// <summary>
        /// Creates an instance of the <see cref="NucleusOneAppField"/> class.
        /// </summary>
        /// <param name="project">The project to perform operations on.</param>
        /// <param name="id">The project's ID.</param>
        public NucleusOneAppField(NucleusOneAppProject project, string id) : base(project.App)
        {
            Project = project;
            Id = id;

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Value cannot be blank.", nameof(id));
            }
        }

        /// <summary>
        /// Uploads a new document into this document folder.
        /// </summary>
        /// <param name="userEmail">The email address of the user by whom the document will be uploaded.</param>
        /// <param name="fileName">The file name to use when uploading the file.</param>
        /// <param name="contentType">The MIME type of the file.</param>
        /// <param name="file">The file to upload.</param>
        /// <returns>A task representing the asynchronous upload operation.</returns>
        public async Task UploadDocument(string userEmail, string fileName, string contentType, byte[] file,
            Dictionary<string, List<string>> fieldIDsAndValues = null)
        {
            await Project.UploadDocument(userEmail, fileName, contentType, file, Id, fieldIDsAndValues);
        }

        /// <summary>
        /// Adds list items to a field's selection list.
        /// </summary>
        /// <param name="items">The list items to add.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task AddListItems(Model.FieldListItemCollection items)
        {
            var qp = StandardQueryParams.Get();

            await Common.Util.DefineN1AppInScope(App, async () =>
            {
                await Common.Model.ListItems.AddListItems(
                  ApiPaths.OrganizationsProjectsFieldsFieldListItemsFormat
                    .ReplaceOrgIdAndProjectIdPlaceholdersUsingProject(Project)
                    .ReplaceFieldIdPlaceholder(Id),
                  items,
                  qp
                );
            });
        }

    }
}
