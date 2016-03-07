using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace MyWebAppPackage.Extensions
{
    public class ValidateAntiForgeryTokenEx : AuthorizeAttribute
    {
        const string TOKEN_KEY = "__RequestVerificationToken";

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var req = filterContext.HttpContext.Request;
            if (req.HttpMethod == WebRequestMethods.Http.Post)
            {
                if (req.IsAjaxRequest())
                {
                    var headerToken = req.Headers.Get(TOKEN_KEY);
                    if (headerToken == null)
                        throw new InvalidOperationException("Header does not contain " + TOKEN_KEY);

                    var tokenCookie = req.Cookies.Get(TOKEN_KEY);
                    if (tokenCookie == null || tokenCookie.Value == null)
                        throw new InvalidOperationException("Cookie does not contain " + TOKEN_KEY);

                    AntiForgery.Validate(tokenCookie.Value, headerToken);
                }
                else
                {
                    new ValidateAntiForgeryTokenAttribute().OnAuthorization(filterContext);
                }
            }
        }

    }
}
