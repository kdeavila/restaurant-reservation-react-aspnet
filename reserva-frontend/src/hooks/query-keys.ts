export const qk = {
  clients: (p?: object) => ["clients", p] as const,
  client: (id: number) => ["clients", id] as const,
  reservations: (p?: object) => ["reservations", p] as const,
  reservation: (id: number) => ["reservations", id] as const,
  tables: (p?: object) => ["tables", p] as const,
  available: (p: object) => ["tables", "available", p] as const,
  tableTypes: () => ["table-types"] as const,
  tableType: (id: number) => ["table-types", id] as const,
  pricingRules: () => ["pricing-rules"] as const,
  users: () => ["users"] as const,
};
