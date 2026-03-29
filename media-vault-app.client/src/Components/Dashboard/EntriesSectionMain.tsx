import type { MediaEntryDetailedDto } from "../../Clients/MediaEntriesClient";
import MediaItem from "./MediaItem";
import { MediaType } from "../../Clients/MediaEntriesClient";
import { mediaSections } from "../../Shared/mediaConstants";
import { useEffect, useMemo, useState } from "react";
import Dropdown from "../Shared/Dropdown";
import type { DropdownItem } from "../Shared/Dropdown";

type Props = {
  mediaEntries: MediaEntryDetailedDto[];
  onClickEntry: (entry: MediaEntryDetailedDto) => void;
  statusSectionType: string;
  currentMainMediaTypeFilter: number;
};

const sortOptions = [
  { value: "rating-desc", label: "Rating: High to Low" },
  { value: "rating-asc", label: "Rating: Low to High" },
  { value: "created-desc", label: "Added: Newest First" },
  { value: "created-asc", label: "Added: Oldest First" },
] as const satisfies DropdownItem[];

type SortOptionValue = (typeof sortOptions)[number]["value"];

const mediaTypeOptions: DropdownItem[] = mediaSections.map(
  ({ type, title }) => ({ value: type, label: title }),
);

export default function EntriesSectionMain({
  mediaEntries,
  onClickEntry,
  statusSectionType,
  currentMainMediaTypeFilter = MediaType.All,
}: Props) {
  const [internalMediaTypeFilter, setSectionMediaTypeFilter] = useState<number>(
    currentMainMediaTypeFilter,
  );
  const [sortBy, setSortBy] = useState<SortOptionValue>("rating-desc");

  useEffect(() => {
    setSectionMediaTypeFilter(currentMainMediaTypeFilter);
  }, [currentMainMediaTypeFilter]);

  const filteredEntries = useMemo(() => {
    const entriesToSort =
      internalMediaTypeFilter === undefined ||
      internalMediaTypeFilter === MediaType.All
        ? [...mediaEntries]
        : mediaEntries.filter(
            (entry) => entry.mediaType === internalMediaTypeFilter,
          );

    const getCreatedAtTime = (entry: MediaEntryDetailedDto) =>
      new Date(entry.createdAtUtc).getTime();

    switch (sortBy) {
      case "rating-asc":
        return entriesToSort.sort((left, right) => left.rating - right.rating);
      case "created-desc":
        return entriesToSort.sort(
          (left, right) => getCreatedAtTime(right) - getCreatedAtTime(left),
        );
      case "created-asc":
        return entriesToSort.sort(
          (left, right) => getCreatedAtTime(left) - getCreatedAtTime(right),
        );
      case "rating-desc":
      default:
        return entriesToSort.sort((left, right) => right.rating - left.rating);
    }
  }, [mediaEntries, internalMediaTypeFilter, sortBy]);

  return (
    <>
      <section className="px-6 py-4">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
          <h2 className="text-2xl font-bold flex items-center gap-2">
            <span className="material-symbols-outlined text-primary">
              play_circle
            </span>
            Your {statusSectionType}
          </h2>
          <div className="flex flex-wrap gap-2">
            <Dropdown
              options={mediaTypeOptions}
              value={internalMediaTypeFilter}
              onChange={(val) => setSectionMediaTypeFilter(Number(val))}
              prefix="Type:"
            />
            <button className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-slate-100 dark:bg-slate-800 text-xs font-medium border border-slate-200 dark:border-slate-700">
              Genre: Sci-Fi{" "}
              <span className="material-symbols-outlined text-xs">
                expand_more
              </span>
            </button>
            <Dropdown
              options={sortOptions as DropdownItem[]}
              value={sortBy}
              onChange={(value) => setSortBy(value as SortOptionValue)}
              prefix="Sort:"
            />
          </div>
        </div>
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-6">
          {filteredEntries.map((entry) => (
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
