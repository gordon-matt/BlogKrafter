namespace BlogKrafter.Tests.Infrastructure;

/// <summary>A signed-in editor, or an anonymous reader when <paramref name="userId"/> is null.</summary>
public sealed class TestUserContext(string? userId = "editor-1", string? userName = "Ada Editor") : IUserContextService
{
    public static TestUserContext Anonymous { get; } = new(null, null);

    public string? GetCurrentUserId() => userId;

    public string? GetCurrentUserName() => userName;

    public bool IsAuthenticated() => userId is not null;

    public bool IsAdministrator() => false;

    public IReadOnlyList<string> GetRoleNames() => [];
}