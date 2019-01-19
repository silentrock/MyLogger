﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyLogger;

namespace MyLoggerTestApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IServiceCollection Services { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            Services = services;

            Services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            Services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //===================================================================================
            Services.Configure<MyLoggerOption>(Configuration.GetSection("MyLoggerOption"));
            //如果想使用IMyLogger的扩展则通过使用AddMyLogger
            services.AddMyLogger();
            //设置系统日志输出的最小级别
            Services.AddLogging( builder =>
            {
                builder
                    //.AddConfiguration(Configuration.GetSection("MyLoggerOption"))
                    // filter for all providers
                    //.AddFilter("System", LogLevel.Debug)
                    // Only for Debug logger, using the provider type or it's alias
                    //.AddFilter("Debug", "System", LogLevel.Information)
                    // Only for Console logger by provider type
                    .AddFilter<MyLoggerProvider>("Microsoft", LogLevel.Information)
                    .AddConsole()
                    .AddDebug();
            });
            //===================================================================================
            return Services.BuildServiceProvider();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            //===================================================================================
            //如果想开启系统日志输出到IMyLogger则使用以下代码
            var option = Services.BuildServiceProvider().GetService< IOptions<MyLoggerOption>>();
            loggerFactory.UseMyLogger(Services);//extension方式添加MyLogger，两种写法效果等同
            //loggerFactory.AddProvider(new MyLoggerProvider(option));
            //===================================================================================

            app.UseMvc(routes =>    
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
