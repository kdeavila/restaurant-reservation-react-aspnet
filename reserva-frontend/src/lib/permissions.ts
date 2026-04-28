import type { Role } from "@/types"

export const can = {
  deleteReservation: (r: Role) => r === "Admin" || r === "Manager",
  cancelReservation: () => true,
  deleteClient: (r: Role) => r === "Admin",
  editClient: () => true,
  editTables: (r: Role) => r === "Admin" || r === "Manager",
  deleteTables: (r: Role) => r === "Admin",
  editPricingRules: (r: Role) => r === "Admin" || r === "Manager",
  seeUsers: (r: Role) => r === "Admin",
  createUser: (r: Role) => r === "Admin",
}
