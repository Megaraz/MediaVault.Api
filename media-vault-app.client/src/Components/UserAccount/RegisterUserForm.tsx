export class RegisterUserFormData {
  username: string = "";
  email: string = "";
  confirmEmail: string = "";
  password: string = "";
  confirmPassword: string = "";
}

type RegisterUserFormProps = {
  formData: RegisterUserFormData;
  onChange: (field: keyof RegisterUserFormData, value: string) => void;
};

export default function RegisterUserForm({
  formData,
  onChange,
}: RegisterUserFormProps) {
  return (
    <>
      {/* Username Row */}
      <div>
        <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
          Username
        </label>
        <div className="relative">
          <span className="material-symbols-outlined pointer-events-none absolute inset-y-0 left-3 flex items-center text-slate-400 text-xl">
            person
          </span>
          <input
            value={formData.username}
            onChange={(event) => onChange("username", event.target.value)}
            placeholder="JohnDoe123"
            required
            className="w-full h-14 pl-12 pr-4 rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-[#101922] focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all placeholder:text-slate-500"
          />
        </div>
      </div>
      {/* Email Row */}
      <div>
        <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
          Email Address
        </label>
        <div className="relative">
          <span className="material-symbols-outlined pointer-events-none absolute inset-y-0 left-3 flex items-center text-slate-400 text-xl">
            mail
          </span>
          <input
            type="email"
            value={formData.email}
            onChange={(event) => onChange("email", event.target.value)}
            placeholder="johndoe@example.com"
            required
            className="w-full h-14 pl-12 pr-4 rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-[#101922] focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all placeholder:text-slate-500"
          />
        </div>
      </div>
      {/* Confirm Email Row */}
      <div>
        <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
          Confirm Email Address
        </label>
        <div className="relative">
          <span className="material-symbols-outlined pointer-events-none absolute inset-y-0 left-3 flex items-center text-slate-400 text-xl">
            mail
          </span>
          <input
            type="email"
            value={formData.confirmEmail}
            onChange={(event) => onChange("confirmEmail", event.target.value)}
            placeholder="johndoe@example.com"
            required
            className="w-full h-14 pl-12 pr-4 rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-[#101922] focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all placeholder:text-slate-500"
          />
        </div>
      </div>
      {/* Password Row */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
            Password
          </label>
          <div className="relative">
            <span className="material-symbols-outlined pointer-events-none absolute inset-y-0 left-3 flex items-center text-slate-400 text-xl">
              lock
            </span>
            <input
              type="password"
              value={formData.password}
              onChange={(event) => onChange("password", event.target.value)}
              placeholder="*********"
              required
              className="w-full h-14 pl-12 pr-4 rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-[#101922] focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all placeholder:text-slate-500"
            />
          </div>
        </div>
        <div>
          <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
            Confirm Password
          </label>
          <div className="relative">
            <span className="material-symbols-outlined pointer-events-none absolute inset-y-0 left-3 flex items-center text-slate-400 text-xl">
              shield
            </span>

            <input
              type="password"
              value={formData.confirmPassword}
              onChange={(event) =>
                onChange("confirmPassword", event.target.value)
              }
              placeholder="*********"
              required
              className="w-full h-14 pl-12 pr-4 rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-[#101922] focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all placeholder:text-slate-500"
            />
          </div>
        </div>
      </div>
    </>
  );
}
