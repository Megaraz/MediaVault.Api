using System.Reflection;
using media_vault_app.Application.Interfaces.Clients;
using Rasmus.SharedKernel.Interfaces.Services.Repositories;

namespace media_vault_app.Tests;

public class ResultPatternCompatibilityTests
{
    [Fact]
    public void ApplicationContracts_AreBoundToPackageResultTypes()
    {
        var method = typeof(IRawgApiClient).GetMethod(nameof(IRawgApiClient.SearchGamesAsync));

        Assert.NotNull(method);
        var resultType = Assert.Single(method.ReturnType.GetGenericArguments());

        Assert.True(resultType.IsGenericType);
        Assert.Equal(typeof(Megaraz.ResultPattern.Result<>), resultType.GetGenericTypeDefinition());
        Assert.NotEqual(typeof(Rasmus.SharedKernel.ResultPattern.Result<>), resultType.GetGenericTypeDefinition());
    }

    [Theory]
    [InlineData(typeof(IRawgApiClient))]
    [InlineData(typeof(IRepo<,>))]
    public void PublicContracts_DoNotExposeLegacyCoreResultPatternTypes(Type markerType)
    {
        var legacyTypeNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "Rasmus.SharedKernel.ResultPattern.Result",
            "Rasmus.SharedKernel.ResultPattern.Error",
            "Rasmus.SharedKernel.ResultPattern.ErrorContext",
            "Rasmus.SharedKernel.ResultPattern.ValidationError"
        };

        var exposedTypes = markerType.Assembly
            .GetExportedTypes()
            .Where(type => type.Namespace?.Contains(".Interfaces", StringComparison.Ordinal) == true)
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType)
                    .Append(method.ReturnType))
                .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Select(property => property.PropertyType)));

        Assert.DoesNotContain(exposedTypes, type => ContainsLegacyCoreType(type, legacyTypeNames));
    }

    private static bool ContainsLegacyCoreType(Type type, IReadOnlySet<string> legacyTypeNames)
    {
        if (type.IsByRef || type.IsArray || type.IsPointer)
            return ContainsLegacyCoreType(type.GetElementType()!, legacyTypeNames);

        var definition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        if (definition.FullName is not null && legacyTypeNames.Contains(definition.FullName))
            return true;

        return type.IsGenericType && type.GetGenericArguments()
            .Any(argument => ContainsLegacyCoreType(argument, legacyTypeNames));
    }
}
