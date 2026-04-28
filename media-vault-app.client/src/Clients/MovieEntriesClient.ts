import type { MovieEntryCreateDto, MovieEntryDetailedDto, MovieEntryUpdateDto } from "../Types/DTOs/MovieEntry";

export default class MovieEntriesClient {
    private baseUrl = "/mediaentries/movies";

    async createMovie(dto: MovieEntryCreateDto): Promise<MovieEntryDetailedDto> {
        const response = await fetch(this.baseUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dto),
        });
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to create movie entry: " + errorMessage);
        }
        return response.json();
    }

    async updateMovie(id: string, dto: MovieEntryUpdateDto): Promise<void> {
        const response = await fetch(`${this.baseUrl}/${id}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dto),
        });
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to update movie entry: " + errorMessage);
        }
    }
}
