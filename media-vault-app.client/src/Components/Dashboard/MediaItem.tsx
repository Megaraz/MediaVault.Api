import {
  MediaTypeLabels,
  type MediaEntryDetailedDto,
} from "../../Clients/MediaEntriesClient";

type MediaEntrySmallProps = {
  entry: MediaEntryDetailedDto;
  onClickEntry: (entry: MediaEntryDetailedDto) => void;
};

export default function MediaItem({
  entry,
  onClickEntry,
}: MediaEntrySmallProps) {
  return (
    <>
      <div
        className="group cursor-pointer"
        key={entry.id}
        onClick={() => onClickEntry(entry)}
      >
        <div className="relative aspect-2/3 rounded-xl overflow-hidden mb-3 shadow-md group-hover:shadow-primary/20 group-hover:shadow-xl transition-all">
          <div
            className="absolute inset-0 bg-cover bg-center transition-transform group-hover:scale-105"
            data-alt="Dark futuristic sci-fi movie poster art"
            style={{
              backgroundImage:
                "url('https://lh3.googleusercontent.com/aida-public/AB6AXuC__5pGl12Q6yug749GjA1RvNEagcHckw9H5jT3tyz3fA7zdtMBmZp1u7IJR7HYb3rSBdbaoLXjIAytVc851yiWNi2Y8jl0JqrByBpBPXIk_jlnUe0NcSWcewaIhe1TMTyWJyTNcPlcAsIPHpCX9zFww7fumM_ZF17xjU6CXzVj1lOiQWNl77bKAUuKpBjtTBJuO6RpgfpKLLz-ammt17SeDkGrWbuSSMMGwja0gOx642PZYGQF7SNSeg4pta0QknKRJn-pRF6TXrY');",
            }}
          ></div>
          <div className="absolute top-2 right-2 bg-black/60 backdrop-blur-md px-2 py-1 rounded-md text-[10px] font-bold text-white uppercase">
            EP 08 / 12
          </div>
          <div className="absolute bottom-0 left-0 right-0 h-1/3 bg-linear-to-t from-black/80 to-transparent"></div>
        </div>
        <h3 className="font-semibold text-sm truncate group-hover:text-primary transition-colors">
          {entry.title}
        </h3>
        <div className="flex items-center justify-between mt-1">
          <span className="text-xs text-slate-500">
            {MediaTypeLabels[entry.mediaType] ?? entry.mediaType}
          </span>
          <div className="flex items-center text-yellow-500">
            <span className="material-symbols-outlined text-xs fill-1">
              star
            </span>
            <span className="text-xs font-medium ml-1">4.9</span>
          </div>
        </div>
      </div>
    </>
  );
}
