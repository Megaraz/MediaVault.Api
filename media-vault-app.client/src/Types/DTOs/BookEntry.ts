import type { 
    MediaEntryDetailedDto, 
    MediaEntryCreateDto 
} from "./MediaEntryBase";


export interface BookEntryDetailedDto extends MediaEntryDetailedDto {
    author?: string;
}

export interface BookEntryCreateDto extends MediaEntryCreateDto {
    author?: string;
}