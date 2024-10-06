using System.Text;
using System.Text.Json;

using System.Text.Json.Serialization;
using LandsatReflectance.Backend.Middleware;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Services;
using LandsatReflectance.Backend.Services.BackgroundServices;
using LandsatReflectance.Backend.Services.NotificationSender;
using LandsatReflectance.Backend.Utils;
using LandsatReflectance.Backend.Utils.SourceGenerators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);


builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Higher priority converters/contexts appear earlier in the list.
    // Converters/contexts with lesser precedence comes first, since we're pre-pending.
    options.SerializerOptions.Converters.Insert(0, new CustomDateTimeConverter());
    options.SerializerOptions.Converters.Insert(0, new MetadataConverter());
    options.SerializerOptions.Converters.Insert(0, new UsgsApiResponseConverter<LoginTokenResponse>());
    options.SerializerOptions.Converters.Insert(0, new UsgsApiResponseConverter<SceneListAddResponse>());
    options.SerializerOptions.Converters.Insert(0, new UsgsApiResponseConverter<SceneListGetResponse>());
    options.SerializerOptions.Converters.Insert(0, new UsgsApiResponseConverter<SceneMetadataListResponse>());
    options.SerializerOptions.Converters.Insert(0, new UsgsApiResponseConverter<SceneSearchResponse>());
    
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, SceneSearchResponseJsonContext.Default);
    
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

#if !DEBUG
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var authSecretKey = new KeysService().AuthSecretKey;
        var key = Encoding.UTF8.GetBytes(authSecretKey);
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = msgReceivedContext =>
            {
                msgReceivedContext.Token = msgReceivedContext.Request.Headers["X-Auth-Token"].FirstOrDefault();
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
#endif



builder.Services.AddControllers();

var keysService = new KeysService();
builder.Services.AddDbContext<DbUserService.UserDbContext>(options =>
    options.UseMySql(keysService.DbConnectionString, ServerVersion.AutoDetect(keysService.DbConnectionString)));

builder.Services.AddDbContext<DbTargetService.TargetDbContext>(options =>
    options.UseMySql(keysService.DbConnectionString, ServerVersion.AutoDetect(keysService.DbConnectionString)));

builder.Services.AddDbContext<DbUserTargetNotificationService.UserTargetNotificationDbContext>(options =>
    options.UseMySql(keysService.DbConnectionString, ServerVersion.AutoDetect(keysService.DbConnectionString)));


builder.Services.AddMemoryCache();

builder.Services.AddScoped<INotificationSenderService, EmailNotificationSenderService>();
builder.Services.AddScoped<INotificationSenderService, SmsNotificationSenderService>();
builder.Services.AddScoped<DbUserTargetNotificationService>();

builder.Services.AddSingleton<KeysService>();
builder.Services.AddSingleton<SceneEntityIdCachingService>();

builder.Services.AddScoped<IUserService, DbUserService>();
builder.Services.AddScoped<ITargetService, DbTargetService>();

builder.Services.AddScoped<UsgsApiService>();

builder.Services.AddHostedService<NotificationService>();


if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c => c.EnableAnnotations());
}

builder.Services.AddCors();



var app = builder.Build();


// Add specific CORS config for release / production
#if DEBUG
app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials());
#endif

app.UseDeveloperExceptionPage();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<DefaultErrorHandlingMiddleware>();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapControllers();

app.Run();