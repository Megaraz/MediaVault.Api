type SelectOptionProps = {
  options: SelectOptionItem[];
  value: string | number;
  onChange: (newValue: string) => void;
  className?: string;
};

export type SelectOptionItem = {
  value: string | number;
  label: string;
};

const defaultClassName: string =
  "w-full px-4 py-3 rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all appearance-none";

export default function SelectOption({
  options,
  value,
  onChange,
  className = defaultClassName,
}: SelectOptionProps) {
  return (
    <select
      className={className}
      value={value}
      onChange={(e) => onChange(e.target.value)}
    >
      {options.map((option) => (
        <option key={option.value} value={option.value}>
          {option.label}

        </option>
      ))}
    </select>
  );
}
