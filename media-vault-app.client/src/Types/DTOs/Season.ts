

export interface SeasonDetailedDto {
    id: string;
    ownerId: string;
    idExternal: string | null;
    name: string | null;
    overview: string | null;
    imageUrl: string | null;
    seasonNumber: number;
    airDate: string | null;
    watchedEpisodes: number;
    episodes: number;
    status: number;
    rating: number;
    createdAtUtc: string;
    updatedAtUtc: string;
}

export interface SeasonMinimalDto {
    id: string;
    ownerId: string;
    idExternal: string | null;
    name: string | null;
    overview: string | null;
    imageUrl: string | null;
    seasonNumber: number;
    airDate: string | null;
    watchedEpisodes: number;
    episodes: number;
    status: number;
    rating: number;
    createdAtUtc: string;
    updatedAtUtc: string;
}

export interface SeasonCreateDto {
    idExternal?: string | null;
    name?: string | null;
    overview?: string | null;
    imageUrl?: string | null;
    seasonNumber: number;
    airDate?: string | null;
    watchedEpisodes: number;
    episodes: number;
    status: number;
    rating: number;
}

export interface SeasonUpdateDto {
    id: string;
    ownerId: string;
    idExternal?: string | null;
    name?: string | null;
    overview?: string | null;
    imageUrl?: string | null;
    seasonNumber: number;
    airDate?: string | null;
    watchedEpisodes: number;
    episodes: number;
    status: number;
    rating: number;
}

