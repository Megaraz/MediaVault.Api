
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

export type MediaEntryCreateDto = {
    idExternal?: string | null;
    status: number;
    title: string;
    rating?: number | null;
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

export default class MediaEntriesClient {
    private baseUrl = "/mediaentries";

    async getAll(pageNumber = 1, pageSize = 10): Promise<MediaEntryDetailedDto[]> {
        const response = await fetch(
            `${this.baseUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`
        );
        if (!response.ok) {
            throw new Error("Failed to fetch media entries");
        }
        return response.json();
    }

    async create(entry: MediaEntryCreateDto): Promise<MediaEntryDetailedDto> {
        const response = await fetch(this.baseUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(entry),
        });
        if (!response.ok) {
            throw new Error("Failed to create media entry");
        }
        return response.json();
    }
}
