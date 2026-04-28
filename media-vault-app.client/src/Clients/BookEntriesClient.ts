import type { BookEntryCreateDto, BookEntryDetailedDto, BookEntryUpdateDto } from "../Types/DTOs/BookEntry";

export default class BookEntriesClient {
    private baseUrl = "/mediaentries/books";

    async createBook(dto: BookEntryCreateDto): Promise<BookEntryDetailedDto> {
        const response = await fetch(this.baseUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dto),
        });
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to create book entry: " + errorMessage);
        }
        return response.json();
    }

    async updateBook(id: string, dto: BookEntryUpdateDto): Promise<void> {
        const response = await fetch(`${this.baseUrl}/${id}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dto),
        });
        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error("Failed to update book entry: " + errorMessage);
        }
    }
}
