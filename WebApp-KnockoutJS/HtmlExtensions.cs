/**
 * Copyright(c) 2016 CiTi Bank, Inc. All Rights Reserved.
 * 
 * ASP.NET MVC Html Customized Extension module.
 * 
 * @author: Jeongyong, Jo
 * @since : 2016-01-29
 * 
 */

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Web.WebPages;
using System.Web.Helpers;
using System.Web.Optimization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;


namespace Citi.MyCitigoldFP.Web
{
    /// <summary>
    /// Html 표현을 위한 확장 기능.
    /// 
    /// @author: Jeongyong, Jo
    /// @date: 016-01-25
    /// </summary>
    public static class HtmlExtensions
    {
        /// <summary>
        /// 열거타입을 Html.DropDownList로 사용할 수 있도록 변환한다.
        /// 열거자의 값은 key(ID)로 사용되고, DescriptionAttribute로 선언된 구문은 View Text로 사용된다.
        /// => SampleEnum 샘플 참조.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 016-01-25
        /// </summary>
        /// <typeparam name="TEnum">타입 열거자</typeparam>
        /// <param name="src">대상 열거자.</param>
        /// <returns><see cref="SelectList"/></returns>
        public static SelectList ToSelectList<TEnum>(this TEnum src) 
            where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var values = from TEnum e in Enum.GetValues(typeof(TEnum))
                         let desc = e.GetType().GetMember(e.ToString()).Select(x=>x.GetCustomAttribute<DescriptionAttribute>()).FirstOrDefault()
                         select new { Id = e, Name = desc == null ? e.ToString() : desc.Description };

            return new SelectList(values, "Id", "Name", src);
        }

        /*
        /// <summary>
        /// 타입 멤버에 선언된 DescriptionAttribute에 구문을 추출하거나 없는 경우 멤버 이름을 반환한다.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 2016-01-25
        /// </summary>
        /// <typeparam name="TEnum">타입 열겨자.</typeparam>
        /// <param name="src">대상 열거자.</param>
        /// <returns>문자열.</returns>
        [Obsolete("Citi.MyCitigoldFP.Common 모듈로 이전되어 추후 제거 예정.")]
        public static string GetDescription<TEnum>(this TEnum src)
            where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var desc = src.GetType().GetMember(src.ToString()).Select(x => x.GetCustomAttribute<DescriptionAttribute>()).FirstOrDefault();
            return desc == null ? src.ToString() : desc.Description;
        }
        */


        /// <summary>        
        /// View Page 표시 영역에 렌더링할 javascript를 등록한다.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 2016-01-25
        /// </summary>
        /// <param name="htmlHelper"><see cref="HtmlHelper"/></param>
        /// <param name="template">javascript content template.</param>
        /// <returns><see cref="MvcHtmlString"/></returns>
        public static MvcHtmlString Scripts(this HtmlHelper htmlHelper, Func<object, HelperResult> template, string key = null)
        {
            htmlHelper.ViewContext.HttpContext.Items["_script_" + (key ?? Guid.NewGuid().ToString())] = template;
            return MvcHtmlString.Empty;
        }

        /// <summary>
        /// 지정된 View page 표시 영역에 javascript를 렌더링한다.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 2016-01-25
        /// </summary>
        /// <param name="htmlhelper"><see cref="HtmlHelper"/></param>
        /// <returns><see cref="IHtmlString"/></returns>
        public static IHtmlString RenderScripts(this HtmlHelper htmlhelper)
        {
            var items = htmlhelper.ViewContext.HttpContext.Items;
            foreach (object key in items.Keys)
            {
                if (key.ToString().StartsWith("_script_"))
                {
                    var template = items[key] as Func<object, HelperResult>;
                    htmlhelper.ViewContext.Writer.Write(template(null) + Environment.NewLine);
                }
            }
            return MvcHtmlString.Empty;
        }

        static Regex ScriptTagRegex = new Regex(@"<(?:([^:]+:|\/))?script.*\>", RegexOptions.Compiled);
        static ConcurrentDictionary<string, string> _jsMinifyCache = new ConcurrentDictionary<string, string>();

        public static IHtmlString RenderBundleScripts(this HtmlHelper htmlhelper, bool isEnable = false)
        {
            var items = htmlhelper.ViewContext.HttpContext.Items;
            foreach (object key in items.Keys)
            {
                if (key.ToString().StartsWith("_script_"))
                {
                    if (!isEnable)
                    {
                        var template = items[key] as Func<object, HelperResult>;
                        htmlhelper.ViewContext.Writer.Write(template(null) + Environment.NewLine);
                    }
                    else
                    {
                        if (!_jsMinifyCache.ContainsKey(key.ToString()))
                        {
                            var template = items[key] as Func<object, HelperResult>;

                            var jsPath = AppDomain.CurrentDomain.BaseDirectory + key + ".js";
                            var virtualPath = "~/" + key + ".js";

                            File.WriteAllText(jsPath, ScriptTagRegex.Replace(template(null) + string.Empty, string.Empty), Encoding.UTF8);

                            BundleTable.Bundles.Add(new ScriptBundle(virtualPath).Include(virtualPath));
                            var bundleContext = new BundleContext(htmlhelper.ViewContext.HttpContext, BundleTable.Bundles, virtualPath);
                            var bundle = BundleTable.Bundles.Single(x => x.Path == virtualPath);
                            var bundleResponse = bundle.GenerateBundleResponse(bundleContext);

                            _jsMinifyCache.TryAdd(key.ToString(), bundleResponse.Content);

                            File.Delete(jsPath);
                        }

                        string minifiedJs;
                        if (_jsMinifyCache.TryGetValue(key.ToString(), out minifiedJs))
                        {
                            htmlhelper.ViewContext.Writer.Write(
                                string.Format("<script type=\"text/javascript\">{1}{0}{1}</script>{1}", minifiedJs, Environment.NewLine));
                        }
                        else
                        {
                            var template = items[key] as Func<object, HelperResult>;
                            htmlhelper.ViewContext.Writer.Write(template(null) + Environment.NewLine);
                        }
                    }
                }
            }
            return MvcHtmlString.Empty;
        }


        /// <summary>
        /// 서버에서 발생한 모델 기반 유효 검사 에러 메세지를 javascript alert으로 출력한다.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 2016-03-14
        /// </summary>
        /// <typeparam name="T">view 바인딩 타입.</typeparam>
        /// <param name="htmlHelper"><see cref="HtmlHelper"/></param>
        /// <param name="viewData"><see cref="ViewDataDictionary`T"/></param>
        /// <returns></returns>
        public static IHtmlString ValidateResultAlert<T>(this HtmlHelper htmlHelper, ViewDataDictionary<T> viewData, bool includeScriptBlock = true)
        {
            var modelState = viewData.ModelState;
            if (!modelState.IsValid)
            {
                string script = string.Empty;
                if (includeScriptBlock)
                    script = @"<script type=""text/javascript"">
    alert({0});
</script>";
                else
                    script = "alert({0})";

                var errors = modelState.Where(x => x.Value.Errors.Count > 0)
                                       .SelectMany(x => x.Value.Errors)
                                       .Select(err => err.ErrorMessage);

                htmlHelper.ViewContext.Writer.Write(string.Format(script, htmlHelper.Raw(Json.Encode(String.Join(Environment.NewLine, errors)))));
            }
            return MvcHtmlString.Empty;
        }

        /// <summary>
        /// Html.BeginForm() 확장 메서드.
        /// Optional parameters로 정의하여 선택적으로 전달할 수 있도록 추가.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 2016-01-25
        /// </summary>
        /// <param name="htmlHelper"><see cref="HtmlHelper"/></param>
        /// <param name="actionName">Action method name.</param>
        /// <param name="controllerName">Controller class name.</param>
        /// <param name="htmlAttributes">html attributes.</param>
        /// <returns><see cref="IDisposable"/></returns>
        public static IDisposable BeginForm(this HtmlHelper htmlHelper, string actionName = null, string controllerName = null, object htmlAttributes = null)
        {
            return System.Web.Mvc.Html.FormExtensions.BeginForm(htmlHelper, actionName, controllerName, FormMethod.Post, htmlAttributes);
        }
    }
}
