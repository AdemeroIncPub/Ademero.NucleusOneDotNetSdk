﻿using Newtonsoft.Json;
using System;

namespace Ademero.NucleusOneDotNetSdk.ApiModel
{
    // Originally generated by nucleus_one_utilities
    // Serializable members must be explicitly marked with [JsonProperty].
    [Serializable]
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class OrganizationMember : Common.ApiModel.Entity<OrganizationMember>
    {
        public OrganizationMember() { }

        #region Properties

        [JsonProperty(PropertyName = "ID")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "CreatedOn")]
        public string CreatedOn { get; set; }

        [JsonProperty(PropertyName = "OrganizationID")]
        public string OrganizationID { get; set; }

        [JsonProperty(PropertyName = "OrganizationName")]
        public string OrganizationName { get; set; }

        [JsonProperty(PropertyName = "UserID")]
        public string UserID { get; set; }

        [JsonProperty(PropertyName = "UserName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "UserNameLower")]
        public string UserNameLower { get; set; }

        [JsonProperty(PropertyName = "UserEmail")]
        public string UserEmail { get; set; }

        [JsonProperty(PropertyName = "Disabled")]
        public bool Disabled { get; set; }

        [JsonProperty(PropertyName = "IsReadOnly")]
        public bool IsReadOnly { get; set; }

        [JsonProperty(PropertyName = "IsAdmin")]
        public bool IsAdmin { get; set; }

        #endregion
    }

    // Originally generated by nucleus_one_utilities
    // Serializable members must be explicitly marked with [JsonProperty].
    [Serializable]
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class OrganizationMemberCollection : QueryResultEntityCollection<OrganizationMemberCollection, OrganizationMember>
    {
        public OrganizationMemberCollection() { }

        #region Properties

        [JsonProperty(nameof(OrganizationMembers))]
        public OrganizationMember[] OrganizationMembers { get; set; }

        #endregion
    }
}
