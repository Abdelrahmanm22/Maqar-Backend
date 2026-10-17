namespace Maqar.Api.Helpers
{
    /// <summary>Lower-cases route tokens so <c>api/[controller]</c> reads <c>/api/properties</c> in URLs and in the OpenAPI document.</summary>
    public sealed class LowercaseRouteTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value) => value?.ToString()?.ToLowerInvariant();
    }
}
