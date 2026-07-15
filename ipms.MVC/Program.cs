using ipms.MVC.Services;

namespace ipms.MVC;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();

        // Session holds the JWT we get back from the API at login.
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(1);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        // Typed client for the IPMS API. AuthTokenHandler attaches the bearer
        // token to every outgoing call automatically.
        builder.Services.AddTransient<AuthTokenHandler>();

        builder.Services
            .AddHttpClient<IpmsApiClient>(client =>
            {
                client.BaseAddress =
                    new Uri(builder.Configuration["ApiBaseUrl"]!);
            })
            .AddHttpMessageHandler<AuthTokenHandler>();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseSession();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}
