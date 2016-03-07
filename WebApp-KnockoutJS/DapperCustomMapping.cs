using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Citi.MyCitigoldFP.Common.ForDapper
{
    /// <summary>
    /// Dapper.net에서 db column과 object Type 간 mapping용 config.
    /// 
    /// @author: Jeongyong, Jo
    /// @date: 2016-02-02
    /// </summary>
    public class DapperTypeMapConfig
    {
        public static void RegistereTypes(params Type[] targets)
        {
            var types = new List<Type>();

            foreach (var target in targets)
            {
                var ts = from t in target.Assembly.GetTypes()
                         where t.IsClass && t.GetProperties().Any(x => x.GetCustomAttribute<ColumnAttribute>() != null)
                         select t;

                types.AddRange(ts);
            }

            foreach (var type in types)
            {
                var map = (Dapper.SqlMapper.ITypeMap)Activator.CreateInstance(typeof(ColumnAttributeTypeMapper<>).MakeGenericType(type));
                Dapper.SqlMapper.SetTypeMap(type, map);
            }
        }
    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// 컬럼 명.
        /// </summary>
        public string ColmunName { get; private set; }

        public ColumnAttribute(string columnName)
        {
            this.ColmunName = columnName;
        }
    }

    public class ColumnAttributeTypeMapper<T> : ColumnTypeSqlMapper
    {
        public ColumnAttributeTypeMapper()
            : base(new Dapper.SqlMapper.ITypeMap[]{
            new Dapper.CustomPropertyTypeMap(typeof(T), (type, columName)=> 
                type.GetProperties().FirstOrDefault(p=>
                    p.GetCustomAttributes(false).OfType<ColumnAttribute>().Any(attr=>
                        attr.ColmunName.Equals(columName, StringComparison.OrdinalIgnoreCase)))),
            new Dapper.DefaultTypeMap(typeof(T))})
        {
        }
    }

    public class ColumnTypeSqlMapper : Dapper.SqlMapper.ITypeMap
    {
        private readonly IEnumerable<Dapper.SqlMapper.ITypeMap> _mappers;

        public ColumnTypeSqlMapper(IEnumerable<Dapper.SqlMapper.ITypeMap> mappers)
        {
            _mappers = mappers;
        }

        public System.Reflection.ConstructorInfo FindConstructor(string[] names, Type[] types)
        {
            foreach (var mapper in _mappers)
            {
                try
                {
                    var result = mapper.FindConstructor(names, types);
                    if (result != null)
                        return result;
                }
                catch
                {
                }
            }
            return null;
        }

        public System.Reflection.ConstructorInfo FindExplicitConstructor()
        {
            return _mappers.Select(m => m.FindExplicitConstructor()).FirstOrDefault(result => result != null);
        }

        public Dapper.SqlMapper.IMemberMap GetConstructorParameter(System.Reflection.ConstructorInfo constructor, string columnName)
        {
            foreach (var mapper in _mappers)
            {
                try
                {
                    var result = mapper.GetConstructorParameter(constructor, columnName);
                    if (result != null)
                        return result;
                }
                catch
                {
                }
            }
            return null;
        }

        public Dapper.SqlMapper.IMemberMap GetMember(string columnName)
        {
            foreach (var mapper in _mappers)
            {
                try
                {
                    var result = mapper.GetMember(columnName);
                    if (result != null)
                        return result;
                }
                catch
                {
                }
            }
            return null;
        }
    }
}
