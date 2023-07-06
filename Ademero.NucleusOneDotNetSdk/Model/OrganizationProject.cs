using System;
using System.Linq;

namespace Ademero.NucleusOneDotNetSdk.Model
{
    [Serializable]
    public class OrganizationProject : Common.Model.Entity<ApiModel.OrganizationProject>
    {
        private OrganizationProject(NucleusOneApp app) : base(app) { }

        public static OrganizationProject FromApiModel(
            ApiModel.OrganizationProject apiModel,
            NucleusOneApp app = null
        )
        {
            return new OrganizationProject(app)
            {
                Id = apiModel.Id,
                OrganizationId = apiModel.OrganizationId,
                AccessType = apiModel.AccessType,
                CreatedOn = apiModel.CreatedOn,
                CreatedByUserId = apiModel.CreatedByUserId,
                CreatedByUserEmail = apiModel.CreatedByUserEmail,
                CreatedByUserName = apiModel.CreatedByUserName,
                Name = apiModel.Name,
                NameLower = apiModel.NameLower,
                Disabled = apiModel.Disabled.HasValue ? apiModel.Disabled.Value : false,
                IsMarkedForPurge = apiModel.IsMarkedForPurge.HasValue ? apiModel.IsMarkedForPurge.Value : false,
                PurgeMarkedOn = apiModel.PurgeMarkedOn,
                PurgeMarkedByUserId = apiModel.PurgeMarkedByUserId,
                PurgeMarkedByUserName = apiModel.PurgeMarkedByUserName,
                PurgeMarkedByUserEmail = apiModel.PurgeMarkedByUserEmail
            };
        }

        #region Properties

        public string Id { get; set; }

        public string OrganizationId { get; set; }

        public string AccessType { get; set; }

        public string CreatedOn { get; set; }

        public string CreatedByUserId { get; set; }

        public string CreatedByUserEmail { get; set; }

        public string CreatedByUserName { get; set; }

        public string Name { get; set; }

        public string NameLower { get; set; }

        public bool Disabled { get; set; }

        public bool IsMarkedForPurge { get; set; }

        public string PurgeMarkedOn { get; set; }

        public string PurgeMarkedByUserId { get; set; }

        public string PurgeMarkedByUserName { get; set; }

        public string PurgeMarkedByUserEmail { get; set; }

        #endregion

        public override ApiModel.OrganizationProject ToApiModel()
        {
            return new ApiModel.OrganizationProject()
            {
                Id = Id,
                OrganizationId = OrganizationId,
                AccessType = AccessType,
                CreatedOn = CreatedOn,
                CreatedByUserId = CreatedByUserId,
                CreatedByUserEmail = CreatedByUserEmail,
                CreatedByUserName = CreatedByUserName,
                Name = Name,
                NameLower = NameLower,
                Disabled = Disabled,
                IsMarkedForPurge = IsMarkedForPurge,
                PurgeMarkedOn = PurgeMarkedOn,
                PurgeMarkedByUserId = PurgeMarkedByUserId,
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
