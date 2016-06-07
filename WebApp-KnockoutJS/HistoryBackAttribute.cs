using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Net;

namespace JOEJ
{
    public class HistoryBackItem
    {
        public Uri Uri { get; set; }
        public DateTime SetTime { get; set; }
    }

    public class HistoryBackAttribute : ActionFilterAttribute
    {
        static object Lock = new object();
        private static Dictionary<string, HistoryBackItem> HistoryBackUrls = new Dictionary<string, HistoryBackItem>();

        static System.Threading.Timer _scheduler;

        static HistoryBackAttribute()
        {
            _scheduler = new System.Threading.Timer(CollectGarbage, null, 3000, 5000);
        }

        private static void CollectGarbage(object state)
        {
            lock (HistoryBackUrls)
            foreach (var key in HistoryBackUrls
                .Where(x => x.Value.SetTime < DateTime.Now.AddSeconds(-10))
                .Select(x => x.Key)
                .ToList())
            {
                HistoryBackUrls.Remove(key);
            }
        }

        public static Uri GetHistoryBackUrl(string userKey)
        {
            if (!HistoryBackUrls.ContainsKey(userKey))
                return null;
            return HistoryBackUrls[userKey].Uri;
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
            var user = ((ControllerBase)filterContext.Controller).User;
            if (user == null || string.IsNullOrWhiteSpace(user.Identity.Name))
                return;

            if (!filterContext.HttpContext.Request.IsAjaxRequest()
                && filterContext.HttpContext.Request.HttpMethod.ToUpper() == "GET"
                && filterContext.Result.GetType() != typeof(PartialViewResult))
            {
                HistoryBackUrls[user.Identity.Name] = new HistoryBackItem { Uri = filterContext.HttpContext.Request.Url, SetTime = DateTime.Now };
            }
        }
    }
}
