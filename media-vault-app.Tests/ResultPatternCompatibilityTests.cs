using media_vault_app.Application.Interfaces.Clients;

namespace media_vault_app.Tests;

public class ResultPatternContractTests
{
    [Fact]
    public void ApplicationContractsAreBoundToPackageResultTypes()
    {
        var method = typeof(IRawgApiClient).GetMethod(nameof(IRawgApiClient.SearchGamesAsync));

        Assert.NotNull(method);
        var resultType = Assert.Single(method.ReturnType.GetGenericArguments());

        Assert.True(resultType.IsGenericType);
        Assert.Equal(typeof(Megaraz.ResultPattern.Result<>), resultType.GetGenericTypeDefinition());
    }
}
