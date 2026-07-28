import { useEffect, useRef, useState } from "react";
import MediaEntriesClient, {
  type MediaEntryMinimalDto,
  MediaTypeLabels,
} from "../../Clients/MediaEntriesClient";

type Props = {
  onClickAddEntry: () => void;
  onSelectSearchResult: (entry: MediaEntryMinimalDto) => void;
};

const DEBOUNCE_DELAY_MS = 400;
const MIN_SEARCH_LENGTH = 3;

export default function MainHeader({
  onClickAddEntry,
  onSelectSearchResult,
}: Props) {
  const [client] = useState(() => new MediaEntriesClient());
  const [searchQuery, setSearchQuery] = useState("");
  const [searchResults, setSearchResults] = useState<MediaEntryMinimalDto[]>([]);
  const [showDropdown, setShowDropdown] = useState(false);
  const [isSearching, setIsSearching] = useState(false);
  const debounceTimer = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (debounceTimer.current) {
      clearTimeout(debounceTimer.current);
    }

    if (searchQuery.length < MIN_SEARCH_LENGTH) {
      setSearchResults([]);
      setShowDropdown(false);
      return;
    }

    debounceTimer.current = setTimeout(async () => {
      setIsSearching(true);
      try {
        const results = await client.searchMediaEntries({ query: searchQuery });
        setSearchResults(results);
        setShowDropdown(results.length > 0);
      } catch {
        setSearchResults([]);
        setShowDropdown(false);
      } finally {
        setIsSearching(false);
      }
    }, DEBOUNCE_DELAY_MS);

    return () => {
      if (debounceTimer.current) {
        clearTimeout(debounceTimer.current);
      }
    };
  }, [client, searchQuery]);

  const handleSelectResult = (entry: MediaEntryMinimalDto) => {
    setSearchQuery("");
    setShowDropdown(false);
    setSearchResults([]);
    onSelectSearchResult(entry);
  };

  return (
    <>
      {/* <!-- Header --> */}
      <header className="sticky top-0 z-10 flex items-center justify-between px-8 py-4 bg-background-light/80 dark:bg-background-dark/80 backdrop-blur-md border-b border-slate-200 dark:border-slate-800">
        <div className="flex items-center gap-6 flex-1">
          <div className="relative w-full max-w-md">
            <span className="material-symbols-outlined pointer-events-none absolute inset-y-0 left-3 flex items-center text-slate-400 text-xl">
              search
            </span>
            <input
              className="w-full pl-10 pr-4 py-2 rounded-lg bg-slate-100 dark:bg-slate-800 border-none focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all text-sm"
              placeholder="Search your library..."
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onFocus={() => {
                if (searchResults.length > 0) setShowDropdown(true);
              }}
              onBlur={() => {
                setTimeout(() => setShowDropdown(false), 150);
              }}
            />

            {/* Spinning icon while searching */}
            {isSearching && (
              <div className="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none">
                <span className="material-symbols-outlined animate-spin text-lg text-slate-400">
                  progress_activity
                </span>
              </div>
            )}

            {/* Dropdown with search results */}
            {showDropdown && (
              <ul className="absolute z-50 mt-1 w-full max-h-60 overflow-y-auto rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 shadow-lg">
                {searchResults.map((entry) => (
                  <li
                    key={entry.id}
                    onMouseDown={() => handleSelectResult(entry)}
                    className="flex items-center gap-3 px-4 py-2 cursor-pointer hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
                  >
                    {entry.imageUrl && (
                      <img
                        src={entry.imageUrl}
                        alt={entry.title}
                        className="h-10 w-10 rounded object-cover shrink-0"
                      />
                    )}
                    <div className="flex flex-col min-w-0">
                      <span className="truncate text-sm text-slate-900 dark:text-slate-100">
                        {entry.title}
                      </span>
                      <span className="text-xs text-slate-400">
                        {MediaTypeLabels[entry.mediaType] ?? "Unknown"}
                      </span>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
        <div className="flex items-center gap-4">
          <button
            onClick={onClickAddEntry}
            className="hidden sm:flex items-center gap-2 bg-primary hover:bg-primary/90 text-white px-4 py-2 rounded-lg font-bold text-sm transition-colors shadow-lg shadow-primary/20"
          >
            <span className="material-symbols-outlined text-sm">add</span>
            <span>Add New Entry</span>
          </button>
          <button className="p-2 text-slate-500 hover:text-primary transition-colors">
            <span className="material-symbols-outlined">notifications</span>
          </button>
          <button className="p-2 text-slate-500 hover:text-primary transition-colors">
            <span className="material-symbols-outlined">settings</span>
          </button>
        </div>
      </header>
    </>
  );
}
