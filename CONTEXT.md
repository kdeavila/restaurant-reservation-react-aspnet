# CONTEXT.md — Frontend Sistema de Reservas
**Lee este archivo COMPLETO antes de escribir una sola línea de código.**
**Versión:** 2.0 — 2026-04-18

---

## Misión

Construir el frontend de un sistema interno de gestión de reservas para restaurante.
El backend ASP.NET Core 9 ya existe y está completo. Tu trabajo es exclusivamente el frontend.
Usarás el código del prototipo de Lovable **solo como referencia visual** — los estilos,
la paleta y la estructura de pantallas. No copies código de ese prototipo; escribe todo desde cero
con las versiones y la arquitectura definidas aquí.

---

## Stack — versiones exactas, no negociables

```
Runtime/PM:     Bun (última versión estable)
Lenguaje:       TypeScript 5 — strict: true siempre
Framework:      React 19
Router:         React Router v7 (modo SPA, sin SSR)
Estilos:        Tailwind CSS v4 (sin tailwind.config — todo en CSS)
Componentes UI: shadcn/ui (compatible con TW v4 y React 19)
Data fetching:  TanStack Query v5
Tablas:         TanStack Table v8
Estado global:  Zustand v5 (SOLO para auth/sesión — nada más)
Formularios:    React Hook Form v7 + Zod v3
Fechas:         date-fns v4
Iconos:         lucide-react (única fuente de iconos)
HTTP:           axios (instancia única con interceptores)
Notificaciones: sonner
```

### Comando de scaffolding inicial (ejecutar una sola vez)

```bash
bun create vite@latest reserva-frontend -- --template react-ts
cd reserva-frontend
bun add react-router-dom @tanstack/react-query @tanstack/react-table \
  zustand react-hook-form zod @hookform/resolvers \
  axios date-fns lucide-react sonner \
  class-variance-authority clsx tailwind-merge
bun add -d tailwindcss @tailwindcss/vite typescript @types/react @types/react-dom
```

### Tailwind v4 — setup en vite.config.ts

```typescript
// vite.config.ts
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import path from "path";

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: { alias: { "@": path.resolve(__dirname, "./src") } },
});
```

### Tailwind v4 — src/index.css (estructura obligatoria)

```css
@import "tailwindcss";
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=Plus+Jakarta+Sans:wght@500;600;700;800&display=swap');

@theme {
  /* Superficies */
  --color-background:           hsl(36 33% 97%);
  --color-surface-card:         hsl(0 0% 100%);
  --color-surface-elevated:     hsl(36 25% 94%);

  /* Brand */
  --color-primary:              hsl(11 47% 52%);
  --color-primary-hover:        hsl(11 47% 46%);
  --color-primary-fg:           hsl(36 33% 97%);
  --color-secondary:            hsl(113 14% 42%);
  --color-secondary-fg:         hsl(36 33% 97%);
  --color-accent:               hsl(36 25% 92%);

  /* Sidebar */
  --color-sidebar:              hsl(240 5% 11%);
  --color-sidebar-fg:           hsl(36 25% 88%);
  --color-sidebar-muted:        hsl(36 10% 60%);
  --color-sidebar-accent:       hsl(240 4% 18%);
  --color-sidebar-accent-fg:    hsl(36 33% 97%);
  --color-sidebar-border:       hsl(240 4% 20%);

  /* Neutrals */
  --color-foreground:           hsl(240 6% 11%);
  --color-muted:                hsl(36 15% 92%);
  --color-muted-fg:             hsl(240 5% 38%);
  --color-border:               hsl(36 18% 88%);
  --color-destructive:          hsl(0 73% 51%);
  --color-destructive-fg:       hsl(0 0% 100%);

  /* Estados */
  --color-status-pending:       hsl(32 95% 44%);
  --color-status-pending-bg:    hsl(32 95% 94%);
  --color-status-confirmed:     hsl(142 71% 36%);
  --color-status-confirmed-bg:  hsl(142 71% 92%);
  --color-status-completed:     hsl(220 9% 46%);
  --color-status-completed-bg:  hsl(220 9% 92%);
  --color-status-cancelled:     hsl(0 73% 51%);
  --color-status-cancelled-bg:  hsl(0 73% 95%);
  --color-status-maintenance:   hsl(21 90% 48%);
  --color-status-maintenance-bg:hsl(21 90% 94%);
  --color-status-active:        hsl(142 71% 36%);
  --color-status-active-bg:     hsl(142 71% 92%);
  --color-status-inactive:      hsl(220 9% 46%);
  --color-status-inactive-bg:   hsl(220 9% 92%);

  /* Tipografía */
  --font-sans:    "Inter", system-ui, sans-serif;
  --font-display: "Plus Jakarta Sans", system-ui, sans-serif;

  /* Radio y sombras */
  --radius:          0.625rem;
  --shadow-card:     0 1px 2px hsl(240 6% 11% / 0.04), 0 4px 12px hsl(240 6% 11% / 0.04);
  --shadow-elevated: 0 4px 16px hsl(240 6% 11% / 0.08), 0 12px 32px hsl(240 6% 11% / 0.06);
  --shadow-cta:      0 4px 14px hsl(11 47% 52% / 0.28);
}

@layer base {
  *, *::before, *::after { box-sizing: border-box; }
  html, body, #root { min-height: 100vh; }
  body {
    background-color: var(--color-background);
    color: var(--color-foreground);
    font-family: var(--font-sans);
    -webkit-font-smoothing: antialiased;
  }
  h1, h2, h3, h4, h5, h6 { font-family: var(--font-display); }
}

@layer components {
  .surface-card {
    background: var(--color-surface-card);
    box-shadow: var(--shadow-card);
    border-radius: var(--radius);
    border: 1px solid color-mix(in srgb, var(--color-border) 60%, transparent);
  }
  .filter-bar {
    background: var(--color-surface-elevated);
    border-top: 1px solid color-mix(in srgb, var(--color-border) 60%, transparent);
    border-bottom: 1px solid color-mix(in srgb, var(--color-border) 60%, transparent);
  }
}
```

---

## Arquitectura — Atomic Design + Capas

Esta arquitectura es **obligatoria**. No mezclar responsabilidades entre capas.

```
src/
├── api/                        ← CAPA DE DATOS: funciones puras, solo axios, sin hooks
│   ├── http.ts                 ← instancia axios + interceptores JWT + manejo 401
│   ├── auth.api.ts
│   ├── clients.api.ts
│   ├── reservations.api.ts
│   ├── tables.api.ts
│   ├── table-types.api.ts
│   ├── pricing-rules.api.ts
│   └── users.api.ts
│
├── store/                      ← ZUSTAND: solo estado de sesión/auth
│   └── auth.store.ts
│
├── hooks/                      ← TANSTACK QUERY: un archivo por entidad
│   ├── query-keys.ts
│   ├── useAuth.ts
│   ├── useClients.ts
│   ├── useReservations.ts
│   ├── useTables.ts
│   ├── useTableTypes.ts
│   ├── usePricingRules.ts
│   └── useUsers.ts
│
├── components/
│   ├── ui/                     ← shadcn/ui PRIMITIVOS — no modificar nunca
│   │
│   ├── atoms/                  ← sin lógica de dominio, sin hooks de query
│   │   ├── StatusBadge.tsx
│   │   ├── RoleBadge.tsx
│   │   ├── PriceSummary.tsx
│   │   ├── DayChips.tsx        ← selector visual días semana (0-6) para reglas de precio
│   │   └── LoadingSpinner.tsx
│   │
│   ├── molecules/              ← combinan átomos, pueden tener estado local simple
│   │   ├── PageHeader.tsx
│   │   ├── DataTable.tsx       ← tabla TanStack Table genérica
│   │   ├── Pagination.tsx
│   │   ├── FilterBar.tsx
│   │   ├── EmptyState.tsx
│   │   ├── ErrorState.tsx
│   │   ├── ConfirmDialog.tsx
│   │   ├── FormField.tsx       ← label + input + error (React Hook Form)
│   │   └── ClientSearchSelect.tsx
│   │
│   └── organisms/              ← lógica de dominio completa, usan hooks
│       ├── AppSidebar.tsx
│       ├── ReservationForm.tsx
│       ├── TableCard.tsx
│       └── PricingRuleRow.tsx
│
├── layouts/
│   ├── AppLayout.tsx           ← sidebar fijo 240px + <Outlet />
│   ├── AuthGuard.tsx           ← redirige a /login si no hay token
│   └── RoleGuard.tsx           ← redirige si el rol no tiene acceso
│
├── pages/                      ← ORQUESTACIÓN PURA — sin lógica, sin estilos inline
│   ├── Login.tsx
│   ├── Disponibilidad.tsx
│   ├── Reservas.tsx
│   ├── CrearReserva.tsx
│   ├── DetalleReserva.tsx
│   ├── Clientes.tsx
│   ├── Mesas.tsx
│   ├── TiposDeMesa.tsx
│   ├── ReglasDePrecio.tsx
│   └── Usuarios.tsx
│
├── types/
│   └── index.ts
│
├── lib/
│   ├── format.ts
│   ├── permissions.ts
│   └── utils.ts                ← cn() para merge de clases
│
├── router/
│   └── index.tsx
│
├── main.tsx
└── index.css
```

### Regla de dependencias — nunca romper esta dirección

```
pages → organisms → molecules → atoms → ui/
pages → hooks → api
pages → store
NUNCA: atoms usa hooks, api importa hooks, ui importa dominio
```

---

## Tipos TypeScript — src/types/index.ts

Crear este archivo primero. Importar siempre desde aquí. No redefinir tipos inline.

```typescript
export type Role = "Admin" | "Manager" | "Employee";
export type ReservationStatus = "Pending" | "Confirmed" | "Completed" | "Cancelled";
export type EntityStatus = "Active" | "Inactive";
export type TableStatus = "Active" | "Inactive" | "Maintenance";
export type PricingRuleType = "Surcharge" | "Discount";

export interface Pagination {
  page: number; pageSize: number; totalCount: number;
  totalPages: number; hasPrevious: boolean; hasNext: boolean;
}

export interface ApiResponse<T> {
  success: boolean; data: T | null; message: string;
  statusCode: number; error: string | null; pagination: Pagination | null;
}

export interface AuthUser {
  id: number; username: string; email: string;
  role: Role; status: EntityStatus; token: string; tokenExpiry: string;
}

export interface TableType {
  id: number; name: string; description?: string;
  basePricePerHour: number; isActive: boolean; tableCount: number; createdAt: string;
}

export interface TableDetailed {
  id: number; code: string; capacity: number;
  location: string; status: TableStatus; tableType: TableType;
}

export interface Client {
  id: number; firstName: string; lastName: string; email: string;
  phone?: string; status: EntityStatus; totalReservations: number; createdAt: string;
}

export interface User {
  id: number; username: string; email: string; role: Role; status: EntityStatus;
}

export interface Reservation {
  id: number; client: Client; table: TableDetailed;
  date: string;       // YYYY-MM-DD
  startTime: string;  // HH:mm:ss
  endTime: string;    // HH:mm:ss
  numberOfGuests: number; basePrice: number; totalPrice: number;
  status: ReservationStatus; notes?: string;
  user: Pick<User, "id" | "username">; createdAt: string;
}

export interface PricingRule {
  id: number; ruleName: string; ruleType: PricingRuleType;
  startTime: string; endTime: string; startDate: string; endDate: string;
  surchargePercentage: number; tableTypeId: number; tableTypeName: string;
  daysOfWeek: number[]; // 0=Dom...6=Sáb
  isActive: boolean;
}

// DTOs
export interface CreateReservationDto {
  clientId: number; tableId: number; date: string;
  startTime: string; endTime: string; numberOfGuests: number; notes?: string;
}
export interface UpdateReservationDto extends Partial<Omit<CreateReservationDto,"clientId">> {
  status?: ReservationStatus;
}
export interface CreateClientDto {
  firstName: string; lastName: string; email: string; phone?: string;
}
export interface CreateTableDto {
  capacity: number; location: string; tableTypeId: number;
}
export interface CreatePricingRuleDto {
  ruleName: string; ruleType: PricingRuleType;
  startTime: string; endTime: string; startDate: string; endDate: string;
  surchargePercentage: number; tableTypeId: number; daysOfWeek: number[];
}
export interface CreateUserDto {
  username: string; email: string; password: string; role?: Role;
}
```

---

## src/api/http.ts — instancia axios (crear antes que todo lo demás)

```typescript
import axios from "axios";
import { useAuthStore } from "@/store/auth.store";

export const http = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? "http://localhost:5000/api/v1",
  headers: { "Content-Type": "application/json" },
});

http.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

http.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().clearSession();
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);
```

---

## src/store/auth.store.ts

```typescript
import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { AuthUser, Role } from "@/types";

interface AuthState {
  user: AuthUser | null; token: string | null; role: Role | null;
  setSession: (user: AuthUser) => void;
  clearSession: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null, token: null, role: null,
      setSession: (user) => set({ user, token: user.token, role: user.role }),
      clearSession: () => set({ user: null, token: null, role: null }),
    }),
    { name: "reserva-auth" }
  )
);
```

---

## Patrón de hooks TanStack Query

### src/hooks/query-keys.ts — centralizar, sin strings sueltos

```typescript
export const qk = {
  clients:      (p?: object) => ["clients", p] as const,
  client:       (id: number) => ["clients", id] as const,
  reservations: (p?: object) => ["reservations", p] as const,
  reservation:  (id: number) => ["reservations", id] as const,
  tables:       (p?: object) => ["tables", p] as const,
  available:    (p: object)  => ["tables", "available", p] as const,
  tableTypes:   ()           => ["table-types"] as const,
  tableType:    (id: number) => ["table-types", id] as const,
  pricingRules: ()           => ["pricing-rules"] as const,
  users:        ()           => ["users"] as const,
};
```

### Patrón de hook por entidad

```typescript
// src/hooks/useClients.ts — replicar para cada entidad
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { clientsApi } from "@/api/clients.api";
import { qk } from "./query-keys";
import { toast } from "sonner";

export const useClientList = (params: Record<string, unknown>) =>
  useQuery({ queryKey: qk.clients(params), queryFn: () => clientsApi.list(params) });

export const useCreateClient = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: clientsApi.create,
    onSuccess: () => { qc.invalidateQueries({ queryKey: qk.clients() }); toast.success("Cliente creado"); },
    onError: (e: Error) => toast.error(e.message),
  });
};
```

---

## src/lib/permissions.ts

```typescript
import type { Role } from "@/types";
export const can = {
  deleteReservation: (r: Role) => r === "Admin" || r === "Manager",
  cancelReservation: (_: Role) => true,
  deleteClient:      (r: Role) => r === "Admin",
  editClient:        (_: Role) => true,
  editTables:        (r: Role) => r === "Admin" || r === "Manager",
  deleteTables:      (r: Role) => r === "Admin",
  editPricingRules:  (r: Role) => r === "Admin" || r === "Manager",
  seeUsers:          (r: Role) => r === "Admin",
  createUser:        (r: Role) => r === "Admin",
};
```

---

## src/lib/format.ts

```typescript
export const formatCurrency = (n: number) =>
  new Intl.NumberFormat("es-ES", { style: "currency", currency: "EUR" }).format(Math.max(0, n));
export const formatDate = (iso: string) =>
  new Intl.DateTimeFormat("es-ES", { day: "2-digit", month: "short", year: "numeric" }).format(new Date(iso));
export const todayISO = () => new Date().toISOString().slice(0, 10);
export const durationHours = (start: string, end: string): number => {
  const [sh, sm] = start.split(":").map(Number);
  const [eh, em] = end.split(":").map(Number);
  return Math.max(0, (eh * 60 + em - (sh * 60 + sm)) / 60);
};
export const dayLabel = (n: number) => ["Dom","Lun","Mar","Mié","Jue","Vie","Sáb"][n] ?? "";
```

---

## API Backend — endpoints completos

Base URL: `VITE_API_URL` en `.env.local`
Auth: `Authorization: Bearer {token}` en todo excepto `/auth/login`

```
POST   /auth/login              body: { email, password }
POST   /auth/register           body: { username, email, password, role? }

GET    /clients                 ?firstName &lastName &email &phone &page &pageSize
GET    /clients/{id}
POST   /clients                 body: CreateClientDto
PATCH  /clients/{id}
DELETE /clients/{id}            → solo Admin

GET    /reservations            ?clientId &tableId &status &date &startTime &endTime &page &pageSize
GET    /reservations/{id}
POST   /reservations            → 201 + ReservationDto con basePrice y totalPrice calculados
PATCH  /reservations/{id}       → 200 + ReservationDto con precios recalculados
DELETE /reservations/{id}

GET    /tables                  ?code &capacity &location &status &page &pageSize
GET    /tables/available        ?date(YYYY-MM-DD) &startTime(HH:mm:ss) &endTime(HH:mm:ss) &numberOfGuests
GET    /tables/{id}
POST   /tables                  body: CreateTableDto
PATCH  /tables/{id}             body: { capacity?, location?, status? }  ← tableTypeId IGNORADO
DELETE /tables/{id}             → solo Admin

GET    /table-types
GET    /table-types/{id}
POST   /table-types             body: { name, basePricePerHour, description? }
PATCH  /table-types/{id}
DELETE /table-types/{id}        → solo Admin

GET    /pricing-rules
GET    /pricing-rules/{id}
POST   /pricing-rules           body: CreatePricingRuleDto
PATCH  /pricing-rules/{id}
DELETE /pricing-rules/{id}

GET    /users
GET    /users/{id}
DELETE /users/{id}
```

### Notas críticas

- No existe endpoint de cotización previa de precio — frontend calcula `basePricePerHour × horas` como estimado; precio real viene en response POST/PATCH
- Error 401 → clearSession() + redirect `/login`
- Error 400 email duplicado → mostrar inline en campo, no en toast
- Filtro de listado de reservas por defecto: `date = todayISO()` (frontend, no API)

---

## Reglas de negocio

### Reservas
- Duración mínima 30 minutos — validar antes de submit con Zod
- No permitir fechas pasadas en el date input (atributo `min={todayISO()}`)
- `numberOfGuests` ≤ `table.capacity` — validar en Zod con `.refine()`
- Cliente, mesa y tipo de mesa deben estar activos
- Transiciones de estado:
  - `Pending` → `Confirmed` | `Cancelled`
  - `Confirmed` → `Cancelled` | `Completed`
  - `Completed` | `Cancelled` → ninguna (bloquear toda edición con banner visual)
- No hay quote endpoint — mostrar `<PriceSummary pendingNote />` antes de confirmar

### Precios
- Nunca mostrar negativos → `Math.max(0, n)` siempre
- Siempre `formatCurrency()` — nunca hardcodear `€` en JSX

---

## Design System — uso en JSX

```tsx
// Superficies
<div className="surface-card p-5 hover:shadow-elevated transition-shadow" />
<div className="filter-bar px-8 py-4" />

// Layout
<aside className="w-[240px] fixed inset-y-0 bg-sidebar text-sidebar-fg flex flex-col" />
<main className="ml-[240px] min-h-screen bg-background" />

// Tipografía
<h1 className="font-display text-3xl font-bold tracking-tight" />
<p className="text-sm text-muted-fg" />

// Color de marca
<span className="text-primary" />
<div className="bg-primary text-primary-fg" />

// Estados — SOLO a través de StatusBadge, nunca clases raw de estado en páginas
<StatusBadge status="Confirmed" />
<StatusBadge status="Maintenance" />
```

---

## Orden de construcción (seguir estrictamente)

```
1.  Scaffolding + dependencias (bun create vite + bun add)
2.  vite.config.ts con @tailwindcss/vite
3.  src/index.css — @theme completo
4.  src/types/index.ts
5.  src/lib/format.ts + permissions.ts + utils.ts
6.  src/api/http.ts
7.  src/api/*.api.ts — todas las entidades
8.  src/store/auth.store.ts
9.  src/hooks/query-keys.ts + use*.ts
10. shadcn/ui init + src/components/ui/
11. src/components/atoms/ — StatusBadge, RoleBadge, PriceSummary, DayChips
12. src/components/molecules/ — PageHeader, DataTable, Pagination, EmptyState, ConfirmDialog, FormField
13. src/components/organisms/AppSidebar.tsx
14. src/layouts/AppLayout.tsx + AuthGuard + RoleGuard
15. src/router/index.tsx + main.tsx
16. src/pages/Login.tsx
17. src/pages/Disponibilidad.tsx  ← primera pantalla funcional
18. src/pages/Reservas.tsx + CrearReserva.tsx + DetalleReserva.tsx
19. src/pages/Clientes.tsx
20. src/pages/Mesas.tsx + TiposDeMesa.tsx
21. src/pages/ReglasDePrecio.tsx
22. src/pages/Usuarios.tsx
```

---

## Convenciones estrictas

1. TypeScript strict — sin `any`, tipar todo explícitamente
2. Imports siempre con alias `@/` — nunca rutas relativas `../../`
3. Un componente por archivo, nombre = nombre del componente
4. Exports nombrados en componentes; default exports solo en páginas (lazy loading)
5. Todo texto visible en español (es-ES)
6. Iconos únicamente de `lucide-react`
7. Colores solo con tokens CSS — nunca `red-500`, `#hex`, `rgb()` directos
8. `formatCurrency()` para todo monto monetario
9. Páginas = orquestación pura — sin fetch directo, sin estilos inline
10. Atoms = sin hooks de query ni efectos de red
11. `sonner` para todos los toasts (éxito y error de mutaciones)
12. `ConfirmDialog` obligatorio antes de acciones destructivas
13. Skeleton mientras carga / EmptyState si vacío / ErrorState si falla

---

## Gaps conocidos — no inventar endpoints

| Gap | Qué hacer |
|---|---|
| Notificación por correo tras reserva | Badge estático "Notificación: pendiente de implementar" |
| Recuperación de contraseña | Texto "Contacta al administrador" en Login |
| Edición de perfil propio | Label "Próximamente" |
| Refresh token | Al expirar tokenExpiry → clearSession() + redirect /login |
| Analytics / métricas | No incluir |
| Imágenes de mesas | No incluir |

---

## Cómo usar este archivo en cada sesión con Qwen

Incluir siempre al inicio del prompt:

```
Lee CONTEXT.md completo antes de empezar.

Tarea: [descripción específica]
Archivos a crear o modificar: [lista]
Restricciones adicionales: [si aplican]
```

Qwen no tiene memoria entre sesiones. CONTEXT.md es su memoria permanente.
