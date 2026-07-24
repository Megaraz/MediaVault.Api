using media_vault_app.Application.DTOs.MediaEntry.Response;

namespace media_vault_app.Application.DTOs.Rawg
{
    public sealed record RawgGameDetailedDto(
        int RawgId,
        string? RawgSlug,
        string? RawgName,
        string? RawgDescription,
        int RawgMetacritic,
        string? RawgReleased,
        string? RawgBackgroundImage,
        string? RawgWebsite,
        IReadOnlyList<string> RawgPlatforms,
        GamePcRequirementsDto? RawgRequirements
    );

    //public sealed record RawgPlatformDto(
    //    RawgPlatform1Dto? Platform1,
    //    string? RawgReleasedAt,
    //    RawgRequirementsDto? RawgRequirements
    //);

    //public sealed record RawgPlatform1Dto(
    //    int RawgPlatform1Id,
    //    string? RawgPlatform1Slug,
    //    string? RawgPlatform1Name
    // );

    //public sealed record RawgRequirementsDto(
    //    string? RawgRequirementsMinimum,
    //     string? RawgRequirementsRecommended
    // );
}
