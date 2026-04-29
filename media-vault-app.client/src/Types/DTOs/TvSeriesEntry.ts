import type {
    MediaEntryDetailedDto,
    MediaEntryCreateDto,
    MediaEntryUpdateDto
} from "./MediaEntryBase";

// TV Series-specific fields on top of the shared base types.
export interface TvSeriesEntryDetailedDto extends MediaEntryDetailedDto {
    totalEpisodes: number;
    totalWatchedEpisodes: number; // How many the user has watched so far
}

export interface TvSeriesEntryCreateDto extends MediaEntryCreateDto {
    totalEpisodes: number;
    totalWatchedEpisodes: number;
}

export interface TvSeriesEntryUpdateDto extends MediaEntryUpdateDto {
    totalEpisodes: number;
    totalWatchedEpisodes: number;
}