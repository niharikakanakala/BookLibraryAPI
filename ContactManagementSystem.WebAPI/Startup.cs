using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ContactManagementSystem.WebAPI.Data;
//using ContactManagementSystem.WebAPI.IServices;
using ContactManagementSystem.WebAPI.Services;

namespace ContactManagementSystem.WebAPI
{
    public class Startup
    {
        public const string DatabaseFileName = "contact_database.db";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ContactContext>(options => options.UseLazyLoadingProxies().UseSqlite($"Data Source={DatabaseFileName}"));
            services.AddTransient<IContactService, ContactService>();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
