
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
    public class AuthorizeAttribute : AuthorizeAttribute
    {
        static readonly Type _allowAnonymousType = typeof(AllowAnonymousAttribute);

        readonly string _role;
        readonly string _serviceName;

        public MfpAuthorizeAttribute(string role, string serviceName = null)
        {
            if (string.IsNullOrEmpty(role))
                throw new ArgumentNullException("role");

            _role = role;
            _serviceName = serviceName;
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
            bool isSkip = filterContext.ActionDescriptor.IsDefined(_allowAnonymousType, true) ||
                        filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(_allowAnonymousType, true);

            if (!isSkip)
            {
                // TODO: 권한 검사 로직 (추후 로그인 사용자와 권한을 분리하는 로직 추가
                var user = filterContext.HttpContext.User as MfpPrincipal;
                if (user == null || !user.Identity.IsAuthenticated)
                {
                    throw new MfpIsNotAuthenticatedException("로그인이 필요합니다.");                    
                }
                if (!user.IsInRole(_role))
                {
                    if (!string.IsNullOrEmpty(_serviceName))
                        throw new MfpNotFoundAuthorizeException("[" + _serviceName + "] 서비스에 대한 권한이 없습니다.");
                    throw new MfpNotFoundAuthorizeException("권한이 없습니다.");
                }
            }

            base.OnAuthorization(filterContext);
        }
    }

    /// <summary>
    /// 로그인 인증 예외
    /// 
    /// @author: Jeongyong, Jo
    /// @date: 2016-03-31
    /// </summary>
    public class MfpIsNotAuthenticatedException : Exception
    {
        public MfpIsNotAuthenticatedException(string message)
            : base(message)
        {

        }
    }

    /// <summary>
    /// 서비스 권한 예외.
    /// 
    /// @author: Jeongyong, Jo
    /// @date: 2016-03-31
    /// </summary>
    public class MfpNotFoundAuthorizeException : Exception
    {
        public MfpNotFoundAuthorizeException(string message) 
            : base (message)
        {

        }
    }
}
