namespace NLogFlake.Services;

public class CorrelationService : ICorrelationService
{
    private readonly Guid _correlationId;

    public string Correlation => _correlationId.ToString();

    public CorrelationService()
    {
        _correlationId = Guid.NewGuid();
    }
}
