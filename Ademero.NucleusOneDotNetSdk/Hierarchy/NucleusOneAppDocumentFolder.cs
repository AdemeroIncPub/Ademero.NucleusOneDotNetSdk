using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Ademero.NucleusOneDotNetSdk.Hierarchy
{
    public class NucleusOneAppDocumentFolder : NucleusOneAppDependent
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
        /// The document folder's ID.
        /// </summary>
        public string Id
        {
            [DebuggerStepThrough]
            get;
        }

        /// <summary>
        /// Creates an instance of the <see cref="NucleusOneAppDocumentFolder"/> class.
        /// </summary>
        /// <param name="project">The project to perform operations on.</param>
        /// <param name="id">The project's ID.</param>
        public NucleusOneAppDocumentFolder(NucleusOneAppProject project, string id) : base(project.App)
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
    }
}
