import { useState } from "react";
import ModularPopupWindow from "../Shared/ModularPopupWindow";
import { useUser } from "../../Shared/UserContext";

type LoginProps = {
  onCancel: () => void;
};

const defaultMainPopupClassName =
  "relative w-full max-w-[440px] bg-slate-900/50 border border-slate-800 rounded-xl shadow-2xl overflow-hidden";

export default function Login({ onCancel }: LoginProps) {
  const { login, isLoading } = useUser();
  const [userNameOrEmail, setUserNameOrEmail] = useState("");
  const [password, setPassword] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();
    setErrorMessage(null);
    setIsSubmitting(true);

    try {
      await login({ userNameOrEmail, password });
      onCancel();
    } catch (error) {
      setErrorMessage(
        error instanceof Error ? error.message : "Failed to login",
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <ModularPopupWindow
      onClose={onCancel}
      mainPopupClassName={defaultMainPopupClassName}
      overlayClassName="fixed inset-0 z-50 flex items-center justify-center bg-background-dark/80 backdrop-blur-md p-4"
    >
      <div className="p-8">
        <div className="mb-6 flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold text-slate-100">Login</h2>
            <p className="text-sm text-slate-400">
              Use your account to continue.
            </p>
          </div>
          <button
            type="button"
            onClick={onCancel}
            className="text-slate-400 transition-colors hover:text-slate-100"
          >
            <span className="material-symbols-outlined">close</span>
          </button>
        </div>

        <form className="space-y-4" onSubmit={handleSubmit}>
          <div>
            <label
              htmlFor="login-username-or-email"
              className="block text-sm text-slate-300"
            >
              Username or Email
            </label>
            <input
              id="login-username-or-email"
              type="text"
              value={userNameOrEmail}
              onChange={(e) => setUserNameOrEmail(e.target.value)}
              required
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-800 px-3 py-2 text-slate-100"
            />
          </div>

          <div>
            <label
              htmlFor="login-password"
              className="block text-sm text-slate-300"
            >
              Password
            </label>
            <input
              id="login-password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-800 px-3 py-2 text-slate-100"
            />
          </div>

          {errorMessage && (
            <p className="text-sm text-red-400">{errorMessage}</p>
          )}

          <button
            type="submit"
            disabled={isSubmitting || isLoading}
            className="w-full rounded-lg bg-primary py-3 font-bold text-white transition-all hover:bg-primary/90 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {isSubmitting ? "Logging in..." : "Login"}
          </button>
        </form>
      </div>
    </ModularPopupWindow>
  );
}
