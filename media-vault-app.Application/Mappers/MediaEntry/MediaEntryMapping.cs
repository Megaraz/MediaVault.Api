using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using media_vault_app.Application.DTOs.Season;
using media_vault_app.Domain.Entities;
using media_vault_app.Domain.Value_Objects;
using MediaEntryEntity = media_vault_app.Domain.Entities.MediaEntry;

namespace media_vault_app.Application.Mappers.MediaEntry;

public static class MediaEntryMapping
{
    public static MovieEntry ToMovie(MovieEntryCreateDto dto) => new()
    {
        Id = Guid.NewGuid(),
        IdExternal = dto.IdExternal,
        Status = dto.Status,
        Title = dto.Title,
        Rating = dto.Rating,
        Review = dto.Review,
        Genres = dto.Genres,
        Overview = dto.Overview,
        ReleaseDate = dto.ReleaseDate,
        ImageUrl = dto.ImageUrl,
        RuntimeMinutes = dto.RuntimeMinutes
    };

    public static MovieEntry ToMovie(Guid id, MovieEntryUpdateDto dto) => new()
    {
        Id = id,
        Version = dto.ExpectedVersion,
        IdExternal = dto.IdExternal,
        Status = dto.Status,
        Title = dto.Title,
        Rating = dto.Rating,
        Review = dto.Review,
        Genres = dto.Genres,
        Overview = dto.Overview,
        ReleaseDate = dto.ReleaseDate,
        ImageUrl = dto.ImageUrl,
        RuntimeMinutes = dto.RuntimeMinutes
    };

    public static TvSeriesEntry ToTvSeries(TvSeriesEntryCreateDto dto) => new()
    {
        Id = Guid.NewGuid(),
        IdExternal = dto.IdExternal,
        Status = dto.Status,
        Title = dto.Title,
        Rating = dto.Rating,
        Review = dto.Review,
        Genres = dto.Genres,
        Overview = dto.Overview,
        ReleaseDate = dto.ReleaseDate,
        ImageUrl = dto.ImageUrl,
        BackdropImageUrl = dto.BackdropImageUrl,
        LastAirDate = dto.LastAirDate,
        NumberOfSeasons = dto.NumberOfSeasons,
        NumberOfEpisodes = dto.NumberOfEpisodes,
        AiringStatus = dto.AiringStatus,
        TotalWatchedEpisodes = dto.TotalWatchedEpisodes,
        Seasons = dto.Seasons.Select(ToSeason).ToList()
    };

    public static TvSeriesEntry ToTvSeries(Guid id, TvSeriesEntryUpdateDto dto) => new()
    {
        Id = id,
        Version = dto.ExpectedVersion,
        IdExternal = dto.IdExternal,
        Status = dto.Status,
        Title = dto.Title,
        Rating = dto.Rating,
        Review = dto.Review,
        Genres = dto.Genres,
        Overview = dto.Overview,
        ReleaseDate = dto.ReleaseDate,
        ImageUrl = dto.ImageUrl,
        BackdropImageUrl = dto.BackdropImageUrl,
        LastAirDate = dto.LastAirDate,
        NumberOfSeasons = dto.NumberOfSeasons,
        NumberOfEpisodes = dto.NumberOfEpisodes,
        AiringStatus = dto.AiringStatus,
        TotalWatchedEpisodes = dto.TotalWatchedEpisodes,
        Seasons = dto.Seasons.Select(ToSeason).ToList()
    };

    public static GameEntry ToGame(GameEntryCreateDto dto) => new()
    {
        Id = Guid.NewGuid(),
        IdExternal = dto.IdExternal,
        Status = dto.Status,
        Title = dto.Title,
        Rating = dto.Rating,
        Review = dto.Review,
        Genres = dto.Genres,
        Overview = dto.Overview,
        ReleaseDate = dto.ReleaseDate,
        ImageUrl = dto.ImageUrl,
        HoursPlayed = dto.HoursPlayed,
        MetacriticRating = dto.MetacriticRating,
        Platforms = dto.Platforms,
        Website = dto.Website,
        PcRequirements = ToPcRequirements(dto.PcRequirements)
    };

    public static GameEntry ToGame(Guid id, GameEntryUpdateDto dto) => new()
    {
        Id = id,
        Version = dto.ExpectedVersion,
        IdExternal = dto.IdExternal,
        Status = dto.Status,
        Title = dto.Title,
        Rating = dto.Rating,
        Review = dto.Review,
        Genres = dto.Genres,
        Overview = dto.Overview,
        ReleaseDate = dto.ReleaseDate,
        ImageUrl = dto.ImageUrl,
        HoursPlayed = dto.HoursPlayed,
        MetacriticRating = dto.MetacriticRating,
        Platforms = dto.Platforms,
        Website = dto.Website,
        PcRequirements = ToPcRequirements(dto.PcRequirements)
    };

    public static BookEntry ToBook(BookEntryCreateDto dto) => new()
    {
        Id = Guid.NewGuid(),
        IdExternal = dto.IdExternal,
        Status = dto.Status,
        Title = dto.Title,
        Rating = dto.Rating,
        Review = dto.Review,
        Genres = dto.Genres,
        Overview = dto.Overview,
        ReleaseDate = dto.ReleaseDate,
        ImageUrl = dto.ImageUrl,
        Author = dto.Author
    };

    public static BookEntry ToBook(Guid id, BookEntryUpdateDto dto) => new()
    {
        Id = id,
        Version = dto.ExpectedVersion,
        IdExternal = dto.IdExternal,
        Status = dto.Status,
        Title = dto.Title,
        Rating = dto.Rating,
        Review = dto.Review,
        Genres = dto.Genres,
        Overview = dto.Overview,
        ReleaseDate = dto.ReleaseDate,
        ImageUrl = dto.ImageUrl,
        Author = dto.Author
    };

    public static MangaEntry ToManga(MangaEntryCreateDto dto) => new()
    {
        Id = Guid.NewGuid(),
        IdExternal = dto.IdExternal,
        Status = dto.Status,
        Title = dto.Title,
        Rating = dto.Rating,
        Review = dto.Review,
        Genres = dto.Genres,
        Overview = dto.Overview,
        ReleaseDate = dto.ReleaseDate,
        ImageUrl = dto.ImageUrl,
        Author = dto.Author
    };

    public static MangaEntry ToManga(Guid id, MangaEntryUpdateDto dto) => new()
    {
        Id = id,
        Version = dto.ExpectedVersion,
        IdExternal = dto.IdExternal,
        Status = dto.Status,
        Title = dto.Title,
        Rating = dto.Rating,
        Review = dto.Review,
        Genres = dto.Genres,
        Overview = dto.Overview,
        ReleaseDate = dto.ReleaseDate,
        ImageUrl = dto.ImageUrl,
        Author = dto.Author
    };

    public static MediaEntryDetailedDto ToDetailed(MediaEntryEntity entity) => entity switch
    {
        MovieEntry movie => ToMovie(movie),
        TvSeriesEntry tvSeries => ToTvSeries(tvSeries),
        GameEntry game => ToGame(game),
        BookEntry book => ToBook(book),
        MangaEntry manga => ToManga(manga),
        _ => throw new NotSupportedException($"Unknown media entity type: {entity.GetType().Name}")
    };

    public static MovieEntryDetailedDto ToMovie(MovieEntry movie) => new()
    {
        Id = movie.Id,
        IdExternal = movie.IdExternal,
        UserId = movie.OwnerId,
        Status = movie.Status,
        Title = movie.Title,
        Rating = movie.Rating,
        Review = movie.Review,
        Genres = movie.Genres,
        Overview = movie.Overview,
        ReleaseDate = movie.ReleaseDate ?? default,
        ImageUrl = movie.ImageUrl,
        CreatedAtUtc = movie.CreatedAtUtc,
        UpdatedAtUtc = movie.UpdatedAtUtc,
        Version = movie.Version,
        RuntimeMinutes = movie.RuntimeMinutes
    };

    public static TvSeriesEntryDetailedDto ToTvSeries(TvSeriesEntry tvSeries) => new()
    {
        Id = tvSeries.Id,
        IdExternal = tvSeries.IdExternal,
        UserId = tvSeries.OwnerId,
        Status = tvSeries.Status,
        Title = tvSeries.Title,
        Rating = tvSeries.Rating,
        Review = tvSeries.Review,
        Genres = tvSeries.Genres,
        Overview = tvSeries.Overview,
        ReleaseDate = tvSeries.ReleaseDate ?? default,
        ImageUrl = tvSeries.ImageUrl,
        CreatedAtUtc = tvSeries.CreatedAtUtc,
        UpdatedAtUtc = tvSeries.UpdatedAtUtc,
        Version = tvSeries.Version,
        BackdropImageUrl = tvSeries.BackdropImageUrl,
        LastAirDate = tvSeries.LastAirDate,
        NumberOfSeasons = tvSeries.NumberOfSeasons,
        NumberOfEpisodes = tvSeries.NumberOfEpisodes,
        AiringStatus = tvSeries.AiringStatus,
        TotalWatchedEpisodes = tvSeries.TotalWatchedEpisodes,
        Seasons = tvSeries.Seasons.Select(ToSeason).ToList()
    };

    public static GameEntryDetailedDto ToGame(GameEntry game) => new()
    {
        Id = game.Id,
        IdExternal = game.IdExternal,
        UserId = game.OwnerId,
        Status = game.Status,
        Title = game.Title,
        Rating = game.Rating,
        Review = game.Review,
        Genres = game.Genres,
        Overview = game.Overview,
        ReleaseDate = game.ReleaseDate ?? default,
        ImageUrl = game.ImageUrl,
        CreatedAtUtc = game.CreatedAtUtc,
        UpdatedAtUtc = game.UpdatedAtUtc,
        Version = game.Version,
        HoursPlayed = game.HoursPlayed,
        MetacriticRating = game.MetacriticRating,
        Platforms = game.Platforms,
        Website = game.Website,
        PcRequirements = game.PcRequirements is { } requirements
            ? new GamePcRequirementsDto(
                requirements.Minimum,
                requirements.Recommended,
                requirements.High,
                requirements.VeryHigh,
                requirements.Ultra)
            : null
    };

    public static BookEntryDetailedDto ToBook(BookEntry book) => new()
    {
        Id = book.Id,
        IdExternal = book.IdExternal,
        UserId = book.OwnerId,
        Status = book.Status,
        Title = book.Title,
        Rating = book.Rating,
        Review = book.Review,
        Genres = book.Genres,
        Overview = book.Overview,
        ReleaseDate = book.ReleaseDate ?? default,
        ImageUrl = book.ImageUrl,
        CreatedAtUtc = book.CreatedAtUtc,
        UpdatedAtUtc = book.UpdatedAtUtc,
        Version = book.Version,
        Author = book.Author
    };

    public static MangaEntryDetailedDto ToManga(MangaEntry manga) => new()
    {
        Id = manga.Id,
        IdExternal = manga.IdExternal,
        UserId = manga.OwnerId,
        Status = manga.Status,
        Title = manga.Title,
        Rating = manga.Rating,
        Review = manga.Review,
        Genres = manga.Genres,
        Overview = manga.Overview,
        ReleaseDate = manga.ReleaseDate ?? default,
        ImageUrl = manga.ImageUrl,
        CreatedAtUtc = manga.CreatedAtUtc,
        UpdatedAtUtc = manga.UpdatedAtUtc,
        Version = manga.Version,
        Author = manga.Author
    };

    public static MediaEntryMinimalDto ToMinimal(MediaEntryEntity entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        ImageUrl = entity.ImageUrl,
        Rating = entity.Rating,
        ReleaseDate = entity.ReleaseDate ?? DateOnly.MinValue,
        Genres = entity.Genres,
        MediaType = entity.MediaType,
        Status = entity.Status,
        CreatedAtUtc = entity.CreatedAtUtc,
        UpdatedAtUtc = entity.UpdatedAtUtc,
        Version = entity.Version
    };

    private static Season ToSeason(SeasonCreateDto dto) => new()
    {
        Id = Guid.NewGuid(),
        TvSeriesEntryId = dto.TvSeriesId,
        IdExternal = dto.IdExternal,
        Name = dto.Name,
        Overview = dto.Overview,
        ImageUrl = dto.ImageUrl,
        SeasonNumber = dto.SeasonNumber,
        AirDate = dto.AirDate,
        WatchedEpisodes = dto.WatchedEpisodes,
        Episodes = dto.Episodes,
        Status = dto.Status,
        Rating = dto.Rating
    };

    private static Season ToSeason(SeasonUpdateDto dto) => new()
    {
        Id = dto.Id,
        TvSeriesEntryId = dto.TvSeriesId,
        IdExternal = dto.IdExternal,
        Name = dto.Name,
        Overview = dto.Overview,
        ImageUrl = dto.ImageUrl,
        SeasonNumber = dto.SeasonNumber,
        AirDate = dto.AirDate,
        WatchedEpisodes = dto.WatchedEpisodes,
        Episodes = dto.Episodes,
        Status = dto.Status,
        Rating = dto.Rating
    };

    private static SeasonMinimalDto ToSeason(Season season) => new()
    {
        Id = season.Id,
        TvSeriesId = season.TvSeriesEntryId,
        IdExternal = season.IdExternal,
        Name = season.Name,
        Overview = season.Overview,
        ImageUrl = season.ImageUrl,
        SeasonNumber = season.SeasonNumber,
        AirDate = season.AirDate,
        WatchedEpisodes = season.WatchedEpisodes,
        Episodes = season.Episodes,
        Status = season.Status,
        Rating = season.Rating,
        CreatedAtUtc = season.CreatedAtUtc,
        UpdatedAtUtc = season.UpdatedAtUtc
    };

    private static GamePcRequirements? ToPcRequirements(GamePcRequirementsDto? dto) => dto is null
        ? null
        : new GamePcRequirements(
            dto.Minimum,
            dto.Recommended,
            dto.High,
            dto.VeryHigh,
            dto.Ultra);
}
