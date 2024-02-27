using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using tsuKeysAPIProject.AdditionalServices.Exceptions;
using tsuKeysAPIProject.AdditionalServices.TokenHelpers;
using tsuKeysAPIProject.AdditionalServices.UserInfoHelper;
using tsuKeysAPIProject.DBContext;
using tsuKeysAPIProject.DBContext.Models;
using tsuKeysAPIProject.Services;
using tsuKeysAPIProject.Services.IServices.IKeyService;
using tsuKeysAPIProject.Services.IServices.IRequestService;
using tsuKeysAPIProject.Services.IServices.IRolesService;
using tsuKeysAPIProject.Services.IServices.IScheduleService;
using tsuKeysAPIProject.Services.IServices.IUserService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IKeyService, KeyService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<TokenInteraction>();
builder.Services.AddScoped<UserInfoHelper>();
builder.Services.AddEndpointsApiExplorer();






builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = "JWTToken",
        ValidAudience = "Human",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("BACKENDDEVELOPINGFORHITSBACKENDDEVELOPINGFORHITSBACKENDDEVELOPINGFORHITSBACKENDDEVELOPINGFORHITSBACKENDDEVELOPINGFORHITSBACKENDDEVELOPINGFORHITS"))
    };
});
builder.Services.AddAuthorization(services =>
{
    services.AddPolicy("TokenNotInBlackList", policy => policy.Requirements.Add(new TokenBlackListRequirment()));
});
builder.Services.AddSingleton<IAuthorizationHandler, TokenInBlackListHandler>();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using var serviceScope = app.Services.CreateScope();
var dbContext = serviceScope.ServiceProvider.GetService<AppDBContext>();
dbContext?.Database.Migrate();

dbContext.TimeSlots.RemoveRange(dbContext.TimeSlots);
dbContext.TimeSlots.AddRange(
            new TimeSlot { SlotNumber = 1, StartTime = TimeOnly.Parse("08:45"), EndTime = TimeOnly.Parse("10:20") },
            new TimeSlot { SlotNumber = 2, StartTime = TimeOnly.Parse("10:35"), EndTime = TimeOnly.Parse("12:10") },
            new TimeSlot { SlotNumber = 3, StartTime = TimeOnly.Parse("12:25"), EndTime = TimeOnly.Parse("14:00") },
            new TimeSlot { SlotNumber = 4, StartTime = TimeOnly.Parse("14:45"), EndTime = TimeOnly.Parse("16:20") },
            new TimeSlot { SlotNumber = 5, StartTime = TimeOnly.Parse("16:35"), EndTime = TimeOnly.Parse("18:10") },
            new TimeSlot { SlotNumber = 6, StartTime = TimeOnly.Parse("18:25"), EndTime = TimeOnly.Parse("20:00") }
        );

dbContext.SaveChanges();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
    .SetIsOriginAllowed(origin => true));

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();