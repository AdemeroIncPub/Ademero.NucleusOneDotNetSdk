﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Ademero.NucleusOneDotNetSdk.ApiModel
{
    // Originally generated by nucleus_one_utilities
    // Serializable members must be explicitly marked with [JsonProperty].
    [Serializable]
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DocumentUpload : Common.ApiModel.Entity<DocumentUpload>
    {
        public DocumentUpload() { }

        #region Properties

        [JsonProperty("SignedUrl")]
        public string SignedUrl { get; set; }

        [JsonProperty("SignedUrl2")]
        public string SignedUrl2 { get; set; }

        [JsonProperty("ObjectName")]
        public string ObjectName { get; set; }

        [JsonProperty("ObjectName2")]
        public string ObjectName2 { get; set; }

        [JsonProperty("UniqueId")]
        public string UniqueId { get; set; }

        [JsonProperty("OriginalFilename")]
        public string OriginalFilename { get; set; }

        [JsonProperty("OriginalFilepath")]
        public string OriginalFilepath { get; set; }

        [JsonProperty("OriginalFileSize")]
        public int OriginalFileSize { get; set; }

        [JsonProperty("FieldIDsAndValues")]
        public Dictionary<string, List<string>> FieldIDsAndValues { get; set; }

        [JsonProperty("DocumentFolderID")]
        public string DocumentFolderID { get; set; }

        [JsonProperty("ContentType")]
        public string ContentType { get; set; }

        #endregion
    }

    // Originally generated by nucleus_one_utilities
    // Serializable members must be explicitly marked with [JsonProperty].
    [Serializable]
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DocumentUploadCollection : QueryResultEntityCollection<DocumentUploadCollection, DocumentUpload>
    {
        public DocumentUploadCollection() { }

        #region Properties

        [JsonProperty("DocumentUploads")]
        public DocumentUpload[] DocumentUploads { get; set; }

        #endregion
    }
}