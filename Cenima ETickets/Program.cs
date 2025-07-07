
using Cinema_ETickets.Utility;
using Cinema_ETickets.Utility.DBInitilizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;

namespace Cinema_ETickets
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<ApplicationDbContext>(
                option => option.UseSqlServer("Data Source=.;DataBase=Cinema_ETickets;Integrated Security=True;" +
                                "Trust Server Certificate=True")
                );

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(option =>
            {
                option.Password.RequiredLength = 4;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped<ICategoryRepository ,CategoryRepository>();
            builder.Services.AddScoped<ICinemaRepository ,CinemaRepository>();
            builder.Services.AddScoped<IActorRepository ,ActorRepository>();
            builder.Services.AddScoped<IMovieRepository ,MovieRepository>();

            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IDBInitializer, DBInitializer>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            using (var scope = app.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<IDBInitializer>();
                dbInitializer.Initialize();
            }

            app.Run();
        }
    }
}
