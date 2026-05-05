// ─────────────────────────────────────────────────────────────
// SeasonSection.tsx
//
// Controlled component for managing the Seasons list inside the
// TV Series create/edit form.
//
// Seasons are owned by TvSeries — no direct API calls happen here.
// All changes are propagated to the parent via onSeasonsChange,
// and saved together with the TvSeries on form submit.
// ─────────────────────────────────────────────────────────────
import { useState } from "react";
import InputText from "../Shared/InputText";
import SelectOption from "../Shared/SelectOption";
import StarRating from "../Shared/StarRating";
import { StatusLabels } from "../../Clients/MediaEntriesClient";
import type { SelectOptionItem } from "../Shared/SelectOption";

const statusOptions: SelectOptionItem[] = Object.entries(StatusLabels).map(
  ([value, label]) => ({ value: Number(value), label }),
);

// ─── Season form state ────────────────────────────────────────
export type SeasonFormData = {
  seasonNumber: string;
  name: string;
  overview: string;
  imageUrl: string;
  airDate: string;
  episodes: string;
  watchedEpisodes: string;
  status: number;
  rating: number;
};

function emptyForm(): SeasonFormData {
  return {
    seasonNumber: "",
    name: "",
    overview: "",
    imageUrl: "",
    airDate: "",
    episodes: "",
    watchedEpisodes: "",
    status: 0,
    rating: 0,
  };
}

// ─── Props ────────────────────────────────────────────────────
type SeasonSectionProps = {
  seasons: SeasonFormData[];
  onSeasonsChange: (seasons: SeasonFormData[]) => void;
};

// ─── Component ───────────────────────────────────────────────
export default function SeasonSection({
  seasons,
  onSeasonsChange,
}: SeasonSectionProps) {
  // null = no season selected; "new" = adding a new one; number = editing by index
  const [activeIndex, setActiveIndex] = useState<number | "new" | null>(null);
  const [editForm, setEditForm] = useState<SeasonFormData>(emptyForm);

  const handleEditChange = (
    field: keyof SeasonFormData,
    value: string | number,
  ) => {
    setEditForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleSelectSeason = (index: number) => {
    setActiveIndex(index);
    setEditForm({ ...seasons[index] });
  };

  const handleOpenNew = () => {
    setActiveIndex("new");
    setEditForm(emptyForm());
  };

  const handleClose = () => {
    setActiveIndex(null);
  };

  const handleSave = () => {
    if (activeIndex === "new") {
      onSeasonsChange([...seasons, editForm]);
    } else if (typeof activeIndex === "number") {
      onSeasonsChange(seasons.map((s, i) => (i === activeIndex ? editForm : s)));
    }
    setActiveIndex(null);
  };

  const handleDelete = () => {
    if (typeof activeIndex === "number") {
      onSeasonsChange(seasons.filter((_, i) => i !== activeIndex));
      setActiveIndex(null);
    }
  };

  const seasonSelectOptions: SelectOptionItem[] = seasons.map((s, i) => ({
    value: i,
    label: s.name
      ? `Season ${s.seasonNumber || i + 1} — ${s.name}`
      : `Season ${s.seasonNumber || i + 1}`,
  }));

  const isEditingSeason = typeof activeIndex === "number";

  return (
    <div className="mt-2 space-y-4">
      {/* Season picker */}
      {seasons.length > 0 && (
        <div>
          <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
            Edit a Season
          </label>
          <SelectOption
            options={[
              { value: "", label: "— Select a season —" },
              ...seasonSelectOptions,
            ]}
            value={isEditingSeason ? (activeIndex ?? "") : ""}
            onChange={(val) => {
              if (val === "") return handleClose();
              handleSelectSeason(Number(val));
            }}
          />
        </div>
      )}

      {seasons.length === 0 && activeIndex === null && (
        <p className="text-sm text-slate-500 dark:text-slate-400">
          No seasons yet. Add one below.
        </p>
      )}

      {/* Add Season button */}
      {activeIndex === null && (
        <button
          type="button"
          onClick={handleOpenNew}
          className="w-full py-2 rounded-lg border border-dashed border-slate-400 dark:border-slate-600 text-sm text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
        >
          + Add Season
        </button>
      )}

      {/* Season form */}
      {activeIndex !== null && (
        <div className="rounded-lg border border-slate-200 dark:border-slate-700 p-4 space-y-4">
          <h3 className="text-sm font-semibold text-slate-700 dark:text-slate-300">
            {activeIndex === "new" ? "New Season" : "Edit Season"}
          </h3>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block mb-1 text-xs font-semibold text-slate-600 dark:text-slate-400">
                Season Number
              </label>
              <InputText
                type="number"
                value={editForm.seasonNumber}
                placeholder="e.g. 1"
                onChange={(val) => handleEditChange("seasonNumber", val)}
              />
            </div>

            <div>
              <label className="block mb-1 text-xs font-semibold text-slate-600 dark:text-slate-400">
                Name (optional)
              </label>
              <InputText
                value={editForm.name}
                placeholder="e.g. The Beginning"
                onChange={(val) => handleEditChange("name", val)}
              />
            </div>

            <div>
              <label className="block mb-1 text-xs font-semibold text-slate-600 dark:text-slate-400">
                Total Episodes
              </label>
              <InputText
                type="number"
                value={editForm.episodes}
                placeholder="e.g. 12"
                onChange={(val) => handleEditChange("episodes", val)}
              />
            </div>

            <div>
              <label className="block mb-1 text-xs font-semibold text-slate-600 dark:text-slate-400">
                Watched Episodes
              </label>
              <InputText
                type="number"
                value={editForm.watchedEpisodes}
                placeholder="e.g. 6"
                onChange={(val) => handleEditChange("watchedEpisodes", val)}
              />
            </div>

            <div>
              <label className="block mb-1 text-xs font-semibold text-slate-600 dark:text-slate-400">
                Air Date
              </label>
              <InputText
                type="date"
                value={editForm.airDate}
                onChange={(val) => handleEditChange("airDate", val)}
              />
            </div>

            <div>
              <label className="block mb-1 text-xs font-semibold text-slate-600 dark:text-slate-400">
                Cover Image URL
              </label>
              <InputText
                type="url"
                value={editForm.imageUrl}
                placeholder="https://..."
                onChange={(val) => handleEditChange("imageUrl", val)}
              />
            </div>

            <div className="col-span-full">
              <label className="block mb-1 text-xs font-semibold text-slate-600 dark:text-slate-400">
                Status
              </label>
              <SelectOption
                options={statusOptions}
                value={editForm.status}
                onChange={(val) => handleEditChange("status", Number(val))}
              />
            </div>

            <div className="col-span-full">
              <label className="block mb-1 text-xs font-semibold text-slate-600 dark:text-slate-400">
                Rating
              </label>
              <StarRating
                rating={editForm.rating}
                onChange={(val) => handleEditChange("rating", val)}
              />
            </div>

            <div className="col-span-full">
              <label className="block mb-1 text-xs font-semibold text-slate-600 dark:text-slate-400">
                Overview
              </label>
              <textarea
                className="w-full px-4 py-3 rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all resize-none"
                placeholder="Short description of the season..."
                rows={2}
                value={editForm.overview}
                onChange={(e) => handleEditChange("overview", e.target.value)}
              />
            </div>
          </div>

          <div className="flex gap-2 pt-2">
            <button
              type="button"
              onClick={handleSave}
              className="flex-1 py-2 rounded-lg bg-primary text-white text-sm font-semibold hover:bg-primary/90 transition-colors"
            >
              {activeIndex === "new" ? "Add Season" : "Save Changes"}
            </button>
            {isEditingSeason && (
              <button
                type="button"
                onClick={handleDelete}
                className="py-2 px-4 rounded-lg border border-red-400 text-red-500 text-sm font-semibold hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
              >
                Delete
              </button>
            )}
            <button
              type="button"
              onClick={handleClose}
              className="py-2 px-4 rounded-lg border border-slate-300 dark:border-slate-600 text-slate-600 dark:text-slate-300 text-sm font-semibold hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
            >
              Cancel
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
