import type {
    MediaEntryDetailedDto,
    MediaEntryCreateDto,
    MediaEntryUpdateDto
} from "./MediaEntryBase";

// Book-specific fields on top of the shared base types.
export interface BookEntryDetailedDto extends MediaEntryDetailedDto {
    author: string | null;
}

export interface BookEntryCreateDto extends MediaEntryCreateDto {
    author?: string | null;
}

export interface BookEntryUpdateDto extends MediaEntryUpdateDto {
    author?: string | null;
}