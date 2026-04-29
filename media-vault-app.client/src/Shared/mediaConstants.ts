import { MediaType, StatusType } from "../Types/DTOs/MediaEntryBase";

// Sidebar navigation sections for filtering by media type.
// MediaType.All (-1) shows every type; the rest filter to a single type.
export const mediaSections = [
  { type: MediaType.All, title: "All" },
  { type: MediaType.Game, title: "Games" },
  { type: MediaType.Book, title: "Books" },
  { type: MediaType.Movie, title: "Movies" },
  { type: MediaType.Series, title: "Series" },
  { type: MediaType.Manga, title: "Manga" },
];

// Status sections rendered on the Dashboard.
// Order here controls the visual order of the sections on screen.
export const statusSections = [
  { type: StatusType.OnGoing, title: "On Going" },
  { type: StatusType.CaughtUp, title: "Caught Up" },
  { type: StatusType.Completed, title: "Completed" },
  { type: StatusType.Backlog, title: "Backlog" },
  { type: StatusType.Dropped, title: "Dropped" },
];
