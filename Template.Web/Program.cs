
using Template.Data.Services;
using Template.Data.Extensions;
using Template.Data.Repositories;

namespace Template.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        // Configure Authentication / Authorisation via extension methods 
        builder.Services.AddCookieAuthentication();

        // Add Database Context using extension method
        builder.Services.AddDatabaseContext(builder.Configuration);

        // Add Application Services to DI   
        builder.Services.AddTransient<IUserService, UserServiceDb>();
        builder.Services.AddTransient<IMailService, SmtpMailService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            // seed users in development mode - using service provider to get UserService from DI
            using var scope = app.Services.CreateScope();
            //Seeder.Seed(scope.ServiceProvider.GetService<IUserService>());
            Seeder.SeedDb(scope.ServiceProvider.GetRequiredService<DatabaseContext>());
            //Seeder.SeedDb();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // ** turn on authentication/authorisation **
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
