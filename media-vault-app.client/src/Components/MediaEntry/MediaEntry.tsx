import { useState } from "react";
import type {
  MediaEntryCreateDto,
  MediaEntryDetailedDto,
  MediaEntryUpdateDto,
} from "../../Clients/MediaEntriesClient";
import DetailedHeader from "./DetailedHeader";
import MediaEntryForm from "./MediaEntryForm";
import type { MediaEntryFormData } from "./MediaEntryForm";
import FormFooter from "./FormFooter";

type MediaEntryProps = {
  createMode?: boolean;
  detailedEntry?: MediaEntryDetailedDto;
  onCreate: (newEntry: MediaEntryCreateDto) => void;
  onSubmit: (updatedEntry: MediaEntryUpdateDto) => void;
  onDelete: (id: string) => void;
  onCancel: () => void;
};

function buildInitialFormData(
  entry?: MediaEntryDetailedDto
): MediaEntryFormData {
  return {
    title: entry?.title ?? "",
    mediaType: entry?.mediaType ?? 0,
    status: entry?.status ?? 0,
    rating: entry?.rating ?? 0,
    review: entry?.review ?? "",
  };
}

function formatSubtitle(entry?: MediaEntryDetailedDto): string | undefined {
  if (!entry) return undefined;
  const date = new Date(entry.createdAtUtc);
  return `Created ${date.toLocaleDateString()}`;
}

export default function MediaEntry({
  createMode,
  detailedEntry,
  onSubmit,
  onDelete,
  onCreate,
  onCancel,
}: MediaEntryProps) {
  const [formData, setFormData] = useState<MediaEntryFormData>(
    buildInitialFormData(detailedEntry)
  );

  const handleChange = (
    field: keyof MediaEntryFormData,
    value: string | number
  ) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    const dto = {
      title: formData.title,
      mediaType: formData.mediaType,
      status: formData.status,
      rating: formData.rating || null,
      review: formData.review || null,
    };

    if (createMode) {
      onCreate(dto as MediaEntryCreateDto);
    } else {
      onSubmit(dto as MediaEntryUpdateDto);
    }
  };

  const handleDelete = () => {
    if (detailedEntry) {
      onDelete(detailedEntry.id);
    }
  };

  return (
    <div className="relative w-full max-w-2xl bg-background-light dark:bg-background-dark rounded-xl shadow-2xl border border-slate-200 dark:border-slate-800 overflow-hidden">
      <DetailedHeader
        createMode={createMode}
        subtitle={formatSubtitle(detailedEntry)}
        onCancel={onCancel}
      />

      <form className="p-6 space-y-6" onSubmit={handleSubmit}>
        <MediaEntryForm formData={formData} onChange={handleChange} />

        <FormFooter
          createMode={createMode}
          onDelete={createMode ? undefined : handleDelete}
          onCancel={onCancel}
        />
      </form>
    </div>
  );
}
