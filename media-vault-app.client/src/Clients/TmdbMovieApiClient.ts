export type MovieSearchResultDto = {
    externalId: number;
    title: string;
    coverImageUrl: string | null;
};

export type MovieSearchRequestDto = {
    query: string;
};

export default class TmdbMovieApiClient {
    private baseUrl = "/tmdbmovieapi";

    async searchMovies(
        request: MovieSearchRequestDto,
        page: number = 1,
        // pageSize: number = 10,
        // ordering?: string
    ): Promise<MovieSearchResultDto[]> {
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
            throw new Error("Failed to search movies: " + errorMessage);
        }

        return response.json();
    }

    async getMovieById(id: number): Promise<MovieSearchResultDto> {
        const response = await fetch(`${this.baseUrl}/${id}`, {
            credentials: "include",
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to fetch movie: " + errorMessage);
        }

        return response.json();
    }
}
