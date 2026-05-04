using System;
using System.Collections.Generic;
using System.Text;

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
        IReadOnlyList<RawgPlatformDto>? RawgPlatforms
    );

    public sealed record RawgPlatformDto(
        RawgPlatform1Dto? Platform1,
        string? RawgReleasedAt,
        RawgRequirementsDto? RawgRequirements
    );

    public sealed record RawgPlatform1Dto(
        int RawgPlatform1Id,
        string? RawgPlatform1Slug,
        string? RawgPlatform1Name
     );

    public sealed record RawgRequirementsDto(
        string? RawgRequirementsMinimum,
         string? RawgRequirementsRecommended
     );
}
