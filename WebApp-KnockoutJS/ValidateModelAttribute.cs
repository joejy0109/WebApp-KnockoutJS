using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace MyWebAppPackage.Extensions
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        static readonly Regex regex = new Regex(@"^\[\d+\]\.(.*)", RegexOptions.Compiled | RegexOptions.Singleline);

        public string ViewName { get; private set; }

        public ValidateModelAttribute()
        {

        }

        public ValidateModelAttribute(string viewName)
        {
            this.ViewName = viewName;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var modelState = filterContext.Controller.ViewData.ModelState;
            var valueProvider = filterContext.Controller.ValueProvider;

            var keyWithNoIncommingValue = modelState.Keys.Where(x => !valueProvider.ContainsPrefix(x));

            foreach (var key in keyWithNoIncommingValue)
            {
                modelState[key].Errors.Clear();
            }

            if (!modelState.IsValid)
            {
                if (string.IsNullOrEmpty(ViewName))
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new {
                            success = false,
                            errors = ModelStateErrors(modelState)
                        },
                        ContentEncoding = Encoding.UTF8
                    };
                }
                else
                {
                    var result = filterContext.IsChildAction ? new PartialViewResult() : (ViewResultBase)new ViewResult();
                    result.ViewName = this.ViewName;
                    result.ViewData = filterContext.Controller.ViewData;
                    filterContext.Result = result;                        
                }
            }


            base.OnActionExecuting(filterContext);
        }

        static object ModelStateErrors(ModelStateDictionary modelState)
        {
            var q = from ms in modelState
                    where regex.IsMatch(ms.Key)
                    select ms;

            if (q.Any())
            {
                return (from ms in q
                        let keySet = regex.Match(ms.Key).Result("$1-$2").Split('-')
                        select new
                        {
                            idx = int.Parse(keySet[0]),
                            key = keySet[1],
                            values = new KeyValuePair<string, IEnumerable<object>>(keySet[1], ms.Value.Errors.Select(r => r.ErrorMessage))
                        }).GroupBy(x => x.idx, y => y.values)
                        .Select(x => new { detail = x.ToDictionary(k => k.Key, v => v.Value) });
            }

            return new { detail = modelState.ToDictionary(k => k.Key, v => v.Value.Errors.Select(x => x.ErrorMessage)) };
        }
    }
}
