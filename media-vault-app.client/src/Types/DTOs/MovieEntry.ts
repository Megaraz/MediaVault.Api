import type {
    MediaEntryDetailedDto,
    MediaEntryCreateDto,
    MediaEntryUpdateDto
} from "./MediaEntryBase";

// Movie-specific fields on top of the shared base types.
export interface MovieEntryDetailedDto extends MediaEntryDetailedDto {
    runtimeMinutes: number;
}
export interface TmdbMovieDetailedDto {
    tmdbBackdropPath?: string;
    tmdbReleaseDate?: string;
    tmdbGenres: TmdbGenreDto[];
    tmdbMovieId: number;
    tmdbOverview?: string;
    tmdbPosterPath?: string;
    tmdbTitle?: string;
    tmdbRunTimeMinutes?: number;
}

export interface TmdbGenreDto {
    tmdbGenreId: number;
    tmdbGenreName?: string;
}


export interface MovieEntryCreateDto extends MediaEntryCreateDto {
    runtimeMinutes: number;
}

export interface MovieEntryUpdateDto extends MediaEntryUpdateDto {
    runtimeMinutes: number;
}