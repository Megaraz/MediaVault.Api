import { useEffect, useState } from "react";
import { Navigate } from "react-router-dom";
import MediaEntriesClient, {
  type MediaEntryDetailedDto,
  type MediaEntrySubmitDto,
  MediaType,
} from "../../Clients/MediaEntriesClient";
import MediaEntry from "../MediaEntry/MediaEntry";
import MediaItem from "../Dashboard/MediaItem";
import { useUser } from "../../Shared/UserContext";

// type MediaEntriesLocationState = {
//   selectedUser?: UserDetailedDto;
// };

export default function Dashboard() {
  const { currentUser, isAuthenticated } = useUser();
  const [entries, setEntries] = useState<MediaEntryDetailedDto[]>([]);
  const [client] = useState(new MediaEntriesClient());
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showPopup, setShowPopup] = useState(false);
  const [selectedEntry, setSelectedEntry] = useState<MediaEntryDetailedDto>();

  useEffect(() => {
  }, [isAuthenticated]);

  if (!isAuthenticated) {
    return <Navigate to="/" />;
  }

  useEffect(() => {
      await handleFetchMediaEntries();
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

  const onClickListedMediaEntry = (entry: MediaEntryDetailedDto) => {
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
    if (!isAuthenticated || !currentUser) {
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
    if (!isAuthenticated || !currentUser) {
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


  return (
<body className="bg-background-light dark:bg-background-dark text-slate-900 dark:text-slate-100 font-display">
<div className="relative flex min-h-screen w-full flex-col overflow-x-hidden">
<div className="flex h-full grow">
{/* <!-- Main Content Area --> */}
<main className="flex-1 flex flex-col min-w-0 h-screen overflow-y-auto">
<div className="p-8 space-y-10">

</div>
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
</body>
  );
}

  return (
    <div
      className={`${loading ? "opacity-50 pointer-events-none" : ""} flex w-2/3 flex-col items-center gap-6 p-6`}
    >
      <div>
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
                    <MediaItem
                      key={entry.id}
                      entry={entry}
                      onClickEntry={onClickListedMediaEntry}
                    />
                  ))}
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
