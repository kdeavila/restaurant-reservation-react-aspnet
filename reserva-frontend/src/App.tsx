import { useEffect } from "react";
import { AppSidebar } from "@/components/organisms/AppSidebar";
import { useAuthStore } from "@/store/auth.store";
import type { AuthUser } from "@/types";

function App() {
  const { user, setSession } = useAuthStore();

  useEffect(() => {
    if (user) return;

    const previewUser: AuthUser = {
      id: 1,
      username: "Marco Rossi",
      email: "marco@ristorante.local",
      role: "Manager",
      status: "Active",
      token: "preview-token",
      tokenExpiry: new Date(Date.now() + 1000 * 60 * 60).toISOString(),
    };

    setSession(previewUser);
  }, [user, setSession]);

  return (
    <div className="min-h-screen bg-background text-foreground">
      <AppSidebar />
      <main className="ml-60">
        <section className="px-8 py-10">
          <h1 className="font-display text-3xl font-bold">Vista previa Sidebar</h1>
          <p className="mt-2 text-muted-fg">
            Componente AppSidebar integrado en App.tsx para revisar look and feel.
          </p>
        </section>
      </main>
    </div>
  );
}

export default App;
