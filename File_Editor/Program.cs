using File_Editor.Controllers;
using Microsoft.AspNetCore.Hosting;

namespace File_Editor
{
    internal class Program
    {
        protected Program()
        {
            
        }
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            CreateUserFoldersIfNotExists(app);

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Accounts}/{action=Login}/{id?}");

            app.Run();
        }

        private static void CreateUserFoldersIfNotExists(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                IWebHostEnvironment webHostEnvironment = services.GetRequiredService<IWebHostEnvironment>();
                string userPath = webHostEnvironment.WebRootPath + AccountsController.USERFOLDER;
                if (!Directory.Exists(userPath))
                {
                    foreach (string userName in AccountsController.Users.Keys)
                    {
                        Directory.CreateDirectory(userPath + userName);
                        File.WriteAllText(userPath + userName + "\\test.txt", "test");
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred creating the local filesystem.");
            }
        }
    }
}