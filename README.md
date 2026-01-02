# Restaurant Reservation System

Sistema web para gestión de reservas de restaurante. Backend en **ASP.NET Core 9 Web API**; frontend en **React** (pendiente). Solo el personal autorizado (Admin, Manager, Employee) puede gestionar clientes, mesas, reglas de precio y reservas con cálculo automático y control de disponibilidad.

**Última actualización:** 2026-01-01

## Qué ofrece
- CRUD de clientes, mesas, tipos de mesa, usuarios y reglas de precio.
- Reservas con validaciones de capacidad y no solapamiento; cálculo dinámico de precios.
- Autenticación y autorización por roles; documentación Swagger en desarrollo.
- Requisito real: notificaciones por correo al crear/modificar reservas (pendiente de implementar).

## Stack
- Backend: ASP.NET Core 9, Entity Framework Core, SQL Server, JWT Bearer, Clean Architecture.
- Frontend: React + Tailwind (a implementar).

## Estado
- Backend: en uso, faltan notificaciones, versión API y ajustes de CORS/Identity.
- Frontend: pendiente.

## Cómo probar rápido (backend)
```bash
cd backend/API
dotnet restore
dotnet ef database update
dotnet run
```

Configura `ConnectionStrings` y `JwtSettings` en `appsettings.Development.json` antes de ejecutar.
