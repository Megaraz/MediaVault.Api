type PopupWindowProps = {
  children: React.ReactNode;
  onClose: () => void;
  mainPopupClassName?: string;
  overlayClassName?: string;
};

const defaultOverlayClassName: string =
  "w-screen h-lvh fixed top-0 left-0 flex justify-center items-center bg-black/80 backdrop-blur-xs z-40";

const defaultMainPopupClassName: string =
  "relative w-full max-w-2xl bg-background-light dark:bg-background-dark rounded-xl shadow-2xl border border-slate-200 dark:border-slate-800 overflow-hidden";

export default function ModularPopupWindow({
  children,
  onClose,
  mainPopupClassName = defaultMainPopupClassName,
  overlayClassName = defaultOverlayClassName,
}: PopupWindowProps) {
  return (
    <div className={overlayClassName} onClick={() => onClose()}>
      <div className={mainPopupClassName} onClick={(e) => e.stopPropagation()}>
        {children}
      </div>
    </div>
  );
}
