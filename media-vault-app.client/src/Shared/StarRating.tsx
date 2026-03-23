type StarRatingProps = {
  rating: number;
  maxStars?: number;
  onChange: (newRating: number) => void;
};

export default function StarRating({
  rating,
  maxStars = 5,
  onChange,
}: StarRatingProps) {
  return (
    <div className="flex items-center gap-1 text-amber-400">
      {Array.from({ length: maxStars }, (_, i) => {
        const starValue = i + 1;
        const isFilled = starValue <= rating;
        return (
          <button
            key={starValue}
            className="hover:scale-110 transition-transform"
            type="button"
            onClick={() => onChange(starValue)}
          >
            <span
              className={`material-symbols-outlined text-3xl ${isFilled ? "star-filled" : ""}`}
            >
              star
            </span>
          </button>
        );
      })}
      <span className="ml-3 text-sm font-medium text-slate-500 dark:text-slate-400">
        {rating}.0 / {maxStars}.0
      </span>
    </div>
  );
}
