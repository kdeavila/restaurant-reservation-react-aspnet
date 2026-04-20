import { useMutation } from "@tanstack/react-query";
import { authApi } from "@/api/auth.api";
import { useAuthStore } from "@/store/auth.store";
import { toast } from "sonner";

export const useLogin = () => {
  const setSession = useAuthStore((state) => state.setSession);
  return useMutation({
    mutationFn: authApi.login,
    onSuccess: (data) => {
      if (data.data) {
        setSession(data.data);
        toast.success("Sesión iniciada");
      }
    },
    onError: (e: Error) => toast.error(e.message),
  });
};
