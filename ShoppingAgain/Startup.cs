using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        private static RNGCryptoServiceProvider RNG = new RNGCryptoServiceProvider();

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
            services.AddSingleton<EventService, EventService>();
            services.AddScoped<ShoppingService>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                        .AddCookie();

            services.AddHttpContextAccessor();

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

        private string BuildNonce(int chars)
        {
            byte[] raw = new byte[chars];
            RNG.GetBytes(raw);

            return System.Convert.ToBase64String(raw);
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
                string n = BuildNonce(12);
                ctx.Items.Add("script-nonce", n);
                ctx.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' 'nonce-" + n + "';");
                await next();
            });

            // Add authentication
            app.UseAuthentication();

            // Serve static files from wwwroot
            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
