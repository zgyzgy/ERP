﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using z;
using z.Extensions;

namespace z.DbHelper.DbDomain
{
    /// <summary>
    /// 所有数据操作类的基类
    /// </summary>
    public class EntityBase
    {
        /// <summary>
        /// 获取表的名字
        /// </summary>
        /// <returns></returns>
        public string GetTableName()
        {
            return this.GetAttribute<DbTableAttribute>()?.Tablename;
        }

        /// <summary>
        /// 获取表的中文名字
        /// </summary>
        /// <returns></returns>
        public string GetComments()
        {
            string str = this.GetAttribute<DbTableAttribute>()?.Tabcomments;
            if (str.IsEmpty())
                str = GetTableName();
            return str;
        }

        /// <summary>
        /// 字段是主键
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsPrimaryKey<TEntity>(Expression<Func<TEntity, string>> p)
        {
            if (p.Body is MemberExpression)
            {
                MemberExpression me = p.Body as MemberExpression;
                PropertyInfo prop = me.Member as PropertyInfo;
                PrimaryKeyAttribute f = prop.GetAttribute<PrimaryKeyAttribute>();
                return f != null;
            }
            else
                throw new Exception("属性类型不正确");
        }

        /// <summary>
        /// 字段中文名称
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public string FieldName<TEntity>(Expression<Func<TEntity, string>> p)
        {
            if (p.Body is MemberExpression)
            {
                MemberExpression me = p.Body as MemberExpression;
                PropertyInfo prop = me.Member as PropertyInfo;
                FieldAttribute f = prop.GetAttribute<FieldAttribute>();
                string fieldname = me.Member.Name;
                if (f != null)
                    fieldname = f.Fieldname;
                return fieldname;
            }
            else
                throw new Exception("属性类型不正确");
        }

        /// <summary>
        /// 获取主键
        /// </summary>
        /// <returns></returns>
        public PropertyInfo[] GetPrimaryKey()
        {
            return GetType().GetProperties()
                .Where(a => a.GetAttribute<PrimaryKeyAttribute>() != null)
                .ToArray();
        }

        /// <summary>
        /// 获取所有字段
        /// </summary>
        /// <returns></returns>
        public PropertyInfo[] GetAllField()
        {
            return GetType().GetProperties().ToArray();
        }

        /// <summary>
        /// 获取所有不是主键的字段
        /// </summary>
        /// <returns></returns>
        public PropertyInfo[] GetFieldWithoutPrimaryKey()
        {
            return GetType().GetProperties()
                .Where(a => a.GetAttribute<PrimaryKeyAttribute>() == null)
                .ToArray();
        }

    }

}
