using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Citi.MyCitigoldFP.Web
{
    public class DecimalModelbinder : IModelBinder
    {
        static Regex regex = new Regex(@"[^\d]", RegexOptions.Compiled | RegexOptions.Singleline);

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            //if (string.IsNullOrEmpty(valueResult.AttemptedValue))
            //{
            //    return 0m;
            //}
            var modelState = new ModelState { Value = valueResult };
            object actualValue = null;
            if (!string.IsNullOrEmpty(valueResult.AttemptedValue))
            {
                try
                {
                    actualValue = Convert.ToDecimal(regex.Replace(valueResult.AttemptedValue, string.Empty), CultureInfo.InvariantCulture);
                }
                catch (FormatException ex)
                {
                    modelState.Errors.Add(ex);
                }
            }
            bindingContext.ModelState.Add(bindingContext.ModelName, modelState);
            return actualValue;
        }
    }   
}
