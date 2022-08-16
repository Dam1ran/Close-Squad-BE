using CS.Application.Options.Abstractions;

namespace CS.Api.Support.Other;
public class OptionsAdapter<TOptions> : IOptions<TOptions> where TOptions : class {
  public TOptions Value { get; private set; }
  public OptionsAdapter(Microsoft.Extensions.Options.IOptions<TOptions> options) => Value = options.Value;
}
