namespace TaskStatusTransitionValidation.RazorMock.Services;

public sealed class ApiMeProvider : IMeProvider
{
    private readonly IApiClient _apiClient;

    public ApiMeProvider(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<MeResponse?> GetMeAsync(string? token, CancellationToken cancellationToken = default)
    {
        return _apiClient.GetMeAsync(token, cancellationToken);
    }
}