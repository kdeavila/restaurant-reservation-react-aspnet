# Restaurant Reservation System

Sistema de gestión de reservas para restaurantes construido con **ASP.NET Core 9** (backend) y **React** (frontend). Diseñado para que administradores y personal del restaurante gestionen reservas, clientes y disponibilidad de mesas.

## Descripción

Sistema que permite al personal autorizado del restaurante:
- Gestionar información de clientes y su historial de reservas
- Administrar mesas con diferentes tipos y precios dinámicos
- Crear, modificar y cancelar reservas con cálculo automático de precios
- Control de acceso basado en roles (Admin, Manager, Employee)

> Solo el personal autorizado puede realizar operaciones. Los clientes no pueden hacer reservas directamente.

## Tech Stack

### Backend
- **Framework**: ASP.NET Core 9 Web API
- **ORM**: Entity Framework Core
- **Database**: SQL Server
- **Authentication**: JWT Bearer tokens
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, API)

### Frontend (Planned)
- **Framework**: React
- **Styling**: Tailwind CSS

## Estructura del Proyecto

```
restaurant-reservation-react-aspnet/
├── backend/
│   └── src/
│       ├── RestaurantReservation.API/           # Controllers, Middleware
│       ├── RestaurantReservation.Application/   # Services, DTOs, UseCases
│       ├── RestaurantReservation.Domain/        # Entities, Enums
│       └── RestaurantReservation.Infrastructure/# DbContext, Repositories
├── frontend/                                    # React app (planned)
└── docs/
    └── erd_diagram.png
```

## Database Schema

![ERD Diagram](docs/erd_diagram.png)

### Entidades Principales

| Entity | Descripción |
|--------|-------------|
| **User** | Usuarios del sistema con roles (Admin, Manager, Employee) |
| **Client** | Clientes del restaurante con historial de reservas |
| **TableType** | Tipos de mesa con precio base por hora |
| **Table** | Mesas físicas con capacidad y estado |
| **Reservation** | Reservas con precios calculados dinámicamente |
| **PricingRule** | Reglas de precios (recargos por día/hora) |

### Enums

- **UserRole**: Admin, Manager, Employee
- **UserStatus**: Active, Inactive
- **ClientStatus**: Active, Inactive
- **TableStatus**: Active, Maintenance, Inactive
- **ReservationStatus**: Pending, Confirmed, Cancelled, Completed

## API Endpoints

> Todos los endpoints (excepto Auth) requieren autenticación JWT.

### Authentication (Público)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Registrar usuario |
| POST | `/api/auth/login` | Iniciar sesión |

### Users (Solo Admin)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/users` | Listar usuarios (con paginación) |
| GET | `/api/users/{id}` | Obtener usuario por ID |
| DELETE | `/api/users/{id}` | Desactivar usuario |

### Clients
| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/clients` | Listar clientes | Admin, Manager, Employee |
| GET | `/api/clients/{id}` | Obtener cliente por ID | Admin, Manager, Employee |
| POST | `/api/clients` | Crear cliente | Admin, Manager, Employee |
| PATCH | `/api/clients/{id}` | Actualizar cliente | Admin, Manager, Employee |
| DELETE | `/api/clients/{id}` | Desactivar cliente | Admin, Manager |

### Tables
| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/tables` | Listar mesas | Admin, Manager, Employee |
| GET | `/api/tables/{id}` | Obtener mesa por ID | Admin, Manager, Employee |
| POST | `/api/tables` | Crear mesa | Admin, Manager |
| PATCH | `/api/tables/{id}` | Actualizar mesa | Admin, Manager |
| DELETE | `/api/tables/{id}` | Desactivar mesa | Admin |

### Table Types
| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/table-types` | Listar tipos de mesa | Admin, Manager, Employee |
| GET | `/api/table-types/{id}` | Obtener tipo por ID | Admin, Manager, Employee |
| POST | `/api/table-types` | Crear tipo de mesa | Admin |
| PATCH | `/api/table-types/{id}` | Actualizar tipo | Admin |
| DELETE | `/api/table-types/{id}` | Desactivar tipo | Admin |

### Reservations
| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/reservations` | Listar reservas | Admin, Manager, Employee |
| GET | `/api/reservations/{id}` | Obtener reserva por ID | Admin, Manager, Employee |
| POST | `/api/reservations` | Crear reserva | Admin, Manager, Employee |
| PATCH | `/api/reservations/{id}` | Actualizar reserva | Admin, Manager, Employee |
| DELETE | `/api/reservations/{id}` | Cancelar reserva | Admin, Manager, Employee |

### Pricing Rules
| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/pricing-rules` | Listar reglas de precios | Admin, Manager, Employee |
| GET | `/api/pricing-rules/{id}` | Obtener regla por ID | Admin, Manager, Employee |
| POST | `/api/pricing-rules` | Crear regla | Admin, Manager |
| PATCH | `/api/pricing-rules/{id}` | Actualizar regla | Admin, Manager |
| DELETE | `/api/pricing-rules/{id}` | Desactivar regla | Admin |

## Reglas de Negocio

1. **Disponibilidad**: No se permiten reservas superpuestas para la misma mesa
2. **Precios Dinámicos**: Calculados según tipo de mesa, duración y reglas de pricing
3. **Validaciones**:
   - Email de cliente único
   - Username de usuario único
   - Reserva mínima de 30 minutos
   - Número de invitados no puede exceder capacidad de mesa
4. **Soft Delete**: Clientes y mesas se desactivan, no se eliminan físicamente

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server
- Node.js 18+ (para frontend)

### Backend Setup

```bash
cd backend/src/RestaurantReservation.API

# Restaurar dependencias
dotnet restore

# Aplicar migraciones
dotnet ef database update

# Ejecutar
dotnet run
```

### Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RestaurantReservationDB;Trusted_Connection=true;"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-chars",
    "Issuer": "RestaurantReservationAPI",
    "Audience": "RestaurantReservationClient",
    "ExpirationMinutes": 60
  }
}
```

## Development Status

### Completado
- [x] Clean Architecture setup
- [x] Entity Framework Core con SQL Server
- [x] CRUD completo para todas las entidades
- [x] Sistema de autenticación JWT
- [x] Autorización por roles en endpoints
- [x] Paginación y filtros en endpoints
- [x] Cálculo dinámico de precios
- [x] Validaciones de negocio
- [x] Respuestas API estandarizadas

### Pendiente
- [ ] Frontend React
- [ ] Notificaciones por email
- [ ] Tests unitarios e integración

---

**Developer**: Keyner  
**Last updated**: 2025-12-02
