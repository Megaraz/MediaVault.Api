import { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import MediaEntriesClient, {
  type MediaEntryDetailedDto,
  type MediaEntryCreateDto,
  StatusLabels,
  MediaTypeLabels,
} from "../../Clients/MediaEntriesClient";
import type { UserDetailedDto } from "../../Clients/UsersClient";

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

  const fetchEntries = async () => {
    if (!selectedUser) {
      setError("Select a user from the Users API Test page before fetching media entries.");
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

  const createEntry = async (dto: MediaEntryCreateDto) => {
    if (!selectedUser) {
      setError("Select a user from the Users API Test page before creating media entries.");
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
    }
  };

  return (
    <div className="flex flex-row gap-6 justify-center items-start p-6">
      <div
        className={`${loading ? "opacity-50 pointer-events-none" : ""} flex flex-col gap-6 p-6`}
      >
        <div className="flex flex-col gap-4">
          <h1 className="text-2xl font-bold">Media Entries API Test</h1>
          <p>This page is for testing the Media Entries API.</p>
          {selectedUser ? (
            <div className="rounded-xl border border-blue-200 bg-blue-50 p-4 text-sm text-slate-700">
              <p className="font-semibold text-slate-900">
                Handle Media Entries for User: {selectedUser.username}
              </p>
              <p>Email: {selectedUser.email}</p>
              <p>User ID: {selectedUser.id}</p>
            </div>
          ) : (
            <div className="rounded-xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-900">
              <p className="font-semibold">No user selected.</p>
              <p className="mb-3">
                Go to the Users API Test page, fetch users, and click one to load that user here.
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

        {error && (
          <div className="text-red-600 font-semibold">⚠️ {error}</div>
        )}

        <div className="flex flex-row justify-between gap-6">
          {/* Left side: fetch & list */}
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
              <>
                <button
                  onClick={fetchEntries}
                  className="mb-4 px-4 py-2 bg-blue-500 text-white rounded disabled:cursor-not-allowed disabled:bg-slate-400"
                  disabled={loading || !selectedUser}
                >
                  Fetch Media Entries
                </button>
                {entries.length > 0 && (
                  <>
                    <h2 className="text-lg font-semibold">
                      All fetched entries
                    </h2>
                    <ul className="flex flex-col gap-6">
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
                  </>
                )}
              </>
            )}
          </div>

          {/* Right side: create form */}
          <div className="flex flex-col gap-4">
            <h2 className="text-lg font-semibold">Add a new media entry</h2>
            <form
              onSubmit={(e) => {
                e.preventDefault();
                const formData = new FormData(e.currentTarget);
                const ratingStr = formData.get("rating") as string;
                const releaseYearStr = formData.get("releaseYear") as string;
                createEntry({
                  title: formData.get("title") as string,
                  mediaType: Number(formData.get("mediaType")),
                  status: Number(formData.get("status")),
                  rating: ratingStr ? Number(ratingStr) : null,
                  genre: (formData.get("genre") as string) || null,
                  releaseYear: releaseYearStr ? Number(releaseYearStr) : null,
                  review: (formData.get("review") as string) || null,
                  imageUrl: (formData.get("imageUrl") as string) || null,
                });
                e.currentTarget.reset();
              }}
              className="space-y-4"
            >
              <div>
                <label htmlFor="title" className="block text-sm font-medium">
                  Title *
                </label>
                <input
                  id="title"
                  name="title"
                  type="text"
                  required
                  className="mt-1 px-3 py-2 border rounded text-black w-full"
                />
              </div>
              <div>
                <label
                  htmlFor="mediaType"
                  className="block text-sm font-medium"
                >
                  Media Type *
                </label>
                <select
                  id="mediaType"
                  name="mediaType"
                  required
                  className="mt-1 px-3 py-2 border rounded text-black w-full"
                >
                  {Object.entries(MediaTypeLabels).map(([value, label]) => (
                    <option key={value} value={value}>
                      {label}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label htmlFor="status" className="block text-sm font-medium">
                  Status *
                </label>
                <select
                  id="status"
                  name="status"
                  required
                  className="mt-1 px-3 py-2 border rounded text-black w-full"
                >
                  {Object.entries(StatusLabels).map(([value, label]) => (
                    <option key={value} value={value}>
                      {label}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label htmlFor="rating" className="block text-sm font-medium">
                  Rating (0.5 - 10)
                </label>
                <input
                  id="rating"
                  name="rating"
                  type="number"
                  min="0.5"
                  max="10"
                  step="0.5"
                  className="mt-1 px-3 py-2 border rounded text-black w-full"
                />
              </div>
              <div>
                <label htmlFor="genre" className="block text-sm font-medium">
                  Genre
                </label>
                <input
                  id="genre"
                  name="genre"
                  type="text"
                  className="mt-1 px-3 py-2 border rounded text-black w-full"
                />
              </div>
              <div>
                <label
                  htmlFor="releaseYear"
                  className="block text-sm font-medium"
                >
                  Release Year
                </label>
                <input
                  id="releaseYear"
                  name="releaseYear"
                  type="number"
                  min="1900"
                  max="2100"
                  className="mt-1 px-3 py-2 border rounded text-black w-full"
                />
              </div>
              <div>
                <label htmlFor="review" className="block text-sm font-medium">
                  Review
                </label>
                <textarea
                  id="review"
                  name="review"
                  rows={3}
                  className="mt-1 px-3 py-2 border rounded text-black w-full"
                />
              </div>
              <div>
                <label htmlFor="imageUrl" className="block text-sm font-medium">
                  Image URL
                </label>
                <input
                  id="imageUrl"
                  name="imageUrl"
                  type="url"
                  className="mt-1 px-3 py-2 border rounded text-black w-full"
                />
              </div>
              <button
                type="submit"
                className="px-4 py-2 bg-green-500 text-white rounded disabled:cursor-not-allowed disabled:bg-slate-400"
                disabled={loading || !selectedUser}
              >
                Add Media Entry
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}
