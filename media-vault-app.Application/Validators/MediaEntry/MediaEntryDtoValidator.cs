using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.DTOs.Season;
using media_vault_app.Application.Interfaces.Validators;
using media_vault_app.Application.Validation;
using media_vault_app.Domain.Enums;
using Megaraz.ResultPattern;

namespace media_vault_app.Application.Validators.MediaEntry;

public class MediaEntryDtoValidator : IMediaEntryDtoValidator
{
    public bool IsValidCreateDto(
        MediaEntryCreateDto createDto,
        ErrorContext errorContext,
        out IReadOnlyList<ValidationError> validationErrors)
    {
        var internalErrors = new List<ValidationError>();

        if (MediaVaultValidatorExtensions.IsMediaVaultNull(createDto, errorContext, out var nullValueError))
        {
            internalErrors.Add(nullValueError);
            validationErrors = internalErrors;
            return false;
        }

        ValidateCommonFields(
            internalErrors,
            errorContext,
            createDto.IdExternal,
            createDto.Status,
            createDto.Title,
            createDto.Rating,
            createDto.Review,
            createDto.Genres,
            createDto.Overview,
            createDto.ImageUrl);

        switch (createDto)
        {
            case MovieEntryCreateDto movie:
                ValidateMovie(internalErrors, errorContext, movie.RuntimeMinutes);
                break;
            case TvSeriesEntryCreateDto tvSeries:
                ValidateTvSeries(
                    internalErrors,
                    errorContext,
                    tvSeries.BackdropImageUrl,
                    tvSeries.NumberOfSeasons,
                    tvSeries.NumberOfEpisodes,
                    tvSeries.AiringStatus,
                    tvSeries.TotalWatchedEpisodes,
                    tvSeries.Seasons);
                break;
            case GameEntryCreateDto game:
                ValidateGame(
                    internalErrors,
                    errorContext,
                    game.HoursPlayed,
                    game.MetacriticRating,
                    game.Website,
                    game.Platforms,
                    game.PcRequirements);
                break;
            case BookEntryCreateDto book:
                ValidateAuthor(internalErrors, errorContext, book.Author);
                break;
            case MangaEntryCreateDto manga:
                ValidateAuthor(internalErrors, errorContext, manga.Author);
                break;
        }

        validationErrors = internalErrors;
        return internalErrors.Count == 0;
    }

    public bool IsValidUpdateDto(
        MediaEntryUpdateDto updateDto,
        ErrorContext errorContext,
        out IReadOnlyList<ValidationError> validationErrors)
    {
        var internalErrors = new List<ValidationError>();

        if (updateDto.IsMediaVaultNull(errorContext, out var nullValueError))
        {
            internalErrors.Add(nullValueError);
            validationErrors = internalErrors;
            return false;
        }

        ValidateCommonFields(
            internalErrors,
            errorContext,
            updateDto.IdExternal,
            updateDto.Status,
            updateDto.Title,
            updateDto.Rating,
            updateDto.Review,
            updateDto.Genres,
            updateDto.Overview,
            updateDto.ImageUrl);
        MediaVaultWriteValidation.AddIntegerRange(
            internalErrors,
            updateDto.ExpectedVersion,
            errorContext,
            nameof(updateDto.ExpectedVersion),
            1,
            int.MaxValue - 1);

        switch (updateDto)
        {
            case MovieEntryUpdateDto movie:
                ValidateMovie(internalErrors, errorContext, movie.RuntimeMinutes);
                break;
            case TvSeriesEntryUpdateDto tvSeries:
                ValidateTvSeries(
                    internalErrors,
                    errorContext,
                    tvSeries.BackdropImageUrl,
                    tvSeries.NumberOfSeasons,
                    tvSeries.NumberOfEpisodes,
                    tvSeries.AiringStatus,
                    tvSeries.TotalWatchedEpisodes,
                    tvSeries.Seasons);
                break;
            case GameEntryUpdateDto game:
                ValidateGame(
                    internalErrors,
                    errorContext,
                    game.HoursPlayed,
                    game.MetacriticRating,
                    game.Website,
                    game.Platforms,
                    game.PcRequirements);
                break;
            case BookEntryUpdateDto book:
                ValidateAuthor(internalErrors, errorContext, book.Author);
                break;
            case MangaEntryUpdateDto manga:
                ValidateAuthor(internalErrors, errorContext, manga.Author);
                break;
        }

        validationErrors = internalErrors;
        return internalErrors.Count == 0;
    }

    private static void ValidateCommonFields(
        List<ValidationError> errors,
        ErrorContext errorContext,
        string? idExternal,
        Status status,
        string? title,
        decimal rating,
        string? review,
        IEnumerable<string?>? genres,
        string? overview,
        string? imageUrl)
    {
        MediaVaultWriteValidation.AddText(
            errors,
            idExternal,
            errorContext,
            nameof(MediaEntryCreateDto.IdExternal),
            MediaVaultWriteValidationPolicy.ExternalIdMaxLength);
        MediaVaultWriteValidation.AddEnum(
            errors,
            status,
            errorContext,
            nameof(MediaEntryCreateDto.Status));
        MediaVaultWriteValidation.AddText(
            errors,
            title,
            errorContext,
            nameof(MediaEntryCreateDto.Title),
            MediaVaultWriteValidationPolicy.TitleMaxLength,
            required: true);
        MediaVaultWriteValidation.AddRating(
            errors,
            rating,
            errorContext,
            nameof(MediaEntryCreateDto.Rating));
        MediaVaultWriteValidation.AddText(
            errors,
            review,
            errorContext,
            nameof(MediaEntryCreateDto.Review),
            MediaVaultWriteValidationPolicy.ReviewMaxLength);
        MediaVaultWriteValidation.AddStringCollection(
            errors,
            genres,
            errorContext,
            nameof(MediaEntryCreateDto.Genres),
            MediaVaultWriteValidationPolicy.MaxGenres);
        MediaVaultWriteValidation.AddText(
            errors,
            overview,
            errorContext,
            nameof(MediaEntryCreateDto.Overview),
            MediaVaultWriteValidationPolicy.OverviewMaxLength);
        MediaVaultWriteValidation.AddUrl(
            errors,
            imageUrl,
            errorContext,
            nameof(MediaEntryCreateDto.ImageUrl));
    }

    private static void ValidateMovie(
        List<ValidationError> errors,
        ErrorContext errorContext,
        int runtimeMinutes)
    {
        MediaVaultWriteValidation.AddIntegerRange(
            errors,
            runtimeMinutes,
            errorContext,
            nameof(MovieEntryCreateDto.RuntimeMinutes),
            0,
            MediaVaultWriteValidationPolicy.MaximumRuntimeMinutes);
    }

    private static void ValidateTvSeries(
        List<ValidationError> errors,
        ErrorContext errorContext,
        string? backdropImageUrl,
        int numberOfSeasons,
        int numberOfEpisodes,
        string? airingStatus,
        int totalWatchedEpisodes,
        IEnumerable<SeasonCreateDto>? seasons)
    {
        ValidateTvSeriesFields(
            errors,
            errorContext,
            backdropImageUrl,
            numberOfSeasons,
            numberOfEpisodes,
            airingStatus,
            totalWatchedEpisodes);

        var items = MediaVaultWriteValidation.MaterializeCollection(
            errors,
            seasons,
            errorContext,
            nameof(TvSeriesEntryCreateDto.Seasons),
            MediaVaultWriteValidationPolicy.MaxSeasons);
        if (items is null)
            return;

        for (var index = 0; index < items.Count; index++)
        {
            ValidateSeason(
                errors,
                errorContext,
                items[index],
                $"{nameof(TvSeriesEntryCreateDto.Seasons)}[{index}]");
        }
    }

    private static void ValidateTvSeries(
        List<ValidationError> errors,
        ErrorContext errorContext,
        string? backdropImageUrl,
        int numberOfSeasons,
        int numberOfEpisodes,
        string? airingStatus,
        int totalWatchedEpisodes,
        IEnumerable<SeasonUpdateDto>? seasons)
    {
        ValidateTvSeriesFields(
            errors,
            errorContext,
            backdropImageUrl,
            numberOfSeasons,
            numberOfEpisodes,
            airingStatus,
            totalWatchedEpisodes);

        var items = MediaVaultWriteValidation.MaterializeCollection(
            errors,
            seasons,
            errorContext,
            nameof(TvSeriesEntryUpdateDto.Seasons),
            MediaVaultWriteValidationPolicy.MaxSeasons);
        if (items is null)
            return;

        for (var index = 0; index < items.Count; index++)
        {
            ValidateSeason(
                errors,
                errorContext,
                items[index],
                $"{nameof(TvSeriesEntryUpdateDto.Seasons)}[{index}]");
        }
    }

    private static void ValidateTvSeriesFields(
        List<ValidationError> errors,
        ErrorContext errorContext,
        string? backdropImageUrl,
        int numberOfSeasons,
        int numberOfEpisodes,
        string? airingStatus,
        int totalWatchedEpisodes)
    {
        MediaVaultWriteValidation.AddUrl(
            errors,
            backdropImageUrl,
            errorContext,
            nameof(TvSeriesEntryCreateDto.BackdropImageUrl));
        MediaVaultWriteValidation.AddIntegerRange(
            errors,
            numberOfSeasons,
            errorContext,
            nameof(TvSeriesEntryCreateDto.NumberOfSeasons),
            0,
            MediaVaultWriteValidationPolicy.MaximumSeasons);
        MediaVaultWriteValidation.AddIntegerRange(
            errors,
            numberOfEpisodes,
            errorContext,
            nameof(TvSeriesEntryCreateDto.NumberOfEpisodes),
            0,
            MediaVaultWriteValidationPolicy.MaximumEpisodes);
        MediaVaultWriteValidation.AddText(
            errors,
            airingStatus,
            errorContext,
            nameof(TvSeriesEntryCreateDto.AiringStatus),
            MediaVaultWriteValidationPolicy.AiringStatusMaxLength);
        MediaVaultWriteValidation.AddIntegerRange(
            errors,
            totalWatchedEpisodes,
            errorContext,
            nameof(TvSeriesEntryCreateDto.TotalWatchedEpisodes),
            0,
            MediaVaultWriteValidationPolicy.MaximumEpisodes);

        if (totalWatchedEpisodes > numberOfEpisodes)
        {
            errors.Add(MediaVaultValidationError.OutOfRange(
                errorContext with { FieldName = nameof(TvSeriesEntryCreateDto.TotalWatchedEpisodes) },
                $"<= {nameof(TvSeriesEntryCreateDto.NumberOfEpisodes)}"));
        }
    }

    private static void ValidateSeason(
        List<ValidationError> errors,
        ErrorContext errorContext,
        SeasonCreateDto season,
        string fieldPrefix)
    {
        ValidateSeasonFields(
            errors,
            errorContext,
            season.IdExternal,
            season.Name,
            season.Overview,
            season.ImageUrl,
            season.SeasonNumber,
            season.WatchedEpisodes,
            season.Episodes,
            season.Status,
            season.Rating,
            fieldPrefix);
    }

    private static void ValidateSeason(
        List<ValidationError> errors,
        ErrorContext errorContext,
        SeasonUpdateDto season,
        string fieldPrefix)
    {
        ValidateSeasonFields(
            errors,
            errorContext,
            season.IdExternal,
            season.Name,
            season.Overview,
            season.ImageUrl,
            season.SeasonNumber,
            season.WatchedEpisodes,
            season.Episodes,
            season.Status,
            season.Rating,
            fieldPrefix);
    }

    private static void ValidateSeasonFields(
        List<ValidationError> errors,
        ErrorContext errorContext,
        string? idExternal,
        string? name,
        string? overview,
        string? imageUrl,
        int seasonNumber,
        int watchedEpisodes,
        int episodes,
        Status status,
        decimal rating,
        string fieldPrefix)
    {
        MediaVaultWriteValidation.AddText(
            errors,
            idExternal,
            errorContext,
            $"{fieldPrefix}.{nameof(SeasonCreateDto.IdExternal)}",
            MediaVaultWriteValidationPolicy.ExternalIdMaxLength);
        MediaVaultWriteValidation.AddText(
            errors,
            name,
            errorContext,
            $"{fieldPrefix}.{nameof(SeasonCreateDto.Name)}",
            MediaVaultWriteValidationPolicy.TitleMaxLength);
        MediaVaultWriteValidation.AddText(
            errors,
            overview,
            errorContext,
            $"{fieldPrefix}.{nameof(SeasonCreateDto.Overview)}",
            MediaVaultWriteValidationPolicy.OverviewMaxLength);
        MediaVaultWriteValidation.AddUrl(
            errors,
            imageUrl,
            errorContext,
            $"{fieldPrefix}.{nameof(SeasonCreateDto.ImageUrl)}");
        MediaVaultWriteValidation.AddIntegerRange(
            errors,
            seasonNumber,
            errorContext,
            $"{fieldPrefix}.{nameof(SeasonCreateDto.SeasonNumber)}",
            0,
            MediaVaultWriteValidationPolicy.MaximumSeasons);
        MediaVaultWriteValidation.AddIntegerRange(
            errors,
            watchedEpisodes,
            errorContext,
            $"{fieldPrefix}.{nameof(SeasonCreateDto.WatchedEpisodes)}",
            0,
            MediaVaultWriteValidationPolicy.MaximumEpisodes);
        MediaVaultWriteValidation.AddIntegerRange(
            errors,
            episodes,
            errorContext,
            $"{fieldPrefix}.{nameof(SeasonCreateDto.Episodes)}",
            0,
            MediaVaultWriteValidationPolicy.MaximumEpisodes);
        MediaVaultWriteValidation.AddEnum(
            errors,
            status,
            errorContext,
            $"{fieldPrefix}.{nameof(SeasonCreateDto.Status)}");
        MediaVaultWriteValidation.AddRating(
            errors,
            rating,
            errorContext,
            $"{fieldPrefix}.{nameof(SeasonCreateDto.Rating)}");

        if (watchedEpisodes > episodes)
        {
            errors.Add(MediaVaultValidationError.OutOfRange(
                errorContext with { FieldName = $"{fieldPrefix}.{nameof(SeasonCreateDto.WatchedEpisodes)}" },
                $"<= {nameof(SeasonCreateDto.Episodes)}"));
        }
    }

    private static void ValidateGame(
        List<ValidationError> errors,
        ErrorContext errorContext,
        int hoursPlayed,
        int metacriticRating,
        string? website,
        IEnumerable<string?>? platforms,
        GamePcRequirementsDto? pcRequirements)
    {
        MediaVaultWriteValidation.AddIntegerRange(
            errors,
            hoursPlayed,
            errorContext,
            nameof(GameEntryCreateDto.HoursPlayed),
            0,
            MediaVaultWriteValidationPolicy.MaximumHoursPlayed);
        MediaVaultWriteValidation.AddIntegerRange(
            errors,
            metacriticRating,
            errorContext,
            nameof(GameEntryCreateDto.MetacriticRating),
            0,
            MediaVaultWriteValidationPolicy.MaximumMetacriticRating);
        MediaVaultWriteValidation.AddUrl(
            errors,
            website,
            errorContext,
            nameof(GameEntryCreateDto.Website));
        MediaVaultWriteValidation.AddStringCollection(
            errors,
            platforms,
            errorContext,
            nameof(GameEntryCreateDto.Platforms),
            MediaVaultWriteValidationPolicy.MaxPlatforms);
        ValidatePcRequirements(errors, errorContext, pcRequirements);
    }

    private static void ValidatePcRequirements(
        List<ValidationError> errors,
        ErrorContext errorContext,
        GamePcRequirementsDto? pcRequirements)
    {
        if (pcRequirements is null)
            return;

        AddPcRequirement(errors, errorContext, nameof(GamePcRequirementsDto.Minimum), pcRequirements.Minimum);
        AddPcRequirement(errors, errorContext, nameof(GamePcRequirementsDto.Recommended), pcRequirements.Recommended);
        AddPcRequirement(errors, errorContext, nameof(GamePcRequirementsDto.High), pcRequirements.High);
        AddPcRequirement(errors, errorContext, nameof(GamePcRequirementsDto.VeryHigh), pcRequirements.VeryHigh);
        AddPcRequirement(errors, errorContext, nameof(GamePcRequirementsDto.Ultra), pcRequirements.Ultra);
    }

    private static void AddPcRequirement(
        List<ValidationError> errors,
        ErrorContext errorContext,
        string fieldName,
        string? value)
    {
        MediaVaultWriteValidation.AddText(
            errors,
            value,
            errorContext,
            $"{nameof(GameEntryCreateDto.PcRequirements)}.{fieldName}",
            MediaVaultWriteValidationPolicy.PcRequirementMaxLength);
    }

    private static void ValidateAuthor(
        List<ValidationError> errors,
        ErrorContext errorContext,
        string? author)
    {
        MediaVaultWriteValidation.AddText(
            errors,
            author,
            errorContext,
            nameof(BookEntryCreateDto.Author),
            MediaVaultWriteValidationPolicy.AuthorMaxLength);
    }
}
