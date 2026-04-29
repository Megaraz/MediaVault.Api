// ─────────────────────────────────────────────────────────────
// Dashboard.tsx
//
// The main page the user lands on after logging in.
// Responsible for:
//   - Fetching all media entries for the logged-in user
//   - Rendering them grouped by status (On Going, Completed, etc.)
//   - Opening the create/edit modal
//   - Routing create/update calls to the correct type-specific client
// ─────────────────────────────────────────────────────────────
import { useEffect, useState } from "react";
import { Navigate } from "react-router-dom";
import MediaEntriesClient, {
  type MediaEntryDetailedDto,
  type MediaEntryMinimalDto,
  MediaType,
  StatusType,
} from "../../Clients/MediaEntriesClient";
import MovieEntriesClient from "../../Clients/MovieEntriesClient";
import TvSeriesEntriesClient from "../../Clients/TvSeriesEntriesClient";
import GameEntriesClient from "../../Clients/GameEntriesClient";
import BookEntriesClient from "../../Clients/BookEntriesClient";
import MangaEntriesClient from "../../Clients/MangaEntriesClient";
import type { MediaEntryFormData } from "../MediaEntry/MediaEntryForm";
import MediaEntryModal from "../MediaEntry/MediaEntryModal";
import { useUser } from "../../Shared/UserContext";
import MainHeader from "../Dashboard/MainHeader";
import Sidebar from "../Dashboard/Sidebar";
import EntriesSectionMain from "../Dashboard/EntriesSectionMain";
import EntriesSectionSub from "../Dashboard/EntriesSectionSub";
import { statusSections } from "../../Shared/mediaConstants";

export default function Dashboard() {
  const { currentUser, isAuthenticated } = useUser();
  const [entries, setEntries] = useState<MediaEntryDetailedDto[]>([]);
  // One shared client for type-agnostic operations (fetch all, fetch by id, delete, search).
  // Separate clients for create/update because each media type has its own endpoint.
  const [client] = useState(() => new MediaEntriesClient());
  const [movieClient] = useState(() => new MovieEntriesClient());
  const [tvSeriesClient] = useState(() => new TvSeriesEntriesClient());
  const [gameClient] = useState(() => new GameEntriesClient());
  const [bookClient] = useState(() => new BookEntriesClient());
  const [mangaClient] = useState(() => new MangaEntriesClient());
  const [, setLoading] = useState(false);
  const [, setError] = useState<string | null>(null);
  const [showPopup, setShowPopup] = useState(false);
  const [selectedEntry, setSelectedEntry] = useState<MediaEntryDetailedDto>();
  // The currently active media type filter driven by the sidebar.
  // -1 (MediaType.All) means show all types.
  const [mainMediaTypeFilter, setMainMediaTypeFilter] = useState<number>(
    MediaType.All,
  );

  useEffect(() => {}, [isAuthenticated]);

  if (!isAuthenticated) {
    return <Navigate to="/" />;
  }

  useEffect(() => {
    const fetchMediaEntries = async () => {
      await handleFetchMediaEntries();
    };
    fetchMediaEntries();
  }, [currentUser]);

  const handleFetchMediaEntries = async () => {
    if (!isAuthenticated || !currentUser) {
      setError(
        "Select a user from the Users API Test page before fetching media entries.",
      );
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const fetched = await client.getMediaEntries();
      setEntries(fetched);
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  };

  // Opens the modal pre-populated with the clicked entry's data.
  const onClickEntry = (entry: MediaEntryDetailedDto) => {
    setSelectedEntry(entry);
    setShowPopup(true);
  };

  // Opens the modal in create mode (no pre-populated entry).
  const onClickCreateEntry = () => {
    if (!currentUser) {
      setError(
        "Select a user from the Users API Test page before creating media entries.",
      );
      return;
    }
    setShowPopup(true);
  };

  // Handles both create (no entryId) and update (entryId provided).
  // Builds shared base fields, then adds type-specific fields and routes
  // to the correct client method based on formData.mediaType.
  const handleSubmitMediaEntry = async (
    formData: MediaEntryFormData,
    entryId?: string,
  ) => {
    if (!isAuthenticated || !currentUser) {
      setError(
        "Select a user from the Users API Test page before saving media entries.",
      );
      throw new Error(
        "Select a user from the Users API Test page before saving media entries.",
      );
    }

    // Fields shared by all media types.
    const baseFields = {
      title: formData.title,
      status: formData.status,
      rating: formData.rating,
      imageUrl: formData.imageUrl.trim() || null,
      review: formData.review || null,
    };

    setLoading(true);
    setError(null);
    try {
      if (entryId) {
        // ── UPDATE ──
        // After updating, re-fetch the full entry from the server to get
        // the latest data (including any server-side changes) and update local state.
        switch (formData.mediaType) {
          case MediaType.Movie:
            await movieClient.updateMovie(entryId, {
              ...baseFields,
              runtimeMinutes: Number(formData.runtimeMinutes) || 0,
            });
            break;
          case MediaType.Series:
            await tvSeriesClient.updateTvSeries(entryId, {
              ...baseFields,
              totalEpisodes: Number(formData.totalEpisodes) || 0,
              totalWatchedEpisodes: Number(formData.totalWatchedEpisodes) || 0,
            });
            break;
          case MediaType.Game:
            await gameClient.updateGame(entryId, {
              ...baseFields,
              devStudioName: formData.devStudioName || null,
              hoursPlayed: Number(formData.hoursPlayed) || 0,
            });
            break;
          case MediaType.Book:
            await bookClient.updateBook(entryId, {
              ...baseFields,
              author: formData.author || null,
            });
            break;
          case MediaType.Manga:
            await mangaClient.updateManga(entryId, {
              ...baseFields,
              author: formData.author || null,
            });
            break;
          default:
            throw new Error("Unknown media type: " + formData.mediaType);
        }
        const fetched = await client.getMediaEntryById(entryId);
        setEntries((prev) => prev.map((e) => (e.id === entryId ? fetched : e)));
      } else {
        // ── CREATE ──
        // Add the new entry to local state immediately after the server confirms it.
        let created: MediaEntryDetailedDto;
        switch (formData.mediaType) {
          case MediaType.Movie:
            created = await movieClient.createMovie({
              ...baseFields,
              runtimeMinutes: Number(formData.runtimeMinutes) || 0,
            });
            break;
          case MediaType.Series:
            created = await tvSeriesClient.createTvSeries({
              ...baseFields,
              totalEpisodes: Number(formData.totalEpisodes) || 0,
              totalWatchedEpisodes: Number(formData.totalWatchedEpisodes) || 0,
            });
            break;
          case MediaType.Game:
            created = await gameClient.createGame({
              ...baseFields,
              devStudioName: formData.devStudioName || null,
              hoursPlayed: Number(formData.hoursPlayed) || 0,
            });
            break;
          case MediaType.Book:
            created = await bookClient.createBook({
              ...baseFields,
              author: formData.author || null,
            });
            break;
          case MediaType.Manga:
            created = await mangaClient.createManga({
              ...baseFields,
              author: formData.author || null,
            });
            break;
          default:
            throw new Error("Unknown media type: " + formData.mediaType);
        }
        setEntries((prev) => [...prev, created]);
      }
    } catch (err) {
      setError((err as Error).message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteMediaEntry = async (entryId: string) => {
    if (!isAuthenticated || !currentUser) {
      setError(
        "Select a user from the Users API Test page before deleting media entries.",
      );
      throw new Error(
        "Select a user from the Users API Test page before deleting media entries.",
      );
    }

    setLoading(true);
    setError(null);
    try {
      await client.deleteMediaEntry(entryId);
      setEntries((prev) => prev.filter((e) => e.id !== entryId));
    } catch (err) {
      setError((err as Error).message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Fetches the full detailed entry for a search result and opens the edit modal.
  const handleSelectSearchResult = async (minimalEntry: MediaEntryMinimalDto) => {
    try {
      const detailed = await client.getMediaEntryById(minimalEntry.id);
      setSelectedEntry(detailed);
      setShowPopup(true);
    } catch (err) {
      setError((err as Error).message);
    }
  };

  const onChangeMainMediaTypeFilter = (mediaType: number | undefined) => {
    setMainMediaTypeFilter(mediaType ?? MediaType.All);
  };

  const onCancel = () => {
    setShowPopup(false);
    setSelectedEntry(undefined);
  };

  return (
    <div className="bg-background-light dark:bg-background-dark text-slate-900 dark:text-slate-100 font-display">
      <div className="relative flex min-h-screen w-full flex-col overflow-x-hidden">
        <div className="flex h-full grow">
          {/* Media Entry Modal Popup Window */}
          {showPopup && (
            <MediaEntryModal
              detailedEntry={selectedEntry}
              onCancel={onCancel}
              onSubmit={handleSubmitMediaEntry}
              onDelete={handleDeleteMediaEntry}
            />
          )}

          {/* Sidebar */}
          <Sidebar
            currentMainMediaTypeFilter={mainMediaTypeFilter}
            onChangeMediaTypeFilter={onChangeMainMediaTypeFilter}
          />

          {/* <!-- Main Content Area --> */}
          <main className="flex-1 flex flex-col min-w-0 h-screen overflow-y-auto">
            {/* Main Header for Dashboard with search and add entry button */}
            <MainHeader
              onClickAddEntry={onClickCreateEntry}
              onSelectSearchResult={handleSelectSearchResult}
            />

            // Each section shows entries for one status value.
  // Backlog gets a compact "list" view; all other statuses get the card grid.
  {entries.length > 0 && (
              <>
                {statusSections.map(({ type, title }) => {
                  const sectionEntriesByStatus = entries.filter(
                    (e) => e.status === type,
                  );

                  return type === StatusType.Backlog ? (
                    <EntriesSectionSub
                      key={type}
                      mediaEntries={sectionEntriesByStatus}
                      onClickEntry={onClickEntry}
                      statusSectionType={title}
                      currentMainMediaTypeFilter={mainMediaTypeFilter}
                    />
                  ) : (
                    <EntriesSectionMain
                      key={type}
                      mediaEntries={sectionEntriesByStatus}
                      onClickEntry={onClickEntry}
                      statusSectionType={title}
                      currentMainMediaTypeFilter={mainMediaTypeFilter}
                    />
                  );
                })}
              </>
            )}

            {/* <!-- Sticky Mobile Nav --> */}
            <div className="lg:hidden sticky bottom-0 z-20 w-full flex items-center justify-around p-3 bg-background-light/90 dark:bg-background-dark/90 backdrop-blur-xl border-t border-slate-200 dark:border-slate-800">
              <button className="p-2 text-primary">
                <span className="material-symbols-outlined">dashboard</span>
              </button>
              <button className="p-2 text-slate-500">
                <span className="material-symbols-outlined">library_books</span>
              </button>
              <button className="flex items-center justify-center h-12 w-12 rounded-full bg-primary text-white shadow-lg shadow-primary/30 -mt-8">
                <span className="material-symbols-outlined">add</span>
              </button>
              <button className="p-2 text-slate-500">
                <span className="material-symbols-outlined">insights</span>
              </button>
              <button className="p-2 text-slate-500">
                <span className="material-symbols-outlined">person</span>
              </button>
            </div>
          </main>
        </div>
      </div>
    </div>
  );
}
