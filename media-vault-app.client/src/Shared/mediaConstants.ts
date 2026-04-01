import { MediaType, StatusType } from "../Clients/MediaEntriesClient";

export const mediaSections = [
  { type: MediaType.All, title: "All" },
  { type: MediaType.Game, title: "Games" },
  { type: MediaType.Book, title: "Books" },
  { type: MediaType.Movie, title: "Movies" },
  { type: MediaType.Series, title: "Series" },
  { type: MediaType.Manga, title: "Manga" },
];


export const statusSections = [
  { type: StatusType.OnGoing, title: "On Going" },
  { type: StatusType.CaughtUp, title: "Caught Up" },
  { type: StatusType.Completed, title: "Completed" },
  { type: StatusType.Backlog, title: "Backlog" },
  { type: StatusType.Dropped, title: "Dropped" },
];
