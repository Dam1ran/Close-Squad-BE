using CS.Application.Persistence.Abstractions.Repositories;
using CS.Application.Persistence.Repositories;

namespace CS.Api.Support.Extensions;
public static class RepositoriesServiceCollectionExtensions {
  public static IServiceCollection AddRepositories(this IServiceCollection services) =>
    services
      .AddScoped<ICsUserRepository, CsUserRepository>();

}
