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

## Roles y permisos (RBAC)
- **Admin**: acceso total. CRUD de usuarios, clientes, mesas, tipos de mesa, reglas de precio y reservas; puede desactivar/eliminar recursos.
- **Manager**: operación completa salvo usuarios. CRUD de clientes, mesas y tipos; crear/actualizar/desactivar reglas de precio; CRUD de reservas.
- **Employee**: operación diaria. Puede crear/actualizar clientes y crear/actualizar/cancelar reservas; consulta catálogos (clientes, mesas, tipos, reglas). No puede crear/editar/borrar usuarios, reglas de precio, mesas ni tipos; no elimina clientes.

## Configuración de variables de entorno (JWT y DB)
No se versionan credenciales. Define antes de ejecutar:
- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`

Se carga `.env` automáticamente con DotNetEnv (ver `.env.example`). `ExpiryInMinutes` puede mantenerse en appsettings o sobreescribirse con `Jwt__ExpiryInMinutes`.

## Cómo probar rápido (backend)
```bash
cd backend/API
dotnet restore
dotnet ef database update
dotnet run
```

Configura `ConnectionStrings` y `JwtSettings` en `appsettings.Development.json` antes de ejecutar.
