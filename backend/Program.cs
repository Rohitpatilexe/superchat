using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using backend.Config;
using backend.Models;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Security.Claims; // Needed for Identity

// IMPORTANT: Uncommented the necessary using statements that were commented out by you
//using backend.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


var builder = WebApplication.CreateBuilder(args);

Env.Load();

// --- CORS policy name ---
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
var connectionString = $"Server={dbHost},{dbPort};Database={dbName};User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
  options.UseSqlServer(connectionString));

// --- COMMENTED OUT: SENDGRID/EMAIL CONFIG ---
/*
var emailConfig = new EmailConfig
{
    SendGridApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
};
// singleton service so it can be injected into other classes.
builder.Services.AddSingleton(emailConfig);
*/


// --- Add Custom Services (UNCOMMENTED) ---
//builder.Services.AddScoped<AuthService>();
//builder.Services.AddScoped<LeadershipService>();
//builder.Services.AddScoped<AdminService>();
//// NOTE: EmailService is commented out until the config above is uncommented.
//// builder.Services.AddScoped<EmailService>(); 
//builder.Services.AddScoped<VendorService>();


// --- COMMENTED OUT: AWS S3 CONFIGURATION ---
/*
// Configuring the AWS S3
var awsCredentials = new BasicAWSCredentials(
    Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
    Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
);
var awsConfig = new AmazonS3Config
{
    RegionEndpoint = RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION"))
};
builder.Services.AddSingleton<IAmazonS3>(new AmazonS3Client(awsCredentials, awsConfig));
*/


// --- COMMENTED OUT: JWT AUTHENTICATION CONFIGURATION ---
/*
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")))
    };
});
*/

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
             policy =>
             {
                 // Allow requests from the default Angular development server origin.
                 // For production, you would replace this with your actual frontend domain.
                 policy.WithOrigins("http://localhost:4200")
     .AllowAnyHeader()
     .AllowAnyMethod();
             });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// NOTE: JWT Auth is commented out, so Swagger configuration for JWT is also not strictly needed
builder.Services.AddSwaggerGen(options =>
{
    // Add a general description for the Swagger page
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My Vendor API", Version = "v1" });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // In production, use the custom error handler.
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

// Add Authentication and Authorization middleware
// NOTE: These are still needed to run the app, 
// even if the JWT scheme is commented out above.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // Needed to map controller routes

// --- Application Initialization for Seeding (Your Logic) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // This line ensures the database exists and all migrations are applied
        context.Database.Migrate();

        // Custom Seeding Logic (We need to insert the Admin user)
        if (!context.Users.Any(u => u.Email == "admin@example.com"))
        {
            var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");

            // NOTE: We are using a placeholder hash now. We will replace this 
            // with the actual BCrypt hash in the AuthService later.
            var adminUser = new User
            {
                Email = "admin@example.com",
                PasswordHash = "PlaceholderHash_CHANGE_ME",
                FirstName = "Root",
                LastName = "Admin",
                CreatedAt = DateTime.UtcNow,
                PublicId = Guid.NewGuid(),
                Roles = new List<Role> { adminRole }
            };
            context.Users.Add(adminUser);
            context.SaveChanges();
            Console.WriteLine("!!! Initial Admin User Seeded Successfully !!!");
        }
    }
    catch (Exception ex)
    {
        // Using a static Logger, as ILogger<Program> may not be available outside the scope
        Console.WriteLine($"[ERROR] An error occurred while seeding the database: {ex.Message}");
    }
}
// --- End Application Initialization ---


app.Run(); // This is the single, final call to run the application