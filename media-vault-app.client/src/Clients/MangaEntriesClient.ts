// Handles create and update API calls for Manga entries.
// Hits /mediaentries/manga which expects MangaEntryCreateDto / MangaEntryUpdateDto.
import type { MangaEntryCreateDto, MangaEntryDetailedDto, MangaEntryUpdateDto } from "../Types/DTOs/MangaEntry";

export default class MangaEntriesClient {
    private baseUrl = "/mediaentries/manga";

    async createManga(dto: MangaEntryCreateDto): Promise<MangaEntryDetailedDto> {
        const response = await fetch(this.baseUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dto),
        });
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to create manga entry: " + errorMessage);
        }
        return response.json();
    }

    async updateManga(id: string, dto: MangaEntryUpdateDto): Promise<void> {
        const response = await fetch(`${this.baseUrl}/${id}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dto),
        });
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to update manga entry: " + errorMessage);
        }
    }
}
