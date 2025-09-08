namespace Infra.Movments;

public sealed class MovmentsApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 10;
}