import { MediaType } from "../../Clients/MediaEntriesClient";
import type { MediaEntryDetailedDto } from "../../Clients/MediaEntriesClient";
import { mediaSections } from "../../Shared/mediaConstants";
import { useEffect, useMemo, useState } from "react";
import MediaItemCompact from "./MediaItemCompact";
import Dropdown from "../Shared/Dropdown";
import type { DropdownItem } from "../Shared/Dropdown";

type Props = {
  mediaEntries: MediaEntryDetailedDto[];
  onClickEntry: (entry: MediaEntryDetailedDto) => void;
  statusSectionType: string;
  currentMainMediaTypeFilter: number;
};

const mediaTypeOptions: DropdownItem[] = mediaSections.map(({ type, title }) => ({
  value: type,
  label: title,
}));

export default function EntriesSectionSub({
  mediaEntries,
  onClickEntry,
  statusSectionType,
  currentMainMediaTypeFilter = MediaType.All,
}: Props) {
  const [internalMediaTypeFilter, setInternalMediaTypeFilter] =
    useState<number>(currentMainMediaTypeFilter);

  useEffect(() => {
    setInternalMediaTypeFilter(currentMainMediaTypeFilter);
  }, [currentMainMediaTypeFilter]);

  const filteredEntries = useMemo(() => {
    if (
      internalMediaTypeFilter === undefined ||
      internalMediaTypeFilter === MediaType.All
    ) {
      return mediaEntries;
    }

    return mediaEntries.filter((entry) => entry.mediaType === internalMediaTypeFilter);
  }, [internalMediaTypeFilter, mediaEntries]);

  return (
    <section className="px-6 py-4">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
          <h2 className="text-2xl font-bold flex items-center gap-2">
            <span className="material-symbols-outlined text-primary">
              list_alt
            </span>
            Your {statusSectionType}
          </h2>
          <div className="flex flex-wrap gap-2">
            <Dropdown
              options={mediaTypeOptions}
              value={internalMediaTypeFilter}
              onChange={(value) => setInternalMediaTypeFilter(Number(value))}
              prefix="Type:"
            />
            <button className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-slate-100 dark:bg-slate-800 text-xs font-medium border border-slate-200 dark:border-slate-700">
              Genre: All{" "}
              <span className="material-symbols-outlined text-xs">
                expand_more
              </span>
            </button>
          </div>
        </div>
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {filteredEntries.map((entry) => (
            <MediaItemCompact
              key={entry.id}
              entry={entry}
              onClickEntry={onClickEntry}
            />
          ))}
        </div>
      </section>
  );
}
