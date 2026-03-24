import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import MediaEntriesClient, {
  type MediaEntryDetailedDto,
  type MediaEntryCreateDto,
  StatusLabels,
  MediaTypeLabels,
} from "../../Clients/MediaEntriesClient";
import type { UserDetailedDto } from "../../Clients/UsersClient";
import ButtonPrimary from "../../Shared/ButtonPrimary";
import MediaEntry from "../MediaEntry/MediaEntry";

type MediaEntriesLocationState = {
  selectedUser?: UserDetailedDto;
};

export default function MediaEntriesApiTest() {
  const navigate = useNavigate();
  const location = useLocation();
  const selectedUser = (location.state as MediaEntriesLocationState | null)
    ?.selectedUser;
  const [entries, setEntries] = useState<MediaEntryDetailedDto[]>([]);
  const [client] = useState(new MediaEntriesClient());
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [showEditForm, setShowEditForm] = useState(false);

  // useEffect(() => {
  //   const fetchEntries = async () => {
  //     await onClickFetchEntries();
  //   };
  //   fetchEntries();
  // }, [entries]);

  const onClickFetchEntries = async () => {
    if (!selectedUser) {
      setError(
        "Select a user from the Users API Test page before fetching media entries.",
      );
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const fetched = await client.getMediaEntries(selectedUser.id);
      setEntries(fetched);
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  };

  const onClickCreateEntry = () => {
    if (!selectedUser) {
      setError(
        "Select a user from the Users API Test page before creating media entries.",
      );
      return;
    }
    setShowCreateForm(true);
  };

  const handleCreateMediaEntry = async (dto: MediaEntryCreateDto) => {
    if (!selectedUser) {
      setError(
        "Select a user from the Users API Test page before creating media entries.",
      );
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const created = await client.createMediaEntry(selectedUser.id, dto);
      setEntries((prev) => [...prev, created]);
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
      setShowCreateForm(false);
    }
  };

  return (
    <div
      className={`${loading ? "opacity-50 pointer-events-none" : ""} flex w-2/3 flex-col items-center gap-6 p-6`}
    >
      {showCreateForm && selectedUser && (
        <MediaEntry
          createMode={true}
          onCreate={handleCreateMediaEntry}
          onCancel={() => setShowCreateForm(false)}
          onDelete={() => undefined}
          onSubmit={() => undefined}
        />
      )}
      {/* MediaEntries Header */}
      <div className="flex flex-col gap-4">
        <div className="flex flex-col gap-2">
          <h1 className="text-2xl font-bold">Media Entries API Test</h1>
          <p>This page is for testing the Media Entries API.</p>
        </div>
        {selectedUser ? (
          <div className="max-w-fit rounded-xl border border-blue-200 bg-blue-50 py-4 px-5 text-sm text-slate-700">
            <p className="font-semibold text-slate-900">
              Handle Media Entries for User: {selectedUser.username}
            </p>
            <p>Email: {selectedUser.email}</p>
            <p>User ID: {selectedUser.id}</p>
          </div>
        ) : (
          <div className="max-w-2/3 rounded-xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-900">
            <p className="font-semibold">No user selected.</p>
            <p className="mb-3">
              Go to the Users API Test page, fetch users, and click one to load
              that user here.
            </p>
            <button
              type="button"
              onClick={() => navigate("/users-api-test")}
              className="rounded bg-amber-500 px-4 py-2 font-medium text-white transition hover:bg-amber-600"
            >
              Go to Users API Test
            </button>
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
                  disabled={loading || !selectedUser}
                >
                  Fetch Media Entries
                </button>
                <button
                  onClick={onClickCreateEntry}
                  className="mb-4 px-4 max-h-fit py-2 bg-green-500 text-white rounded disabled:cursor-not-allowed disabled:bg-slate-400"
                  disabled={loading || !selectedUser}
                >
                  Create New Entry
                </button>
              </div>
              {entries.length > 0 && (
                <div className="flex flex-col gap-4">
                  <h2 className="text-lg font-semibold">All fetched entries</h2>
                  <ul className="flex flex-row flex-wrap gap-6">
                    {entries.map((entry) => (
                      <div key={entry.id} className="border p-3 rounded">
                        <li>
                          <b>Title:</b> {entry.title}
                        </li>
                        <li>
                          <b>Type:</b>{" "}
                          {MediaTypeLabels[entry.mediaType] ?? entry.mediaType}
                        </li>
                        <li>
                          <b>Status:</b>{" "}
                          {StatusLabels[entry.status] ?? entry.status}
                        </li>
                        <li>
                          <b>Rating:</b> {entry.rating}
                        </li>
                        <li>
                          <b>Genre:</b> {entry.genre ?? "N/A"}
                        </li>
                        <li>
                          <b>Release Year:</b> {entry.releaseYear || "N/A"}
                        </li>
                        <li>
                          <b>Review:</b> {entry.review ?? "N/A"}
                        </li>
                      </div>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
