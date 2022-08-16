namespace CS.Application.Options.Abstractions;
public interface IOptions<TOptions> where TOptions: class {
  TOptions Value { get; }
}
