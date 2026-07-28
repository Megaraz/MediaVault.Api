import { createContext, type Dispatch, type SetStateAction } from "react";
import type {
  UserDetailedDto,
  UserLoginDto,
} from "../Clients/UsersClient";

export type UserContextType = {
  currentUser: UserDetailedDto | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: UserLoginDto) => Promise<UserDetailedDto>;
  logout: () => Promise<void>;
  refreshCurrentUser: () => Promise<UserDetailedDto | null>;
  setCurrentUser: Dispatch<SetStateAction<UserDetailedDto | null>>;
};

export const UserContext = createContext<UserContextType | undefined>(undefined);
