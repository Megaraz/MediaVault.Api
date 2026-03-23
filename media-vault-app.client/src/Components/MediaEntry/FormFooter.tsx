type FormFooterProps = {
  createMode?: boolean;
  onDelete?: () => void;
  onCancel: () => void;
};

export default function FormFooter({
  createMode,
  onDelete,
  onCancel,
}: FormFooterProps) {
  return (
    <div className="flex flex-col sm:flex-row items-center justify-between gap-4 pt-6 border-t border-slate-200 dark:border-slate-800">
      {/* Only show delete in edit mode */}
      {!createMode && onDelete ? (
        <button
          className="w-full sm:w-auto flex items-center justify-center gap-2 px-6 py-3 rounded-lg text-red-500 hover:bg-red-50 dark:hover:bg-red-950/30 font-semibold transition-colors"
          type="button"
          onClick={onDelete}
        >
          <span className="material-symbols-outlined">delete</span>
          Remove Entry
        </button>
      ) : (
        <div />
      )}

      <div className="flex w-full sm:w-auto gap-3">
        <button
          className="flex-1 sm:flex-none px-6 py-3 rounded-lg border border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300 font-semibold hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
          type="button"
          onClick={onCancel}
        >
          Cancel
        </button>
        <button
          className="flex-1 sm:flex-none px-8 py-3 rounded-lg bg-primary hover:bg-primary/90 text-white font-semibold shadow-lg shadow-primary/20 transition-all"
          type="submit"
        >
          {createMode ? "Create Entry" : "Save Changes"}
        </button>
      </div>
    </div>
  );
}
