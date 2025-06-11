using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TestApp.Models;
using TestApp.Service;

var builder = WebApplication.CreateBuilder(args);
var appsettings = builder.Configuration;

// --- Service Configuration ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// Register your services
builder.Services.AddSingleton<apiController>();
builder.Services.AddSingleton<dbServices>();
builder.Services.AddSingleton<commonFunctions>();

// --- JWT Authentication ---
var jwtKey = appsettings["jwt_config:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured in appsettings.json");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = appsettings["jwt_config:Issuer"],
            ValidAudience = appsettings["jwt_config:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });
builder.Services.AddAuthorization();


// --- Swagger Configuration ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TestApp API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            new string[] {}
        }
    });
});


// --- CORS ---
builder.Services.AddCors();

var app = builder.Build();

// --- HTTP Request Pipeline Configuration ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseAuthentication();
app.UseAuthorization();

// --- Explicit Endpoint Definitions ---
app.UseEndpoints(endpoints =>
{
    endpoints.MapPost("/register", async (HttpContext http) =>
    {
        var api = http.RequestServices.GetRequiredService<apiController>();
        var rData = await http.Request.ReadFromJsonAsync<requestData>();
        if (rData != null)
        {
            var result = await api.Register(rData);
            await http.Response.WriteAsJsonAsync(result);
        }
    }).AllowAnonymous();

    endpoints.MapPost("/login", async (HttpContext http) =>
    {
        var api = http.RequestServices.GetRequiredService<apiController>();
        var rData = await http.Request.ReadFromJsonAsync<requestData>();
        if (rData != null)
        {
            var result = await api.Login(rData);
            await http.Response.WriteAsJsonAsync(result);
        }
    }).AllowAnonymous();

    endpoints.MapPost("/getUserDetails", async (HttpContext http) =>
    {
        var api = http.RequestServices.GetRequiredService<apiController>();
        var rData = await http.Request.ReadFromJsonAsync<requestData>();
        if (rData != null)
        {
            var result = await api.GetUserDetails(rData);
            await http.Response.WriteAsJsonAsync(result);
        }
    }).RequireAuthorization();

    //New Update Endpoint
    endpoints.MapPost("/updateUser", async (HttpContext http) =>
    {
        var api = http.RequestServices.GetRequiredService<apiController>();
        var rData = await http.Request.ReadFromJsonAsync<requestData>();
        if (rData != null)
        {
            var result = await api.UpdateUser(rData);
            await http.Response.WriteAsJsonAsync(result);
        }
    }).RequireAuthorization();

    // Delete Endpoint
    endpoints.MapPost("/deleteUser", async (HttpContext http) =>
    {
        var api = http.RequestServices.GetRequiredService<apiController>();
        var rData = await http.Request.ReadFromJsonAsync<requestData>();
        if (rData != null)
        {
            var result = await api.DeleteUser(rData);
            await http.Response.WriteAsJsonAsync(result);
        }
    }).RequireAuthorization();

    endpoints.MapGet("/", (HttpContext c) => c.Response.WriteAsJsonAsync("API is Running"));
});

app.Run();