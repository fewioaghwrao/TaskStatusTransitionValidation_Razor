namespace TaskStatusTransitionValidation.RazorMock.Services;

public interface IMeProvider
{
    Task<MeResponse?> GetMeAsync(string? token, CancellationToken cancellationToken = default);
}