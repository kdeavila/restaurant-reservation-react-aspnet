import { type ReactNode, useEffect } from "react"
import { Navigate } from "react-router-dom"
import { useAuthStore } from "@/store/auth.store"

interface AuthGuardProps {
  children: ReactNode
}

export function AuthGuard({ children }: AuthGuardProps) {
  const token = useAuthStore((state) => state.token)
  const user = useAuthStore((state) => state.user)

  const tokenExpiry = user?.tokenExpiry

  useEffect(() => {
    if (!token || !tokenExpiry) {
      return
    }

    const expiresAt = new Date(tokenExpiry).getTime()
    if (Number.isNaN(expiresAt)) {
      return
    }

    const clearSession = () => useAuthStore.getState().clearSession()
    const msUntilExpiry = expiresAt - Date.now()

    if (msUntilExpiry <= 0) {
      const handler = setTimeout(() => {
        clearSession()
      }, 0)
      return () => clearTimeout(handler)
    }

    const timeoutId = setTimeout(() => {
      clearSession()
    }, msUntilExpiry)

    return () => clearTimeout(timeoutId)
  }, [token, tokenExpiry])

  if (!token) {
    return <Navigate to="/login" replace />
  }

  return <>{children}</>
}
