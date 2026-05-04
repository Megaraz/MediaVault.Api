// ─────────────────────────────────────────────────────────────
// MediaEntryForm.tsx
//
// Renders the input fields inside the create/edit modal.
// It is a "controlled component" — it does not manage its own state.
// The parent (MediaEntryModal) owns the form data and passes it down.
//
// Flow:
//   1. If no media type is selected yet (mediaType < 0), only the type
//      picker is shown so the user picks a type first.
//   2. Once a type is selected, the full form renders including
//      type-specific fields at the bottom (e.g. runtime for movies).
// ─────────────────────────────────────────────────────────────
import {
  MediaTypeLabels,
  StatusLabels,
  MediaType,
} from "../../Clients/MediaEntriesClient";
import type { SelectOptionItem } from "../../Components/Shared/SelectOption";
import InputText from "../../Components/Shared/InputText";
import SelectOption from "../../Components/Shared/SelectOption";
import StarRating from "../../Components/Shared/StarRating";
import TitleSearchInput from "./TitleSearchInput";
import type { SearchResult } from "./TitleSearchInput";

// All form fields in a single flat object.
// Type-specific fields (runtimeMinutes, author, etc.) are always present
// but only rendered and populated when the matching media type is selected.
export type MediaEntryFormData = {
  title?: string;
  imageUrl?: string;
  backdropUrl?: string;
  mediaType: number;
  status: number;
  rating: number;
  review: string;
  releaseDate?: string;
  genres: string[];
  overview?: string;
  // Movie-specific
  runtimeMinutes?: string;
  // TV Series-specific
  totalEpisodes?: string;
  totalWatchedEpisodes?: string;
  // Game-specific
  metaCriticRating?: number;
  hoursPlayed?: string;
  platforms?: string;
  website?: string;
  // Book / Manga-specific
  author?: string;
};

type MediaEntryFormProps = {
  formData: MediaEntryFormData;
  onChange: (field: keyof MediaEntryFormData, value: string | number) => void;
  onSelectResult: (result: SearchResult) => void;
  isEditMode: boolean;
};

// Build dropdown option lists once (outside the component) so they
// are not recreated on every render.
const mediaTypeOptions: SelectOptionItem[] = Object.entries(
  MediaTypeLabels,
).map(([value, label]) => ({ value: Number(value), label }));

const statusOptions: SelectOptionItem[] = Object.entries(StatusLabels).map(
  ([value, label]) => ({ value: Number(value), label }),
);

export default function MediaEntryForm({
  formData,
  onChange,
  onSelectResult,
  isEditMode,
}: MediaEntryFormProps) {
  // Step 1: if no type chosen yet, only show the type selector.
  // mediaType -1 is the "not selected" sentinel set in buildInitialFormData.
  if (formData.mediaType < 0) {
    return (
      <div>
        <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
          Media Type
        </label>
        <SelectOption
          options={mediaTypeOptions}
          value={formData.mediaType}
          onChange={(val) => onChange("mediaType", Number(val))}
        />
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
      {/* Title */}
      <div className="col-span-full">
        <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
          Entry Title
        </label>
        <TitleSearchInput
          placeholder="e.g. Elden Ring, The Great Gatsby"
          titleInputValue={formData.title}
          onChange={(value) => onChange("title", value)}
          mediaType={formData.mediaType}
          isEditMode={isEditMode}
          onSelectResult={(result) => {
            onSelectResult(result);
            onChange("title", result.title);
            if (result.coverImageUrl)
              onChange("imageUrl", result.coverImageUrl);
          }}
        />
      </div>

      <div className="col-span-full">
        <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
          Image URL
        </label>
        <InputText
          type="url"
          value={formData.imageUrl}
          placeholder="https://example.com/cover.jpg"
          onChange={(value) => onChange("imageUrl", value)}
        />
      </div>

      {/* Media Type */}
      <div>
        <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
          Media Type
        </label>
        <SelectOption
          options={mediaTypeOptions}
          value={formData.mediaType}
          onChange={(val) => onChange("mediaType", Number(val))}
        />
      </div>

      {/* Status */}
      <div>
        <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
          Status
        </label>
        <SelectOption
          options={statusOptions}
          value={formData.status}
          onChange={(val) => onChange("status", Number(val))}
        />
      </div>

      {/* Rating */}
      <div className="col-span-full">
        <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
          Rating
        </label>
        <StarRating
          rating={formData.rating}
          onChange={(val) => onChange("rating", val)}
        />
      </div>

      {/* Review */}
      <div className="col-span-full">
        <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
          Your Review
        </label>
        <textarea
          className="w-full px-4 py-3 rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all resize-none"
          placeholder="Write your thoughts here..."
          rows={4}
          value={formData.review}
          onChange={(e) => onChange("review", e.target.value)}
        />
      </div>

      {/* Movie-specific fields */}
      {formData.mediaType === MediaType.Movie && (
        <>
          <div>
            <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
              Runtime (minutes)
            </label>
            <InputText
              type="number"
              value={formData.runtimeMinutes}
              placeholder="e.g. 148"
              onChange={(val) => onChange("runtimeMinutes", val)}
            />
          </div>
          <div>
            <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
              Release Date
            </label>
            <InputText
              type="date"
              value={formData.releaseDate}
              onChange={(val) => onChange("releaseDate", val)}
            />
          </div>
          <div className="col-span-full">
            <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
              Genres
            </label>
            <InputText
              value={
                Array.isArray(formData.genres)
                  ? formData.genres.join(", ")
                  : formData.genres
              }
              placeholder="e.g. Action, Drama"
              onChange={(val) => onChange("genres", val)}
            />
          </div>
          <div className="col-span-full">
            <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
              Overview
            </label>
            <textarea
              className="w-full px-4 py-3 rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all resize-none"
              placeholder="Short description of the movie..."
              rows={3}
              value={formData.overview ?? ""}
              onChange={(e) => onChange("overview", e.target.value)}
            />
          </div>
          <div className="col-span-full">
            <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
              Backdrop URL
            </label>
            <InputText
              type="url"
              value={formData.backdropUrl}
              placeholder="https://example.com/backdrop.jpg"
              onChange={(val) => onChange("backdropUrl", val)}
            />
          </div>
        </>
      )}

      {/* TV Series-specific fields */}
      {formData.mediaType === MediaType.Series && (
        <>
          <div>
            <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
              Total Episodes
            </label>
            <InputText
              type="number"
              value={formData.totalEpisodes}
              placeholder="e.g. 24"
              onChange={(val) => onChange("totalEpisodes", val)}
            />
          </div>
          <div>
            <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
              Episodes Watched
            </label>
            <InputText
              type="number"
              value={formData.totalWatchedEpisodes}
              placeholder="e.g. 12"
              onChange={(val) => onChange("totalWatchedEpisodes", val)}
            />
          </div>
        </>
      )}

      {/* Game-specific fields */}
      {formData.mediaType === MediaType.Game && (
        <>
          <div>
            <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
              Hours Played
            </label>
            <InputText
              type="number"
              value={formData.hoursPlayed}
              placeholder="e.g. 80"
              onChange={(val) => onChange("hoursPlayed", val)}
            />
          </div>
          <div>
            <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
              Metacritic Rating
            </label>
            <InputText
              type="number"
              value={formData.metaCriticRating?.toString()}
              placeholder="e.g. 87"
              onChange={(val) => onChange("metaCriticRating", Number(val))}
            />
          </div>
          <div className="col-span-full">
            <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
              Platforms
            </label>
            <InputText
              value={formData.platforms}
              placeholder="e.g. PC, PlayStation 5"
              onChange={(val) => onChange("platforms", val)}
            />
          </div>
          <div className="col-span-full">
            <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
              Website
            </label>
            <InputText
              type="url"
              value={formData.website}
              placeholder="https://example.com"
              onChange={(val) => onChange("website", val)}
            />
          </div>
        </>
      )}

      {/* Book / Manga-specific fields */}
      {(formData.mediaType === MediaType.Book ||
        formData.mediaType === MediaType.Manga) && (
        <div>
          <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
            Author
          </label>
          <InputText
            value={formData.author}
            placeholder="e.g. Kentaro Miura"
            onChange={(val) => onChange("author", val)}
          />
        </div>
      )}
    </div>
  );
}
