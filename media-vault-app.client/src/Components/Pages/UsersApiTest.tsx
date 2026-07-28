import { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  type UserDetailedDto,
  type UserCreateDto,
} from "../../Clients/UsersClient";
import UsersClient from "../../Clients/UsersClient";

export default function UsersApiTest() {
  const navigate = useNavigate();
  const [users, setUsers] = useState<UserDetailedDto[]>([]);
  const [client] = useState(new UsersClient());
  const [loading, setLoading] = useState(false);
  const [timeout, setTimeoutMs] = useState(5000);
  const [timedOut, setTimedOut] = useState(false);

  const handleUserClick = (user: UserDetailedDto) => {
    navigate("/media-entries-api-test", {
      state: {
        selectedUser: user,
      },
    });
  };

  const createUser = async (newUser: UserCreateDto) => {
    setLoading(true);
    try {
      await client.registerUser(newUser);
      setUsers(await client.getUsers());
    } catch (error) {
      console.error("Failed to create user:", error);
    } finally {
      setLoading(false);
    }
  };

  const fetchUsers = async () => {
    setLoading(true);
    setTimedOut(false);

    try {
      const timeoutPromise = new Promise((_, reject) =>
        setTimeout(() => reject(new Error("Connection timed out")), timeout),
      );

      const simulatedDelay = new Promise((resolve) =>
        setTimeout(resolve, 1500),
      );

      const [fetchedUsers] = (await Promise.race([
        Promise.all([client.getUsers(), simulatedDelay]),
        timeoutPromise,
      ])) as [UserDetailedDto[], unknown];

      setUsers(fetchedUsers);
    } catch (error) {
      if ((error as Error).message === "Connection timed out") {
        setTimedOut(true);
      } else {
        console.error("Failed to fetch users:", error);
      }
    } finally {
      setLoading(false);
    }
  };
  return (
    <div className="flex flex-row gap-6 justify-center items-center p-6">
      <div
        className={`${loading ? "opacity-50 pointer-events-none" : ""} flex flex-col gap-6 p-6`}
      >
        <div className="flex flex-col gap-4">
          <h1>Users API Test</h1>
          <p>This page is for testing the Users API.</p>
          <div>
            <label htmlFor="timeout" className="block text-sm font-medium">
              Timeout (ms)
            </label>
            <input
              id="timeout"
              type="number"
              min="100"
              max="30000"
              value={timeout}
              onChange={(e) => setTimeoutMs(Number(e.target.value))}
              className="mt-1 px-3 py-2 border rounded text-black w-32"
            />
          </div>
        </div>
        <div className="flex flex-row justify-between gap-6">
          <div className="flex flex-col justify-center items-center gap-4">
            {loading ? (
              <div className="flex flex-col items-center gap-4">
                <div className="flex items-center justify-center">
                  <div className="relative h-12 w-12">
                    <div className="absolute inset-0 rounded-full border-4 border-gray-300 border-t-blue-500 animate-spin"></div>
                  </div>
                </div>
                <p className="text-sm text-gray-600">Loading...</p>
              </div>
            ) : timedOut ? (
              <div className="text-red-600 font-semibold">
                ⚠️ Connection timed out
              </div>
            ) : (
              <>
                <button
                  onClick={fetchUsers}
                  className="mb-4 px-4 py-2 bg-blue-500 text-white rounded"
                  disabled={loading}
                >
                  Fetch Users
                </button>
                {users.length > 0 && (
                  <>
                    <div className="flex flex-col items-center gap-2">
                      <h2 className="text-lg font-semibold">
                        All fetched users
                      </h2>
                      <p className="text-sm text-slate-600">
                        Click a user to manage that user&apos;s media entries.
                      </p>
                    </div>
                    <ul className="flex flex-col gap-6">
                      {users.map((user) => (
                        <li key={user.id} className="list-none">
                          <button
                            type="button"
                            onClick={() => handleUserClick(user)}
                            className="w-full rounded-xl border border-slate-200 bg-gray-600 p-4 text-left shadow-sm transition duration-150 hover:-translate-y-1 hover:border-blue-400 hover:shadow-md active:translate-y-0 active:scale-[0.99]"
                          >
                            <p>
                              <b>ID:</b> {user.id}
                            </p>
                            <p>
                              <b>Username:</b> {user.username}
                            </p>
                            <p>
                              <b>Email:</b> {user.email}
                            </p>
                          </button>
                        </li>
                      ))}
                    </ul>
                  </>
                )}
              </>
            )}
          </div>

          <div className="flex flex-col gap-4">
            <h2>Create a new user</h2>
            <form
              onSubmit={(e) => {
                e.preventDefault();
                const formData = new FormData(e.currentTarget);
                createUser({
                  username: formData.get("username") as string,
                  email: formData.get("email") as string,
                  confirmEmail: formData.get("confirmEmail") as string,
                  password: formData.get("password") as string,
                  confirmPassword: formData.get("confirmPassword") as string,
                });
                e.currentTarget.reset();
              }}
              className="space-y-4"
            >
              <div>
                <label htmlFor="username" className="block text-sm font-medium">
                  Username
                </label>
                <input
                  id="username"
                  name="username"
                  type="text"
                  required
                  className="mt-1 px-3 py-2 border rounded text-black"
                />
              </div>
              <div>
                <label htmlFor="email" className="block text-sm font-medium">
                  Email
                </label>
                <input
                  id="email"
                  name="email"
                  type="email"
                  required
                  className="mt-1 px-3 py-2 border rounded text-black"
                />
              </div>
              <div>
                <label
                  htmlFor="confirmEmail"
                  className="block text-sm font-medium"
                >
                  Confirm Email
                </label>
                <input
                  id="confirmEmail"
                  name="confirmEmail"
                  type="email"
                  required
                  className="mt-1 px-3 py-2 border rounded text-black"
                />
              </div>
              <div>
                <label htmlFor="password" className="block text-sm font-medium">
                  Password
                </label>
                <input
                  id="password"
                  name="password"
                  type="password"
                  required
                  className="mt-1 px-3 py-2 border rounded text-black"
                />
              </div>
              <div>
                <label
                  htmlFor="confirmPassword"
                  className="block text-sm font-medium"
                >
                  Confirm Password
                </label>
                <input
                  id="confirmPassword"
                  name="confirmPassword"
                  type="password"
                  required
                  className="mt-1 px-3 py-2 border rounded"
                />
              </div>
              <button
                type="submit"
                className="px-4 py-2 bg-green-500 text-white rounded"
                disabled={loading}
              >
                Create User
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}
