﻿using Furion.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Furion.Localization
{
    /// <summary>
    /// 全局多语言静态类
    /// </summary>
    [SkipScan]
    public static class L
    {
        /// <summary>
        /// 语言类型
        /// </summary>
        public readonly static Type LangType;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static L()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            LangType = entryAssembly.GetType($"{entryAssembly.GetName().Name}.Lang")
                ?? throw new InvalidOperationException("`Lang.cs` type not found in entry assembly.");
        }

        /// <summary>
        /// String 多语言
        /// </summary>
        public static IStringLocalizer @Text => App.GetService<IStringLocalizerFactory>().Create(LangType);

        /// <summary>
        /// Html 多语言
        /// </summary>
        public static IHtmlLocalizer @Html => App.GetService<IHtmlLocalizerFactory>().Create(LangType);

        /// <summary>
        /// 设置多语言区域
        /// </summary>
        /// <param name="culture"></param>
        public static void SetCulture(string culture)
        {
            var httpContext = App.HttpContext;
            if (httpContext == null) return;

            httpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
        }

        /// <summary>
        /// 获取系统提供的语言列表
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetCultures()
        {
            var httpContext = App.HttpContext;
            if (httpContext == null) return new Dictionary<string, string>();

            // 获取请求本地特性选项
            var locOptions = httpContext.RequestServices.GetService<IOptions<RequestLocalizationOptions>>().Value;

            // 获取请求特性
            var requestCulture = httpContext.Features.Get<IRequestCultureFeature>();

            // 获取语言符号和名称
            var cultureItems = locOptions.SupportedUICultures
                .ToDictionary(u => u.Name, u => u.DisplayName);

            return cultureItems;
        }
    }
}