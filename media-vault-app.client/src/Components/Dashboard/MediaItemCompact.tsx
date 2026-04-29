// Compact list-row card used in the Backlog section.
// Shows a small thumbnail, title, and key metadata in a single row.
import type { MediaEntryDetailedDto } from "../../Clients/MediaEntriesClient";
import MediaTypeBadge from "../Shared/MediaTypeBadge";

type MediaItemCompactProps = {
	entry: MediaEntryDetailedDto;
	onClickEntry: (entry: MediaEntryDetailedDto) => void;
};

// Builds the small subtitle line shown under the title.
// Uses the first genre (if any) and the release year.
function getCompactMeta(entry: MediaEntryDetailedDto) {
	const details = [entry.genres[0] ?? null, (entry.releaseYear ?? 0) > 0 ? String(entry.releaseYear) : null]
		.filter(Boolean)
		.join(" • ");

	return details || "No details yet";
}

export default function MediaItemCompact({
	entry,
	onClickEntry,
}: MediaItemCompactProps) {
	return (
		<div
			className="flex items-center gap-4 rounded-xl border border-slate-200 bg-slate-100 p-3 transition-colors hover:border-primary/50 dark:border-slate-800 dark:bg-slate-800/50"
			onClick={() => onClickEntry(entry)}
			onKeyDown={(event) => {
				if (event.key === "Enter" || event.key === " ") {
					event.preventDefault();
					onClickEntry(entry);
				}
			}}
			role="button"
			tabIndex={0}
		>
			<div
				className="h-20 w-14 shrink-0 rounded-lg bg-slate-200 bg-cover bg-center dark:bg-slate-700"
				style={{ backgroundImage: entry.imageUrl ? `url('${entry.imageUrl}')` : undefined }}
			/>

			<div className="min-w-0 flex-1">
				<h3 className="truncate text-sm font-semibold transition-colors hover:text-primary">
					{entry.title}
				</h3>
				<p className="mb-2 text-xs text-slate-500 dark:text-slate-400">
					{getCompactMeta(entry)}
				</p>
				<div className="flex items-center gap-1.5">
					<MediaTypeBadge mediaType={entry.mediaType} />
					<span className="inline-flex items-center rounded bg-slate-200 px-1.5 py-0.5 text-[10px] font-bold text-slate-700 dark:bg-slate-700 dark:text-slate-200">
						{entry.rating.toFixed(1)}
					</span>
				</div>
			</div>

			<button
				type="button"
				className="rounded-full p-2 transition-colors hover:bg-primary/20 hover:text-primary"
				onClick={(event) => {
					event.stopPropagation();
					onClickEntry(entry);
				}}
				aria-label={`Open ${entry.title ?? "entry"}`}
			>
				<span className="material-symbols-outlined text-lg">play_arrow</span>
			</button>
		</div>
	);
}
