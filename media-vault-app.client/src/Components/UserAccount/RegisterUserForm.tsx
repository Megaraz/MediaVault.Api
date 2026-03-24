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

export default function RegisterUserForm({
  formData,
  onChange,
}: RegisterUserFormProps) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
      {/* Username */}
      <div className="col-span-full">
        <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
          Username
        </label>
        <InputText
          value={formData.username}
          onChange={(val) => onChange("username", val)}
          placeholder="e.g. johndoe123"
        />
      </div>

      {/* Email */}
      <div>
        <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
          Email
        </label>
        <InputText
          type="email"
          value={formData.email}
          onChange={(val) => onChange("email", val)}
          placeholder="e.g. johndoe@example.com"
        />
      </div>

      {/* Confirm Email */}
      <div>
        <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
          Confirm Email
        </label>
        <InputText
          type="email"
          value={formData.confirmEmail}
          onChange={(val) => onChange("confirmEmail", val)}
          placeholder="e.g. johndoe@example.com"
        />
      </div>

      {/* Password */}
      <div>
        <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
          Password
        </label>
        <InputText
          type="password"
          value={formData.password}
          onChange={(val) => onChange("password", val)}
          placeholder="Enter your password"
        />
      </div>

      {/* Confirm Password */}
      <div>
        <label className="block mb-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
          Confirm Password
        </label>
        <InputText
          type="password"
          value={formData.confirmPassword}
          onChange={(val) => onChange("confirmPassword", val)}
          placeholder="Confirm your password"
        />
      </div>
    </div>
  );
}
