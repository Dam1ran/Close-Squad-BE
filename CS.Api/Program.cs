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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddControllers(options => {
  options.Filters.Add<ExceptionFilter>();
  options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
  options.Filters.Add<ValidateAntiforgeryTokenFilter>();
});

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
  options.HeaderName = ApiConstants.AntiforgeryHeaderPlaceholder;
});


// builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocument();
builder.Services.AddMediatR(typeof(CommandHandler<>));
builder.Services.AddTransient<IEmailService, SendGridEmailService>();
builder.Services.AddTransient<ITemplatedEmailService, TemplatedEmailService>();
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
      .WithHeaders(ApiConstants.AntiforgeryCookiePlaceholder, ApiConstants.AntiforgeryHeaderPlaceholder, ApiConstants.ContentType)
      .WithExposedHeaders(ApiConstants.AntiforgeryCookiePlaceholder, ApiConstants.ContentType)
  )
);


var app = builder.Build();

if (app.Environment.IsDevelopment()) {
  app.UseOpenApi();
  app.UseSwaggerUi3();
}

app.UseCors();

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


// app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<MainHub>("/MainHub");

app.Run();
