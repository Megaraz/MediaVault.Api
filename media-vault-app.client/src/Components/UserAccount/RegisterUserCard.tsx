import { useState } from "react";
import type { UserCreateDto } from "../../Clients/UsersClient";
import ModularPopupWindow from "../Shared/ModularPopupWindow";
import RegisterUserForm, { RegisterUserFormData } from "./RegisterUserForm";
import UsersClient from "../../Clients/UsersClient";

function isMatch(fieldName: string, value1: string, value2: string): boolean {
  if (value1 !== value2) {
    alert(`${fieldName} do not match`);
    return false;
  }
  return true;
}

function isValidFormData(data: RegisterUserFormData): boolean {
  if (
    !data.username ||
    !data.email ||
    !data.confirmEmail ||
    !data.password ||
    !data.confirmPassword
  ) {
    alert("Please fill in all fields");
    return false;
  }
  return true;
}

const defaultCardClassName: string =
  "w-full max-w-[480px] bg-slate-100 dark:bg-[#182634] rounded-xl shadow-2xl border border-slate-200 dark:border-slate-800 overflow-hidden";
// "relative w-full max-w-[480px] bg-slate-900/80 border border-slate-800 rounded-xl shadow-2xl overflow-hidden";

type RegisterProps = {
  onCancel: (toLogin: boolean) => void;
};

export default function RegisterUser({ onCancel }: RegisterProps) {
  const [client] = useState(() => new UsersClient());
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formData, setFormData] = useState<RegisterUserFormData>(
    new RegisterUserFormData(),
  );

  const handleSubmit = async (e: React.SubmitEvent) => {
    e.preventDefault();
    setErrorMessage(null);
    setIsSubmitting(true);

    if (!isValidFormData(formData)) {
      setIsSubmitting(false);
      return;
    }

    if (!isMatch("Emails", formData.email, formData.confirmEmail)) {
      setIsSubmitting(false);
      return;
    }

    if (!isMatch("Passwords", formData.password, formData.confirmPassword)) {
      setIsSubmitting(false);
      return;
    }

    const dto: UserCreateDto = {
      username: formData.username,
      email: formData.email,
      confirmEmail: formData.confirmEmail,
      password: formData.password,
      confirmPassword: formData.confirmPassword,
    };

    try {
      await client.registerUser(dto);
      onCancel(true);
    } catch (error) {
      setErrorMessage(
        error instanceof Error ? error.message : "Failed to register user",
      );
    } finally {
      setIsSubmitting(false);
      onCancel(true);
    }
  };

  const handleChange = (field: keyof RegisterUserFormData, value: string) => {
    setFormData((prev) => {
      return { ...prev, [field]: value };
    });
  };

  return (
    <ModularPopupWindow
      onClose={() => onCancel(false)}
      cardClassName={defaultCardClassName}
    >
      <div className="px-8 pt-10 pb-6 text-center relative">
        <button
          type="button"
          onClick={() => onCancel(false)}
          className="absolute top-6 right-6 text-slate-500 dark:text-slate-400 hover:text-slate-700 dark:hover:text-slate-100 transition-colors"
        >
          <span className="material-symbols-outlined">close</span>
        </button>
        <div className="inline-flex items-center justify-center p-3 rounded-full bg-primary/10 mb-4">
          <span className="material-symbols-outlined text-primary text-3xl">
            person_add
          </span>
        </div>
        <div className="space-y-1">
          <h1 className="text-3xl font-black tracking-tight text-slate-900 dark:text-white">
            Create Account
          </h1>
          <p className="text-slate-500 dark:text-slate-400 mt-2">
            Join the community and start logging your media today.
          </p>
        </div>
      </div>
      <form className="px-8 pb-10 space-y-4" onSubmit={handleSubmit}>
        <RegisterUserForm formData={formData} onChange={handleChange} />
        {/* Terms */}
        <div className="flex items-center pt-2">
          <input
            className="w-5 h-5 rounded border-slate-300 dark:border-slate-700 bg-white dark:bg-[#101922] text-primary focus:ring-primary focus:ring-offset-0"
            id="terms"
            type="checkbox"
          />
          <label
            className="ml-3 text-sm font-medium text-slate-600 dark:text-slate-400 leading-snug"
            htmlFor="terms"
          >
            I agree to the{" "}
            <a className="text-primary hover:underline" href="#">
              Terms of Service
            </a>{" "}
            and{" "}
            <a className="text-primary hover:underline" href="#">
              Privacy Policy
            </a>
            .
          </label>
        </div>
        {/* Button */}

        {errorMessage && <p className="text-sm text-red-400">{errorMessage}</p>}
        <button
          type="submit"
          disabled={isSubmitting}
          className="w-full h-14 bg-primary hover:bg-primary/90 text-white font-bold rounded-lg shadow-lg shadow-primary/20 transition-all flex items-center justify-center gap-2 group"
        >
          <span>{isSubmitting ? "Creating Account..." : "Create Account"}</span>
          <span className="material-symbols-outlined group-hover:translate-x-1 transition-transform">
            arrow_forward
          </span>
        </button>
      </form>

      {/* Footer */}
      <div className="px-8 pb-10 text-center">
        <p className="text-center text-sm text-slate-500 dark:text-slate-400">
          Already have an account?
          <a
            className="ms-1 text-primary font-bold hover:underline"
            href="#"
            onClick={() => onCancel(true)}
          >
            Sign In
          </a>
        </p>
      </div>
    </ModularPopupWindow>
  );
}
