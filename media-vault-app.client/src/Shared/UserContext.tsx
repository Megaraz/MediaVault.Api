import {
  useCallback,
  useEffect,
  useMemo,
  useState,
} from "react";
import UsersClient, {
  type UserDetailedDto,
  type UserLoginDto,
} from "../Clients/UsersClient";
import { UserContext } from "./UserContextDefinition";

type UserProviderProps = {
  children: React.ReactNode;
};

export function UserProvider({ children }: UserProviderProps) {
  const [currentUser, setCurrentUser] = useState<UserDetailedDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [client] = useState(() => new UsersClient());

  const refreshCurrentUser = useCallback(async () => {
    try {
      const user = await client.getCurrentUser();
      setCurrentUser(user);
      return user;
    } catch {
      setCurrentUser(null);
      return null;
    }
  }, [client]);

  useEffect(() => {
    let isMounted = true;

    const loadCurrentUser = async () => {
      try {
        const user = await client.getCurrentUser();

        if (isMounted) {
          setCurrentUser(user);
        }
      } catch {
        if (isMounted) {
          setCurrentUser(null);
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    void loadCurrentUser();

    return () => {
      isMounted = false;
    };
  }, [client]);

  const login = useCallback(async (credentials: UserLoginDto) => {
    const user = await client.login(credentials);
    setCurrentUser(user);
    return user;
  }, [client]);

  const logout = useCallback(async () => {
    await client.logout();
    setCurrentUser(null);
  }, [client]);

  const value = useMemo(
    () => ({
      currentUser,
      isAuthenticated: currentUser !== null,
      isLoading,
      login,
      logout,
      refreshCurrentUser,
      setCurrentUser,
    }),
    [currentUser, isLoading, login, logout, refreshCurrentUser],
  );

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>;
}
