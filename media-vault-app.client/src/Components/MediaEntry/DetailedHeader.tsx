type DetailedHeaderProps = {
  isEditMode: boolean;
  subtitle?: string;
  onCancel: () => void;
  imgUrl?: string;
};

export default function DetailedHeader({
  isEditMode,
  subtitle,
  onCancel,
  imgUrl,
}: DetailedHeaderProps) {
  return (
    <header className="flex items-center justify-between border-b border-slate-200 dark:border-slate-800 px-6 py-5">
      <div className="flex items-center gap-4">
        <div
          className={`flex items-center justify-center rounded-lg bg-primary/10 text-primary ${imgUrl ? "w-16 h-20" : "size-10"}`}
        >
          {imgUrl ? (
            <img
              src={imgUrl}
              alt="Entry"
              className="w-full h-full object-cover rounded-lg"
            />
          ) : (
            <span className="material-symbols-outlined">edit_note</span>
          )}
        </div>
        <div>
          <h2 className="text-slate-900 dark:text-slate-100 text-xl font-bold leading-tight">
            {isEditMode ? "Edit" : "New"} Entry
          </h2>
          {subtitle && (
            <p className="text-slate-500 dark:text-slate-400 text-xs">
              {subtitle}
            </p>
          )}
        </div>
      </div>
      <button
        className="flex items-start justify-center size-10 rounded-full hover:bg-slate-100 dark:hover:bg-slate-800 text-slate-500 transition-colors"
        type="button"
        onClick={onCancel}
      >
        <span className="material-symbols-outlined">close</span>
      </button>
    </header>
  );
}
