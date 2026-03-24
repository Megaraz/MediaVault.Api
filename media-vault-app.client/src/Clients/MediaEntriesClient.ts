
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


export const StatusLabels: Record<number, string> = {
    0: "OnGoing",
    1: "Completed",
    2: "Backlog",
    3: "Dropped",
};

export const MediaTypeLabels: Record<number, string> = {
    0: "Movie",
    1: "Series",
    2: "Book",
    3: "Manga",
    4: "Game",
};


export const StatusType = {
    OnGoing: 0,
    Completed: 1,
    Backlog: 2,
    Dropped: 3,
} as const;

export const MediaType = {
    Movie: 0,
    Series: 1,
    Book: 2,
    Manga: 3,
    Game: 4,
} as const;

export default class MediaEntriesClient {
    private baseUrl = "/mediaentries";

    async createMediaEntry(userId: string, entry: MediaEntrySubmitDto): Promise<MediaEntryDetailedDto> {
        const response = await fetch(`${this.baseUrl}/${userId}`, {
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
    async getMediaEntries(userId: string, pageNumber = 1, pageSize = 10): Promise<MediaEntryDetailedDto[]> {
        const response = await fetch(
            `${this.baseUrl}/${userId}?pageNumber=${pageNumber}&pageSize=${pageSize}`
        );
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to fetch media entries: " + errorMessage);
        }
        return response.json();
    }

    async getMediaEntryById(userId: string, entryId: string): Promise<MediaEntryDetailedDto> {
        const response = await fetch(`${this.baseUrl}/${userId}/${entryId}`);
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to fetch media entry: " + errorMessage);
        }
        return response.json();
    }

    async updateMediaEntry(userId: string, entryId: string, updatedEntry: MediaEntrySubmitDto): Promise<void> {
        const response = await fetch(`${this.baseUrl}/${userId}/${entryId}`, {
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

    async deleteMediaEntry(userId: string, entryId: string): Promise<void> {
        const response = await fetch(`${this.baseUrl}/${userId}/${entryId}`, {
            method: "DELETE",
        });
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to delete media entry: " + errorMessage);
        }
    }

}
