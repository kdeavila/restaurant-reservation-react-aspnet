import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { pricingRulesApi } from "@/api/pricing-rules.api";
import { qk } from "./query-keys";
import { toast } from "sonner";

export const usePricingRuleList = () =>
  useQuery({ queryKey: qk.pricingRules(), queryFn: () => pricingRulesApi.list() });

export const usePricingRuleDetail = (id: number) =>
  useQuery({ queryKey: qk.pricingRules(), queryFn: () => pricingRulesApi.detail(id), enabled: !!id });

export const useCreatePricingRule = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: pricingRulesApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: qk.pricingRules() });
      toast.success("Regla de precio creada");
    },
    onError: (e: Error) => toast.error(e.message),
  });
};

export const useUpdatePricingRule = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: Record<string, unknown> }) => pricingRulesApi.update(id, dto),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: qk.pricingRules() });
      toast.success("Regla de precio actualizada");
    },
    onError: (e: Error) => toast.error(e.message),
  });
};

export const useDeletePricingRule = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: pricingRulesApi.remove,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: qk.pricingRules() });
      toast.success("Regla de precio eliminada");
    },
    onError: (e: Error) => toast.error(e.message),
  });
};
