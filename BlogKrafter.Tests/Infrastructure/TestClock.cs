namespace BlogKrafter.Tests.Infrastructure;

/// <summary>A clock the test moves by hand, so scheduled posts can be tested without waiting.</summary>
public sealed class TestClock(DateTime utcNow) : TimeProvider
{
    public DateTime UtcNow { get; set; } = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

    public override DateTimeOffset GetUtcNow() => new(UtcNow);

    public void Advance(TimeSpan by) => UtcNow += by;
}

public sealed class TestTenant(string tenantId = "") : IBlogTenantProvider
{
    public string TenantId { get; set; } = tenantId;

    public string GetTenantId() => TenantId;
}
