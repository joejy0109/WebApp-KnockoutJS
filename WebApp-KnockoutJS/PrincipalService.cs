
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace JOEJY
{
    /// <summary>
    /// MFP 인증 사용자 정보 설정 서비스.
    /// 
    /// @author: Jeongyong, Jo
    /// @date: 2016-02-05 
    /// </summary>
    public static class MfpPrincipalService
    {
        private static readonly JavaScriptSerializer jss = new JavaScriptSerializer();

        /// <summary>
        /// forms 인증 정보에서 데이터를 추출하여 HttpContext.User 항목을 설정한다.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 2016-02-05
        /// </summary>
        /// <param name="context"></param>
        public static void SetMfpPrincipal(this HttpContext context)
        {
            var authCookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                System.Diagnostics.Stopwatch stop = System.Diagnostics.Stopwatch.StartNew();

                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket == null)
                    return;

                var user = jss.Deserialize<MfpUser>(authTicket.UserData);
                context.User = new MfpPrincipal(authTicket.Name, user);

                System.Diagnostics.Debug.WriteLine("set principal time by forms: {0}", stop.Elapsed);
            }
            else
            {
                
                var authCookie2 = context.Request.Cookies[MfpSessionAuthService.Identity];
                if (authCookie2 != null)
                {
                    System.Diagnostics.Stopwatch stop = System.Diagnostics.Stopwatch.StartNew();
                    
                    var user = Common.Caching.CacheStore.Get<MfpUser>(authCookie2.Value);
                    if (user != null)
                    {
                        context.User = new MfpPrincipal(authCookie2.Value, user);
                    }

                    System.Diagnostics.Debug.WriteLine("set principal time by cache: {0}", stop.Elapsed);
                }
            }
        }
    }
}
