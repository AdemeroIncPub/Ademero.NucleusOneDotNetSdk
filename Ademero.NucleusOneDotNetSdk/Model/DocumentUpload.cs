﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Ademero.NucleusOneDotNetSdk.Model
{
    // Originally generated by nucleus_one_utilities.
    [Serializable]
    public class DocumentUpload : Common.Model.Entity<ApiModel.DocumentUpload>
    {
        private DocumentUpload(NucleusOneApp app) : base(app) { }

        public static DocumentUpload FromApiModel(ApiModel.DocumentUpload apiModel, NucleusOneApp app = null)
        {
            return new DocumentUpload(app)
            {
                SignedUrl = apiModel.SignedUrl,
                SignedUrl2 = apiModel.SignedUrl2,
                ObjectName = apiModel.ObjectName,
                ObjectName2 = apiModel.ObjectName2,
                UniqueId = apiModel.UniqueId,
                OriginalFilename = apiModel.OriginalFilename,
                OriginalFilepath = apiModel.OriginalFilepath,
                OriginalFileSize = apiModel.OriginalFileSize,
                FieldIDsAndValues = apiModel.FieldIDsAndValues,
                DocumentFolderID = apiModel.DocumentFolderID,
                ContentType = apiModel.ContentType
            };
        }

        #region Properties

        public string SignedUrl { get; set; }

        public string SignedUrl2 { get; set; }

        public string ObjectName { get; set; }

        public string ObjectName2 { get; set; }

        public string UniqueId { get; set; }

        public string OriginalFilename { get; set; }

        public string OriginalFilepath { get; set; }

        public int OriginalFileSize { get; set; }

        public Dictionary<string, List<string>> FieldIDsAndValues { get; set; }

        public string DocumentFolderID { get; set; }

        public string ContentType { get; set; }

        #endregion

        public override ApiModel.DocumentUpload ToApiModel()
        {
            return new ApiModel.DocumentUpload()
            {
                SignedUrl = this.SignedUrl,
                SignedUrl2 = this.SignedUrl2,
                ObjectName = this.ObjectName,
                ObjectName2 = this.ObjectName2,
                UniqueId = this.UniqueId,
                OriginalFilename = this.OriginalFilename,
                OriginalFilepath = this.OriginalFilepath,
                OriginalFileSize = this.OriginalFileSize,
                FieldIDsAndValues = this.FieldIDsAndValues,
                DocumentFolderID = this.DocumentFolderID
            };
        }
    }

    [Serializable]
    public class DocumentUploadCollection
        : Common.Model.EntityCollection<DocumentUpload, ApiModel.DocumentUploadCollection>
    {
        public DocumentUploadCollection(
            DocumentUpload[] items,
            NucleusOneApp app = null
        ) : base(app, items) { }

        public static DocumentUploadCollection FromApiModel(
            ApiModel.DocumentUploadCollection apiModel,
            NucleusOneApp app = null
        )
        {
            return new DocumentUploadCollection(
                items: apiModel.DocumentUploads?.Select((x) => DocumentUpload.FromApiModel(x, app)).ToArray());
        }

        public override ApiModel.DocumentUploadCollection ToApiModel()
        {
            return new ApiModel.DocumentUploadCollection()
            {
                DocumentUploads = Items.Select((x) => x.ToApiModel()).ToArray()
            };
        }
    }
}
