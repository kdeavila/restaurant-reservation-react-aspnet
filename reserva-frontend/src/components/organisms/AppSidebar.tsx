import {
  Armchair,
  CalendarRange,
  ClipboardList,
  LogOut,
  Percent,
  ShieldCheck,
  Tag,
  Users,
  UtensilsCrossed,
} from "lucide-react"
import { useLocation, useNavigate } from "react-router-dom"
import { RoleBadge } from "@/components/atoms/RoleBadge"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { Button } from "@/components/ui/button"
import { can } from "@/lib/permissions"
import { cn } from "@/lib/utils"
import { useAuthStore } from "@/store/auth.store"

interface NavItem {
  path: string
  label: string
  icon: React.ReactNode
  show?: boolean
}

export function AppSidebar() {
  const navigate = useNavigate()
  const location = useLocation()
  const { user, role, clearSession } = useAuthStore()

  if (!user || !role) return null

  const initials = user.username
    .split(" ")
    .slice(0, 2)
    .map((n) => n[0])
    .join("")
    .toUpperCase()
    .slice(0, 2)

  const navItems: NavItem[] = [
    {
      path: "/disponibilidad",
      label: "Disponibilidad",
      icon: <CalendarRange className="w-5 h-5" />,
    },
    {
      path: "/reservas",
      label: "Reservas",
      icon: <ClipboardList className="w-5 h-5" />,
    },
    {
      path: "/clientes",
      label: "Clientes",
      icon: <Users className="w-5 h-5" />,
    },
    {
      path: "/mesas",
      label: "Mesas",
      icon: <Armchair className="w-5 h-5" />,
    },
    {
      path: "/tipos-de-mesa",
      label: "Tipos de Mesa",
      icon: <Tag className="w-5 h-5" />,
    },
    {
      path: "/reglas-de-precio",
      label: "Reglas de Precio",
      icon: <Percent className="w-5 h-5" />,
    },
    {
      path: "/usuarios",
      label: "Usuarios",
      icon: <ShieldCheck className="w-5 h-5" />,
      show: can.seeUsers(role),
    },
  ]

  const handleLogout = () => {
    clearSession()
    navigate("/login")
  }

  return (
    <aside className="w-60 fixed inset-y-0 left-0 bg-sidebar text-sidebar-fg flex flex-col">
      {/* Brand */}
      <div className="p-6 space-y-1 border-b border-sidebar-border">
        <div className="flex items-center gap-3">
          <UtensilsCrossed className="w-6 h-6 text-primary" />
          <div>
            <div className="font-display font-bold text-lg">Ristorante</div>
            <div className="text-xs uppercase tracking-widest text-sidebar-muted">Reservas</div>
          </div>
        </div>
      </div>

      {/* User Card */}
      <div className="p-4 border-b border-sidebar-border">
        <div className="flex items-center gap-3">
          <Avatar className="w-10 h-10 bg-sidebar-accent text-sidebar-accent-fg">
            <AvatarFallback>{initials}</AvatarFallback>
          </Avatar>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-semibold truncate">{user.username}</p>
            <RoleBadge
              role={role}
              className="mt-1 text-xs px-2 py-0.5 bg-sidebar-accent/50 text-sidebar-fg"
            />
          </div>
        </div>
      </div>

      {/* Nav */}
      <nav className="flex-1 px-4 py-6 space-y-2 overflow-y-auto">
        {navItems
          .filter((item) => item.show !== false)
          .map((item) => (
            <NavLink
              key={item.path}
              {...item}
              isActive={location.pathname === item.path}
              onNavigate={navigate}
            />
          ))}
      </nav>

      {/* Footer */}
      <div className="p-4 border-t border-sidebar-border">
        <Button
          onClick={handleLogout}
          variant="outline"
          size="sm"
          className="w-full justify-start gap-2 bg-sidebar-accent/20 border-sidebar-accent text-sidebar-fg hover:bg-sidebar-accent/40"
        >
          <LogOut className="w-4 h-4" />
          Cerrar sesión
        </Button>
      </div>
    </aside>
  )
}

interface NavLinkProps {
  path: string
  label: string
  icon: React.ReactNode
  isActive: boolean
  onNavigate: (path: string) => void
}

function NavLink({ path, label, icon, isActive, onNavigate }: NavLinkProps) {
  return (
    <button
      type="button"
      onClick={() => onNavigate(path)}
      className={cn(
        "flex items-center gap-3 px-4 py-2.5 rounded-md text-sm font-medium transition-colors w-full",
        "hover:bg-sidebar-accent/50 text-sidebar-fg",
        isActive && "bg-sidebar-accent text-sidebar-accent-fg",
      )}
    >
      {icon}
      <span>{label}</span>
    </button>
  )
}
