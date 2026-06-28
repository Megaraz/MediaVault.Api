import type { MediaEntrySearchResultDto, SearchRequestDto } from "../Types/DTOs/MediaEntryBase";
import type { TmdbMovieDetailedDto } from "../Types/DTOs/MovieEntry";
import type { TmdbTvSeriesDetailedDto } from "../Types/DTOs/TvSeriesEntry";
import { apiFetch } from "./apiFetch";


export default class TmdbApiClient {
    private baseUrl = "/tmdbapi";

    async searchMovies(
        request: SearchRequestDto,
        page: number = 1,
    ): Promise<MediaEntrySearchResultDto[]> {
        const params = new URLSearchParams();
        params.set("page", page.toString());

        const response = await apiFetch(`${this.baseUrl}/movie/search?${params}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(request),
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to search movies: " + errorMessage);
        }

        return response.json();
    }

    async getMovieById(id: number): Promise<TmdbMovieDetailedDto> {
        const response = await apiFetch(`${this.baseUrl}/movie/${id}`);

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to fetch movie: " + errorMessage);
        }

        return response.json();
    }

    async searchTvSeries(
        request: SearchRequestDto,
        page: number = 1,
    ): Promise<MediaEntrySearchResultDto[]> {
        const params = new URLSearchParams();
        params.set("page", page.toString());

        const response = await apiFetch(`${this.baseUrl}/tv/search?${params}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(request),
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to search TV series: " + errorMessage);
        }

        return response.json();
    }

    async getTvSeriesById(id: number): Promise<TmdbTvSeriesDetailedDto> {
        const response = await apiFetch(`${this.baseUrl}/tv/${id}`);

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to fetch TV series: " + errorMessage);
        }

        return response.json();
    }
}
