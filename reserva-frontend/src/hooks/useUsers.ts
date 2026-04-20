import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { usersApi } from "@/api/users.api";
import { qk } from "./query-keys";
import { toast } from "sonner";

export const useUserList = () =>
  useQuery({ queryKey: qk.users(), queryFn: () => usersApi.list() });

export const useUserDetail = (id: number) =>
  useQuery({ queryKey: qk.users(), queryFn: () => usersApi.detail(id), enabled: !!id });

export const useDeleteUser = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: usersApi.remove,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: qk.users() });
      toast.success("Usuario eliminado");
    },
    onError: (e: Error) => toast.error(e.message),
  });
};
