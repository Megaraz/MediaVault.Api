import type {
    MediaEntryDetailedDto,
    MediaEntryCreateDto,
    MediaEntryUpdateDto
} from "./MediaEntryBase";

export interface MovieEntryDetailedDto extends MediaEntryDetailedDto {
    runtimeMinutes: number;
}

export interface MovieEntryCreateDto extends MediaEntryCreateDto {
    runtimeMinutes: number;
}

export interface MovieEntryUpdateDto extends MediaEntryUpdateDto {
    runtimeMinutes: number;
}