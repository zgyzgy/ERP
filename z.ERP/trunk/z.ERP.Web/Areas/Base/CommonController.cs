﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using z.DbHelper.DbDomain;
using z.ERP.Services;
using z.Extensions;
using z.MVC5.Results;
using z.WebPage;

namespace z.ERP.Web.Areas.Base
{
    public class CommonController : BaseController
    {
        public CommonController()
        {
        }

        public DataGridResult Search(string Service, string Method)
        {
            Type type = service.GetType();
            PropertyInfo propertyInfo = type.GetProperty(Service);
            if (propertyInfo == null)
                throw new Exception($"无效的Service:{Service}");
            if (!propertyInfo.PropertyType.BaseOn<ServiceBase>())
                throw new Exception($"Service:{Service}不继承于ServiceBase");
            ServiceBase list = propertyInfo.GetValue(service, null) as ServiceBase;
            MethodInfo mi = propertyInfo.PropertyType.GetMethod(Method);
            if (mi == null)
                throw new Exception($"无效的Method:{Method}");
            if (!mi.ReturnType.BaseOn<UIResult>())
                throw new Exception($"Method:{Method}返回值错误");
            var d = mi.Invoke(list, null) as DataGridResult;
            return d;
        }
        
    }
}