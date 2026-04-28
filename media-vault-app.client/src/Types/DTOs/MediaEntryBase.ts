// Base response
export interface MediaEntryDetailedDto {
  id: string;
  idExternal: string | null;
  userId: string;
  status: number;
  title: string;
  rating: number;
  review: string | null;
  genres: string[] | null;   // note: your backend now uses ICollection<string>
  releaseYear: number | null;
  imageUrl: string | null;
  mediaType: number;
  createdAtUtc: string;
}

// Base create
export interface MediaEntryCreateDto {
  idExternal?: string | null;
  status: number;
  title: string;
  rating: number;
  review?: string | null;
  genres?: string[] | null;
  releaseYear?: number | null;
  imageUrl?: string | null;
  // mediaType is implicit — determined by which endpoint you call
}

export type MediaEntrySubmitDto = MediaEntryCreateDto;

export type MediaEntryMinimalDto = {
    id: string;
    title: string;
    mediaType: number;
    imageUrl: string | null;
};

export type MediaEntrySearchRequestDto = {
    query: string;
};

export const StatusLabels: Record<number, string> = {
    0: "OnGoing",
    1: "Completed",
    2: "Backlog",
    3: "Dropped",
    4: "Caught Up"
};

export const MediaTypeLabels: Record<number, string> = {
    0: "All",
    1: "Movie",
    2: "Series",
    3: "Book",
    4: "Manga",
    5: "Game",
};


export const StatusType = {
    OnGoing: 0,
    Completed: 1,
    Backlog: 2,
    Dropped: 3,
    CaughtUp: 4,
} as const;

export const MediaType = {
    All: 0,
    Movie: 1,
    Series: 2,
    Book: 3,
    Manga: 4,
    Game: 5,
} as const;