export type TmdbSearchResultDto = {
    externalId: string;
    title: string;
    coverImageUrl: string | null;
};

export type TmdbSearchRequestDto = {
    query: string;
};

export default class TmdbApiClient {
    private baseUrl = "/tmdbapi";

    async searchMovies(
        request: TmdbSearchRequestDto,
        page: number = 1,
    ): Promise<TmdbSearchResultDto[]> {
        const params = new URLSearchParams();
        params.set("page", page.toString());

        const response = await fetch(`${this.baseUrl}/movie/search?${params}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            credentials: "include",
            body: JSON.stringify(request),
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to search movies: " + errorMessage);
        }

        return response.json();
    }

    async getMovieById(id: number): Promise<TmdbSearchResultDto> {
        const response = await fetch(`${this.baseUrl}/movie/${id}`, {
            credentials: "include",
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to fetch movie: " + errorMessage);
        }

        return response.json();
    }

    async searchTvSeries(
        request: TmdbSearchRequestDto,
        page: number = 1,
    ): Promise<TmdbSearchResultDto[]> {
        const params = new URLSearchParams();
        params.set("page", page.toString());

        const response = await fetch(`${this.baseUrl}/tv/search?${params}`, {
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

    async getTvSeriesById(id: number): Promise<TmdbSearchResultDto> {
        const response = await fetch(`${this.baseUrl}/tv/${id}`, {
            credentials: "include",
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to fetch TV series: " + errorMessage);
        }

        return response.json();
    }
}
