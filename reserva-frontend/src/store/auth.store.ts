import { create } from "zustand"
import { persist } from "zustand/middleware"
import type { AuthUser, Role } from "@/types"

interface AuthState {
  user: AuthUser | null
  token: string | null
  role: Role | null
  setSession: (user: AuthUser) => void
  clearSession: () => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      role: null,
      setSession: (user) => set({ user, token: user.token, role: user.role }),
      clearSession: () => set({ user: null, token: null, role: null }),
    }),
    { name: "reserva-auth" },
  ),
)
