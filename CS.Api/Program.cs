using System.Text;
using CS.Api.Communications;
using CS.Api.Services;
using CS.Api.Support;
using CS.Api.Support.Filters;
using CS.Application;
using CS.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CS.Core.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using CS.Persistence;
using Microsoft.EntityFrameworkCore;
using CS.Application.Persistence.Abstractions;
using MediatR;
using CS.Application.Commands.Abstractions;
using CS.Infrastructure.Services.Abstractions;
using CS.Infrastructure.Services;
using CS.Application.Services.Abstractions;
using Microsoft.AspNetCore.HttpOverrides;
using CS.Api.Support.Middleware;
using CS.Api.Services.Abstractions;
using CS.Api.Support.Other;
using CS.Api.Support.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);

// JwtConfig jwtConfig = new ();
// builder.Configuration.GetSection("JwtConfig").Bind(jwtConfig);
// builder.Services.AddAuthentication(opt => {
//       opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//       opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//   }).AddJwtBearer(options => {
//       options.TokenValidationParameters = new TokenValidationParameters {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = true,
//         ValidateIssuerSigningKey = true,
//         ValidIssuer = jwtConfig.ValidIssuer,
//         ValidAudience = jwtConfig.ValidAudience,
//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret))
//       };
//   });

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

// builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocument();
builder.Services.AddApplicationOptions();
builder.Services.AddMediatR(typeof(CommandHandler<>));
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddTransient<IEmailService, SendGridEmailService>();
builder.Services.AddTransient<ITemplatedEmailService, TemplatedEmailService>();
builder.Services.AddSingleton<ICachedUserService, CachedUserService>();
builder.Services.AddSingleton<ICaptchaCacheService, CaptchaCacheService>();
builder.Services.AddScoped(typeof(IStatisticsCacheService<>), typeof(StatisticsCacheService<>));
builder.Services.AddScoped<ICaptchaService, CaptchaService>();
builder.Services.AddSignalR();

builder.Services.AddDbContext<Context>(o => {
  o.UseSqlServer(
    builder.Configuration.GetSection("DefaultConnection").Value,
    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
})
.AddScoped<IContext>(provider => provider.GetRequiredService<Context>());

builder.Services.AddIdentity<User, Role>(o => {
    o.SignIn.RequireConfirmedEmail = false;
    o.User.RequireUniqueEmail = true;
    o.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    o.Password =
      new PasswordOptions {
        RequiredLength = 8,
        RequireUppercase = true,
        RequireNonAlphanumeric = true,
        RequireDigit = true,
        RequireLowercase = true
    };
  })
  .AddEntityFrameworkStores<Context>()
  .AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(1));

builder.Services.AddSingleton<ITickService, TickService>();
builder.Services.AddSingleton<TESTNAHUI>();
builder.Services.AddScoped<IUserService, UserService>();


builder.Services.AddCors(options =>
  options.AddDefaultPolicy(ob =>
    ob.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Value.Split(";"))
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials()
      .WithHeaders(Api_Constants.AntiforgeryCookiePlaceholder, Api_Constants.AntiforgeryHeaderPlaceholder, Api_Constants.ContentType)
      .WithExposedHeaders(Api_Constants.AntiforgeryCookiePlaceholder, Api_Constants.ContentType)
  )
);


builder.Services.AddControllers(options => {
  options.Filters.Add<ExceptionFilter>();
  options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
  options.Filters.Add<ValidateAntiforgeryTokenFilter>();
});

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

app.UseMiddleware<AttributeRateLimiting>();

app.UseSession();

app.UseMiddleware<CaptchaInquiry>();

app.UseAuthentication();
app.UseMiddleware<AuthenticatedClientRateLimiting>();


app.UseAuthorization();
app.MapControllers();

app.MapHub<MainHub>("/MainHub");

app.Run();
