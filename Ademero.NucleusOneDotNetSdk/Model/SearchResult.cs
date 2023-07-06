﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ademero.NucleusOneDotNetSdk.Model
{
    // Originally generated by nucleus_one_utilities
    [Serializable]
    public class SearchResult : Common.Model.Entity<ApiModel.SearchResult>
    {
        private SearchResult(NucleusOneApp app) : base(app) { }

        public static SearchResult FromApiModel(
            ApiModel.SearchResult apiModel,
            NucleusOneApp app = null
        )
        {
            return new SearchResult(app)
            {
                Id = apiModel.Id,
                ContentType = apiModel.ContentType,
                AncestorIds = apiModel.AncestorIds,
                OrganizationId = apiModel.OrganizationId,
                ProjectId = apiModel.ProjectId,
                ProjectName = apiModel.ProjectName,
                ProjectAccessType = apiModel.ProjectAccessType,
                ItemId = apiModel.ItemId,
                ItemType = apiModel.ItemType,
                UniqueId = apiModel.UniqueId,
                Name = apiModel.Name,
                CreatedOn = apiModel.CreatedOn,
                DueOn = apiModel.DueOn,
                Priority = apiModel.Priority ?? 0,
                TaskMilestoneName = apiModel.TaskMilestoneName,
                TaskStateName = apiModel.TaskStateName,
                TaskDurationMultiplier = apiModel.TaskDurationMultiplier ?? 0.0,
                TaskDurationInterval = apiModel.TaskDurationInterval,
                Tags = apiModel.Tags,
                CompletedOn = apiModel.CompletedOn,
                PurgeDate = apiModel.PurgeDate,
                PageCount = apiModel.PageCount ?? 0,
                FileSize = apiModel.FileSize ?? 0,
                ThumbnailUrl = apiModel.ThumbnailUrl,
                IsSigned = apiModel.IsSigned ?? false,
                AssignmentUserEmail = apiModel.AssignmentUserEmail,
                AssignmentUserEmails = apiModel.AssignmentUserEmails,
                AssignmentUserName = apiModel.AssignmentUserName,
                DocumentId = apiModel.DocumentId,
                DocumentFolderId = apiModel.DocumentFolderId,
                DocumentOrigin = apiModel.DocumentOrigin,
                DocumentFolderPath = apiModel.DocumentFolderPath,
                DocumentFolderHexColor = apiModel.DocumentFolderHexColor,
                DocumentSignatureSessionId = apiModel.DocumentSignatureSessionId,
                DocumentSignatureSessionIsActive = apiModel.DocumentSignatureSessionIsActive ?? false,
                Description = apiModel.Description,
                PreviewMetadata = apiModel.PreviewMetadata
                    .Select(x => x.ToDictionary(y => y.Key, y => y.Value))
                    .ToArray(),
                PrimaryDocument = TaskDocument.FromApiModel(apiModel.PrimaryDocument),
                UserName = apiModel.UserName,
                UserEmail = apiModel.UserEmail,
                CreatedByUserEmail = apiModel.CreatedByUserEmail,
                CreatedByUserName = apiModel.CreatedByUserName,
                CompletedByUserEmail = apiModel.CompletedByUserEmail,
                CompletedByUserName = apiModel.CompletedByUserName,
                ProcessName = apiModel.ProcessName,
                ProcessElementName = apiModel.ProcessElementName,
                Result = apiModel.Result,
                Score = apiModel.Score ?? 0.0
            };
        }

        #region Properties

        public string ContentType { get; set; }

        public string Id { get; set; }

        public string[] AncestorIds { get; set; }

        public string OrganizationId { get; set; }

        public string ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string ProjectAccessType { get; set; }

        public string ItemId { get; set; }

        public string ItemType { get; set; }

        public string UniqueId { get; set; }

        public string Name { get; set; }

        public string CreatedOn { get; set; }

        public string DueOn { get; set; }

        public int Priority { get; set; }

        public string TaskMilestoneName { get; set; }

        public string TaskStateName { get; set; }

        public double TaskDurationMultiplier { get; set; }

        public string TaskDurationInterval { get; set; }

        public string[] Tags { get; set; }

        public string CompletedOn { get; set; }

        public string PurgeDate { get; set; }

        public int PageCount { get; set; }

        public int FileSize { get; set; }

        public string ThumbnailUrl { get; set; }

        public bool IsSigned { get; set; }

        public string AssignmentUserEmail { get; set; }

        public string[] AssignmentUserEmails { get; set; }

        public string AssignmentUserName { get; set; }

        public string DocumentId { get; set; }

        public string DocumentFolderId { get; set; }

        public string DocumentOrigin { get; set; }

        public string DocumentFolderPath { get; set; }

        public string DocumentFolderHexColor { get; set; }

        public string DocumentSignatureSessionId { get; set; }

        public bool DocumentSignatureSessionIsActive { get; set; }

        public string Description { get; set; }

        public Dictionary<string, string>[] PreviewMetadata { get; set; }

        public TaskDocument PrimaryDocument { get; set; }

        public string UserName { get; set; }

        public string UserEmail { get; set; }

        public string CreatedByUserEmail { get; set; }

        public string CreatedByUserName { get; set; }

        public string CompletedByUserEmail { get; set; }

        public string CompletedByUserName { get; set; }

        public string ProcessName { get; set; }

        public string ProcessElementName { get; set; }

        public string Result { get; set; }

        public double Score { get; set; }

        #endregion

        public override ApiModel.SearchResult ToApiModel()
        {
            return new ApiModel.SearchResult()
            {
                Id = Id,
                ContentType = ContentType,
                AncestorIds = AncestorIds,
                OrganizationId = OrganizationId,
                ProjectId = ProjectId,
                ProjectName = ProjectName,
                ProjectAccessType = ProjectAccessType,
                ItemId = ItemId,
                ItemType = ItemType,
                UniqueId = UniqueId,
                Name = Name,
                CreatedOn = CreatedOn,
                DueOn = DueOn,
                Priority = Priority,
                TaskMilestoneName = TaskMilestoneName,
                TaskStateName = TaskStateName,
                TaskDurationMultiplier = TaskDurationMultiplier,
                TaskDurationInterval = TaskDurationInterval,
                Tags = Tags,
                CompletedOn = CompletedOn,
                PurgeDate = PurgeDate,
                PageCount = PageCount,
                FileSize = FileSize,
                ThumbnailUrl = ThumbnailUrl,
                IsSigned = IsSigned,
                AssignmentUserEmail = AssignmentUserEmail,
                AssignmentUserEmails = AssignmentUserEmails,
                AssignmentUserName = AssignmentUserName,
                DocumentId = DocumentId,
                DocumentFolderId = DocumentFolderId,
                DocumentOrigin = DocumentOrigin,
                DocumentFolderPath = DocumentFolderPath,
                DocumentFolderHexColor = DocumentFolderHexColor,
                DocumentSignatureSessionId = DocumentSignatureSessionId,
                DocumentSignatureSessionIsActive = DocumentSignatureSessionIsActive,
                Description = Description,
                PreviewMetadata = PreviewMetadata,
                PrimaryDocument = PrimaryDocument?.ToApiModel(),
                UserName = UserName,
                UserEmail = UserEmail,
                CreatedByUserEmail = CreatedByUserEmail,
                CreatedByUserName = CreatedByUserName,
                CompletedByUserEmail = CompletedByUserEmail,
                CompletedByUserName = CompletedByUserName,
                ProcessName = ProcessName,
                ProcessElementName = ProcessElementName,
                Result = Result,
                Score = Score
            };
        }
    }

    [Serializable]
    public class SearchResultCollection : Common.Model.EntityCollection<SearchResult, ApiModel.SearchResultCollection>
    {
        public SearchResultCollection(
            SearchResult[] items,
            NucleusOneApp app = null
        ) : base(app, items) { }

        public static SearchResultCollection FromApiModel(
            ApiModel.SearchResultCollection apiModel,
            NucleusOneApp app = null
        )
        {
            return new SearchResultCollection(
                items: apiModel.SearchResults?.Select((x) => SearchResult.FromApiModel(x, app)).ToArray());
        }

        public override ApiModel.SearchResultCollection ToApiModel()
        {
            return new ApiModel.SearchResultCollection()
            {
                SearchResults = Items.Select((x) => x.ToApiModel()).ToArray()
            };
        }
    }
}