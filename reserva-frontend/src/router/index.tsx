import { lazy, Suspense, type ReactNode } from "react";
import { createBrowserRouter, Navigate } from "react-router-dom";
import { AppLayout } from "@/layouts/AppLayout";
import { AuthGuard } from "@/layouts/AuthGuard";
import { RoleGuard } from "@/layouts/RoleGuard";
import { LoadingSpinner } from "@/components/atoms/LoadingSpinner";

const LoginPage = lazy(() => import("@/pages/Login"));
const DisponibilidadPage = lazy(() => import("@/pages/Disponibilidad"));
const ReservasPage = lazy(() => import("@/pages/Reservas"));
const CrearReservaPage = lazy(() => import("@/pages/CrearReserva"));
const DetalleReservaPage = lazy(() => import("@/pages/DetalleReserva"));
const ClientesPage = lazy(() => import("@/pages/Clientes"));
const MesasPage = lazy(() => import("@/pages/Mesas"));
const TiposDeMesaPage = lazy(() => import("@/pages/TiposDeMesa"));
const ReglasDePrecioPage = lazy(() => import("@/pages/ReglasDePrecio"));
const UsuariosPage = lazy(() => import("@/pages/Usuarios"));
const NotFoundPage = lazy(() => import("@/pages/NotFound"));

function LazyPage({ children }: { children: ReactNode }) {
  return <Suspense fallback={<LoadingSpinner />}>{children}</Suspense>;
}

export const router = createBrowserRouter([
  {
    path: "/",
    element: <Navigate to="/disponibilidad" replace />,
  },
  {
    path: "/login",
    element: (
      <LazyPage>
        <LoginPage />
      </LazyPage>
    ),
  },
  {
    element: (
      <AuthGuard>
        <AppLayout />
      </AuthGuard>
    ),
    children: [
      {
        path: "/disponibilidad",
        element: (
          <LazyPage>
            <DisponibilidadPage />
          </LazyPage>
        ),
      },
      {
        path: "/reservas",
        element: (
          <LazyPage>
            <ReservasPage />
          </LazyPage>
        ),
      },
      {
        path: "/reservas/nueva",
        element: (
          <LazyPage>
            <CrearReservaPage />
          </LazyPage>
        ),
      },
      {
        path: "/reservas/:id",
        element: (
          <LazyPage>
            <DetalleReservaPage />
          </LazyPage>
        ),
      },
      {
        path: "/clientes",
        element: (
          <LazyPage>
            <ClientesPage />
          </LazyPage>
        ),
      },
      {
        path: "/mesas",
        element: (
          <LazyPage>
            <MesasPage />
          </LazyPage>
        ),
      },
      {
        path: "/tipos-de-mesa",
        element: (
          <LazyPage>
            <TiposDeMesaPage />
          </LazyPage>
        ),
      },
      {
        path: "/reglas-de-precio",
        element: (
          <LazyPage>
            <ReglasDePrecioPage />
          </LazyPage>
        ),
      },
      {
        path: "/usuarios",
        element: (
          <RoleGuard allowed={["Admin"]}>
            <LazyPage>
              <UsuariosPage />
            </LazyPage>
          </RoleGuard>
        ),
      },
    ],
  },
  {
    path: "*",
    element: (
      <LazyPage>
        <NotFoundPage />
      </LazyPage>
    ),
  },
]);
