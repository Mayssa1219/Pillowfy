using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pillowfy.Data;
using Pillowfy.Interfaces;
using Pillowfy.Models;
using Pillowfy.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Database Configuration
builder.Services.AddDbContext<PilloWfyDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Configuration
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

// JWT Configuration
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(key),
		ValidateIssuer = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidateAudience = true,
		ValidAudience = builder.Configuration["Jwt:Audience"],
		ValidateLifetime = true
	};
});

// CORS Configuration
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", builder =>
	{
		builder.AllowAnyOrigin()
			   .AllowAnyMethod()
			   .AllowAnyHeader();
	});
});

// Services Registration
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IChambreService, ChambreService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed Database
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	await SeedDatabase(services);
}

app.Run();

// Seed Database Method
async Task SeedDatabase(IServiceProvider serviceProvider)
{
	var context = serviceProvider.GetRequiredService<PilloWfyDbContext>();
	var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

	await context.Database.MigrateAsync();

	// Create Roles
	var roles = new[] { "Admin", "Owner", "Customer" };
	foreach (var role in roles)
	{
		if (!await roleManager.RoleExistsAsync(role))
		{
			await roleManager.CreateAsync(new IdentityRole(role));
		}
	}

	// Create Admin User
	var adminEmail = "admin@pillowfy.com";
	var adminUser = await userManager.FindByEmailAsync(adminEmail);
	if (adminUser == null)
	{
		adminUser = new ApplicationUser
		{
			UserName = adminEmail,
			Email = adminEmail,
			FirstName = "Admin",
			LastName = "Pillowfy",
			PhoneNumber = "1234567890",
			EmailConfirmed = true
		};

		await userManager.CreateAsync(adminUser, "Admin@123456");
		await userManager.AddToRoleAsync(adminUser, "Admin");
	}
}