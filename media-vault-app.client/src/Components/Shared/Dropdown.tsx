import { useEffect, useRef, useState } from "react";

export type DropdownItem = {
  value: string | number;
  label: string;
};

type DropdownProps = {
  options: DropdownItem[];
  value: string | number;
  onChange: (newValue: string | number) => void;
  prefix?: string;
};

export default function Dropdown({
  options,
  value,
  onChange,
  prefix,
}: DropdownProps) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  const selectedLabel =
    options.find((o) => o.value === value)?.label ?? options[0]?.label ?? "";

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  return (
    <div ref={ref} className="relative">
      <button
        type="button"
        onClick={() => setOpen((prev) => !prev)}
        className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-slate-100 dark:bg-slate-800 text-xs font-medium border border-slate-200 dark:border-slate-700 hover:bg-slate-200 dark:hover:bg-slate-700 transition-colors"
      >
        {prefix && <span>{prefix}</span>}
        {selectedLabel}
        <span
          className={`material-symbols-outlined text-xs transition-transform ${open ? "rotate-180" : ""}`}
        >
          expand_more
        </span>
      </button>

      {open && (
        <ul className="absolute right-0 z-30 mt-1 min-w-35 rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900 shadow-lg shadow-black/10 dark:shadow-black/30 py-1 text-xs animate-in fade-in slide-in-from-top-1">
          {options.map((option) => {
            const isActive = option.value === value;
            return (
              <li key={option.value}>
                <button
                  type="button"
                  onClick={() => {
                    onChange(option.value);
                    setOpen(false);
                  }}
                  className={`w-full text-left px-3 py-2 transition-colors ${
                    isActive
                      ? "bg-primary/10 text-primary font-semibold"
                      : "hover:bg-slate-100 dark:hover:bg-slate-800"
                  }`}
                >
                  {option.label}
                </button>
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
}
