﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace z.Extensions
{
    public static class ObjectExtension
    {
        #region 序列化
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(this object obj, bool simple = false)
        {
            return JsonConvert.SerializeObject(obj, simple ? Formatting.Indented : Formatting.None);
        }

        /// <summary>
        /// 反序列化json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T ToObj<T>(this string str)
        {
            return JsonConvert.DeserializeObject<T>(str);
        }

        /// <summary>
        /// 深度拷贝
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepClone<T>(this object obj)
        {
            return obj.ToJson().ToObj<T>();
        }

        /// <summary>
        /// 获取实体类的str形式
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToCommonString(this object obj)
        {
            return obj.ToCommonString("\r\n", "=", true, true, true);
        }

        /// <summary>
        /// 使用特定方式输出字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="endstr">分隔符</param>
        /// <param name="midstr">键值对分隔符(仅当显示键和值时有效</param>
        /// <param name="HasNull">输出空值</param>
        /// <param name="hasname">输出键名</param>
        /// <param name="hasvalue">输出值</param>
        /// <returns></returns>
        public static string ToCommonString(this object obj, string endstr, string midstr, bool HasNull, bool hasname, bool hasvalue)
        {
            List<string> strlist = new List<string>();
            PropertyInfo[] props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in props)
            {
                string typename = p.Name;
                string value = p.GetValue(obj, null) == null ? "" : p.GetValue(obj, null).ToString();
                if (HasNull || !string.IsNullOrEmpty(value))
                {
                    if (hasname && hasvalue)
                    {
                        strlist.Add(typename + midstr + value);
                    }
                    else if (hasvalue)
                    {
                        strlist.Add(value);
                    }
                    else if (hasname)
                    {
                        strlist.Add(typename);
                    }
                }
            }
            return String.Join(endstr, strlist);
        }

        /// <summary>
        /// 序列化为字典集
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> ToDictionary(this object obj)
        {
            return obj.ToDictionary<string>();
        }

        /// <summary>
        /// 序列化为字典集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Dictionary<string, T> ToDictionary<T>(this object obj) where T : class
        {
            Dictionary<string, T> dic = new Dictionary<string, T>();
            List<string> strlist = new List<string>();
            PropertyInfo[] props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in props)
            {
                string typename = p.Name;
                T value = p.GetValue(obj, null) == null ? default(T) : p.GetValue(obj, null) as T;
                dic.Add(typename, value);
            }
            return dic;
        }
        #endregion
        #region 继承
        /// <summary>
        /// 继承于此类
        /// </summary>
        /// <param name="T1"></param>
        /// <param name="T2"></param>
        /// <returns></returns>
        public static bool BaseOn(this Type T1, Type T2)
        {
            if (T1 == T2)
            {
                return true;
            }
            else
            {
                if (T1.BaseType == null)
                {
                    return false;
                }
                else
                    return T1.BaseType.BaseOn(T2);
            }
        }

        /// <summary>
        /// 继承于此类
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="T1"></param>
        /// <returns></returns>
        public static bool BaseOn<T2>(this Type T1)
        {
            return T1.BaseOn(typeof(T2));
        }
        #endregion
        #region 反射
        /// <summary>  
        /// 根据属性名获取属性值  
        /// </summary>  
        /// <typeparam name="T">对象类型</typeparam>  
        /// <param name="t">对象</param>  
        /// <param name="name">属性名</param>  
        /// <returns>属性的值</returns>  
        public static object GetPropertyValue<T>(this T t, string name)
        {
            Type type = t.GetType();
            PropertyInfo p = type.GetProperty(name);
            if (p == null)
            {
                throw new Exception($"类型{t.GetType().Name}没有名为{name}的属性");
            }
            var param_obj = Expression.Parameter(typeof(T));
            var param_val = Expression.Parameter(typeof(object));

            //转成真实类型，防止Dynamic类型转换成object  
            var body_obj = Expression.Convert(param_obj, type);

            var body = Expression.Property(body_obj, p);
            var getValue = Expression.Lambda<Func<T, object>>(body, param_obj).Compile();
            return getValue(t);
        }

        /// <summary>  
        /// 根据属性名称设置属性的值  
        /// </summary>  
        /// <typeparam name="T">对象类型</typeparam>  
        /// <param name="t">对象</param>  
        /// <param name="name">属性名</param>  
        /// <param name="value">属性的值</param>  
        public static void SetPropertyValue<T>(this T t, string name, object value)
        {
            Type type = t.GetType();
            PropertyInfo p = type.GetProperty(name);
            if (p == null)
            {
                throw new Exception($"类型{t.GetType().Name}没有名为{name}的属性");
            }
            var param_obj = Expression.Parameter(type);
            var param_val = Expression.Parameter(typeof(object));
            var body_obj = Expression.Convert(param_obj, type);
            var body_val = Expression.Convert(param_val, p.PropertyType);

            //获取设置属性的值的方法  
            var setMethod = p.GetSetMethod(true);

            //如果只是只读,则setMethod==null  
            if (setMethod != null)
            {
                var body = Expression.Call(param_obj, p.GetSetMethod(), body_val);
                var setValue = Expression.Lambda<Action<T, object>>(body, param_obj, param_val).Compile();
                setValue(t, value);
            }
        }
        #endregion
    }
}
