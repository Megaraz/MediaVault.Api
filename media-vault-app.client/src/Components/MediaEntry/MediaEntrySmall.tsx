import {
  MediaTypeLabels,
  StatusLabels,
  type MediaEntryDetailedDto,
} from "../../Clients/MediaEntriesClient";

type MediaEntrySmallProps = {
  entry: MediaEntryDetailedDto;
  onClickEntry: (entry: MediaEntryDetailedDto) => void;
};

export default function MediaEntrySmall({
  entry,
  onClickEntry,
}: MediaEntrySmallProps) {
  return (
    <div
      key={entry.id}
      className="border p-3 rounded hover:cursor-pointer hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
      onClick={() => onClickEntry(entry)}
    >
      <ul>
        <li>
          <b>Title:</b> {entry.title}
        </li>
        <li>
          <b>Type:</b> {MediaTypeLabels[entry.mediaType] ?? entry.mediaType}
        </li>
        <li>
          <b>Status:</b> {StatusLabels[entry.status] ?? entry.status}
        </li>
        <li>
          <b>Rating:</b> {entry.rating > 0 ? `${entry.rating.toFixed(1)} / 5.0` : "Not rated"}
        </li>
        <li>
          <b>Genre:</b> {entry.genre ?? "N/A"}
        </li>
        <li>
          <b>Release Year:</b> {entry.releaseYear || "N/A"}
        </li>
        <li>
          <b>Review:</b> {entry.review ?? "N/A"}
        </li>
      </ul>
    </div>
  );
}
