using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShoppingAgain.Contexts;
using ShoppingAgain.Services;

namespace ShoppingAgain
{
    public class Startup
    {

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment env { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            this.env = env;

            using var db = new ShoppingContext();
            db.Database.EnsureCreated();
            db.Database.Migrate(); 
        } 

        public void ConfigureServices(IServiceCollection services)
        {
            // Setup DI
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddScoped<ShoppingContext>();
            services.AddScoped<ShoppingService>();

            // Setup MVC
            IMvcBuilder builder = services.AddMvc();
            if (env.IsDevelopment())
            {
                builder.AddRazorRuntimeCompilation();
            }

            // Setup Database
            services.AddEntityFrameworkSqlite().AddDbContext<ShoppingContext>();
        }

        public void Configure(IApplicationBuilder app)
        {

            if (env.IsDevelopment())
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


            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
