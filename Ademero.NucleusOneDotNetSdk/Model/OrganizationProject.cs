using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ademero.NucleusOneDotNetSdk.Model
{
    [Serializable]
    public class OrganizationProject : Common.Model.Entity<ApiModel.OrganizationProject>
    {
        private OrganizationProject(NucleusOneApp app) : base(app) { }

        public static OrganizationProject FromApiModel(ApiModel.OrganizationProject apiModel, NucleusOneApp app)
        {
            return new OrganizationProject(app)
            {
                Id = apiModel.Id,
                OrganizationID = apiModel.OrganizationID,
                AccessType = apiModel.AccessType,
                CreatedOn = apiModel.CreatedOn,
                CreatedByUserID = apiModel.CreatedByUserID,
                CreatedByUserEmail = apiModel.CreatedByUserEmail,
                CreatedByUserName = apiModel.CreatedByUserName,
                Name = apiModel.Name,
                NameLower = apiModel.NameLower,
                Disabled = apiModel.Disabled.HasValue ? apiModel.Disabled.Value : false,
                IsMarkedForPurge = apiModel.IsMarkedForPurge.HasValue ? apiModel.IsMarkedForPurge.Value : false,
                PurgeMarkedOn = apiModel.PurgeMarkedOn,
                PurgeMarkedByUserID = apiModel.PurgeMarkedByUserID,
                PurgeMarkedByUserName = apiModel.PurgeMarkedByUserName,
                PurgeMarkedByUserEmail = apiModel.PurgeMarkedByUserEmail
            };
        }

        #region Properties

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("organizationID")]
        public string OrganizationID { get; set; }

        [JsonProperty("accessType")]
        public string AccessType { get; set; }

        [JsonProperty("createdOn")]
        public string CreatedOn { get; set; }

        [JsonProperty("createdByUserID")]
        public string CreatedByUserID { get; set; }

        [JsonProperty("createdByUserEmail")]
        public string CreatedByUserEmail { get; set; }

        [JsonProperty("createdByUserName")]
        public string CreatedByUserName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nameLower")]
        public string NameLower { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }

        [JsonProperty("isMarkedForPurge")]
        public bool IsMarkedForPurge { get; set; }

        [JsonProperty("purgeMarkedOn")]
        public string PurgeMarkedOn { get; set; }

        [JsonProperty("purgeMarkedByUserID")]
        public string PurgeMarkedByUserID { get; set; }

        [JsonProperty("purgeMarkedByUserName")]
        public string PurgeMarkedByUserName { get; set; }

        [JsonProperty("purgeMarkedByUserEmail")]
        public string PurgeMarkedByUserEmail { get; set; }

        #endregion

        public override ApiModel.OrganizationProject ToApiModel()
        {
            return new ApiModel.OrganizationProject()
            {
                Id = Id,
                OrganizationID = OrganizationID,
                AccessType = AccessType,
                CreatedOn = CreatedOn,
                CreatedByUserID = CreatedByUserID,
                CreatedByUserEmail = CreatedByUserEmail,
                CreatedByUserName = CreatedByUserName,
                Name = Name,
                NameLower = NameLower,
                Disabled = Disabled,
                IsMarkedForPurge = IsMarkedForPurge,
                PurgeMarkedOn = PurgeMarkedOn,
                PurgeMarkedByUserID = PurgeMarkedByUserID,
                PurgeMarkedByUserName = PurgeMarkedByUserName,
                PurgeMarkedByUserEmail = PurgeMarkedByUserEmail
            };
        }
    }

    [Serializable]
    public class OrganizationProjectCollection : Common.Model.EntityCollection<OrganizationProject, ApiModel.OrganizationProjectCollection>
    {
        public OrganizationProjectCollection(
            OrganizationProject[] items,
            NucleusOneApp app = null
        ) : base(app, items) { }

        public static OrganizationProjectCollection FromApiModel(
            ApiModel.OrganizationProjectCollection apiModel,
            NucleusOneApp app = null
        )
        {
            return new OrganizationProjectCollection(
                items: apiModel.Projects?.Select((x) => OrganizationProject.FromApiModel(x, app)).ToArray());
        }

        public override ApiModel.OrganizationProjectCollection ToApiModel()
        {
            return new ApiModel.OrganizationProjectCollection()
            {
                Projects = Items.Select((x) => x.ToApiModel()).ToArray()
            };
        }
    }
}
