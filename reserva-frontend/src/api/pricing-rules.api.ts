import type { ApiResponse, CreatePricingRuleDto, PricingRule } from "@/types"
import { http } from "./http"

export const pricingRulesApi = {
  list: async () => {
    const { data } = await http.get<ApiResponse<PricingRule[]>>("/pricing-rules")
    return data
  },
  detail: async (id: number) => {
    const { data } = await http.get<ApiResponse<PricingRule>>(`/pricing-rules/${id}`)
    return data
  },
  create: async (dto: CreatePricingRuleDto) => {
    const { data } = await http.post<ApiResponse<PricingRule>>("/pricing-rules", dto)
    return data
  },
  update: async (id: number, dto: Partial<CreatePricingRuleDto>) => {
    const { data } = await http.patch<ApiResponse<PricingRule>>(`/pricing-rules/${id}`, dto)
    return data
  },
  remove: async (id: number) => {
    const { data } = await http.delete<ApiResponse<void>>(`/pricing-rules/${id}`)
    return data
  },
}
