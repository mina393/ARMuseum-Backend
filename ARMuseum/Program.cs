using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ARMuseum.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ARMuseum.Models;
using ARMuseum.Hubs;
using X.Paymob.CashIn;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using ARMuseum.Dtos;
using ARMuseum.Settings;
using ARMuseum.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// --- Configure Services ---

// 1. Configure Database Context and ASP.NET Core Identity
builder.Services.AddDbContext<OurDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<OurDbContext>()
.AddDefaultTokenProviders();

// 2. Configure application settings using the Options pattern
builder.Services.Configure<MailtrapSettings>(builder.Configuration.GetSection("MailtrapSettings"));
builder.Services.Configure<PaymobSettings>(builder.Configuration.GetSection("PaymobSettings"));
builder.Services.Configure<AdminUserSeedSettings>(builder.Configuration.GetSection("AdminUserSeedSettings"));

// 3. Register custom application services
builder.Services.AddTransient<IEmailService, MailtrapEmailService>();
builder.Services.AddScoped<PaymobPaymentService>();

// 4. Configure Authentication schemes (JWT and Facebook)
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Secret))
{
    throw new InvalidOperationException("JWT settings are not configured correctly.");
}
builder.Services.AddSingleton(jwtSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => // Configure JWT Bearer validation
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
})
.AddFacebook(facebookOptions => // Configure Facebook external login
{
    facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"];
    facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
});

// 5. Configure Paymob services
var paymobSettings = builder.Configuration.GetSection("PaymobSettings").Get<PaymobSettings>();
builder.Services.AddPaymobCashIn(options =>
{
    // API Key and HMAC are loaded securely from configuration
    options.ApiKey = paymobSettings.ApiKey;
    options.Hmac = paymobSettings.Hmac;
});

// 6. Register other essential services
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader();
    });
});

var app = builder.Build();

// --- Configure the HTTP Request Pipeline ---

// Enable Swagger for development environments
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // This serves files from the default wwwroot folder
app.UseCors("AllowAll");

// Add content type mappings to support Unity WebGL files
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".data"] = "application/octet-stream";
provider.Mappings[".wasm"] = "application/wasm";
provider.Mappings[".unityweb"] = "application/octet-stream";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// --- Seed the Database with Default Roles and Admin User on Startup ---
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var dbContext = serviceProvider.GetRequiredService<OurDbContext>();

    // Get admin settings securely from the configuration
    var adminSettings = serviceProvider.GetRequiredService<IOptions<AdminUserSeedSettings>>().Value;

    // Apply any pending Entity Framework migrations
    dbContext.Database.Migrate();

    // Create "USER" and "Admin" roles if they don't exist
    if (!await roleManager.RoleExistsAsync("USER"))
    {
        await roleManager.CreateAsync(new IdentityRole("USER"));
    }
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Create the default admin account if it doesn't exist
    if (await userManager.FindByEmailAsync(adminSettings.Email) == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = "adminuser",
            Email = adminSettings.Email,
            EmailConfirmed = true,
            UFirstName = "Admin",
            ULastName = "User"
        };

        var result = await userManager.CreateAsync(adminUser, adminSettings.Password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

// Map controllers and SignalR hubs
app.MapControllers();
app.MapHub<MuseumTrackingHub>("/museumHub");

// The following line can be uncommented for specific deployment scenarios like Docker
// app.Urls.Add("http://0.0.0.0:5168"); 

app.Run();