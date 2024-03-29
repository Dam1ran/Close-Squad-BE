using System.Text.RegularExpressions;

namespace CS.Api.Support.Other;
public class SlugifyParameterTransformer : IOutboundParameterTransformer {
  public string? TransformOutbound(object? value) =>
    value == null ? null : Regex.Replace(value.ToString()!, "([a-z])([A-Z])", "$1-$2").ToLower();
}
