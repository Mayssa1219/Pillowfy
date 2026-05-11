using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Pillowfy.Data;
using Pillowfy.Factory;
using Pillowfy.Interfaces;
using Pillowfy.Models;
using Pillowfy.Services;
using Pillowfy.Services.EmailService;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// ===================== SERVICES =====================

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

// Swagger + JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pillowfy API",
        Version = "v1"
    });

    // ✅ Fix : résout les conflits de schémas avec Identity + vos modèles
    c.CustomSchemaIds(type => type.FullName);

    // ✅ Fix : ignore les actions sans route explicite
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        return apiDesc.RelativePath != null;
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// DB
builder.Services.AddDbContext<PilloWfyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<Pillowfy.Services.EmailService.IEmailService, Pillowfy.Services.EmailService.EmailService>();

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<PilloWfyDbContext>()
.AddDefaultTokenProviders();

// ===================== JWT =====================

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
    throw new Exception("Jwt:Key is missing in appsettings.json");

builder.Services.AddAuthentication(options =>
{
    // ✅ Cookie = scheme par défaut pour les controllers MVC (Owner, Client)
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Login/Index";       // redirige vers login si non connecté
    options.AccessDeniedPath = "/Login/Index";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddJwtBearer(options =>
{
    // ✅ JWT = scheme pour les appels fetch('/api/...')
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IChambreService, ChambreService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IChambreFactory, ChambreFactory>();
builder.Services.AddScoped<PaiementService>();
builder.Services.AddScoped<AvisService>();
builder.Services.AddScoped<StatistiqueService>();
builder.Services.AddScoped<OwnerDashboardService>();
builder.Services.AddScoped<OwnerPaiementService>();
builder.Services.AddScoped<OwnerRevenusService>();
builder.Services.AddScoped<OwnerAvisService>();

// Cookie Auth pour les vues Admin (en plus du JWT)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Login";
    options.AccessDeniedPath = "/Admin/Login";
});

var app = builder.Build();

// ===================== MIDDLEWARE =====================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// MVC ROUTING
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

// ===================== SEED =====================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedDatabase(services);
}

app.Run();

// ===================== SEED =====================

async Task SeedDatabase(IServiceProvider serviceProvider)
{
    var context = serviceProvider.GetRequiredService<PilloWfyDbContext>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // 1. Migrations
    await context.Database.MigrateAsync();

    // 2. Créer les rôles
    var roles = new[] { "Admin", "Owner", "Customer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded)
                throw new Exception($"Erreur création rôle '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    // 3. Seed Admin
    await SeedUser(userManager,
        email: "admin@pillowfy.com",
        firstName: "Admin",
        lastName: "Pillowfy",
        phone: "1234567890",
        password: "Admin@123456",
        role: "Admin"
    );

    // 4. Seed Owner (optionnel)
    await SeedUser(userManager,
        email: "mayssa@pillowfy.com",
        firstName: "Jrad",
        lastName: "Mayssa",
        phone: "0987654321",
        password: "Owner@123456",
        role: "Owner"
    );
}

// ✅ Méthode réutilisable pour chaque user seed
async Task SeedUser(
    UserManager<ApplicationUser> userManager,
    string email, string firstName, string lastName,
    string phone, string password, string role)
{
    var existingUser = await userManager.FindByEmailAsync(email);
    if (existingUser != null) return; // Déjà seedé

    var user = new ApplicationUser
    {
        UserName = email,
        Email = email,
        FirstName = firstName,
        LastName = lastName,
        PhoneNumber = phone,
        EmailConfirmed = true,
        IsActive = true
    };

    var createResult = await userManager.CreateAsync(user, password);
    if (!createResult.Succeeded)
        throw new Exception($"Erreur création user '{email}': {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

    var roleResult = await userManager.AddToRoleAsync(user, role);
    if (!roleResult.Succeeded)
        throw new Exception($"Erreur assignation rôle '{role}' à '{email}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");


}