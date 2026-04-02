import { MediaType } from "../../Clients/MediaEntriesClient";
import { useUser } from "../../Shared/UserContext";
import { mediaSections } from "../../Shared/mediaConstants";

type Props = {
  onChangeMediaTypeFilter: (mediaType: number | undefined) => void;
  currentMainMediaTypeFilter: number;
};

export default function Sidebar({
  onChangeMediaTypeFilter,
  currentMainMediaTypeFilter = MediaType.All,
}: Props) {
  const { currentUser } = useUser();

  const mapMediaSectionsToIcons = (
    mediaSection: (typeof mediaSections)[number],
  ): string => {
    switch (mediaSection.type) {
      case MediaType.Game:
        return "sports_esports";
      case MediaType.Book:
        return "menu_book";
      case MediaType.Movie:
        return "movie";
      case MediaType.Series:
        return "tv";
      case MediaType.Manga:
        return "auto_stories";
      case MediaType.All:
        return "globe";
      default:
        return "help_outline";
    }
  };

  return (
    <>
      {/* <!-- Sidebar --> */}
      <aside className="w-64 border-r border-slate-200 dark:border-slate-800 flex flex-col p-4 shrink-0 bg-background-light dark:bg-background-dark lg:flex">
        <div className="flex items-center gap-3 px-2 mb-8 text-primary">
          <span className="material-symbols-outlined text-3xl">database</span>
          <h2 className="text-xl font-bold tracking-tight">MediaVault</h2>
        </div>
        <div className="flex flex-col gap-6">
          <div className="flex flex-col gap-1">
            <p className="px-3 text-xs font-semibold text-slate-500 uppercase tracking-wider mb-2">
              Main Menu
            </p>
            <a
              className="flex items-center gap-3 px-3 py-2 rounded-lg bg-primary/10 text-primary font-medium"
              href="#"
            >
              <span className="material-symbols-outlined">dashboard</span>
              <span>Dashboard</span>
            </a>

            <a
              className="flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
              href="/"
            >
              <span className="material-symbols-outlined">home</span>
              <span>Home</span>
            </a>
          </div>
          <div className="flex flex-col gap-1">
            <p className="px-3 text-xs font-semibold text-slate-500 uppercase tracking-wider mb-2">
              My Lists
            </p>

            {mediaSections.map(({ type, title }) => {
              const isCurrent = currentMainMediaTypeFilter === type;

              return (
                <button
                  key={type}
                  onClick={() => onChangeMediaTypeFilter(type)}
                  className={`flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors text-left ${
                    isCurrent ? "bg-primary/10 text-primary" : ""
                  }`}
                >
                  <span className="material-symbols-outlined">
                    {mapMediaSectionsToIcons({ type, title })}
                  </span>
                  <span>{title}</span>
                </button>
              );
            })}
          </div>
        </div>
        <div className="mt-auto pt-4 border-t border-slate-200 dark:border-slate-800">
          <div className="flex items-center gap-3 px-3 py-2">
            <div
              className="h-10 w-10 rounded-full bg-slate-200 dark:bg-slate-700 overflow-hidden"
              data-alt="Professional user profile avatar photo"
              style={{
                backgroundImage:
                  "url('https://lh3.googleusercontent.com/aida-public/AB6AXuDDHyTylWFr0zaja945Aj5lJQ33ZTw-iWfxjC_xx_GP0D-NGD4NiLyvAUrvEErFTkAN4SjExdGy6VgbJ6cpUBsEzounLI6FCIMuGhT-vBxh0eJ2LQ6_eQwWXm47VKSTfX9GjLAqdVvVT4AJ-G4ZwV-4QwDl0WMPwnBlCXz8k3wjNnJhwSs8sqQWWqwciYBU7L3-cbjkU2a20XHc5JDAsR2SMk6lX7vUVRdhz24K87JwfRTUrS5rP_4T2Tc7LJWAoqSZYGTeG-g5u_w')",
                backgroundSize: "cover",
              }}
            ></div>
            <div className="flex flex-col">
              <span className="text-sm font-semibold">
                {currentUser?.username}
              </span>
              <span className="text-xs text-slate-500">Pro Curator</span>
            </div>
          </div>
        </div>
      </aside>
    </>
  );
}
