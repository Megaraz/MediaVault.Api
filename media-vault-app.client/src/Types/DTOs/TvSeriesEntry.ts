import type {
    MediaEntryDetailedDto,
    MediaEntryCreateDto,
    MediaEntryUpdateDto
} from "./MediaEntryBase";
import type { TmdbGenreDto } from "./MovieEntry";
import type { SeasonCreateDto, SeasonMinimalDto } from "./Season";

// TV Series-specific fields on top of the shared base types.
export interface TvSeriesEntryDetailedDto extends MediaEntryDetailedDto {
    numberOfEpisodes: number;
    totalWatchedEpisodes: number; // How many the user has watched so far
    backdropImageUrl?: string | null;
    firstAirDate?: string | null;
    lastAirDate?: string | null;
    numberOfSeasons: number;
    airingStatus?: string | null;
    seasons?: SeasonMinimalDto[];
}

export interface TmdbTvSeriesDetailedDto {
    tmdbBackdropPath?: string | null;
    tmdbFirstAirDate?: string | null;
    tmdbGenres?: TmdbGenreDto[] | null;
    tmdbTvSeriesId: number;
    tmdbLastAirDate?: string | null;
    tmdbName?: string | null;
    tmdbNumberOfEpisodes: number;
    tmdbNumberOfSeasons: number;
    tmdbOverview?: string | null;
    tmdbPosterPath?: string | null;
    tmdbSeasons?: TmdbSeasonDto[] | null;
    tmdbStatus?: string | null;
}

export interface TmdbSeasonDto {
    tmdbAirDate?: string | null;
    tmdbEpisodeCount: number;
    tmdbName?: string | null;
    tmdbOverview?: string | null;
    tmdbPosterPath?: string | null;
    tmdbSeasonNumber: number;
}

export interface TvSeriesEntryCreateDto extends MediaEntryCreateDto {
    numberOfEpisodes: number;
    totalWatchedEpisodes: number;
    backdropImageUrl?: string | null;
    firstAirDate?: string | null;
    lastAirDate?: string | null;
    numberOfSeasons: number;
    airingStatus?: string | null;
    seasons?: SeasonCreateDto[]; // Optional initial seasons to create along with the series
}


export interface TvSeriesEntryUpdateDto extends MediaEntryUpdateDto {
    numberOfEpisodes: number;
    totalWatchedEpisodes: number;
    backdropImageUrl?: string | null;
    firstAirDate?: string | null;
    lastAirDate?: string | null;
    numberOfSeasons: number;
    airingStatus?: string | null;
    seasons?: SeasonCreateDto[];
}