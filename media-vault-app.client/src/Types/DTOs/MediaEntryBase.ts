// ─────────────────────────────────────────────────────────────
// MediaEntryBase.ts
//
// Shared base types for all media entries.
// The backend uses an abstract base record + concrete sub-types
// for each media type (Movie, TvSeries, Book, etc.).
// We mirror that same hierarchy here in TypeScript.
// ─────────────────────────────────────────────────────────────

// The full details of a media entry as returned from GET endpoints.
// Concrete sub-types (e.g. MovieEntryDetailedDto) extend this.
export interface MediaEntryDetailedDto {
    id: string;
    idExternal: string | null; // ID from an external API (e.g. TMDB, RAWG)
    userId: string;
    status: number;            // Maps to StatusType constants below
    title: string;
    rating: number;
    overview: string | null;
    review: string | null;
    genres: string[] | null;
    releaseDate: string | null;
    imageUrl: string | null;
    mediaType: number;         // Maps to MediaType constants below
    createdAtUtc: string;      // ISO 8601 date string from the server
}

// The shape sent to POST (create) endpoints.
// mediaType is NOT included — it is determined by which endpoint you call
// (e.g. POST /mediaentries/movies vs /mediaentries/games).
export interface MediaEntryCreateDto {
    idExternal?: string | null;
    status: number;
    title: string;
    overview?: string | null;
    rating: number;
    review?: string | null;
    genres?: string[] | null;
    releaseDate?: string | null;
    imageUrl?: string | null;
}


export interface MediaEntrySearchResultDto {
    idExternal: string;
    title: string;
    coverImageUrl: string | null;
};

export interface SearchRequestDto {
    query: string;
};

// The shape sent to PUT (update) endpoints. Same fields as create.
export interface MediaEntryUpdateDto {
    idExternal?: string | null;
    status: number;
    title: string;
    overview?: string | null;
    rating: number;
    review?: string | null;
    genres?: string[] | null;
    releaseDate?: string | null;
    imageUrl?: string | null;
}

// Lightweight shape returned by the GetEntries endpoint — just enough to show in a list.
export interface MediaEntryMinimalDto {
    id: string;
    title: string;
    status: number;
    genres?: string[] | null;
    releaseDate?: string | null;
    mediaType: number;
    rating: number;
    imageUrl: string | null;
    createdAtUtc: string;      // ISO 8601 date string from the server
};

// Request body for the search endpoint.
export type MediaEntrySearchRequestDto = {
    query: string;
};

// Human-readable labels for status values, used in dropdowns and badges.
export const StatusLabels: Record<number, string> = {
    0: "OnGoing",
    1: "Completed",
    2: "Backlog",
    3: "Dropped",
    4: "Caught Up"
};

// Human-readable labels for media type values.
export const MediaTypeLabels: Record<number, string> = {
    0: "Movie",
    1: "Series",
    2: "Book",
    3: "Manga",
    4: "Game",
};

// Named constants for status values — use these instead of raw numbers in code.
export const StatusType = {
    OnGoing: 0,
    Completed: 1,
    Backlog: 2,
    Dropped: 3,
    CaughtUp: 4,
} as const;

// Named constants for media type values.
// NOTE: The values here must match the backend MediaType enum exactly:
//   Movie=0, TvSeries=1, Book=2, Manga=3, Game=4
// "All" is a frontend-only sentinel value (-1) used for "show everything" filters.
// It is never sent to the backend.
export const MediaType = {
    All: -1,
    Movie: 0,
    Series: 1,
    Book: 2,
    Manga: 3,
    Game: 4,
} as const;