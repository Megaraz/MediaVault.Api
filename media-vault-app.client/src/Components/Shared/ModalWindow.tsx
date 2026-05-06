type PopupWindowProps = {
  children: React.ReactNode;
  onClose: () => void;
  cardClassName?: string;
  overlayClassName?: string;
};

const defaultOverlayClassName: string =
  "fixed inset-0 z-50 flex items-center justify-center bg-background-dark/80 backdrop-blur-md p-4";
// "w-screen h-lvh fixed top-0 left-0 flex justify-center items-center bg-black/80 backdrop-blur-xs z-40";

const defaultCardClassName: string =
  "relative w-full max-w-4xl bg-background-light dark:bg-background-dark rounded-xl shadow-2xl border border-slate-200 dark:border-slate-800 flex flex-col max-h-[90vh] overflow-hidden";

export default function ModalWindow({
  children,
  onClose,
  cardClassName = defaultCardClassName,
  overlayClassName = defaultOverlayClassName,
}: PopupWindowProps) {
  return (
    <div className={overlayClassName} onClick={() => onClose()}>
      <div className={cardClassName} onClick={(e) => e.stopPropagation()}>
        {children}
      </div>
    </div>
  );
}
