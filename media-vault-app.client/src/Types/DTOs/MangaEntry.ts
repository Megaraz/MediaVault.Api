import type {
    MediaEntryDetailedDto,
    MediaEntryCreateDto
} from "./MediaEntryBase";


export interface MangaEntryDetailedDto extends MediaEntryDetailedDto {
    author?: string;
}

export interface MangaEntryCreateDto extends MediaEntryCreateDto {
    author?: string;
}