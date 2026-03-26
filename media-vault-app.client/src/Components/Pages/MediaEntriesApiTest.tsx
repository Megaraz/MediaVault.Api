import { useEffect, useState } from "react";
import { Navigate } from "react-router-dom";
import MediaEntriesClient, {
  type MediaEntryDetailedDto,
  type MediaEntrySubmitDto,
  MediaType,
} from "../../Clients/MediaEntriesClient";
import MediaEntry from "../MediaEntry/MediaEntry";
import MediaEntrySmall from "../MediaEntry/MediaEntrySmall";
import { useUser } from "../../Shared/UserContext";

// type MediaEntriesLocationState = {
//   selectedUser?: UserDetailedDto;
// };

export default function MediaEntriesApiTest() {
  const { currentUser, isAuthenticated } = useUser();
  const [entries, setEntries] = useState<MediaEntryDetailedDto[]>([]);
  const [client] = useState(new MediaEntriesClient());
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showPopup, setShowPopup] = useState(false);
  const [selectedEntry, setSelectedEntry] = useState<MediaEntryDetailedDto>();

  useEffect(() => {
    // Effect runs when currentUser changes, but we handle redirect in JSX
  }, [isAuthenticated]);

  if (!isAuthenticated) {
    return <Navigate to="/" />;
  }

  const onClickFetchEntries = async () => {
    if (!currentUser) {
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
    if (!currentUser) {
      setError(
        "Select a user from the Users API Test page before creating media entries.",
      );
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const created = await client.createMediaEntry(dto);
      setEntries((prev) => [...prev, created]);
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
      setShowPopup(false);
    }
  };

  const handleDeleteMediaEntry = async (entryId: string) => {
    if (!currentUser) {
      setError(
        "Select a user from the Users API Test page before deleting media entries.",
      );
      return;
    }

    setLoading(true);
    setError(null);
    try {
      await client.deleteMediaEntry(entryId);
      setEntries((prev) => prev.filter((e) => e.id !== entryId));
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
      setSelectedEntry(undefined);
      setShowPopup(false);
    }
  };

  const handleUpdateMediaEntry = async (
    updateDto: MediaEntrySubmitDto,
    entryId?: string,
  ) => {
    if (!currentUser) {
      setError(
        "Select a user from the Users API Test page before updating media entries.",
      );
      return;
    }

    if (!entryId) {
      setError("Entry ID is required for updating a media entry.");
      return;
    }

    setLoading(true);
    setError(null);
    try {
      await client.updateMediaEntry(entryId, updateDto);

      const fetched = await client.getMediaEntryById(entryId);
      setEntries((prev) => prev.map((e) => (e.id === entryId ? fetched : e)));
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
      setSelectedEntry(undefined);
      setShowPopup(false);
    }
  };

  const onCancel = () => {
    setShowPopup(false);
    setSelectedEntry(undefined);
  };

  const mediaSections = [
    { type: MediaType.Game, title: "Games" },
    { type: MediaType.Book, title: "Books" },
    { type: MediaType.Movie, title: "Movies" },
    { type: MediaType.Series, title: "Series" },
    { type: MediaType.Manga, title: "Manga" },
  ];

  // const statusSections = [
  //   { type: StatusType.OnGoing, title: "On Going" },
  //   { type: StatusType.Completed, title: "Completed" },
  //   { type: StatusType.Backlog, title: "Backlog" },
  //   { type: StatusType.Dropped, title: "Dropped" },
  // ];

  return (
    <div
      className={`${loading ? "opacity-50 pointer-events-none" : ""} flex w-2/3 flex-col items-center gap-6 p-6`}
    >
      {showPopup && currentUser && (
        <MediaEntry
          detailedEntry={selectedEntry ?? undefined}
          onSubmit={(dto) =>
            selectedEntry
              ? handleUpdateMediaEntry(dto, selectedEntry.id)
              : handleCreateMediaEntry(dto)
          }
          onCancel={onCancel}
          onDelete={() =>
            selectedEntry && handleDeleteMediaEntry(selectedEntry.id)
          }
        />
      )}
      {/* MediaEntries Header */}
      <div className="flex flex-col gap-4">
        <div className="flex flex-col gap-2">
          <h1 className="text-2xl font-bold">Media Entries API Test</h1>
          <p>This page is for testing the Media Entries API.</p>
        </div>
        {currentUser ? (
          <div className="max-w-fit rounded-xl border border-blue-200 bg-blue-50 py-4 px-5 text-sm text-slate-700">
            <p className="font-semibold text-slate-900">
              Handle Media Entries for User: {currentUser.username}
            </p>
            <p>Email: {currentUser.email}</p>
            <p>User ID: {currentUser.id}</p>
          </div>
        ) : (
          <div className="max-w-[66%] rounded-xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-900">
            <p className="font-semibold">No user selected.</p>
            <p className="mb-3">
              Go to the Users API Test page, fetch users, and click one to load
              that user here.
            </p>
          </div>
        )}
      </div>

      {error && <div className="text-red-600 font-semibold">⚠️ {error}</div>}

      <div className="flex flex-row justify-between gap-6">
        <div className="flex flex-col justify-center items-center gap-4">
          {loading ? (
            <div className="flex flex-col items-center gap-4">
              <div className="flex items-center justify-center">
                <div className="relative h-12 w-12">
                  <div className="absolute inset-0 rounded-full border-4 border-gray-300 border-t-blue-500 animate-spin"></div>
                </div>
              </div>
              <p className="text-sm text-gray-600">Loading...</p>
            </div>
          ) : (
            <div>
              <div className="flex flex-row justify-center gap-6">
                <button
                  onClick={onClickFetchEntries}
                  className="mb-4 px-4 max-h-fit py-2 bg-blue-500 text-white rounded disabled:cursor-not-allowed disabled:bg-slate-400"
                  disabled={loading || !currentUser}
                >
                  Fetch Media Entries
                </button>
                <button
                  onClick={onClickCreateEntry}
                  className="mb-4 px-4 max-h-fit py-2 bg-green-500 text-white rounded disabled:cursor-not-allowed disabled:bg-slate-400"
                  disabled={loading || !currentUser}
                >
                  Create New Entry
                </button>
              </div>
              {entries.length > 0 && (
                <div className="flex flex-col gap-4">
                  <h2 className="text-lg font-semibold">All fetched entries</h2>
                  <div className="flex flex-row flex-wrap gap-6">
                    {mediaSections.map(({ type, title }) => {
                      const sectionEntries = entries.filter(
                        (e) => e.mediaType === type,
                      );

                      if (sectionEntries.length === 0) {
                        return null;
                      }

                      return (
                        <div key={type} className="flex flex-col gap-2">
                          <h3 className="text-md font-medium">{title}</h3>
                          {sectionEntries.map((entry) => (
                            <MediaEntrySmall
                              key={entry.id}
                              entry={entry}
                              onClickEntry={onClickEntry}
                            />
                          ))}
                        </div>
                      );
                    })}
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
