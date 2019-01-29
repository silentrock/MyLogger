﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLogger
{
    public class MyLoggerExceptionMiddleWare
    {
        private RequestDelegate _next;

        private MyLoggerExceptionMiddleWareOption _option;


        private MyLogger<MyLoggerExceptionMiddleWare> _logger;

        public MyLoggerExceptionMiddleWare(
            RequestDelegate next, 
            MyLoggerExceptionMiddleWareOption option, 
            MyLogger<MyLoggerExceptionMiddleWare> logger)
        {
            _next = next;
            _option = option;
            _logger = logger;
        }


        public async Task Invoke(HttpContext context)
        {
            Exception exception = null;

            try
            {
                await _next(context);   //调用管道执行下一个中间件
            }
            catch (Exception ex)
            {
                exception = ex;
                context.Response.Clear();
                context.Response.StatusCode = 500;   //发生未捕获的异常，手动设置状态码

                if (_option.ErrorHandingPath.HasValue)
                {
                    context.Request.Path = _option.ErrorHandingPath;
                }
                await _next(context);
            }
            finally
            {
                if (exception !=null)
                    _logger.LogException(exception);
            }
        }
    }
}