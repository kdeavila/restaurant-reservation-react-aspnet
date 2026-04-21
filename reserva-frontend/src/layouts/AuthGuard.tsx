import { useEffect, type ReactNode } from "react";
import { Navigate } from "react-router-dom";
import { useAuthStore } from "@/store/auth.store";

interface AuthGuardProps {
  children: ReactNode;
}

export function AuthGuard({ children }: AuthGuardProps) {
  const token = useAuthStore((state) => state.token);
  const user = useAuthStore((state) => state.user);
  const clearSession = useAuthStore((state) => state.clearSession);

  const isExpired =
    !user?.tokenExpiry ||
    new Date(user.tokenExpiry).getTime() <= Date.now();

  useEffect(() => {
    if (token && isExpired) {
      clearSession();
    }
  }, [token, isExpired, clearSession]);

  if (!token || isExpired) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}
