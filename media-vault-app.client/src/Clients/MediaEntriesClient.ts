
export type MediaEntryDetailedDto = {
    id: string;
    idExternal: string | null;
    userId: string;
    status: number;
    title: string | null;
    rating: number;
    review: string | null;
    genre: string | null;
    releaseYear: number;
    imageUrl: string | null;
    mediaType: number;
    createdAtUtc: string;
};

export type MediaEntrySubmitDto = {
    idExternal?: string | null;
    status: number;
    title: string;
    rating: number;
    review?: string | null;
    genre?: string | null;
    releaseYear?: number | null;
    imageUrl?: string | null;
    mediaType: number;
};

export type MediaEntryMinimalDto = {
    id: string;
    title: string;
    mediaType: number;
    imageUrl: string | null;
};

export type MediaEntrySearchRequestDto = {
    query: string;
};

export const StatusLabels: Record<number, string> = {
    0: "OnGoing",
    1: "Completed",
    2: "Backlog",
    3: "Dropped",
    4: "Caught Up"
};

export const MediaTypeLabels: Record<number, string> = {
    0: "All",
    1: "Movie",
    2: "Series",
    3: "Book",
    4: "Manga",
    5: "Game",
};


export const StatusType = {
    OnGoing: 0,
    Completed: 1,
    Backlog: 2,
    Dropped: 3,
    CaughtUp: 4,
} as const;

export const MediaType = {
    All: 0,
    Movie: 1,
    Series: 2,
    Book: 3,
    Manga: 4,
    Game: 5,
} as const;

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
