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

const defaultclassNameName: string =
  "relative w-full max-w-[480px] bg-slate-900/50 border border-slate-800 rounded-xl shadow-2xl overflow-hidden";

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

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
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
      mainPopupClassName={defaultclassNameName}
      overlayClassName="fixed inset-0 z-50 flex items-center justify-center bg-background-dark/80 backdrop-blur-md p-4"
    >
      <div className="p-8 pb-0">
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-2 text-primary">
            <span className="material-symbols-outlined text-2xl">
              motion_photos_on
            </span>
            <h2 className="text-slate-100 text-xl font-bold tracking-tight">
              MediaVault
            </h2>
          </div>
          <button
            onClick={() => onCancel(false)}
            className="text-slate-400 hover:text-slate-100 transition-colors"
          >
            <span className="material-symbols-outlined">close</span>
          </button>
        </div>
        <div className="space-y-1">
          <h1 className="text-2xl font-bold text-slate-100">Create Account</h1>
          <p className="text-slate-400 text-sm">
            Join the community and start logging your media today.
          </p>
        </div>
      </div>
      <form className="p-8 pt-6 space-y-4" onSubmit={handleSubmit}>
        <RegisterUserForm formData={formData} onChange={handleChange} />
        {/* Terms */}
        <div className="flex items-start gap-3 pt-2">
          <input
            className="mt-1 h-4 w-4 rounded border-slate-700 bg-slate-800 text-primary focus:ring-primary focus:ring-offset-slate-900"
            id="terms"
            type="checkbox"
          />
          <label
            className="text-sm text-slate-400 leading-snug"
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
          className="w-full bg-primary hover:bg-primary/90 text-white font-bold py-3.5 rounded-lg shadow-lg shadow-primary/20 transition-all flex items-center justify-center gap-2 mt-4"
        >
          <span>{isSubmitting ? "Creating Account..." : "Create Account"}</span>
          <span className="material-symbols-outlined text-lg">
            arrow_forward
          </span>
        </button>
      </form>

      {/* Footer */}
      <div className="px-8 pb-8 text-center">
        <p className="text-sm text-slate-400">
          Already have an account?
          <a
            className="ms-1 text-primary font-semibold hover:underline"
            href="#"
            onClick={() => onCancel(true)}
          >
            Sign In
          </a>
        </p>
      </div>
      {/* Decorative Element */}
      <div className="absolute top-0 right-0 p-4 opacity-10 pointer-events-none">
        <span className="material-symbols-outlined text-[120px]">
          person_add
        </span>
      </div>
    </ModularPopupWindow>
  );
}
