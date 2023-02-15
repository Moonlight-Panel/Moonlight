using BlazorTable;
using CurrieTechnologies.Razor.SweetAlert2;
using Moonlight.App.Database;
using Moonlight.App.Helpers;
using Moonlight.App.Repositories;
using Moonlight.App.Repositories.Servers;
using Moonlight.App.Services;
using Moonlight.App.Services.Interop;
using Moonlight.App.Services.Sessions;

namespace Moonlight
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddHttpContextAccessor();
            
            // Databases
            builder.Services.AddDbContext<DataContext>();
            
            // Repositories
            builder.Services.AddSingleton<SessionRepository>();
            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<NodeRepository>();
            builder.Services.AddScoped<ServerRepository>();
            builder.Services.AddScoped<ServerBackupRepository>();
            
            // Services
            builder.Services.AddScoped<TranslationService>();
            builder.Services.AddSingleton<ConfigService>();
            builder.Services.AddScoped<CookieService>();
            builder.Services.AddScoped<IdentityService>();
            builder.Services.AddScoped<IpLocateService>();
            builder.Services.AddScoped<SessionService>();
            builder.Services.AddScoped<TranslationService>();
            builder.Services.AddScoped<AlertService>();
            
            // Helpers
            builder.Services.AddSingleton<TranslationHelper>();
            
            // Third party services

            builder.Services.AddBlazorTable();
            builder.Services.AddSweetAlert2(options => { options.Theme = SweetAlertTheme.Dark; });
            builder.Services.AddBlazorContextMenu();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}