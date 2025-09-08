namespace Infra.Movments;

public sealed class MovmentsApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 10;
}
