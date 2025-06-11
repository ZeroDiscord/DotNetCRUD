using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http; // added by me
using Microsoft.Net.Http.Headers; // added by me
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.IO;
using System.Collections;
using Org.BouncyCastle.Asn1.Ocsp;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using COMMON_API.Service;

IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();



WebHost.CreateDefaultBuilder().
ConfigureServices(s =>
{
  s.AddHttpClient();
  s.AddHttpContextAccessor();
  s.AddSingleton<apiController>();
  s.AddSingleton<dbServices>();
  s.AddAuthorization();
  s.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidIssuer = appsettings["jwt_config:Issuer"].ToString(),
      ValidAudience = appsettings["jwt_config:Audience"],
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appsettings["jwt_config:Key"])),
    };
  });

  s.AddCors();

  s.AddControllers();
  s.AddEndpointsApiExplorer();
  s.AddSwaggerGen();

}).

Configure(app =>
 {
   var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
   if (env.IsDevelopment())
   {
     app.UseSwagger();
     app.UseSwaggerUI();
   }
   app.UseStaticFiles();
   app.UseRouting();
   app.UseAuthentication();
   app.UseAuthorization();

   app.UseCors(options =>
       options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

   app.UseEndpoints(e =>
   {
     var api_fn = e.ServiceProvider.GetRequiredService<apiController>();
     try
     {
       e.MapPost("/getUserDetails",
       [Authorize] async (HttpContext http) =>
              {
                var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
                Dictionary<string, object> prm = new Dictionary<string, object>();
                requestData rData = JsonSerializer.Deserialize<requestData>(body);
                try
                {
                  if (rData.eventID == "1001")
                    await http.Response.WriteAsJsonAsync(await api_fn.getUserDetails(rData));
                }
                catch (System.Exception ex)
                {
                  Console.WriteLine(ex);
                }


              });
       e.MapPost("/register", [AllowAnonymous] async (HttpContext http) =>
              {
              var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
              var rData = JsonSerializer.Deserialize<requestData>(body);
              var result = await api_fn.Register(rData);
              await http.Response.WriteAsJsonAsync(result);
              });

       e.MapPost("/login", [AllowAnonymous] async (HttpContext http) =>
              {
                var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
                var rData = JsonSerializer.Deserialize<requestData>(body);
                var result = await api_fn.Login(rData);
                await http.Response.WriteAsJsonAsync(result);
              });


       e.MapGet("/",

             async c => await c.Response.WriteAsJsonAsync("Hello Anish!.."));
       e.MapGet("/bing",
         async c => await c.Response.WriteAsJsonAsync("{'Name':'Anish','Age':'26','Project':'COMMON_API'}"));
       e.MapGet("/dbstring",
            async c =>
            {
              dbServices dspoly = new dbServices();
              await c.Response.WriteAsJsonAsync("{'string':" + appsettings["mongodb:connStr"] + appsettings["db:connStrPrimary"] + appsettings["db2:connStrPrimary"] + dspoly.connectDBPrimary() + "}");

            });
     }
     catch (Exception ex)
     {
       Console.Write(ex);
     }

   });
 }).Build().Run();



public record requestData
{
  [Required]
  public string eventID { get; set; }
  [Required]
  public IDictionary<string, object> addInfo { get; set; }
}

public record responseData
{
  public responseData()
  {
    eventID = "";
    rStatus = 0;
    rData = new Dictionary<string, object>();

  }
  [Required]
  public int rStatus { get; set; } = 0;
  [Required]
  public string eventID { get; set; }
  public IDictionary<string, object> addInfo { get; set; }
  public Dictionary<string, object> rData { get; set; }
}
