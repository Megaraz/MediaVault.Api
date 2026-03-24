type ButtonPrimaryProps = {
  children: React.ReactNode;
  className?: string;
  onClick?: () => void;
};

function ButtonPrimary({
  children,
  className = "",
  onClick,
}: ButtonPrimaryProps) {
  return (
    <button
      onClick={onClick}
      className={`
        px-4 
        py-2 
      bg-blue-600 
      hover:bg-blue-700 
      text-white rounded-md + ${className}
    `}
    >
      {children}
    </button>
  );
}
export default ButtonPrimary;