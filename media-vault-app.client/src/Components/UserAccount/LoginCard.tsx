import { useState } from "react";
import ModalWindow from "../Shared/ModalWindow";
import { useUser } from "../../Shared/UserContext";
import { useNavigate } from "react-router-dom";

type LoginProps = {
  onCancel: (toRegister: boolean) => void;
};

const defaultCardClassName =
  "w-full max-w-[480px] bg-slate-100 dark:bg-[#182634] rounded-xl shadow-2xl border border-slate-200 dark:border-slate-800 overflow-hidden";
// "relative w-full max-w-[440px] bg-slate-900/80 border border-slate-800 rounded-xl shadow-2xl overflow-hidden";

export default function Login({ onCancel }: LoginProps) {
  const { login, isLoading } = useUser();
  const [userNameOrEmail, setUserNameOrEmail] = useState("");
  const [password, setPassword] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);

  const handleSubmit = async (e: React.SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();
    setErrorMessage(null);
    setIsSubmitting(true);

    try {
      await login({ userNameOrEmail, password });
      onCancel(false); // Close the login modal
      navigate("/dashboard"); // Redirect to the dashboard after successful login

    } catch (error) {
      setErrorMessage(
        error instanceof Error ? error.message : "Failed to login",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const togglePasswordVisibility = () => {
    setShowPassword((prev) => !prev);
  }

  return (
    <ModalWindow onClose={() => onCancel(false)} cardClassName={defaultCardClassName}>
      {/* Header */}
      <div className="px-8 pt-10 pb-6 text-center">
        <div className="inline-flex items-center justify-center p-3 rounded-full bg-primary/10 mb-4">
          <span className="material-symbols-outlined text-primary text-3xl">
            lock_open
          </span>
        </div>
        <h2 className="text-3xl font-black tracking-tight text-slate-900 dark:text-white">
          Welcome Back
        </h2>
        <p className="text-slate-500 dark:text-slate-400 mt-2">
          Log in to your MediaLog account
        </p>
      </div>
      <div className="px-8 pb-10 space-y-6">
        {/* Form Fields */}
        <form className="space-y-4" onSubmit={handleSubmit}>
          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
              Email or Username
            </label>
            <div className="relative">
              <span className="material-symbols-outlined pointer-events-none absolute inset-y-0 left-3 flex items-center text-slate-400 text-xl">
                person
              </span>
              <input
                id="login-username-or-email"
                type="text"
                value={userNameOrEmail}
                onChange={(e) => setUserNameOrEmail(e.target.value)}
                required
                className="w-full h-14 pl-12 pr-4 rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-[#101922] focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all placeholder:text-slate-500"
                placeholder="name@example.com"
              />
            </div>
          </div>
          <div>
            <div className="flex justify-between mb-2">
              <label className="text-sm font-medium text-slate-700 dark:text-slate-300">
                Password
              </label>
              {/* <a
                className="text-sm font-semibold text-primary hover:underline"
                href="#"
              >
                Forgot password?
              </a> */}
            </div>
            <div className="relative">
              <span className="material-symbols-outlined pointer-events-none absolute inset-y-0 left-3 flex items-center text-slate-400 text-xl">
                lock
              </span>

              <input
                id="login-password"
                type={showPassword ? "text" : "password"}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                className="w-full h-14 pl-12 pr-12 rounded-lg border border-slate-300 dark:border-slate-700 bg-white dark:bg-[#101922] focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all placeholder:text-slate-500"
                placeholder="••••••••"
              />
              <button
                type="button"
                className="absolute inset-y-0 right-4 flex items-center"
                onClick={togglePasswordVisibility}
              >
                <span className="material-symbols-outlined text-slate-400 text-xl">
                  {showPassword ? "visibility_off" : "visibility"}
                </span>
              </button>
            </div>
          </div>

          {/* Remember Me */}
          <div className="flex items-center">
            <input
              className="w-5 h-5 rounded border-slate-300 dark:border-slate-700 bg-white dark:bg-[#101922] text-primary focus:ring-primary focus:ring-offset-0"
              id="remember"
              type="checkbox"
            />
            <label
              className="ml-3 text-sm font-medium text-slate-600 dark:text-slate-400"
              htmlFor="remember"
            >
              Remember me for 30 days
            </label>
          </div>
          {/* Submit */}
          {errorMessage && (
            <p className="text-sm text-red-400">{errorMessage}</p>
          )}

          <button
            type="submit"
            disabled={isSubmitting || isLoading}
            className="w-full h-14 bg-primary hover:bg-primary/90 text-white font-bold rounded-lg shadow-lg shadow-primary/20 transition-all flex items-center justify-center gap-2 group"
          >
            {isSubmitting ? "Logging in..." : "Login"}
            <span className="material-symbols-outlined group-hover:translate-x-1 transition-transform">
              arrow_forward
            </span>
          </button>
        </form>
        {/* Footer Link */}
        <p className="text-center text-sm text-slate-500 dark:text-slate-400">
          Don't have an account?
          <a className="ms-1 text-primary font-bold hover:underline" href="#" onClick={() => onCancel(true)}>
            Create an account
          </a>
        </p>
      </div>
    </ModalWindow>
  );
}
