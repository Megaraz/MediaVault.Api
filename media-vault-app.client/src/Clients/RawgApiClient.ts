export type GameSearchResultDto = {
    externalId: string;
    title: string;
    coverImageUrl: string | null;
    //slug: string;
};

export type GameSearchRequestDto = {
    query: string;
};

export default class RawgApiClient {
    private baseUrl = "/rawgapi";

    async searchGames(
        request: GameSearchRequestDto,
        page: number = 1,
        pageSize: number = 10,
        searchPrecise?: boolean,
        searchExact?: boolean,
        ordering?: string
    ): Promise<GameSearchResultDto[]> {
        const params = new URLSearchParams();
        params.set("page", page.toString());
        params.set("pageSize", pageSize.toString());
        if (searchPrecise !== undefined) params.set("searchPrecise", searchPrecise.toString());
        if (searchExact !== undefined) params.set("searchExact", searchExact.toString());
        if (ordering) params.set("ordering", ordering);

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
            throw new Error("Failed to search games: " + errorMessage);
        }

        return response.json();
    }

    async getGameById(id: number): Promise<GameSearchResultDto> {
        const response = await fetch(`${this.baseUrl}/${id}`, {
            credentials: "include",
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to fetch game: " + errorMessage);
        }

        return response.json();
    }
}
