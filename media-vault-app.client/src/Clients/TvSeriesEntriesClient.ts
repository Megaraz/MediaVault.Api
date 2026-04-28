import type { TvSeriesEntryCreateDto, TvSeriesEntryDetailedDto, TvSeriesEntryUpdateDto } from "../Types/DTOs/TvSeriesEntry";

export default class TvSeriesEntriesClient {
    private baseUrl = "/mediaentries/tv-series";

    async createTvSeries(dto: TvSeriesEntryCreateDto): Promise<TvSeriesEntryDetailedDto> {
        const response = await fetch(this.baseUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dto),
        });
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to create TV series entry: " + errorMessage);
        }
        return response.json();
    }

    async updateTvSeries(id: string, dto: TvSeriesEntryUpdateDto): Promise<void> {
        const response = await fetch(`${this.baseUrl}/${id}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dto),
        });
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to update TV series entry: " + errorMessage);
        }
    }
}
