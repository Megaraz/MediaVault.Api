// import ButtonPrimary from "./ButtonPrimary";

type InputText = {
  value?: string;
  onChange: (newValue: string) => void;
  //   onSubmit: (newValue: string) => void;
  placeholder?: string;
  //   typeOfAction: string;
  className?: string;
};

const defaultClassName: string =
  "w-full px-4 py-3 rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-100 focus:ring-2 focus:ring-primary focus:border-primary outline-none transition-all";

export default function TextInputComp({
  value = "",
  onChange,
  //   onSubmit,
  //   typeOfAction,
  className = defaultClassName,
  placeholder = "",
}: InputText) {
  return (
    <>
      <input
        type="text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className={className}
        placeholder={placeholder}
      />
      {/* <ButtonPrimary onClick={() => onSubmit(value)}>
        {typeOfAction}
      </ButtonPrimary> */}
    </>
  );
}
