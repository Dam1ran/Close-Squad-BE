using CS.Api.Communications;
using CS.Api.Services;
using CS.Api.Support;
using CS.Api.Support.Filters;
using CS.Application;
using CS.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using CS.Persistence;
using Microsoft.EntityFrameworkCore;
using CS.Application.Persistence.Abstractions;
using MediatR;
using CS.Application.Commands.Abstractions;
using CS.Infrastructure.Services;
using CS.Application.Services.Abstractions;
using Microsoft.AspNetCore.HttpOverrides;
using CS.Api.Support.Middleware;
using CS.Api.Services.Abstractions;
using CS.Api.Support.Other;
using CS.Api.Support.Extensions;
using CS.Application.Services.Implementations;
using CS.Application.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using CS.Core.Enums;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);

var jwtOption = builder.Configuration.GetSection(JwtOptions.Jwt).Get<JwtOptions>();
builder.Services.AddAuthentication(opt => {
      opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      opt.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
  }).AddJwtBearer(options => {
      options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(jwtOption.ClockSkewSeconds),
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOption.ValidIssuer,
        ValidAudience = jwtOption.ValidAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOption.Secret))

      };
      options.Events = new() {
        OnMessageReceived = (context) => {
          var path = context.HttpContext.Request.Path;
          if (path.StartsWithSegments("/MainHub")) {
            var accessToken = context.Request.Query["access_token"];
            context.Token = accessToken;
          }
          return Task.CompletedTask;
        }
      };
  });


builder.Services.AddDataProtection();

builder.Services.AddAuthorization(o => {
  o.AddPolicy(Api_Constants.AdministrationPolicy, policy => policy.RequireRole(UserRole.ADM.ToString()).RequireAuthenticatedUser());
  o.AddPolicy(Api_Constants.GameMasterPolicy, policy => policy.RequireRole(UserRole.GMA.ToString()).RequireAuthenticatedUser());
  o.AddPolicy(Api_Constants.ManagementPolicy, policy => policy.RequireRole(UserRole.ADM.ToString(), UserRole.GMA.ToString()).RequireAuthenticatedUser());
  o.FallbackPolicy =
    new AuthorizationPolicyBuilder()
      .RequireAuthenticatedUser()
      .Build();
});

builder.Services.AddAntiforgery(options => {
  options.Cookie.SameSite = SameSiteMode.None;
  options.Cookie.HttpOnly = true;
  options.SuppressXFrameOptionsHeader = false;
  options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
  options.HeaderName = Api_Constants.AntiforgeryHeaderPlaceholder;
});

builder.Services.Configure<ForwardedHeadersOptions>(options => {
  options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
  options.ForwardLimit = 1; // change when are known proxies or load balancers
  // options.KnownProxies.Add(IPAddress.Parse("::ffff:172.168.0.57")); // add if the case and ignore ForwardLimit
  options.KnownNetworks.Clear();
  options.KnownProxies.Clear();
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
  // options.IdleTimeout = TimeSpan.FromMinutes(10);
  options.Cookie.SameSite = SameSiteMode.None;
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
  options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocument();
builder.Services.AddApplicationOptions();
builder.Services.AddRepositories();
builder.Services.AddMediatR(typeof(CommandHandler<>));
builder.Services.AddScoped<ICsTimeLimitedDataProtector, CsTimeLimitedDataProtector>();
builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddScoped<IUserTokenService, UserTokenService>();
builder.Services.AddTransient<IEmailService, SendGridEmailService>();
builder.Services.AddTransient<ITemplatedEmailService, TemplatedEmailService>();
builder.Services.AddSingleton<ICachedUserService, CachedUserService>();
builder.Services.AddScoped<ICaptchaService, CaptchaService>();


builder.Services.AddSignalR(o => {
  o.AddFilter<AuthHubFilter>();
  o.MaximumParallelInvocationsPerClient = 3;
  o.MaximumReceiveMessageSize = 8192;
});
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();

builder.Services.AddDbContext<Context>(o => {
  o.UseSqlServer(
    builder.Configuration.GetSection("DefaultConnection").Value,
    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
})
.AddScoped<IContext>(provider => provider.GetRequiredService<Context>());


builder.Services.AddSingleton<IPlayerService, PlayerService>();
builder.Services.AddSingleton<IWorldMapService, WorldMapService>();

builder.Services.AddSingleton<ITickService, TickService>();
builder.Services.AddSingleton<TESTNAHUI>();


builder.Services.AddCors(options =>
  options.AddDefaultPolicy(ob =>
    ob.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Value.Split(";"))
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials()
      .WithHeaders(
        Api_Constants.AntiforgeryCookiePlaceholder,
        Api_Constants.AntiforgeryHeaderPlaceholder,
        Api_Constants.ContentType,
        Api_Constants.AuthorizationHeader,
        Api_Constants.XRequestedWith,
        Api_Constants.XSignalRUserAgent)
      .WithExposedHeaders(Api_Constants.AntiforgeryCookiePlaceholder, Api_Constants.ContentType)
  )
);


builder.Services.AddControllers(options => {
  options.Filters.Add<ExceptionFilter>();
  options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
  options.Filters.Add<ValidateAntiforgeryTokenFilter>();
});


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
var app = builder.Build();


app.UseMiddleware<CatchMiddlewareExceptions>();

app.UseRouting();
app.UseCors();


if (app.Environment.IsDevelopment()) {
  app.UseOpenApi();
  app.UseSwaggerUi3();
}

app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseSecurityHeaders(policies => policies
  .AddFrameOptionsDeny()
  .AddXssProtectionBlock()
  .AddContentTypeOptionsNoSniff()
  .RemoveServerHeader()
  .AddContentSecurityPolicy(builder => {
      builder.AddObjectSrc().None();
      builder.AddFormAction().Self();
      builder.AddFrameAncestors().None();
  })
  .AddCrossOriginOpenerPolicy(builder => builder.SameOrigin())
  .AddCrossOriginResourcePolicy(builder => builder.SameOrigin())
  .AddCrossOriginEmbedderPolicy(builder => builder.RequireCorp()));

app.UseAuthentication();

app.UseMiddleware<AttributeRateLimiting>();


app.UseSession();
app.UseMiddleware<CaptchaInquiry>();

app.UseMiddleware<AuthenticatedClientRateLimiting>();

app.UseMiddleware<CheckSameToken>();

app.UseAuthorization();
app.MapControllers();

app.MapHub<MainHub>("/MainHub", options => {
  options.ApplicationMaxBufferSize = 16384;
  options.TransportMaxBufferSize = 16384;
  options.CloseOnAuthenticationExpiration = true;
});

app.Run();
