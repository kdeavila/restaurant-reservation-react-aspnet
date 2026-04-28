export type Role = "Admin" | "Manager" | "Employee"
export type ReservationStatus = "Pending" | "Confirmed" | "Completed" | "Cancelled"
export type EntityStatus = "Active" | "Inactive"
export type TableStatus = "Active" | "Inactive" | "Maintenance"
export type PricingRuleType = "Surcharge" | "Discount"

export interface Pagination {
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
}

export interface ApiResponse<T> {
  success: boolean
  data: T | null
  message: string
  statusCode: number
  error: string | null
  pagination: Pagination | null
}

export interface AuthUser {
  id: number
  username: string
  email: string
  role: Role
  status: EntityStatus
  token: string
  tokenExpiry: string
}

export interface TableType {
  id: number
  name: string
  description?: string
  basePricePerHour: number
  isActive: boolean
  tableCount: number
  createdAt: string
}

export interface TableDetailed {
  id: number
  code: string
  capacity: number
  location: string
  status: TableStatus
  tableType: TableType
}

export interface Client {
  id: number
  firstName: string
  lastName: string
  email: string
  phone?: string
  status: EntityStatus
  totalReservations: number
  createdAt: string
}

export interface User {
  id: number
  username: string
  email: string
  role: Role
  status: EntityStatus
}

export interface Reservation {
  id: number
  client: Client
  table: TableDetailed
  date: string
  startTime: string
  endTime: string
  numberOfGuests: number
  basePrice: number
  totalPrice: number
  status: ReservationStatus
  notes?: string
  user: Pick<User, "id" | "username">
  createdAt: string
}

export interface PricingRule {
  id: number
  ruleName: string
  ruleType: PricingRuleType
  startTime: string
  endTime: string
  startDate: string
  endDate: string
  surchargePercentage: number
  tableTypeId: number
  tableTypeName: string
  daysOfWeek: number[]
  isActive: boolean
}

export interface CreateReservationDto {
  clientId: number
  tableId: number
  date: string
  startTime: string
  endTime: string
  numberOfGuests: number
  notes?: string
}

export interface UpdateReservationDto extends Partial<Omit<CreateReservationDto, "clientId">> {
  status?: ReservationStatus
}

export interface CreateClientDto {
  firstName: string
  lastName: string
  email: string
  phone?: string
}

export interface CreateTableDto {
  capacity: number
  location: string
  tableTypeId: number
}

export interface CreatePricingRuleDto {
  ruleName: string
  ruleType: PricingRuleType
  startTime: string
  endTime: string
  startDate: string
  endDate: string
  surchargePercentage: number
  tableTypeId: number
  daysOfWeek: number[]
}

export interface CreateUserDto {
  username: string
  email: string
  password: string
  role?: Role
}
