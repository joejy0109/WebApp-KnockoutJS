
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web;
using Citi.MyCitigoldFP.Common;
using System.Configuration;
using System.Web.Helpers;
using Citi.MyCitigoldFP.Common.Web.Auth;
using System.Net;

namespace AAAAAAAAAAA
{
    /// <summary>
    /// 별도로 처리되지 않고 throw 되는 모든 예외에 대해서 관리한다.
    /// 
    /// @author: Jeongyong, Jo
    /// @since: 2016-02-02
    /// </summary>
    public class ErrorHandlerAttribute : HandleErrorAttribute
    {
        private static readonly string DefaultUrl = ConfigurationManager.AppSettings["MainUrl"];

        private static readonly string JsScripts = @"
<script type='text/javascript'>
    alert({0});
    location.href = '{1}';
</script>
";
        // todo: 오류를 기록할 logger
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception == null || filterContext.ExceptionHandled) 
                return;

            filterContext.ExceptionHandled = true;
            /* 
             * TODO: business로 정의된 예외와 그렇지 않은 예외를 구분한다.
             * business로 정의된 예외는 클라이언트에 메세지를 제공하거나 재가공하여 전달하고, 
             * 그렇지 않은 예외는 추상적인 메세지(예) 관리자에게 문의하세요)로 일괄 통일한다. 
             * 
             * 또는 일괄 통일.
             * 
             * 모든 예외는 로그로 기록한다.             
             */

            var ex = filterContext.Exception.InnerException ?? filterContext.Exception;
            var ctx = filterContext.HttpContext;

            // 예외 종류에 따라 메세지 강제화. 필요에 따라 수정하시오.
            string message;
            string returnUrl = null;
            
            if (ex is MfpIsNotAuthenticatedException)
            {
                message = ex.Message;
                returnUrl = DefaultUrl; // 로그인 url.
                ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else if (ex is MfpNotFoundAuthorizeException)
            {
                message = ex.Message;
                ctx.Response.StatusCode = 600; // IE에서 정의된 ERROR(401, 402 등등)에 대해 자체 페이지를 표시해주는 문제가 있음
            }
            else if (ex is HttpAntiForgeryException)
            {
                message = "권한 없는 악의적인 요청이 발견되었습니다.";
                ctx.Response.StatusCode = 601;
            }
            else if (ex is MfpException)
            {
                message = "허용되지 않는 요청입니다.\n(" + ex.Message + ")";
                ctx.Response.StatusCode = 602;
            }
            else
            {
                // TODO: 시스템 예외는 사용자로부터 감추고 대체 메세지를 제공하는 것이 좋다.
                // ex) '일시적으로 시스템에 장애가 발생하였습니다. 관리자에게 문의하세요.'
#if DebugMode 
                message = ex.Message;
#else
                message = "일시적으로 시스템 장애가 발생하였습니다.\n관리자에게 문의해 주시기 바랍니다.\n연락처: (02-000-0000).";
#endif
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
                        
            filterContext.Result = new ContentResult {
                Content = filterContext.HttpContext.Request.IsAjaxRequest()
                            ? message
                            : string.Format(JsScripts,
                                    Json.Encode(message), 
                                    returnUrl 
                                    ?? filterContext.HttpContext.Request.UrlReferrer
                                    ?? (object)DefaultUrl)
            };

            

            base.OnException(filterContext);
        } 
        
    }
}
