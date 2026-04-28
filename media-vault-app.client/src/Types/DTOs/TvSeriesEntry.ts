import type {
    MediaEntryDetailedDto,
    MediaEntryCreateDto,
    MediaEntryUpdateDto
} from "./MediaEntryBase";

export interface TvSeriesEntryDetailedDto extends MediaEntryDetailedDto {
    totalEpisodes: number;
    totalWatchedEpisodes: number;
}

export interface TvSeriesEntryCreateDto extends MediaEntryCreateDto {
    totalEpisodes: number;
    totalWatchedEpisodes: number;
}

export interface TvSeriesEntryUpdateDto extends MediaEntryUpdateDto {
    totalEpisodes: number;
    totalWatchedEpisodes: number;
}