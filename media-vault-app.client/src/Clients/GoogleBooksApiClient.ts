import type { SearchResult } from "../Components/MediaEntry/TitleSearchInput";
import type { SearchRequestDto } from "../Types/DTOs/MediaEntryBase";

export interface GoogleBooksDetailedDto extends SearchResult {
    author: string;
};

export default class GoogleBooksApiClient {
    private baseUrl = "/googlebooksapi";

    async searchBooks(
        request: SearchRequestDto,
        page: number = 1,
        pageSize: number = 8
    ): Promise<GoogleBooksDetailedDto[]> {
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
            throw new Error("Failed to search books: " + errorMessage);
        }

        return response.json();
    }

    async getBookById(volumeId: string): Promise<GoogleBooksDetailedDto> {
        const response = await fetch(`${this.baseUrl}/${volumeId}`, {
            credentials: "include",
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to fetch book: " + errorMessage);
        }

        return response.json();
    }
}
