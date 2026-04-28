import { Outlet } from "react-router-dom"
import { AppSidebar } from "@/components/organisms/AppSidebar"

export function AppLayout() {
  return (
    <div className="flex min-h-screen bg-background">
      <AppSidebar />
      <main className="ml-60 flex-1 min-h-screen">
        <Outlet />
      </main>
    </div>
  )
}
