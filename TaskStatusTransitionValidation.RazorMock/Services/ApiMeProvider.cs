namespace TaskStatusTransitionValidation.RazorMock.Services;

public sealed class ApiMeProvider : IMeProvider
{
    private readonly ApiClient _apiClient;

    public ApiMeProvider(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<MeResponse?> GetMeAsync(string? token, CancellationToken cancellationToken = default)
    {
        return _apiClient.GetMeAsync(token, cancellationToken);
    }
}