// Card used in the grid sections (On Going, Completed, etc.).
// Shows a poster image, title, media type badge, and rating.
import type { MediaEntryMinimalDto } from "../../Clients/MediaEntriesClient";
import MediaTypeBadge from "../Shared/MediaTypeBadge";

type MediaEntrySmallProps = {
  entry: MediaEntryMinimalDto;
  onClickEntry: (entry: MediaEntryMinimalDto) => void;
};

export default function MediaItem({
  entry,
  onClickEntry,
}: MediaEntrySmallProps) {
  return (
    <div
      className="group relative z-0 cursor-pointer transform-gpu transition-transform duration-700 ease-[cubic-bezier(0.16,1,0.3,1)] hover:z-10 hover:scale-[1.025] motion-reduce:transition-none motion-reduce:hover:scale-100"
      key={entry.id}
      onClick={() => onClickEntry(entry)}
    >
      <div className="-m-2 rounded-2xl bg-transparent p-2 transition-[background-color,box-shadow] duration-700 ease-[cubic-bezier(0.16,1,0.3,1)] group-hover:bg-slate-100/90 group-hover:shadow-2xl group-hover:shadow-slate-900/10 dark:group-hover:bg-slate-800/80 dark:group-hover:shadow-black/30 motion-reduce:transition-none">
        <div className="relative mb-3 aspect-2/3 overflow-hidden rounded-xl shadow-md transition-shadow duration-700 ease-[cubic-bezier(0.16,1,0.3,1)] group-hover:shadow-primary/20 group-hover:shadow-xl motion-reduce:transition-none">
          <div
            className="absolute inset-0 bg-cover bg-center"
            data-alt="Dark futuristic sci-fi movie poster art"
            style={{
              backgroundImage: `url('${entry.imageUrl ?? ""}')`,
            }}
          ></div>
          {/* <div className="absolute top-2 right-2 bg-black/60 backdrop-blur-md px-2 py-1 rounded-md text-[10px] font-bold text-white uppercase">
            EP 08 / 12
          </div> */}
          <div className="absolute bottom-0 left-0 right-0 h-1/3 bg-linear-to-t from-black/80 to-transparent"></div>
        </div>
        <h3 className="truncate text-sm font-semibold transition-colors duration-700 ease-[cubic-bezier(0.16,1,0.3,1)] group-hover:text-primary motion-reduce:transition-none">
          {entry.title}
        </h3>
        <div className="mt-1 flex items-center justify-between">
          <MediaTypeBadge mediaType={entry.mediaType} />
          <div className="flex items-center text-yellow-500">
            <span className="material-symbols-outlined text-xs fill-1">
              star
            </span>
            <span className="ml-1 text-xs font-medium">
              {entry.rating.toFixed(1)}
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}
