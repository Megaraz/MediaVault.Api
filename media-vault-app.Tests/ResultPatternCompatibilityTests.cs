using media_vault_app.Application.Interfaces.Clients;

namespace media_vault_app.Tests;

public class ResultPatternCompatibilityTests
{
    [Fact]
    public void ApplicationContracts_RemainBoundToLegacyResultTypesDuringCoexistence()
    {
        var method = typeof(IRawgApiClient).GetMethod(nameof(IRawgApiClient.SearchGamesAsync));

        Assert.NotNull(method);
        var resultType = Assert.Single(method.ReturnType.GetGenericArguments());

        Assert.True(resultType.IsGenericType);
        Assert.Equal(
            typeof(Rasmus.SharedKernel.ResultPattern.Result<>),
            resultType.GetGenericTypeDefinition());
        Assert.NotEqual(
            typeof(Megaraz.ResultPattern.Result<>),
            resultType.GetGenericTypeDefinition());
    }
}
