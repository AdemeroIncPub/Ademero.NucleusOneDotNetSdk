﻿using System;
using System.Linq;

namespace Ademero.NucleusOneDotNetSdk.Model
{
    // Originally generated by nucleus_one_utilities
    [Serializable]
    public class Tag : Common.Model.Entity<ApiModel.Tag>
    {
        private Tag(NucleusOneApp app) : base(app) { }

        public static Tag FromApiModel(
            ApiModel.Tag apiModel,
            NucleusOneApp app = null
        )
        {
            if (apiModel == null)
                return null;
            return new Tag(app)
            {
                Text = apiModel.Text,
                TextLower = apiModel.TextLower,
                ModifiedOn = apiModel.ModifiedOn,
                AssetItemTag = AssetItemTag.FromApiModel(apiModel.AssetItemTag)
            };
        }

        #region Properties

        public string Text { get; set; }

        public string TextLower { get; set; }

        public string ModifiedOn { get; set; }

        public AssetItemTag AssetItemTag { get; set; }

        #endregion

        public override ApiModel.Tag ToApiModel()
        {
            return new ApiModel.Tag()
            {
                Text = Text,
                TextLower = TextLower,
                ModifiedOn = ModifiedOn,
                AssetItemTag = AssetItemTag.ToApiModel()
            };
        }
    }

    [Serializable]
    public class TagCollection : Common.Model.EntityCollection<Tag, ApiModel.TagCollection>
    {
        public TagCollection(
            Tag[] items,
            NucleusOneApp app = null
        ) : base(app, items) { }

        public static TagCollection FromApiModel(
            ApiModel.TagCollection apiModel,
            NucleusOneApp app = null
        )
        {
            if (apiModel == null)
                return null;
            return new TagCollection(
                items: apiModel.Tags?.Select((x) => Tag.FromApiModel(x, app)).ToArray());
        }

        public override ApiModel.TagCollection ToApiModel()
        {
            return new ApiModel.TagCollection()
            {
                Tags = Items.Select((x) => x.ToApiModel()).ToArray()
            };
        }
    }
}