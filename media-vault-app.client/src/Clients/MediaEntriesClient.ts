import type {
    MediaEntryDetailedDto,
    MediaEntryMinimalDto,
    MediaEntrySearchRequestDto,
    MediaEntrySubmitDto
} from "../Types/DTOs/MediaEntryBase";


export default class MediaEntriesClient {
    private baseUrl = "/mediaentries";

    async createMediaEntry(entry: MediaEntrySubmitDto): Promise<MediaEntryDetailedDto> {
        const response = await fetch(this.baseUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(entry),
        });
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to create media entry: " + errorMessage);
        }
        return response.json();
    }

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

    async updateMediaEntry(entryId: string, updatedEntry: MediaEntrySubmitDto): Promise<void> {
        const response = await fetch(`${this.baseUrl}/${entryId}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(updatedEntry),
        });
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to update media entry: " + errorMessage);
        }
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
