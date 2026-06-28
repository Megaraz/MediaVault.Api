import type { MediaEntrySearchResultDto, SearchRequestDto } from "../Types/DTOs/MediaEntryBase";
import { apiFetch } from "./apiFetch";

export interface RawgGameDetailedDto {
    rawgId: number;
    rawgSlug?: string;
    rawgName?: string;
    rawgDescription?: string;
    rawgMetacritic: number;
    rawgReleased?: string;
    rawgBackgroundImage?: string;
    rawgWebsite?: string;
    rawgPlatforms?: string[];
    rawgRequirements?: GamePcRequirementsDto;
}
export interface GamePcRequirementsDto {
    minimum?: string;
    recommended?: string;
    high?: string;
    veryHigh?: string;
    ultra?: string;
}


export default class RawgApiClient {
    private baseUrl = "/rawgapi";

    async searchGames(
        request: SearchRequestDto,
        page: number = 1,
        pageSize: number = 10,
        searchPrecise?: boolean,
        searchExact?: boolean,
        ordering?: string
    ): Promise<MediaEntrySearchResultDto[]> {
        const params = new URLSearchParams();
        params.set("page", page.toString());
        params.set("pageSize", pageSize.toString());
        if (searchPrecise !== undefined) params.set("searchPrecise", searchPrecise.toString());
        if (searchExact !== undefined) params.set("searchExact", searchExact.toString());
        if (ordering) params.set("ordering", ordering);

        const response = await apiFetch(`${this.baseUrl}/search?${params}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(request),
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to search games: " + errorMessage);
        }

        return response.json();
    }

    async getGameById(id: number): Promise<RawgGameDetailedDto> {
        const response = await apiFetch(`${this.baseUrl}/${id}`);

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to fetch game: " + errorMessage);
        }

        return response.json();
    }
}
