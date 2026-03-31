export type TvSearchResultDto = {
    externalId: number;
    title: string;
    coverImageUrl: string | null;
};

export type TvSearchRequestDto = {
    query: string;
};

export default class TmdbTvSeriesApiClient {
    private baseUrl = "/tmdbtvseriesapi";

    async searchTvSeries(
        request: TvSearchRequestDto,
        page: number = 1,
        // pageSize: number = 10,
        // ordering?: string
    ): Promise<TvSearchResultDto[]> {
        const params = new URLSearchParams();
        params.set("page", page.toString());
        // params.set("pageSize", pageSize.toString());
        // if (ordering) params.set("ordering", ordering);

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
            throw new Error("Failed to search TV series: " + errorMessage);
        }

        return response.json();
    }

    async getTvSeriesById(id: number): Promise<TvSearchResultDto> {
        const response = await fetch(`${this.baseUrl}/${id}`, {
            credentials: "include",
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to fetch TV series: " + errorMessage);
        }

        return response.json();
    }
}
