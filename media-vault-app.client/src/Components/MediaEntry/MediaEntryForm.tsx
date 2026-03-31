import {
  MediaTypeLabels,
  StatusLabels,
} from "../../Clients/MediaEntriesClient";
import type { SelectOptionItem } from "../../Components/Shared/SelectOption";
import InputText from "../../Components/Shared/InputText";
import SelectOption from "../../Components/Shared/SelectOption";
import StarRating from "../../Components/Shared/StarRating";
import TitleSearchInput from "./TitleSearchInput";
export type MediaEntryFormData = {
  title: string;
  imageUrl: string;
  mediaType: number;
  status: number;
  rating: number;
  review: string;
};

type MediaEntryFormProps = {
  formData: MediaEntryFormData;
  onChange: (field: keyof MediaEntryFormData, value: string | number) => void;
  isEditMode: boolean;
};

const mediaTypeOptions: SelectOptionItem[] = Object.entries(
  MediaTypeLabels,
).map(([value, label]) => ({ value: Number(value), label }));

const statusOptions: SelectOptionItem[] = Object.entries(StatusLabels).map(
  ([value, label]) => ({ value: Number(value), label }),
);

export default function MediaEntryForm({
  formData,
  onChange,
  isEditMode,
}: MediaEntryFormProps) {
  if (formData.mediaType === 0) {
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
          onSelectGame={(game) => {
            // When a game is picked from the dropdown, also fill in the cover image
            onChange("title", game.title);
            if (game.coverImageUrl) onChange("imageUrl", game.coverImageUrl);
          }}
        />

        {/* <input
          className="w-full px-4 py-3 rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all"
          placeholder="e.g. Elden Ring, The Great Gatsby"
          type="text"
          value={formData.title}
          onChange={(e) => onChange("title", e.target.value)}
        /> */}
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
    </div>
  );
}
