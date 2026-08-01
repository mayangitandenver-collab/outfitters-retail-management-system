using Outfitters.Domain.Common;

namespace Outfitters.Tests;

public sealed class BaseEntityTests
{
    private sealed class TestEntity : BaseEntity
    {
    }

    [Fact]
    public void New_entity_has_identifier_and_utc_created_timestamp()
    {
        var entity = new TestEntity();

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.True(entity.CreatedAtUtc <= DateTime.UtcNow);
        Assert.False(entity.IsDeleted);
    }
}
