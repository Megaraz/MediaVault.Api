import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
  type Dispatch,
  type SetStateAction,
} from "react";
import UsersClient, {
  type UserDetailedDto,
  type UserLoginDto,
} from "../Clients/UsersClient";

type UserContextType = {
  currentUser: UserDetailedDto | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: UserLoginDto) => Promise<UserDetailedDto>;
  logout: () => Promise<void>;
  refreshCurrentUser: () => Promise<UserDetailedDto | null>;
  setCurrentUser: Dispatch<SetStateAction<UserDetailedDto | null>>;
};

type UserProviderProps = {
  children: React.ReactNode;
};

const UserContext = createContext<UserContextType | undefined>(undefined);

export function UserProvider({ children }: UserProviderProps) {
  const [currentUser, setCurrentUser] = useState<UserDetailedDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [client] = useState(() => new UsersClient());

  const refreshCurrentUser = async () => {
    try {
      const user = await client.getCurrentUser();
      setCurrentUser(user);
      return user;
    } catch {
      setCurrentUser(null);
      return null;
    }
  };

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

  const login = async (credentials: UserLoginDto) => {
    const user = await client.login(credentials);
    setCurrentUser(user);
    return user;
  };

  const logout = async () => {
    await client.logout();
    setCurrentUser(null);
  };

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
    [currentUser, isLoading],
  );

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>;
}

export function useUser() {
  const context = useContext(UserContext);

  if (!context) {
    throw new Error("useUser must be used inside UserProvider");
  }

  return context;
}
