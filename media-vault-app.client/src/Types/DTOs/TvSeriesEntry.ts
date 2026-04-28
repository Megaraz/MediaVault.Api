import type { 
    MediaEntryDetailedDto, 
    MediaEntryCreateDto 
} from "./MediaEntryBase";

export interface TvSeriesEntryDetailedDto extends MediaEntryDetailedDto {
  totalEpisodes: number;
  totalWatchedEpisodes: number;
}

export interface TvSeriesEntryCreateDto extends MediaEntryCreateDto {
  totalEpisodes: number;
  totalWatchedEpisodes: number;
}