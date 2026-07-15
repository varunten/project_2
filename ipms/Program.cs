using System.Text;
using IPMS.BAL.IService;
using IPMS.BAL.Service;
using IPMS.DAL.IRepository;
using IPMS.DAL.Repository;
using IPMS.DAL.Data;
using IPMS.DTO.Dtos;
using IPMS.Middlewares;
using IPMS.Seeders;


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;


namespace IPMS;


public class Program
{
    public static async Task Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);



        builder.Services.AddControllers();


        // When [ApiController] model validation fails (e.g. a required field is
        // missing), return 422 with our ErrorResponse shape instead of the
        // default 400 ProblemDetails.
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                Dictionary<string, string[]> errors = context.ModelState
                    .Where(kvp => kvp.Value is not null && kvp.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors
                            .Select(e => e.ErrorMessage)
                            .ToArray()
                    );

                ErrorResponse response = new()
                {
                    Success = false,
                    Message = "Validation failed. Required data is missing or invalid.",
                    Errors = errors
                };

                return new UnprocessableEntityObjectResult(response);
            };
        });


        builder.Services.AddOpenApi();



        builder.Services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
        });



 
        builder.Services.AddDbContext<AppDbContext>(
            options =>
            {
                options.UseSqlServer(
                    builder.Configuration
                        .GetConnectionString(
                            "DefaultConnection"
                        )
                );
            }
        );





        // ---- Repositories (Data Access Layer) ----
        builder.Services.AddScoped<IAuthRepository, AuthRepository>();
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
        builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();
        builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
        builder.Services.AddScoped<IPremiumPaymentRepository, PremiumPaymentRepository>();
        builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
        builder.Services.AddScoped<ISessionRepository, SessionRepository>();


        // ---- Services (Business Logic Layer) ----
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<ICustomerService, CustomerService>();
        builder.Services.AddScoped<IQuoteService, QuoteService>();
        builder.Services.AddScoped<IPolicyService, PolicyService>();
        builder.Services.AddScoped<IPremiumPaymentService, PremiumPaymentService>();
        builder.Services.AddScoped<IClaimService, ClaimService>();
        builder.Services.AddScoped<ISessionService, SessionService>();



        builder.Services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme
            )
            .AddJwtBearer(options =>
            {

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {

                        ValidateIssuer = true,

                        ValidIssuer =
                            builder.Configuration[
                                "AppSettings:Issuer"
                            ],



                        ValidateAudience = true,

                        ValidAudience =
                            builder.Configuration[
                                "AppSettings:Audience"
                            ],



                        ValidateLifetime = true,



                        ValidateIssuerSigningKey = true,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    builder.Configuration[
                                        "AppSettings:Token"
                                    ]!
                                )
                            )
                    };
            });



        builder.Services.AddAuthorization();



        var app = builder.Build();

        using(var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;


            var db =
                services.GetRequiredService<AppDbContext>();


            await db.Database.MigrateAsync();


            await RoleSeeder.SeedAsync(db);


            await AdminSeeder.SeedAsync(db);
        }


        if(app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.MapScalarApiReference();
        }



        // Must run first so it can catch exceptions from everything below.
        app.UseMiddleware<ExceptionHandlingMiddleware>();



        app.UseHttpsRedirection();



        app.UseAuthentication();



        app.UseMiddleware<SessionValidationMiddleware>();



        app.UseAuthorization();



        app.MapControllers();



        app.Run();
    }
}