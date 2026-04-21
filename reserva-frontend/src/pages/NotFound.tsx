import { Link } from "react-router-dom";

export default function NotFound() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-3 p-8 text-center">
      <p className="text-2xl font-semibold">Página no encontrada</p>
      <Link to="/disponibilidad" className="text-sm underline underline-offset-4">
        Volver a disponibilidad
      </Link>
    </main>
  );
}
