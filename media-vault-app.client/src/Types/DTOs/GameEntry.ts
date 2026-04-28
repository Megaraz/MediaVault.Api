import type { 
    MediaEntryDetailedDto, 
    MediaEntryCreateDto 
} from "./MediaEntryBase";

export interface GameEntryDetailedDto extends MediaEntryDetailedDto {
  devStudioName: string | null;
  hoursPlayed: number;
}

export interface GameEntryCreateDto extends MediaEntryCreateDto {
  devStudioName?: string | null;
  hoursPlayed?: number;
}