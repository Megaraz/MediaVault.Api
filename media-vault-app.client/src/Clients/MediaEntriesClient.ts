// ─────────────────────────────────────────────────────────────
// MediaEntriesClient.ts
//
// Handles the shared (type-agnostic) API operations:
//   - GET all entries
//   - GET entry by ID
//   - POST search
//   - DELETE
//
// Create and update are intentionally NOT here because they are
// type-specific — each media type has its own endpoint and DTO.
// See MovieEntriesClient, GameEntriesClient etc. for those.
//
// Also re-exports shared types and constants from MediaEntryBase
// so other files can do a single import from this module.
// ─────────────────────────────────────────────────────────────
import type {
    MediaEntryDetailedDto,
    MediaEntryMinimalDto,
    MediaEntrySearchRequestDto,
} from "../Types/DTOs/MediaEntryBase";

// Re-export shared types and constants so existing component imports keep working
export type {
    MediaEntryDetailedDto,
    MediaEntryMinimalDto,
    MediaEntrySearchRequestDto,
    MediaEntryCreateDto,
    MediaEntryUpdateDto,
} from "../Types/DTOs/MediaEntryBase";
export { StatusLabels, MediaTypeLabels, StatusType, MediaType } from "../Types/DTOs/MediaEntryBase";


export default class MediaEntriesClient {
    private baseUrl = "/mediaentries";

    async searchMediaEntries(
        request: MediaEntrySearchRequestDto,
        page: number = 1,
        pageSize: number = 10
    ): Promise<MediaEntryMinimalDto[]> {
        const params = new URLSearchParams();
        params.set("page", page.toString());
        params.set("pageSize", pageSize.toString());

        const response = await fetch(`${this.baseUrl}/search?${params}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            credentials: "include",
            body: JSON.stringify(request),
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to search media entries: " + errorMessage);
        }

        return response.json();
    }

    async getMediaEntries(pageNumber = 1, pageSize = 25): Promise<MediaEntryDetailedDto[]> {
        const response = await fetch(
            `${this.baseUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`
        );
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to fetch media entries: " + errorMessage);
        }
        return response.json();
    }

    async getMediaEntryById(entryId: string): Promise<MediaEntryDetailedDto> {
        const response = await fetch(`${this.baseUrl}/${entryId}`);
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to fetch media entry: " + errorMessage);
        }
        return response.json();
    }

    async deleteMediaEntry(entryId: string): Promise<void> {
        const response = await fetch(`${this.baseUrl}/${entryId}`, {
            method: "DELETE",
        });
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to delete media entry: " + errorMessage);
        }
    }

}
