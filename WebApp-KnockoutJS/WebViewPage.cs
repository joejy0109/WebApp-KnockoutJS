
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Citi.MyCitigoldFP.Common.Web.Auth
{
    /// <summary>
    /// MPF 인증 사용자를 ASP.NET MVC View에서 직접 사용할 수 있도록 제공한다.
    /// 
    /// @author: Jeongyong, Jo
    /// @date: 2016-02-05
    /// </summary>
    public abstract class MfpWebViewPage : WebViewPage
    {
        public virtual new MfpPrincipal User
        {
            get { return base.User as MfpPrincipal; }
        }
    }

    /// <summary>
    /// MPF 인증 사용자를 ASP.NET MVC View에서 직접 사용할 수 있도록 제공한다.
    /// 
    /// @author: Jeongyong, Jo
    /// @date: 2016-02-05
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class MfpWebViewPage<TModel> : WebViewPage<TModel>
    {
        public virtual new MfpPrincipal User
        {
            get { return base.User as MfpPrincipal; }
        }
    }
}
