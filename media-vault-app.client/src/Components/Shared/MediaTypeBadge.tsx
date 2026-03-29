import {
  MediaType,
  MediaTypeLabels,
} from "../../Clients/MediaEntriesClient";

type MediaTypeBadgeProps = {
  mediaType: number;
  className?: string;
};

const mediaTypeBadgeStyles: Record<number, string> = {
  [MediaType.Movie]: "bg-orange-500/10 text-orange-600 dark:text-orange-400",
  [MediaType.Series]: "bg-sky-500/10 text-sky-600 dark:text-sky-400",
  [MediaType.Book]: "bg-green-500/10 text-green-600 dark:text-green-400",
  [MediaType.Manga]: "bg-pink-500/10 text-pink-600 dark:text-pink-400",
  [MediaType.Game]: "bg-primary/10 text-primary",
};

export default function MediaTypeBadge({
  mediaType,
  className = "",
}: MediaTypeBadgeProps) {
  const label = MediaTypeLabels[mediaType] ?? String(mediaType);
  const tone =
    mediaTypeBadgeStyles[mediaType] ??
    "bg-slate-200 text-slate-700 dark:bg-slate-700 dark:text-slate-200";

  return (
    <span
      className={`inline-flex items-center rounded px-1.5 py-0.5 text-[10px] font-bold uppercase tracking-wide ${tone} ${className}`.trim()}
    >
      {label}
    </span>
  );
}