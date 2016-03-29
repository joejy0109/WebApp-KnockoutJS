using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Citi.MyCitigoldFP.Common.Web.Auth
{
    /// <summary>
    /// 인증된 사용자 정보.
    /// 
    /// @author: Jeongyong, Jo
    /// @date: 2016-02-05
    /// </summary>
    public class MfpUser
    {
        string _uniqueKey = null;

        /// <summary>
        /// 시스템에서 key로 사용하기 위한 전용 속성.
        /// </summary>
        public string UniqueKey {
            get 
            {
                if (string.IsNullOrEmpty(_uniqueKey))
                    _uniqueKey = string.Format("{0}__{1}", this.BankerNo, Guid.NewGuid().ToString().Substring(0, 8));
                return _uniqueKey;
            }
            set { _uniqueKey = value; }
        }

        /// <summary>
        /// 행원 번호.
        /// </summary>
        public int BankerNo { get; set; }

        /// <summary>
        /// 아이디.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 이름.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 지점코드
        /// </summary>
        public string Branch { get; set; }

        /// <summary>
        /// 지점명
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// 영업본부 코드
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// 영업본부 명 
        /// </summary>
        public string RegionName { get; set; }

        /// <summary>
        /// 접속한 사용자권한
        /// </summary>
        public string LoginRole { get; set; }

        IList<string> _roles;
        /// <summary>
        /// 부여된 권한 목록.
        /// </summary>
        public IList<string> Roles
        {
            get { return (_roles = _roles ?? new List<string>()); }
            set { _roles = value; }
        }            
    }
}
