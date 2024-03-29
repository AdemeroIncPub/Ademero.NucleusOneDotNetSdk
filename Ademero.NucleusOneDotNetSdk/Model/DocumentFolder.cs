﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Ademero.NucleusOneDotNetSdk.Model
{
    // Originally generated by nucleus_one_utilities.
    [Serializable]
    public class DocumentFolder : Common.Model.Entity<ApiModel.DocumentFolder>
    {
        private DocumentFolder(NucleusOneApp app) : base(app) { }

        public static DocumentFolder FromApiModel(
            ApiModel.DocumentFolder apiModel,
            NucleusOneApp app = null
        )
        {
            if (apiModel == null)
                return null;
            return new DocumentFolder(app)
            {
                Id = apiModel.Id,
                UniqueId = apiModel.UniqueId,
                ParentId = apiModel.ParentId,
                AncestorIds = apiModel.AncestorIds,
                OrganizationId = apiModel.OrganizationId,
                ProjectId = apiModel.ProjectId,
                ProjectName = apiModel.ProjectName,
                ProjectAccess = ProjectAccess.FromApiModel(apiModel.ProjectAccess),
                CreatedOn = apiModel.CreatedOn,
                CreatedByUserEmail = apiModel.CreatedByUserEmail,
                CreatedByUserName = apiModel.CreatedByUserName,
                CreatedByUserId = apiModel.CreatedByUserId,
                CreatedByWorkflow = apiModel.CreatedByWorkflow,
                ModifiedOn = apiModel.ModifiedOn,
                ModifiedByUserEmail = apiModel.ModifiedByUserEmail,
                ModifiedByUserName = apiModel.ModifiedByUserName,
                ModifiedByUserId = apiModel.ModifiedByUserId,
                Name = apiModel.Name,
                NameLower = apiModel.NameLower,
                Depth = apiModel.Depth,
                AncestorAssignmentUserEmails = apiModel.AncestorAssignmentUserEmails,
                AssignmentUserEmails = apiModel.AssignmentUserEmails,
                HexColor = apiModel.HexColor
            };
        }

        #region Properties

        public string Id { get; set; }
        public string UniqueId { get; set; }
        public string ParentId { get; set; }
        public List<string> AncestorIds { get; set; }
        public string OrganizationId { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public ProjectAccess ProjectAccess { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedByUserEmail { get; set; }
        public string CreatedByUserName { get; set; }
        public string CreatedByUserId { get; set; }
        public bool CreatedByWorkflow { get; set; }
        public string ModifiedOn { get; set; }
        public string ModifiedByUserEmail { get; set; }
        public string ModifiedByUserName { get; set; }
        public string ModifiedByUserId { get; set; }
        public string Name { get; set; }
        public string NameLower { get; set; }
        public int Depth { get; set; }
        public List<string> AncestorAssignmentUserEmails { get; set; }
        public List<string> AssignmentUserEmails { get; set; }
        public string HexColor { get; set; }

        #endregion

        public override ApiModel.DocumentFolder ToApiModel()
        {
            return new ApiModel.DocumentFolder()
            {
                Id = Id,
                UniqueId = UniqueId,
                ParentId = ParentId,
                AncestorIds = AncestorIds,
                OrganizationId = OrganizationId,
                ProjectId = ProjectId,
                ProjectName = ProjectName,
                ProjectAccess = ProjectAccess.ToApiModel(),
                CreatedOn = CreatedOn,
                CreatedByUserEmail = CreatedByUserEmail,
                CreatedByUserName = CreatedByUserName,
                CreatedByUserId = CreatedByUserId,
                CreatedByWorkflow = CreatedByWorkflow,
                ModifiedOn = ModifiedOn,
                ModifiedByUserEmail = ModifiedByUserEmail,
                ModifiedByUserName = ModifiedByUserName,
                ModifiedByUserId = ModifiedByUserId,
                Name = Name,
                NameLower = NameLower,
                Depth = Depth,
                AncestorAssignmentUserEmails = AncestorAssignmentUserEmails,
                AssignmentUserEmails = AssignmentUserEmails,
                HexColor = HexColor
            };
        }
    }

    [Serializable]
    public class DocumentFolderCollection
        : Common.Model.EntityCollection<DocumentFolder, ApiModel.DocumentFolderCollection>
    {
        public DocumentFolderCollection(
            DocumentFolder[] items,
            NucleusOneApp app = null
        ) : base(app, items) { }

        public static DocumentFolderCollection FromApiModel(
            ApiModel.DocumentFolderCollection apiModel,
            NucleusOneApp app = null
        )
        {
            if (apiModel == null)
                return null;
            return new DocumentFolderCollection(
                items: apiModel.DocumentFolders?.Select((x) => DocumentFolder.FromApiModel(x, app)).ToArray());
        }

        public override ApiModel.DocumentFolderCollection ToApiModel()
        {
            return new ApiModel.DocumentFolderCollection()
            {
                DocumentFolders = Items.Select((x) => x.ToApiModel()).ToArray()
            };
        }
    }
}
