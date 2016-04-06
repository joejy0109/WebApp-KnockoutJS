using Citi.MyCitigoldFP.Common.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace JOEJY
{
    /// <summary>
    /// <see cref="System.Web.Security.FormsAuthentication"/>을 기반으로 하는 MFP 사용자 인증서비스.
    /// 
    /// @author: Jeongyong, Jo
    /// @date: 2016-02-05 
    /// </summary>
    public sealed class MfpFormsAuthService : MfpAuthService
    {
        private static readonly JavaScriptSerializer jss = new JavaScriptSerializer();

        /// <summary>
        /// MPF 사용자에 대한 인증을 처리한다.
        /// </summary>
        /// <param name="user">사용자 정보 모델 컨테이너.</param>
        /// <param name="isPersistent">인증정보의 영속성 여부.</param>
        public override void SignIn(MfpUser user, bool isPersistent = false)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var userData = jss.Serialize(user);
            var ticket = new FormsAuthenticationTicket(1,
                user.BankerNo.ToString(),
                DateTime.Now,
                DateTime.Now.Add(FormsAuthentication.Timeout),
                isPersistent,
                userData,
                FormsAuthentication.FormsCookiePath);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));            
            Context.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// MFP 사용자 인증을 해제한다.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 2016-02-05
        /// </summary>
        public override void SignOut()
        {
            Context.Session.Abandon();
            FormsAuthentication.SignOut();
        }
    }

    /// <summary>
    /// <see cref="System.Web.SessionState.HttpSessionState"/>을 저장소로 하는 MFP 인증서비스.
    /// 
    /// @author: Jeongyong, Jo
    /// @date: 2016-02-05
    /// </summary>
    public sealed class MfpSessionAuthService : MfpAuthService
    {
        private static readonly IEncryptor _encryptor = new Aes128Encryptor();

        public const string Identity = "CitiBanker";       

        public static IEncryptor Encryptor
        {
            get { return _encryptor; }
        }

        /// <summary>
        /// MFP 사용자 인증을 처리한다.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 2016-02-05
        /// </summary>
        /// <param name="user">사용자 정보 모델 컨테이너.</param>
        /// <param name="isPersistent">[Obsolete] 세션메모리 기반에서는 사용되지 않는다.(Web.Config > SessionState 환경에 영향을 받음)</param>
        public override void SignIn(MfpUser user, bool isPersistent = false)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var key = Guid.NewGuid().ToString();
            var cookie = new HttpCookie(Identity, key);

            Caching.CacheStore.Set(key, user);

            if (Context.Request.Cookies.Get(Identity) != null)
                Context.Request.Cookies.Remove(Identity);

            Context.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// MFP 사용자 인증을 해제한다.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 2016-02-05
        /// </summary>
        public override void SignOut()
        {
            var cookie = Context.Request.Cookies[Identity];
            if (cookie != null)
                Caching.CacheStore.Remove(cookie.Value);
            Context.Session.Abandon();
        }
    }


    /// <summary>
    /// MFP 사용자 인증 서비스를 위한 시그니처 추상 클래스.
    /// 
    /// @author: jeongyong, Jo
    /// @date: 2016-02-05
    /// </summary>
    public abstract class MfpAuthService
    {
        /// <summary>
        /// 현재 요청의 <see cref="HttpContext"/> 인스턴스.
        /// </summary>
        protected static HttpContext Context
        {
            get { return HttpContext.Current; }
        }

        /// <summary>
        /// MPF 사용자에 대한 인증을 처리한다.
        /// </summary>
        /// <param name="user">사용자 정보 모델 컨테이너.</param>
        /// <param name="isPersistent">인증정보의 영속성 여부.</param>
        public abstract void SignIn(MfpUser user, bool isPersistent = false);

        /// <summary>
        /// MFP 사용자 인증를 해제한다.
        /// </summary>
        public abstract void SignOut();
    }
}
