import type { GameEntryCreateDto, GameEntryDetailedDto, GameEntryUpdateDto } from "../Types/DTOs/GameEntry";

export default class GameEntriesClient {
    private baseUrl = "/mediaentries/games";

    async createGame(dto: GameEntryCreateDto): Promise<GameEntryDetailedDto> {
        const response = await fetch(this.baseUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dto),
        });
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to create game entry: " + errorMessage);
        }
        return response.json();
    }

    async updateGame(id: string, dto: GameEntryUpdateDto): Promise<void> {
        const response = await fetch(`${this.baseUrl}/${id}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dto),
        });
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to update game entry: " + errorMessage);
        }
    }
}
