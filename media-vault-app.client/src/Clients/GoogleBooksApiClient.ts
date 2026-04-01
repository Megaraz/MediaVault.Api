export type BookSearchResultDto = {
    externalId: string;
    title: string;
    coverImageUrl: string | null;
};

export type BookSearchRequestDto = {
    query: string;
};

export default class GoogleBooksApiClient {
    private baseUrl = "/googlebooksapi";

    async searchBooks(
        request: BookSearchRequestDto,
        page: number = 1,
        pageSize: number = 10
    ): Promise<BookSearchResultDto[]> {
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

    async getBookById(volumeId: string): Promise<BookSearchResultDto> {
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
