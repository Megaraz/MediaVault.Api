import type {
    MediaEntryDetailedDto,
    MediaEntryCreateDto,
    MediaEntryUpdateDto
} from "./MediaEntryBase";

// Manga-specific fields on top of the shared base types.
export interface MangaEntryDetailedDto extends MediaEntryDetailedDto {
    author: string | null;
}

export interface MangaEntryCreateDto extends MediaEntryCreateDto {
    author?: string | null;
}

export interface MangaEntryUpdateDto extends MediaEntryUpdateDto {
    author?: string | null;
}