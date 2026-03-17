export default function HomePage() {
  return (
    <>
      <div className="mx-auto max-w-7xl px-6 md:px-20 py-12 md:py-24">
        <div className="flex flex-col gap-12 lg:flex-row items-center">
          <div className="flex flex-col gap-8 lg:w-1/2 text-left">
            <div className="flex flex-col gap-4">
              <span className="text-primary font-bold tracking-widest text-xs uppercase">
                Your Digital Archive
              </span>
              <h1 className="text-slate-900 dark:text-white text-5xl md:text-6xl font-black leading-tight tracking-tight">
                Track Every Story. <br />
                <span className="text-primary">Log Every Moment.</span>
              </h1>
              <p className="text-slate-600 dark:text-slate-400 text-lg md:text-xl font-normal leading-relaxed max-w-135">
                The ultimate companion for your media journey. Log movies,
                books, games, and more in one sleek, unified interface designed
                for creators and consumers.
              </p>
            </div>
            <div className="flex flex-wrap gap-4">
              <button className="flex min-w-40 cursor-pointer items-center justify-center rounded-xl h-14 px-8 bg-primary text-white text-lg font-bold transition-all hover:scale-[1.02]">
                Get Started Free
              </button>
              <button className="flex min-w-40 cursor-pointer items-center justify-center rounded-xl h-14 px-8 border-2 border-slate-200 dark:border-slate-800 text-slate-900 dark:text-white text-lg font-bold transition-all hover:bg-slate-100 dark:hover:bg-slate-800">
                Watch Demo
              </button>
            </div>
            <div className="flex items-center gap-4 pt-4">
              <div className="flex -space-x-3">
                <div
                  className="size-10 rounded-full border-2 border-background-dark bg-slate-300"
                  data-alt="User avatar 1"
                  style={{
                    backgroundImage:
                      "url('https://lh3.googleusercontent.com/aida-public/AB6AXuBniM9g60nKg_4a3LpwKfijoce5PJCBXUgdv85igR2r_ZuQo-WwrMACuUOtrb5nrQEh_CfIZOpvlTPbqeHNiNsnaMZkS1uYV2mQ7Ey_M5xQtHWGSeXmzhLzPVmImIxnMJZICk-nSo1XYFCl6pGau18vHGUK5TRk18gC1LKSL8UX0SMqlV-7PP5CsyBCYXgsdyHv9X2vCXvrJUH-HWWcxdI0ZJSMVCpyWEaojG_Vyko95DThzqBjgHSctJ5iG-C3ILSPLdJWbHiGAyQ')",
                  }}
                ></div>
                <div
                  className="size-10 rounded-full border-2 border-background-dark bg-slate-400"
                  data-alt="User avatar 2"
                  style={{
                    backgroundImage:
                      "url('https://lh3.googleusercontent.com/aida-public/AB6AXuCWRhJsTmYkGyUogh2c3UcOI0AjzTfPrh-P9mBaRgDrxnN_watcQetQhKMBaQcjoMqoPN1XltF3qMq3YbwMOk1PabXs2LWd6HPus_sHBhk8vqA5Utx1IbxC9YqPoRkkDhNRTJo9IviigXrdOA9mLrxDwR6sS3RUX8qUQYjyZQEHYDuMll9kvUWhJ5pKzTMy9I50Pg7jvtciY1nXoIzPRPC4ybQGK-aetioMGmzoiYXUYGpsF6PZ06C1q9QbK0mLdLbQ7QfbSy1dMXE')",
                  }}
                ></div>
                <div
                  className="size-10 rounded-full border-2 border-background-dark bg-slate-500"
                  data-alt="User avatar 3"
                  style={{
                    backgroundImage:
                      "url('https://lh3.googleusercontent.com/aida-public/AB6AXuBEGWilV5k7e1bFVRosvri8Kwswl7qO2s4PS2x8_-K4_2itK6flKEyMbHul2t6-BA3aXRzCjXxjNXX5-s0m6gQwMwmJmW3cpS75yPKVr_MoZW51HlZUTuBE5XwRf-l785ZRAeV_UA94RX5OM12UhwT6f6PMQwzTN0lKgpBojun92QG6sV4b5Q3fr3e3S9Ywjt3zBJVIiW1Qosv3JXn1y91nbG4EZkS8OJKL05rF7BVss2LwTK9fx7vWzqAC2ejAYzwDm6Ma5AxqTA0')",
                  }}
                ></div>
              </div>
              <p className="text-sm text-slate-500 dark:text-slate-400 font-medium">
                Joined by 12,000+ enthusiasts this month
              </p>
            </div>
          </div>
          <div className="lg:w-1/2 w-full">
            <div className="relative group">
              <div className="absolute -inset-1 bg-linear-to-r from-primary to-blue-600 rounded-2xl blur opacity-25 group-hover:opacity-40 transition duration-1000"></div>
              <div className="relative bg-slate-200 dark:bg-slate-800 rounded-2xl overflow-hidden border border-slate-300 dark:border-slate-700 aspect-16/10 shadow-2xl">
                <div className="bg-slate-100 dark:bg-slate-900/50 p-4 border-b border-slate-300 dark:border-slate-700 flex items-center gap-2">
                  <div className="flex gap-1.5">
                    <div className="size-3 rounded-full bg-red-400"></div>
                    <div className="size-3 rounded-full bg-amber-400"></div>
                    <div className="size-3 rounded-full bg-emerald-400"></div>
                  </div>
                </div>
                <img
                  alt="App Dashboard Preview"
                  className="w-full h-full object-cover"
                  data-alt="Modern media tracking dashboard user interface preview"
                  src="https://lh3.googleusercontent.com/aida-public/AB6AXuDn6oRhYsE8rz4rb5ecBrepeQqNrYOkZ4Wpqr6CPV3sXZsLm5Q_7oDBE94xyEO6zPCL2K5ZE-WnRhAShgiL2Os8Odo90wGoVvKlixh83St5a3aYDLS2VXLyJN5upsdWE-5CJi16nxuRa2MdPatkK6xvuwxlfC09rP5BDZ5D9J5fq0YmWhRTqg1Sy7UG-YRjSnlc88TEiyQ_mCdDIKN5XEkDJPbwTORuU6_9YpVjQvSusWBbEgy1zuhDAIH83N8xlJtT_5OEXRsuIQk"
                />
              </div>
            </div>
          </div>
        </div>
      </div>

      <section
        className="bg-slate-100 dark:bg-slate-900/40 py-24"
        id="features"
      >
        <div className="mx-auto max-w-7xl px-6 md:px-20">
          <div className="flex flex-col gap-4 text-center mb-16">
            <h2 className="text-slate-900 dark:text-white text-4xl font-black leading-tight tracking-tight">
              Everything you consume, in one place
            </h2>
            <p className="text-slate-600 dark:text-slate-400 text-lg font-normal max-w-180 mx-auto">
              MediaVault helps you stay on top of your entertainment across all
              mediums with powerful tracking tools and automated insights.
            </p>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <div className="group flex flex-col gap-6 rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-background-dark p-8 transition-all hover:border-primary/50 hover:shadow-xl hover:shadow-primary/5">
              <div className="flex items-center justify-center size-14 rounded-xl bg-primary/10 text-primary group-hover:bg-primary group-hover:text-white transition-all">
                <span className="material-symbols-outlined text-3xl">
                  movie
                </span>
              </div>
              <div className="flex flex-col gap-2">
                <h3 className="text-slate-900 dark:text-white text-xl font-bold">
                  Movies &amp; TV
                </h3>
                <p className="text-slate-600 dark:text-slate-400 text-base leading-relaxed">
                  Automatic synchronization with streaming services. Track
                  seasons, episodes, and personal ratings.
                </p>
              </div>
            </div>
            <div className="group flex flex-col gap-6 rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-background-dark p-8 transition-all hover:border-primary/50 hover:shadow-xl hover:shadow-primary/5">
              <div className="flex items-center justify-center size-14 rounded-xl bg-primary/10 text-primary group-hover:bg-primary group-hover:text-white transition-all">
                <span className="material-symbols-outlined text-3xl">
                  menu_book
                </span>
              </div>
              <div className="flex flex-col gap-2">
                <h3 className="text-slate-900 dark:text-white text-xl font-bold">
                  Books &amp; Manga
                </h3>
                <p className="text-slate-600 dark:text-slate-400 text-base leading-relaxed">
                  Log daily reading progress. Track pages read, library
                  location, and detailed series collection management.
                </p>
              </div>
            </div>
            <div className="group flex flex-col gap-6 rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-background-dark p-8 transition-all hover:border-primary/50 hover:shadow-xl hover:shadow-primary/5">
              <div className="flex items-center justify-center size-14 rounded-xl bg-primary/10 text-primary group-hover:bg-primary group-hover:text-white transition-all">
                <span className="material-symbols-outlined text-3xl">
                  sports_esports
                </span>
              </div>
              <div className="flex flex-col gap-2">
                <h3 className="text-slate-900 dark:text-white text-xl font-bold">
                  Video Games
                </h3>
                <p className="text-slate-600 dark:text-slate-400 text-base leading-relaxed">
                  Manage your infinite backlog. Track play sessions,
                  achievements, and upcoming release dates.
                </p>
              </div>
            </div>
            <div className="group flex flex-col gap-6 rounded-2xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-background-dark p-8 transition-all hover:border-primary/50 hover:shadow-xl hover:shadow-primary/5">
              <div className="flex items-center justify-center size-14 rounded-xl bg-primary/10 text-primary group-hover:bg-primary group-hover:text-white transition-all">
                <span className="material-symbols-outlined text-3xl">
                  podcasts
                </span>
              </div>
              <div className="flex flex-col gap-2">
                <h3 className="text-slate-900 dark:text-white text-xl font-bold">
                  Podcasts
                </h3>
                <p className="text-slate-600 dark:text-slate-400 text-base leading-relaxed">
                  Never miss an episode. Archive your favorite moments and share
                  clips with your social circle.
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>
      <section className="mx-auto max-w-7xl px-6 md:px-20 py-24">
        <div className="relative bg-primary rounded-3xl overflow-hidden px-8 py-16 md:py-24 text-center">
          <div className="absolute inset-0 opacity-20 pointer-events-none">
            <div className="absolute top-0 left-0 w-64 h-64 bg-white rounded-full -translate-x-1/2 -translate-y-1/2 blur-3xl"></div>
            <div className="absolute bottom-0 right-0 w-96 h-96 bg-blue-400 rounded-full translate-x-1/3 translate-y-1/3 blur-3xl"></div>
          </div>
          <div className="relative flex flex-col items-center gap-8 max-w-200 mx-auto">
            <h2 className="text-white text-4xl md:text-5xl font-black leading-tight tracking-tight">
              Ready to start your media log?
            </h2>
            <p className="text-blue-50 text-lg md:text-xl font-normal opacity-90 leading-relaxed">
              Join thousands of users who have organized their entertainment
              life. Simple to start, powerful to use, and completely free to
              explore.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 w-full justify-center">
              <button className="flex min-w-50 cursor-pointer items-center justify-center rounded-xl h-14 px-8 bg-white text-primary text-lg font-bold transition-all hover:bg-slate-50">
                Download App
              </button>
              <button className="flex min-w-50 cursor-pointer items-center justify-center rounded-xl h-14 px-8 border-2 border-white/30 bg-transparent text-white text-lg font-bold transition-all hover:bg-white/10">
                Try Web Version
              </button>
            </div>
          </div>
        </div>
      </section>
    </>
  );
}
