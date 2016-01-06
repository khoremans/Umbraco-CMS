﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core.Services;

namespace Umbraco.Web.Editors
{

    //TODO:  We'll need to be careful about the security on this controller, when we start implementing 
    // methods to modify content types we'll need to enforce security on the individual methods, we
    // cannot put security on the whole controller because things like GetAllowedChildren are required for content editing.

    /// <summary>
    /// An API controller used for dealing with content types
    /// </summary>
    [PluginController("UmbracoApi")]
    public class ContentTypeController : ContentTypeControllerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ContentTypeController()
            : this(UmbracoContext.Current)
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public ContentTypeController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }


        /// <summary>
        /// Gets all user defined properties.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllPropertyTypeAliases()
        {
            return ApplicationContext.Services.ContentTypeService.GetAllPropertyTypeAliases();
        }

        /// <summary>
        /// Returns the allowed child content type objects for the content item id passed in
        /// </summary>
        /// <param name="contentId"></param>
        public IEnumerable<ContentTypeBasic> GetAllowedChildren(int contentId)
        {
            if (contentId == Constants.System.RecycleBinContent)
                return Enumerable.Empty<ContentTypeBasic>();

            IEnumerable<IContentType> types;
            if (contentId == Constants.System.Root)
            {
                types = Services.ContentTypeService.GetAllContentTypes().ToList();

                //if no allowed root types are set, just return everything
                if(types.Any(x => x.AllowedAsRoot))
                    types = types.Where(x => x.AllowedAsRoot);
            }
            else
            {
                var contentItem = Services.ContentService.GetById(contentId);
                if (contentItem == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                var ids = contentItem.ContentType.AllowedContentTypes.Select(x => x.Id.Value).ToArray();
                
                if (ids.Any() == false) return Enumerable.Empty<ContentTypeBasic>();

                types = Services.ContentTypeService.GetAllContentTypes(ids).ToList();
            }

            var basics = types.Select(Mapper.Map<IContentType, ContentTypeBasic>).ToList();

            var localizedTextService = Services.TextService;
            foreach (var basic in basics)
            {
                basic.Name = localizedTextService.UmbracoDictionaryTranslate(basic.Name);
                basic.Description = localizedTextService.UmbracoDictionaryTranslate(basic.Description);
            }
            return basics;
        }
    }
}