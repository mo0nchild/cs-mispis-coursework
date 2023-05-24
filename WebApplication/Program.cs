using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Security.Claims;
using WebApplication.Models;
using WebApplication.Pages;
using WebApplication.Services;

namespace WebApplication
{
    using ASPNETBuilder = Microsoft.AspNetCore.Builder;
    public partial class Program : object
    {
        public static void Main(string[] args)
        {
            var builder = ASPNETBuilder::WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages(); builder.Services.AddControllers();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/authorization");
                options.LoginPath = new PathString("/authorization");
            });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Client", item => item.RequireClaim(ClaimTypes.Role, IndexMode.Client.ToString()));
                options.AddPolicy("Provider", item => item.RequireClaim(ClaimTypes.Role, IndexMode.Provider.ToString()));

                options.AddPolicy("Delivery", item => item.RequireClaim(ClaimTypes.Role, IndexMode.Delivery.ToString()));
                options.AddPolicy("Manager", item => item.RequireClaim(ClaimTypes.Role, IndexMode.Manager.ToString()));
                options.AddPolicy("Builder", item => item.RequireClaim(ClaimTypes.Role, IndexMode.Builder.ToString()));
            });
            builder.Services.AddDbContextFactory<MispisCourseworkContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddTransient<IProfileService, ProfileService>();

            var application = builder.Build();
            application.Map("/", (HttpContext context) => Results.LocalRedirect("/profile/login"));

            application.UseHttpsRedirection().UseStaticFiles();
            application.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Icons")),
                RequestPath = new PathString("/icons")
            });
            application.UseRouting().UseAuthentication().UseAuthorization();

            application.MapControllers(); application.MapRazorPages();
            application.Run();
        }
        public Program() : base() { }
    }
}