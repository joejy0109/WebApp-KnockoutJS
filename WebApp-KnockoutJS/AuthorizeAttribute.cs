using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Citi.MyCitigoldFP.Common.Web.Auth
{
    /// <summary>
    /// MPF 인증 사용자에 대한 권한을 검사한다.
    /// 
    /// @author: Jeongyong, Jo
    /// @date: 2016-02-05
    /// </summary>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method, AllowMultiple=false, Inherited=true)]
    public class MfpAuthorizeAttribute : AuthorizeAttribute
    {
        static readonly Type _allowAnonymousType = typeof(AllowAnonymousAttribute);

        readonly string _role;
        
        public MfpAuthorizeAttribute(string role)
        {
            if (string.IsNullOrEmpty(role))
                throw new ArgumentNullException("role");

            _role = role;
        }

        /// <summary>
        /// Action mehtod 진입 전에 권한을 검사한다.
        /// </summary>
        /// <param name="filterContext"><see cref="AuthorizationContext"/></param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            // AllowAnonymousAttribute가 선언된 Action Method 및 Controller는 검사를 통과한다.
            bool skip = filterContext.ActionDescriptor.IsDefined(_allowAnonymousType, true) ||
                        filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(_allowAnonymousType, true);

            if (skip) return;

            // TODO: 권한 검사 로직 (추후 로그인 사용자와 권한을 분리하는 로직 추가
            var user = filterContext.HttpContext.User as MfpPrincipal;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                throw new InvalidOperationException("로그인이 필요합니다.");
            }
            if (!user.IsInRole(_role))
            {
                throw new InvalidOperationException("권한이 없습니다.");
            }

            base.OnAuthorization(filterContext);
        }
    }
}
