import { useState } from "react";
import type {
  MediaEntrySubmitDto,
  MediaEntryDetailedDto,
} from "../../Clients/MediaEntriesClient";
import DetailedHeader from "./DetailedHeader";
import MediaEntryForm from "./MediaEntryForm";
import type { MediaEntryFormData } from "./MediaEntryForm";
import FormFooter from "./FormFooter";
import ModularPopupWindow from "../../Components/Shared/ModularPopupWindow";

type MediaEntryProps = {
  detailedEntry?: MediaEntryDetailedDto;
  onSubmit: (updatedEntry: MediaEntrySubmitDto, entryId?: string) => void;
  onDelete: (id: string) => void;
  onCancel: () => void;
};

function buildInitialFormData(
  entry?: MediaEntryDetailedDto,
): MediaEntryFormData {
  return {
    title: entry?.title ?? "",
    imageUrl: entry?.imageUrl ?? "",
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
  detailedEntry,
  onSubmit,
  onDelete,
  onCancel,
}: MediaEntryProps) {
  const [formData, setFormData] = useState<MediaEntryFormData>(
    buildInitialFormData(detailedEntry),
  );

  const isEditMode = detailedEntry != null && detailedEntry.id != null;

  const handleChange = (
    field: keyof MediaEntryFormData,
    value: string | number,
  ) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = (e: React.SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();

    const dto: MediaEntrySubmitDto = {
      title: formData.title,
      mediaType: formData.mediaType,
      status: formData.status,
      rating: formData.rating,
      imageUrl: formData.imageUrl.trim() || null,
      review: formData.review || null,
    };

    onSubmit(dto, detailedEntry?.id);
  };

  const handleDelete = () => {
    if (detailedEntry) {
      onDelete(detailedEntry.id);
    }
  };

  return (
    <ModularPopupWindow onClose={onCancel}>
      <DetailedHeader
        isEditMode={isEditMode}
        subtitle={formatSubtitle(detailedEntry)}
        onCancel={onCancel}
      />

      <form className="p-6 space-y-6" onSubmit={handleSubmit}>
        <MediaEntryForm formData={formData} onChange={handleChange} />

        <FormFooter
          isEditMode={isEditMode}
          onDelete={isEditMode ? handleDelete : undefined}
          onCancel={onCancel}
        />
      </form>
    </ModularPopupWindow>
  );
}
