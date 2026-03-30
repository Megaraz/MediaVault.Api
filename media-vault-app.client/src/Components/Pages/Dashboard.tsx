import { useEffect, useState } from "react";
import { Navigate } from "react-router-dom";
import MediaEntriesClient, {
  type MediaEntryDetailedDto,
  type MediaEntrySubmitDto,
  MediaType,
  StatusType,
} from "../../Clients/MediaEntriesClient";
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
  const [client] = useState(new MediaEntriesClient());
  const [, setLoading] = useState(false);
  const [, setError] = useState<string | null>(null);
  const [showPopup, setShowPopup] = useState(false);
  const [selectedEntry, setSelectedEntry] = useState<MediaEntryDetailedDto>();
  const [, setSearchQuery] = useState("");
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

  const onClickEntry = (entry: MediaEntryDetailedDto) => {
    setSelectedEntry(entry);
    setShowPopup(true);
  };

  const onClickCreateEntry = () => {
    if (!currentUser) {
      setError(
        "Select a user from the Users API Test page before creating media entries.",
      );
      return;
    }
    setShowPopup(true);
  };

  const handleCreateMediaEntry = async (dto: MediaEntrySubmitDto) => {
    if (!isAuthenticated || !currentUser) {
      setError(
        "Select a user from the Users API Test page before creating media entries.",
      );
      throw new Error(
        "Select a user from the Users API Test page before creating media entries.",
      );
    }

    setLoading(true);
    setError(null);
    try {
      const created = await client.createMediaEntry(dto);
      setEntries((prev) => [...prev, created]);
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

  const onChangeMainMediaTypeFilter = (mediaType: number | undefined) => {
    setMainMediaTypeFilter(mediaType ?? MediaType.All);
  };

  const handleUpdateMediaEntry = async (
    updateDto: MediaEntrySubmitDto,
    entryId?: string,
  ) => {
    if (!isAuthenticated || !currentUser) {
      setError(
        "Select a user from the Users API Test page before updating media entries.",
      );
      throw new Error(
        "Select a user from the Users API Test page before updating media entries.",
      );
    }

    if (!entryId) {
      setError("Entry ID is required for updating a media entry.");
      throw new Error("Entry ID is required for updating a media entry.");
    }

    setLoading(true);
    setError(null);
    try {
      await client.updateMediaEntry(entryId, updateDto);

      const fetched = await client.getMediaEntryById(entryId);
      setEntries((prev) => prev.map((e) => (e.id === entryId ? fetched : e)));
    } catch (err) {
      setError((err as Error).message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = (query: string) => {
    setSearchQuery(query);

      // For simplicity, this example only filters the already fetched entries on the client side.
      // In a real application, you might want to implement server-side searching.

    // This will trigger the useMemo hooks in the EntriesSection components to re-filter the displayed entries based on the search query.



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
              onSubmit={
                selectedEntry ? handleUpdateMediaEntry : handleCreateMediaEntry
              }
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
              onChangeSearch={(query) => handleSearch(query)}
            />

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
