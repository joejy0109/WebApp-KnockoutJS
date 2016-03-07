using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace WebApp-KnockoutJS
{
    /// <summary>
    /// 확장 기능을 위한 클래스
    /// 2016-02-02
    /// </summary>
    public static class ExtensionsHelper
    {
        /// <summary>
        /// 열거자 멤버에 선언된 DescriptionAttribute에 구문을 추출하거나 없는 경우 멤버 이름을 문자열로 반환한다.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 2016-02-02
        /// </summary>
        /// <typeparam name="TEnum">타입 열겨자.</typeparam>
        /// <param name="src">대상 열거자.</param>
        /// <returns>문자열.</returns>
        public static string GetDescription<TEnum>(this TEnum src)
            where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var desc = src.GetType()
                .GetMember(src.ToString())
                .Select(x => x.GetCustomAttribute<DescriptionAttribute>())
                .FirstOrDefault();

            return desc == null ? src.ToString() : desc.Description;
        }

        /// <summary>
        /// 수의 지정된 자릿수만큼 올림 처리한다.
        /// 
        /// @author: Jeongyong, Jo
        /// @date: 2015-02-17
        /// </summary>
        /// <param name="value">입력 수</param>
        /// <param name="figures">올림 처리할 자릿수.</param>
        /// <returns>올림처리된 <paramref name="value"/> 값.</returns>
        public static decimal TypeCeiling(this decimal value, int figures)
        {
            decimal result = Math.Ceiling(value);
            for (int i = 1; i <= figures; i++)
                result = Math.Ceiling(result / (decimal)Math.Pow(10, i)) * (decimal)Math.Pow(10, i);
            return result;
        }

        /// <summary>
        /// <see cref="IEnumerable<T>"/>컬렉션에 대한 ForEach 확장 메서드.
        /// @author: Jeongyong, Jo
        /// @date: 2015-02-23
        /// </summary>
        /// <typeparam name="T">지정된 타입.</typeparam>
        /// <param name="source">컬렉션 소스.</param>
        /// <param name="action">처리할 식.</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        static IDictionary<Type, Delegate> _cachedIL = new Dictionary<Type, Delegate>();
        static IDictionary<Type, Delegate> _cachedDeepIL = new Dictionary<Type, Delegate>();

        public static T Copy<T>(this T obj)
        {
            Delegate execution;

            if (!_cachedIL.TryGetValue(typeof(T), out execution))
            {
                var dymMethod = new DynamicMethod("DoClone", typeof(T), new Type[] { typeof(T) }, true);
                var cinfo = obj.GetType().GetConstructor(new Type[] { });
                var g = dymMethod.GetILGenerator();

                var lbf = g.DeclareLocal(typeof(T));

                g.Emit(OpCodes.Newobj, cinfo);
                g.Emit(OpCodes.Stloc_0);

                var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var field in fields)
                {
                    g.Emit(OpCodes.Ldloc_0);
                    g.Emit(OpCodes.Ldarg_0);
                    g.Emit(OpCodes.Ldfld, field);
                    g.Emit(OpCodes.Stfld, field);
                }

                g.Emit(OpCodes.Ldloc_0);
                g.Emit(OpCodes.Ret);

                execution = dymMethod.CreateDelegate(typeof(Func<T, T>));

                _cachedIL.Add(typeof(T), execution);
            }

            return ((Func<T, T>)execution)(obj);
        }

        public static T DeepCopy<T>(this T obj)
        {
            Delegate execution;

            if (!_cachedDeepIL.TryGetValue(typeof(T), out execution))
            {
                var dymMethod = new DynamicMethod("DoClone", typeof(T), new Type[] { typeof(T) }, Assembly.GetExecutingAssembly().ManifestModule, true);
                var cinfo = obj.GetType().GetConstructor(new Type[] { });
                var g = dymMethod.GetILGenerator();

                var lbf = g.DeclareLocal(typeof(T));

                g.Emit(OpCodes.Newobj, cinfo);
                g.Emit(OpCodes.Stloc_0);

                var fields = obj.GetType().GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var fld in fields)
                {
                    if (fld.FieldType.IsValueType || fld.FieldType == typeof(string))
                    {
                        CopyValueType(g, fld);
                    }
                    else if (fld.FieldType.IsClass)
                    {
                        var lbfTemp = g.DeclareLocal(fld.FieldType);
                        CopyReferenceType(g, fld, lbfTemp);
                    }
                }

                g.Emit(OpCodes.Ldloc_0);
                g.Emit(OpCodes.Ret);
                execution = dymMethod.CreateDelegate(typeof(Func<T, T>));
                _cachedIL.Add(typeof(T), execution);
            }

            return ((Func<T, T>)execution)(obj);
        }

        private static void CopyValueType(ILGenerator g, FieldInfo fld)
        {
            g.Emit(OpCodes.Ldloc_0);
            g.Emit(OpCodes.Ldarg_0);
            g.Emit(OpCodes.Ldfld, fld);
            g.Emit(OpCodes.Stfld, fld);
        }

        private static void CopyReferenceType(ILGenerator g, FieldInfo fld, LocalBuilder lbfTemp)
        {
            if (fld.FieldType.GetInterface("IEnumerable") != null)
            {
                if (fld.FieldType.IsGenericType)
                {
                    var argType = fld.FieldType.GetGenericArguments()[0];
                    var gencType = Type.GetType(
                        string.Format("System.Collections.Generic.IEnumerable`1[{0}]", argType.FullName));
                    var ci = fld.FieldType.GetConstructor(new Type[] { gencType });
                    if (ci != null)
                    {
                        g.Emit(OpCodes.Ldarg_0);
                        g.Emit(OpCodes.Ldfld, fld);
                        g.Emit(OpCodes.Newobj, ci);
                        g.Emit(OpCodes.Stloc, lbfTemp);
                        g.Emit(OpCodes.Stfld, fld);

                        g.Emit(OpCodes.Ldloc_0);
                        g.Emit(OpCodes.Ldloc, lbfTemp);
                        g.Emit(OpCodes.Stfld, fld);
                    }
                }
            }
            else
            {
                var ci = fld.FieldType.GetConstructor(new Type[] { });
                g.Emit(OpCodes.Newobj, ci);
                g.Emit(OpCodes.Stloc, lbfTemp);

                g.Emit(OpCodes.Ldloc_0);
                g.Emit(OpCodes.Ldloc, lbfTemp);
                g.Emit(OpCodes.Stfld, fld);

                var refObjFields = fld.FieldType.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var cfld in refObjFields)
                {
                    if (cfld.FieldType.IsValueType || cfld.FieldType == typeof(string))
                    {
                        g.Emit(OpCodes.Ldloc_1);
                        g.Emit(OpCodes.Ldarg_0);
                        g.Emit(OpCodes.Ldfld, fld);
                        g.Emit(OpCodes.Ldfld, cfld);
                        g.Emit(OpCodes.Stfld, cfld);
                    }
                    else
                    {
                        CopyReferenceType(g, cfld, g.DeclareLocal(cfld.FieldType));
                    }
                }
            }
        }
    }
}
