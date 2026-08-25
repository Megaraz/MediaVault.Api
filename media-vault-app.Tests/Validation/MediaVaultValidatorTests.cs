using media_vault_app.Application.Validation;

namespace media_vault_app.Tests.Validation;

public class MediaVaultValidatorIsValidIdTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ReturnsFalseForInvalidInteger(int id) =>
        Assert.False(MediaVaultValidator.IsValidId(id));

    [Fact]
    public void ReturnsFalseForNullInteger() =>
        Assert.False(MediaVaultValidator.IsValidId<int?>(null));

    [Fact]
    public void ReturnsTrueForValidInteger() =>
        Assert.True(MediaVaultValidator.IsValidId(123));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnsFalseForMissingString(string? id) =>
        Assert.False(MediaVaultValidator.IsValidId(id));

    [Fact]
    public void ReturnsTrueForValidString() =>
        Assert.True(MediaVaultValidator.IsValidId("valid-id"));

    [Fact]
    public void ReturnsFalseForEmptyGuid() =>
        Assert.False(MediaVaultValidator.IsValidId(Guid.Empty));

    [Fact]
    public void ReturnsFalseForNullGuid() =>
        Assert.False(MediaVaultValidator.IsValidId<Guid?>(null));

    [Fact]
    public void ReturnsTrueForValidGuid() =>
        Assert.True(MediaVaultValidator.IsValidId(Guid.NewGuid()));

    [Fact]
    public void ReturnsFalseForDefaultLong() =>
        Assert.False(MediaVaultValidator.IsValidId(default(long)));

    [Fact]
    public void ReturnsTrueForValidLong() =>
        Assert.True(MediaVaultValidator.IsValidId(1L));
}
