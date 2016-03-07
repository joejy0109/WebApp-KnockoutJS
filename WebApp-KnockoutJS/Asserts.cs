using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Citi.MyCitigoldFP.Common.Utility
{
    /// <summary>
    /// 예외 처리 Helper.
    /// 
    /// @author: Jeongyong, Jo
    /// @date: 2016-02-04
    /// </summary>
    public class Asserts
    {
        /// <summary>
        /// 익명의 타입으로 제공된 항목에 대한 null검사를 수행한다.
        /// 
        /// @author: jeongyong, Jo
        /// @date: 2016-02-04
        /// </summary>
        /// <typeparam name="T">타입.</typeparam>
        /// <param name="item">항목이 할당된 익명의 객체.</param>
        public static void ValidateAnonymous<T>(T item)
        {
            var typ = typeof(T);

            //if (!typ.Name.Contains("AnonymousType"))
            //    throw new InvalidOperationException("익명의 타입만 지원합니다.");
            if (!IsAnonymousType(typ))
            {
                if (item == null)
                    throw new ArgumentNullException(typ.Name);
                return;
            }

            foreach (var prop in typeof(T).GetProperties())
            {
#if DEBUG
                Debug.WriteLine("Name:{0}, Value:{1}", prop.Name, prop.GetValue(item));
#endif
                if (prop.PropertyType == typeof(String))
                {
                    if (string.IsNullOrEmpty(prop.GetValue(item) as string))
                        throw new MfpException(prop.Name, prop.GetValue(item));
                }
                if (prop.GetValue(item) == null)
                    throw new MfpException(prop.Name, prop.GetValue(item));
            }
        }

        /// <summary>
        /// 선택적 항목에 대하여 Null검사를 수행하고, null 값 발견시 <see cref="ValidateAnonymous"/>을 throw한다.        
        /// 현재 Property 멤버만 지원됨.
        /// 
        /// @author: jeongyong, jo
        /// @date: 2014-02-04
        /// </summary>
        /// <typeparam name="T">대상 타입.</typeparam>
        /// <param name="src">검사할 object.</param>
        /// <param name="targets">검사 object의 항목(s).</param>        
        public static void ValidateModel<T>(T src, params Expression<Func<T, object>>[] targets)
        {
            if (src == null)
                throw new ArgumentNullException(typeof(T).Name);

            foreach (var target in targets)
            {
                var memberExpr = GetMemberExpr(target.Body);
                var name = memberExpr.Member.Name;
                var val = target.Compile()(src);

                foreach (var attr in memberExpr.Member.GetCustomAttributes())
                {
                    var validationAttr = attr as ValidationAttribute;
                    if (validationAttr != null && !validationAttr.IsValid(val))
                        throw new MfpException(validationAttr.ErrorMessage, val);
                }
            }
        }

        private static readonly Type[] RangeTypes = new[] { typeof(short),typeof(int),typeof(long),typeof(double) };
        public static void Range<T>(object value, T min, T max, string paramName, string errorMessage = null) where T : struct
        {
            if (!RangeTypes.Contains(typeof(T)))
                throw new TypeAccessException("허용되지 않는 타입 입니다.");

            var validator = new RangeAttribute(typeof(T), min.ToString(), max.ToString());
            if (!validator.IsValid(value))
                throw new MfpException(errorMessage?? paramName + " 값이 허용된 범위를 벗어났습니다.", value);
        }

        public static void Required(object value, string paramName, string errorMessage = null)
        {
            var validator = new RequiredAttribute();
            if (!validator.IsValid(value))
                throw new MfpException(errorMessage ?? paramName + "은 필수 입력 값입니다.", value);
        }

        public static void Regex(object value, string paramName, string pattern, string errorMessage = null)
        {
            var validator = new RegularExpressionAttribute(pattern);
            if (!validator.IsValid(value))
                throw new MfpException(errorMessage ?? paramName + " 항목이 규칙에 부합하지 않습니다", value);
        }
        /// <summary>
        /// 선택적 항목에 대하여 Null검사를 수행하고, null 값 발견시 <see cref="ValidateAnonymous"/>을 throw한다.        
        /// 현재 Property 멤버만 지원됨.
        /// 
        /// @author: jeongyong, jo
        /// @date: 2014-02-04
        /// </summary>
        /// <typeparam name="T">대상 타입.</typeparam>
        /// <param name="src">검사할 object.</param>
        /// <param name="targets">검사 object의 항목(s).</param>
        public static void Validate<T>(T src, params string[] names)
        {
            if (src == null)
                throw new ArgumentNullException(typeof(T).Name);

            var typ = typeof(T);

            foreach (PropertyInfo pi in typ.GetProperties().Where(x => names.Contains(x.Name, StringComparer.OrdinalIgnoreCase)))
            {
                var val = typ.GetProperty(pi.Name).GetValue(src).ToString();

                if (pi.PropertyType == typeof(string))
                {
                    if (string.IsNullOrEmpty(val)) throw new MfpException(pi.Name, val);
                }
                else if (pi.PropertyType == typeof(Nullable) || pi.PropertyType == typeof(Nullable<>))
                {
                    if (val == null) throw new MfpException(pi.Name, val);
                }
#if DEBUG
                Debug.WriteLine("Type->{0} - {1}: {2}", pi.PropertyType, pi.Name, typ.GetProperty(pi.Name).GetValue(src));
#endif
            }
        }

        /// <summary>
        /// 지정 타입이 익명 타입인지 검사한다.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 2016-02-04
        /// </summary>
        /// <param name="typ">타입.</param>
        /// <returns>익명 타입 여부.</returns>
        static bool IsAnonymousType(Type typ)
        {
            if (typ.IsGenericType)
            {
                var def = typ.GetGenericTypeDefinition();
                if (def.IsClass && def.IsSealed && def.Attributes.HasFlag(TypeAttributes.NotPublic))
                {
                    var attrs = def.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);
                    if (attrs != null && attrs.Length > 0) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 람다 식(Lamda Expression)으로 제공된 델리게이트 식의 대상 멤버의 이름을 가져온다.
        /// 
        /// @author: jeongyong, jo
        /// @date: 2014-02-04 
        /// </summary>
        /// <param name="expr">Expression.</param>
        /// <returns>타입 멤버 이름.</returns>
        static string GetMemberName(Expression expr)
        {
            if (expr is MemberExpression)
            {
                var memExpr = (MemberExpression)expr;
                if (memExpr.Expression.NodeType == ExpressionType.MemberAccess)
                    return GetMemberName(memExpr.Expression) + "." + memExpr.Member.Name;
                return memExpr.Member.Name;
            }
            if (expr is UnaryExpression)
            {
                var unaryExpr = (UnaryExpression)expr;
                return GetMemberName(unaryExpr.Operand);
            }
            return null;
        }

        /// <summary>
        /// 람다 식(Lamda Expression)의 실제 타입 expression 정보를 가져온다..
        /// 
        /// @author: jeongyong, jo
        /// @date: 2014-02-04
        /// </summary>
        /// <param name="expr">expression.</param>
        /// <returns><see cref="MemberExpression"/></returns>
        static MemberExpression GetMemberExpr(Expression expr)
        {
            if (expr is MemberExpression)
            {
                var memExpr = (MemberExpression)expr;
                if (memExpr.Expression.NodeType == ExpressionType.MemberAccess)
                    return GetMemberExpr(memExpr.Expression);
                return memExpr;
            }
            if (expr is UnaryExpression)
            {
                var unaryExpr = (UnaryExpression)expr;
                return GetMemberExpr(unaryExpr.Operand);
            }
            return null;
        }
    }
}
