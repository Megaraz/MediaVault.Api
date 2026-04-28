import type {
    MediaEntryDetailedDto,
    MediaEntryCreateDto,
    MediaEntryUpdateDto
} from "./MediaEntryBase";

export interface GameEntryDetailedDto extends MediaEntryDetailedDto {
    devStudioName: string | null;
    hoursPlayed: number;
}

export interface GameEntryCreateDto extends MediaEntryCreateDto {
    devStudioName?: string | null;
    hoursPlayed?: number;
}

export interface GameEntryUpdateDto extends MediaEntryUpdateDto {
    devStudioName?: string | null;
    hoursPlayed?: number;
}