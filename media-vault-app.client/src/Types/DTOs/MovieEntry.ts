import type {
    MediaEntryDetailedDto,
    MediaEntryCreateDto
} from "./MediaEntryBase";

export interface MovieEntryDetailedDto extends MediaEntryDetailedDto {
    runtimeMinutes: number;
}

export interface MovieEntryCreateDto extends MediaEntryCreateDto {
    runtimeMinutes: number;
}