using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Citi.MyCitigoldFP.Common.Web.Auth
{
    /// <summary>
    /// Web 인증 사용자를 위한 환경 모듈.
    /// 
    /// @author: Jeongyong, Jo
    /// @date: 2016-02-05
    /// </summary>
    public class MfpPrincipal : IPrincipal
    {
        MfpUser _user;
        
        public MfpPrincipal(string name, MfpUser user)
        {   
            _user = user;
            Identity = new GenericIdentity(name);
        }

        /// <summary>
        /// 인증된 사용자 정보.
        /// </summary>
        public MfpUser RM
        {
            get { return _user; }
        }

        /// <summary>
        /// 시스템에서 Key로 사용할 전용 속성.
        /// </summary>
        public string UniqueKey { get { return _user.UniqueKey; } }

        /// <summary>
        /// 행 번호.
        /// </summary>
        public int BankerNo { get { return _user.BankerNo; } }

        /// <summary>
        /// 사용자 아이디.
        /// </summary>
        public string ID { get { return _user.ID; } }

        /// <summary>
        /// 사용자 이름.
        /// </summary>
        public string Name { get { return _user.Name; } }

        /// <summary>
        /// 지점 명
        /// </summary>
        public string BranchName { get { return _user.BranchName; } }

        /// <summary>
        /// 지점코드 
        /// </summary>
        public string Branch { get { return _user.Branch; } }

        /// <summary>
        /// 영업본부 명 
        /// </summary>
        public string RegionName { get { return _user.RegionName; } }

        /// <summary>
        /// 영업본부 코드
        /// </summary>
        public string Region { get { return _user.Region; } }

        /// <summary>
        /// 접속한 사용자권한
        /// </summary>
        public string LoginRole { get { return _user.LoginRole; } }

        #region Implement IPrincipal interface
        /// <summary>
        /// 식별 정보.
        /// </summary>
        public IIdentity Identity
        {
            get;
            private set;
        }
        
        /// <summary>
        /// 제공된 role이 존재하는지 여부를 반환한다.
        /// </summary>
        /// <param name="role">role.</param>
        /// <returns><paramref name="role"/>을 통해 제공된 값이 포함되어 있는지 여부.</returns>
        public bool IsInRole(string role)
        {
            return _user.Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        } 
        #endregion
    }
}
