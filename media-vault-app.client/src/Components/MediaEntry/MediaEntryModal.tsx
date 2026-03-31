import { useState } from "react";
import type {
  MediaEntrySubmitDto,
  MediaEntryDetailedDto,
} from "../../Clients/MediaEntriesClient";
import DetailedHeader from "./DetailedHeader";
import MediaEntryForm from "./MediaEntryForm";
import type { MediaEntryFormData } from "./MediaEntryForm";
import FormFooter from "./FormFooter";
import ModalWindow from "../Shared/ModalWindow";

const SUCCESS_STATE_DELAY_MS = 1000;

type MediaEntryProps = {
  detailedEntry?: MediaEntryDetailedDto;
  onSubmit: (
    updatedEntry: MediaEntrySubmitDto,
    entryId?: string,
  ) => Promise<void> | void;
  onDelete: (id: string) => Promise<void> | void;
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

export default function MediaEntryModal({
  detailedEntry,
  onSubmit,
  onDelete,
  onCancel,
}: MediaEntryProps) {
  const [formData, setFormData] = useState<MediaEntryFormData>(
    buildInitialFormData(detailedEntry),
  );
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showSuccessState, setShowSuccessState] = useState(false);
  const [deleteSuccessState, setDeleteSuccessState] = useState(false);

  const isEditMode = detailedEntry != null && detailedEntry.id != null;
  const isBusy = isSubmitting || showSuccessState || deleteSuccessState;

  const handleChange = (
    field: keyof MediaEntryFormData,
    value: string | number,
  ) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e: React.SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();
    setIsSubmitting(true);
    setShowSuccessState(false);

    const dto: MediaEntrySubmitDto = {
      title: formData.title,
      mediaType: formData.mediaType,
      status: formData.status,
      rating: formData.rating,
      imageUrl: formData.imageUrl.trim() || null,
      review: formData.review || null,
    };

    try {
      await onSubmit(dto, detailedEntry?.id);
      setIsSubmitting(false);
      setShowSuccessState(true);
      await new Promise((resolve) => {
        window.setTimeout(resolve, SUCCESS_STATE_DELAY_MS);
      });
      onCancel();
    } catch {
      setIsSubmitting(false);
      setShowSuccessState(false);
    }
  };

  const handleDelete = async () => {
    if (detailedEntry) {
      setIsSubmitting(true);
      setShowSuccessState(false);
      setDeleteSuccessState(false);

      try {
        await onDelete(detailedEntry.id);
        setIsSubmitting(false);
        setShowSuccessState(true);
        setDeleteSuccessState(true);
        await new Promise((resolve) => {
          window.setTimeout(resolve, SUCCESS_STATE_DELAY_MS);
        });
        onCancel();
      } catch {
        setDeleteSuccessState(false);
        setIsSubmitting(false);
        setShowSuccessState(false);
      }
    }
  };

  return (
    <ModalWindow
      onClose={isBusy ? () => undefined : onCancel}
      overlayClassName={
        showSuccessState
          ? "fixed inset-0 z-50 flex items-center justify-center bg-black/85 backdrop-blur-md p-4"
          : undefined
      }
      cardClassName={
        showSuccessState
          ? "relative w-full max-w-md rounded-2xl border border-slate-800 bg-background-dark px-10 py-12 text-center shadow-2xl"
          : undefined
      }
    >
      {showSuccessState ? (
        <>
          <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-primary/15 text-primary">
            <span className="material-symbols-outlined text-5xl">check</span>
          </div>
          <h2 className="mb-3 text-3xl font-black tracking-tight text-white">
            {deleteSuccessState
              ? "Entry Deleted"
              : isEditMode
                ? "Entry Updated"
                : "Entry Created"}
          </h2>
          <p className="mb-8 text-base leading-relaxed text-slate-300">
            {deleteSuccessState
              ? "The entry was deleted successfully. Returning you to the dashboard now."
              : isEditMode
                ? "The entry was updated successfully. Returning you to the dashboard now."
                : "The entry was created successfully. Returning you to the dashboard now."}
          </p>
          <div className="flex flex-col gap-3">
            <div className="h-1.5 w-full overflow-hidden rounded-full bg-slate-800">
              <div className="success-progress-bar h-full rounded-full bg-primary" />
            </div>
            <p className="text-[10px] font-bold uppercase tracking-[0.2em] text-slate-500">
              Redirecting to Dashboard
            </p>
          </div>
        </>
      ) : (
        <>
          <DetailedHeader
            isEditMode={isEditMode}
            subtitle={formatSubtitle(detailedEntry)}
            onCancel={onCancel}
          />

          <form className="space-y-6 p-6" onSubmit={handleSubmit}>
            <MediaEntryForm
              formData={formData}
              onChange={handleChange}
              isEditMode={isEditMode}
            />

            <FormFooter
              isEditMode={isEditMode}
              onDelete={isEditMode ? handleDelete : undefined}
              onCancel={onCancel}
            />
          </form>
        </>
      )}
    </ModalWindow>
  );
}
