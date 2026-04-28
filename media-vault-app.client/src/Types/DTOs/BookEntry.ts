import type {
    MediaEntryDetailedDto,
    MediaEntryCreateDto,
    MediaEntryUpdateDto
} from "./MediaEntryBase";

export interface BookEntryDetailedDto extends MediaEntryDetailedDto {
    author: string | null;
}

export interface BookEntryCreateDto extends MediaEntryCreateDto {
    author?: string | null;
}

export interface BookEntryUpdateDto extends MediaEntryUpdateDto {
    author?: string | null;
}