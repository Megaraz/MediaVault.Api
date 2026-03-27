import type { MediaEntryDetailedDto } from "../../Clients/MediaEntriesClient";
import MediaItem from "./MediaItem";
import { StatusType } from "../../Clients/MediaEntriesClient";

type props = {
  mediaEntries: MediaEntryDetailedDto[];
  onClickEntry: (entry: MediaEntryDetailedDto) => void;
};

export default function EntriesSectionMain({
  mediaEntries,
  onClickEntry,
}: props) {

  const statusSections = [
    { type: StatusType.OnGoing, title: "On Going" },
    { type: StatusType.Completed, title: "Completed" },
    { type: StatusType.Backlog, title: "Backlog" },
    { type: StatusType.Dropped, title: "Dropped" },
  ];

  return (
    <>
      <section>
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
          <h2 className="text-2xl font-bold flex items-center gap-2">
            <span className="material-symbols-outlined text-primary">
              play_circle
            </span>
            Your On-Going
          </h2>
          <div className="flex flex-wrap gap-2">
            <button className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-slate-100 dark:bg-slate-800 text-xs font-medium border border-slate-200 dark:border-slate-700">
              Type: All{" "}
              <span className="material-symbols-outlined text-xs">
                expand_more
              </span>
            </button>
            <button className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-slate-100 dark:bg-slate-800 text-xs font-medium border border-slate-200 dark:border-slate-700">
              Genre: Sci-Fi{" "}
              <span className="material-symbols-outlined text-xs">
                expand_more
              </span>
            </button>
            <button className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-slate-100 dark:bg-slate-800 text-xs font-medium border border-slate-200 dark:border-slate-700">
              Last Updated{" "}
              <span className="material-symbols-outlined text-xs">sort</span>
            </button>
          </div>
        </div>
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-6">
          {mediaEntries.map((entry) => (
            <MediaItem
              key={entry.id}
              entry={entry}
              onClickEntry={() => onClickEntry(entry)}
            />
          ))}
        </div>
      </section>
    </>
  );
}
