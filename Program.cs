using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OnlineAuction.Data;
using OnlineAuction.Hubs;
using OnlineAuction.Interfaces;
using OnlineAuction.Models;
using OnlineAuction.Services;

namespace OnlineAuction
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSignalR();
            builder.Services.AddSingleton<MandarinService>();
            builder.Services.AddControllersWithViews();
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Регистрация EmailConfig к секции EmailSettings в appsettings.json
            builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection("EmailSettings"));

            // Регистрация EmailSender как сервиса
            builder.Services.AddTransient<EmailSender>();
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<MyDbContext>(options => options.UseNpgsql(connectionString));
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IMandarinRepository, MandarinRepository>();
            builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();

            // Настройка аутентификации
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login"; // Путь к странице входа
                });

            var app = builder.Build();

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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapHub<MandarinsHub>("/mandarinsHub");

            app.Run();
        }
    }
}