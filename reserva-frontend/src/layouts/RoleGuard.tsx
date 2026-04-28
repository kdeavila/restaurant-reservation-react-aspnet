import type { ReactNode } from "react"
import { Navigate } from "react-router-dom"
import { useAuthStore } from "@/store/auth.store"
import type { Role } from "@/types"

interface RoleGuardProps {
  children: ReactNode
  allowed: Role[]
}

export function RoleGuard({ children, allowed }: RoleGuardProps) {
  const role = useAuthStore((state) => state.role)

  if (!role || !allowed.includes(role)) {
    return <Navigate to="/disponibilidad" replace />
  }

  return <>{children}</>
}
