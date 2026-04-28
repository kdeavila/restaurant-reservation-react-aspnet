import { useMutation } from "@tanstack/react-query"
import { authApi } from "@/api/auth.api"
import { useAuthStore } from "@/store/auth.store"

export const useLogin = () => {
  const setSession = useAuthStore((state) => state.setSession)
  return useMutation({
    mutationFn: async (dto: { email: string; password: string }) => {
      const response = await authApi.login(dto)
      const user = response.data

      if (!response.success || !user) {
        throw new Error(response.error ?? response.message ?? "No fue posible iniciar sesión")
      }

      return { ...response, data: user }
    },
    onSuccess: (response) => {
      setSession(response.data)
    },
  })
}
