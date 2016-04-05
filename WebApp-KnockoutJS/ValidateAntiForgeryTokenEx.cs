using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace MyWebAppPackage.Extensions
{
    /// <summary>
    /// Cross-Site Request Forgery를 위한 토큰 검사기.
    /// 
    /// @author: Jeongyong, Jo
    /// @date: 2016-02-24
    /// </summary>
    public sealed class ValidateAntiForgeryTokenExAttribute : AuthorizeAttribute
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
                        throw new HttpAntiForgeryException(string.Format("Header does not contain {0}", TOKEN_KEY));

                    var cookieToken = req.Cookies.Get(AntiForgeryConfig.CookieName);
                    if (cookieToken == null || cookieToken.Value == null)
                        throw new HttpAntiForgeryException(string.Format("Cookie does not contain {0}", TOKEN_KEY));

                    AntiForgery.Validate(cookieToken.Value, headerToken);
                }
                else
                {
                    new ValidateAntiForgeryTokenAttribute().OnAuthorization(filterContext);
                }
            }

            base.OnAuthorization(filterContext);
        }
    }

    public class AntiForgeryConfigure
    {
        public static void Init(HttpContextBase ctx)
        {
            AntiForgeryConfig.UniqueClaimTypeIdentifier = System.Security.Claims.ClaimTypes.NameIdentifier;
            //AntiForgeryConfig.AdditionalDataProvider.ValidateAdditionalData(ctx, "joejy");
        }
    }
}
