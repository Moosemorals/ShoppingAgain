using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShoppingAgain.Contexts;
using ShoppingAgain.Events;
using ShoppingAgain.Services;

namespace ShoppingAgain
{
    public class Startup
    { 
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Env { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            this.Env = env;

            using var shopingDB = new ShoppingContext();
            shopingDB.Database.EnsureCreated();
            shopingDB.Database.Migrate();

            using var eventsDB = new EventContext();
            eventsDB.Database.EnsureCreated();
            eventsDB.Database.Migrate();
        } 

        public void ConfigureServices(IServiceCollection services)
        {
            // Setup DI
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<ShoppingService>();

            // Setup MVC
            IMvcBuilder builder = services.AddMvc(options => options.EnableEndpointRouting = false);
            if (Env.IsDevelopment())
            {
                builder.AddRazorRuntimeCompilation();
            }

            // Setup Database
            services.AddEntityFrameworkSqlite()
                .AddDbContext<ShoppingContext>()
                .AddDbContext<EventContext>();
        }

        public void Configure(IApplicationBuilder app)
        {

            if (Env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Add CSP header
            app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; ");
                await next();
            });

            // Serve static files from wwwroot
            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
