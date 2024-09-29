using System.Text;
using System.Text.Json;

using System.Text.Json.Serialization;
using LandsatReflectance.Backend.Middleware;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Services;
using LandsatReflectance.Backend.Utils;
using LandsatReflectance.Backend.Utils.SourceGenerators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

builder.Services.AddControllers();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<KeysService>();
builder.Services.AddSingleton<SceneEntityIdCachingService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UsgsApiService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.EnableAnnotations());



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<DefaultErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();