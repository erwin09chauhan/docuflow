import { createContext, useContext, useState, useEffect, useRef } from "react";
import type { ReactNode } from "react";
import type { User } from "../../types";
import { setAccessToken } from "../../lib/axios";
import { getMe } from "../../api/auth";
import { useQueryClient } from "@tanstack/react-query";

interface AuthContextType {
  user: User | null;
  setUser: (user: User | null) => void;
  isAuthenticated: boolean;
  isLoading: boolean;
  signOut: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

const USER_KEY = "docuflow_user";
const TOKEN_KEY = "docuflow_token";

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUserState] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const initialized = useRef(false);
  const queryClient = useQueryClient();

  useEffect(() => {
    if (initialized.current) return;
    initialized.current = true;

    const storedToken = localStorage.getItem(TOKEN_KEY);
    if (!storedToken) {
      setTimeout(() => setIsLoading(false), 0);
      return;
    }

    setAccessToken(storedToken);

    getMe()
      .then((fetchedUser) => setUserState(fetchedUser))
      .catch(() => {
        setUserState(null);
        localStorage.removeItem(USER_KEY);
        localStorage.removeItem(TOKEN_KEY);
        setAccessToken(null);
      })
      .finally(() => setIsLoading(false));
  }, []);

  const setUser = (user: User | null) => {
    setUserState(user);
    if (user) {
      localStorage.setItem(USER_KEY, JSON.stringify(user));
    } else {
      localStorage.removeItem(USER_KEY);
      localStorage.removeItem(TOKEN_KEY);
    }
  };

  const signOut = () => {
    setUser(null);
    setAccessToken(null);
    queryClient.clear();
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        setUser,
        isAuthenticated: !!user,
        isLoading,
        signOut,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within AuthProvider");
  return context;
};
