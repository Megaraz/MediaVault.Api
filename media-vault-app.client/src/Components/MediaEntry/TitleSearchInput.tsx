import { useEffect, useRef, useState } from "react";
import RawgApiClient, {
  type GameSearchResultDto,
} from "../../Clients/RawgApiClient";
import { MediaType } from "../../Clients/MediaEntriesClient";

type TitleSearchProps = {
  titleInputValue?: string;
  onChange: (newValue: string) => void;
  //   /** Called when the user picks a game from the dropdown, so the parent can set imageUrl etc. */
  onSelectGame?: (game: GameSearchResultDto) => void;
  placeholder?: string;
  className?: string;
  mediaType?: number;
  isEditMode: boolean;
};

// How long to wait after the user stops typing before firing the API call.
const DEBOUNCE_DELAY_MS = 400;
// Minimum characters before we start searching.
const MIN_SEARCH_LENGTH = 3;

const defaultClassName: string =
  "w-full px-4 py-3 rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all";

export default function TitleSearchInput({
  titleInputValue: value = "",
  onChange,
  onSelectGame,
  className = defaultClassName,
  placeholder = "",
  mediaType,
  isEditMode,
}: TitleSearchProps) {
  // Lazily create the client once (arrow function form of useState avoids re-creating on every render)
  const [client] = useState(() => new RawgApiClient());
  const [searchResults, setSearchResults] = useState<GameSearchResultDto[]>([]);
  const [showDropdown, setShowDropdown] = useState(false);
  const [isSearching, setIsSearching] = useState(false);

  // Ref to hold the debounce timer ID so we can cancel it when the user types again
  const debounceTimer = useRef<ReturnType<typeof setTimeout> | null>(null);
  // Flag to skip the next search when value changed because the user picked a result
  const justSelected = useRef(false);

  // Flag to track if the user has typed in the input, to avoid searching on initial value when in edit mode
  const userHasTyped = useRef(!isEditMode);

  // Only enable typeahead when the selected media type is "Game"
  const isSearchEnabled = mediaType === MediaType.Game;

  // ── Debounced search effect ──
  // Runs every time `value` or `isSearchEnabled` changes.
  // It waits DEBOUNCE_DELAY_MS after the last keystroke, then fires the API call.
  useEffect(() => {
    // Cancel any previously queued search (this is the "debounce" part)
    if (debounceTimer.current) {
      clearTimeout(debounceTimer.current);
    }

    // Don't search if the user hasn't typed yet (e.g. we're in edit mode and just got the initial value)
    if (!userHasTyped.current) {
      return;
    }

    // If the value changed because the user picked a result, skip this search
    if (justSelected.current) {
      justSelected.current = false;
      return;
    }

    // Don't search if feature is disabled or the query is too short
    if (!isSearchEnabled || value.length < MIN_SEARCH_LENGTH) {
      setSearchResults([]);
      setShowDropdown(false);
      return;
    }

    // Queue a new search after the delay
    debounceTimer.current = setTimeout(async () => {
      setIsSearching(true);
      try {
        const results = await client.searchGames(
          { search: value },
          1, // page
          8, // pageSize – keep the dropdown manageable
        );
        setSearchResults(results);
        setShowDropdown(results.length > 0);
      } catch {
        setSearchResults([]);
        setShowDropdown(false);
      } finally {
        setIsSearching(false);
      }
    }, DEBOUNCE_DELAY_MS);

    // Cleanup: cancel the timer if the component unmounts or value changes before it fires
    return () => {
      if (debounceTimer.current) {
        clearTimeout(debounceTimer.current);
      }
    };
  }, [value, isSearchEnabled]);

  const handleSelectGame = (game: GameSearchResultDto) => {
    // Tell the effect to skip the search triggered by this value change
    justSelected.current = true;
    // Update the title field with the selected game's title
    onChange(game.title);
    // Notify the parent so it can also set imageUrl / other fields
    onSelectGame?.(game);
    // Close the dropdown
    setShowDropdown(false);
    setSearchResults([]);
  };

  return (
    <div className="relative">
      <input
        type="text"
        value={value}
        onChange={(e) => {
          onChange(e.target.value);
          userHasTyped.current = true;
        }}
        onFocus={() => {
          // Re-show the dropdown if we still have results (e.g. user clicked away and came back)
          if (searchResults.length > 0) setShowDropdown(true);
        }}
        onBlur={() => {
          // Delay closing so that a click on a dropdown item can register first
          setTimeout(() => setShowDropdown(false), 150);
        }}
        className={className}
        placeholder={placeholder}
      />

      {/* Spinning icon while a search is in-flight */}
      {isSearching && (
        <div className="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none">
          <span className="material-symbols-outlined animate-spin text-lg text-slate-400">
            progress_activity
          </span>
        </div>
      )}

      {/* ── Dropdown with search results ── */}
      {showDropdown && (
        <ul className="absolute z-50 mt-1 w-full max-h-60 overflow-y-auto rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 shadow-lg">
          {searchResults.map((game) => (
            <li
              key={game.externalId}
              // onMouseDown fires before onBlur, so the click registers before the dropdown hides
              onMouseDown={() => handleSelectGame(game)}
              className="flex items-center gap-3 px-4 py-2 cursor-pointer hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
            >
              {game.coverImageUrl && (
                <img
                  src={game.coverImageUrl}
                  alt={game.title}
                  className="h-10 w-10 rounded object-cover shrink-0"
                />
              )}
              <span className="truncate text-sm text-slate-900 dark:text-slate-100">
                {game.title}
              </span>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
