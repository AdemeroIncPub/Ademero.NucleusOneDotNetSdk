﻿using System;
using System.Linq;

namespace Ademero.NucleusOneDotNetSdk.Model
{
    // Originally generated by nucleus_one_utilities.
    [Serializable]
    public class OrganizationMember : Common.Model.Entity<ApiModel.OrganizationMember>
    {
        private OrganizationMember(NucleusOneApp app) : base(app) { }

        public static OrganizationMember FromApiModel(
            ApiModel.OrganizationMember apiModel,
            NucleusOneApp app = null
        )
        {
            if (apiModel == null)
                return null;
            return new OrganizationMember(app)
            {
                Id = apiModel.Id,
                CreatedOn = apiModel.CreatedOn,
                OrganizationID = apiModel.OrganizationID,
                OrganizationName = apiModel.OrganizationName,
                UserID = apiModel.UserID,
                UserName = apiModel.UserName,
                UserNameLower = apiModel.UserNameLower,
                UserEmail = apiModel.UserEmail,
                Disabled = apiModel.Disabled,
                IsReadOnly = apiModel.IsReadOnly,
                IsAdmin = apiModel.IsAdmin
            };
        }

        #region Properties

        public string Id { get; private set; }

        public string CreatedOn { get; private set; }

        public string OrganizationID { get; private set; }

        public string OrganizationName { get; private set; }

        public string UserID { get; private set; }

        public string UserName { get; private set; }

        public string UserNameLower { get; private set; }

        public string UserEmail { get; private set; }

        public bool Disabled { get; private set; }

        public bool IsReadOnly { get; private set; }

        public bool IsAdmin { get; private set; }

        #endregion

        public override ApiModel.OrganizationMember ToApiModel()
        {
            return new ApiModel.OrganizationMember()
            {
                Id = Id,
                CreatedOn = CreatedOn,
                OrganizationID = OrganizationID,
                OrganizationName = OrganizationName,
                UserID = UserID,
                UserName = UserName,
                UserNameLower = UserNameLower,
                UserEmail = UserEmail,
                Disabled = Disabled,
                IsReadOnly = IsReadOnly,
                IsAdmin = IsAdmin
            };
        }
    }

    [Serializable]
    public class OrganizationMemberCollection
        : Common.Model.EntityCollection<OrganizationMember, ApiModel.OrganizationMemberCollection>
    {
        public OrganizationMemberCollection(
            OrganizationMember[] items,
            NucleusOneApp app = null
        ) : base(app, items) { }

        public static OrganizationMemberCollection FromApiModel(
            ApiModel.OrganizationMemberCollection apiModel,
            NucleusOneApp app = null
        )
        {
            if (apiModel == null)
                return null;
            return new OrganizationMemberCollection(
                items: apiModel.OrganizationMembers?.Select((x) => OrganizationMember.FromApiModel(x, app)).ToArray());
        }

        public override ApiModel.OrganizationMemberCollection ToApiModel()
        {
            return new ApiModel.OrganizationMemberCollection()
            {
                OrganizationMembers = Items.Select((x) => x.ToApiModel()).ToArray()
            };
        }
    }
}
