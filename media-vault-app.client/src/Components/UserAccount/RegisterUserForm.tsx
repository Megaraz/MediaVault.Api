import InputText from "../Shared/InputText";

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

function isMatch(
  fieldName: string,
  value1: string,
  value2: string,
): boolean {
  if (value1 !== value2) {
    alert(`${fieldName} do not match`);
    return false;
  }
  return true;
}

export default function RegisterUserForm({
  formData,
  onChange,
}: RegisterUserFormProps) {
  return (
    <>
      {/* Username Row */}
      <div className="space-y-2">
        <label className="block text-sm font-medium text-slate-300">
          Username
        </label>
        <div className="relative">
          <span className="material-symbols-outlined pointer-events-none absolute inset-y-0 left-3 flex items-center text-slate-500 text-xl">
            person
          </span>
          <InputText
            value={formData.username}
            onChange={(val) => onChange("username", val)}
            placeholder="JohnDoe123"
            className="w-full bg-slate-800/50 border border-slate-700 rounded-lg py-3 pl-11 pr-4 text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary transition-all"
          />
        </div>
      </div>
      {/* Email Row */}
      <div className="space-y-2">
        <label className="block text-sm font-medium text-slate-300">
          Email Address
        </label>
        <div className="relative">
          <span className="material-symbols-outlined pointer-events-none absolute inset-y-0 left-3 flex items-center text-slate-500 text-xl">
            mail
          </span>
          <InputText
            type="email"
            value={formData.email}
            onChange={(val) => onChange("email", val)}
            placeholder="johndoe@example.com"
            className="w-full bg-slate-800/50 border border-slate-700 rounded-lg py-3 pl-11 pr-4 text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary transition-all"
          />
        </div>
      </div>
      {/* Confirm Email Row */}
      <div className="space-y-2">
        <label className="block text-sm font-medium text-slate-300">
          Confirm Email Address
        </label>
        <div className="relative">
          <span className="material-symbols-outlined pointer-events-none absolute inset-y-0 left-3 flex items-center text-slate-500 text-xl">
            mail
          </span>
          <InputText
            type="email"
            value={formData.confirmEmail}
            onChange={(val) => onChange("confirmEmail", val)}
            placeholder="johndoe@example.com"
            className="w-full bg-slate-800/50 border border-slate-700 rounded-lg py-3 pl-11 pr-4 text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary transition-all"
          />
        </div>
      </div>
      {/* Password Row */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div className="space-y-2">
          <label className="block text-sm font-medium text-slate-300">
            Password
          </label>
          <div className="relative">
            <span className="material-symbols-outlined pointer-events-none absolute inset-y-0 left-3 flex items-center text-slate-500 text-xl">
              lock
            </span>
            <InputText
              type="password"
              value={formData.password}
              onChange={(val) => onChange("password", val)}
              placeholder="*********"
              className="w-full bg-slate-800/50 border border-slate-700 rounded-lg py-3 pl-11 pr-4 text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary transition-all"
            />
          </div>
        </div>
        <div className="space-y-2">
          <label className="block text-sm font-medium text-slate-300">
            Confirm Password
          </label>
          <div className="relative">
            <span className="material-symbols-outlined pointer-events-none absolute inset-y-0 left-3 flex items-center text-slate-500 text-xl">
              shield
            </span>

            <InputText
              type="password"
              value={formData.confirmPassword}
              onChange={(val) => onChange("confirmPassword", val)}
              placeholder="*********"
              className="w-full bg-slate-800/50 border border-slate-700 rounded-lg py-3 pl-11 pr-4 text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary transition-all"
            />
          </div>
        </div>
      </div>
    </>
  );
}
