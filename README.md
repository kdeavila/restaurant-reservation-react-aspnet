# Restaurant Reservation System

Sistema web para gestión de reservas de restaurante. Backend en **ASP.NET Core 9 Web API**; frontend en **React** (pendiente). Solo el personal autorizado (Admin, Manager, Employee) puede gestionar clientes, mesas, reglas de precio y reservas con cálculo automático y control de disponibilidad.

**Última actualización:** 2026-01-07

## Qué ofrece
Implementa los 6 requisitos funcionales especificados en `docs/restaurant-reservation.md`:
1. ✅ **Registro de Clientes**: CRUD completo con validación de email único e historial de reservas.
2. ✅ **Gestión de Reservas**: Crear, modificar y cancelar reservas con validación de disponibilidad.
3. ✅ **Consulta de Disponibilidad**: Endpoint para verificar mesas disponibles en tiempo real.
4. ✅ **Gestión por Administradores**: Control total de reservas por parte del personal.
5. 🔮 **Notificaciones de Confirmación**: Envío de correos automáticos (pendiente de implementar).
6. ✅ **Historial de Reservas**: Consulta de reservas pasadas por cliente.

Además incluye:
- CRUD de mesas, tipos de mesa, usuarios y reglas de precio dinámicas.
- Autenticación JWT y autorización por roles (Admin, Manager, Employee).
- Validación de no solapamiento de reservas y capacidad de mesas.
- Documentación Swagger en desarrollo.

## Stack
- Backend: ASP.NET Core 9, Entity Framework Core, PostgreSQL, JWT Bearer, Clean Architecture.
- Frontend: React + Tailwind (a implementar).

## Estado
- **Backend**: 5 de 6 requisitos funcionales implementados. Falta: notificaciones por correo.
- **Testing**: Setup básico configurado (fixtures, factory, contenedores de prueba).
- **Frontend**: Pendiente de desarrollo.

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
