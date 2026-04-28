import { zodResolver } from "@hookform/resolvers/zod"
import { Eye, EyeOff, UtensilsCrossed } from "lucide-react"
import { useState } from "react"
import { useForm } from "react-hook-form"
import { useNavigate } from "react-router-dom"
import { z } from "zod"
import { FormField } from "@/components/molecules/FormField"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { useLogin } from "@/hooks/useAuth"

const loginSchema = z.object({
  email: z.string().email("Correo electrónico inválido"),
  password: z.string().min(1, "La contraseña es requerida"),
})

type LoginFormValues = z.infer<typeof loginSchema>

export default function Login() {
  const navigate = useNavigate()
  const loginMutation = useLogin()
  const [showPassword, setShowPassword] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  })

  const onSubmit = async (values: LoginFormValues) => {
    setFormError(null)

    try {
      await loginMutation.mutateAsync(values)
      navigate("/disponibilidad")
    } catch {
      setFormError("Credenciales inválidas. Verifica tu correo y contraseña.")
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-4 py-10">
      <section className="surface-card w-full max-w-105 p-8 sm:p-10">
        <div className="flex flex-col items-center text-center">
          <div className="flex size-14 items-center justify-center rounded-full bg-primary text-primary-fg shadow-cta">
            <UtensilsCrossed className="size-6" />
          </div>
          <h1 className="mt-5 text-3xl font-bold tracking-tight">Ristorante</h1>
          <p className="mt-1 text-sm text-muted-fg">Sistema de Reservas</p>
        </div>

        <form className="mt-8 space-y-5" onSubmit={handleSubmit(onSubmit)} noValidate>
          <FormField label="Correo electrónico" error={errors.email?.message} required>
            <Input
              type="email"
              autoComplete="email"
              aria-invalid={Boolean(errors.email)}
              placeholder="correo@empresa.com"
              {...register("email")}
            />
          </FormField>

          <FormField label="Contraseña" error={errors.password?.message} required>
            <div className="relative">
              <Input
                type={showPassword ? "text" : "password"}
                autoComplete="current-password"
                aria-invalid={Boolean(errors.password)}
                placeholder="••••••••"
                className="pr-10"
                {...register("password")}
              />
              <button
                type="button"
                onClick={() => setShowPassword((current) => !current)}
                className="absolute inset-y-0 right-0 flex items-center px-3 text-muted-fg transition-colors hover:text-foreground"
                aria-label={showPassword ? "Ocultar contraseña" : "Mostrar contraseña"}
              >
                {showPassword ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
              </button>
            </div>
          </FormField>

          <Button type="submit" size="lg" className="w-full" disabled={loginMutation.isPending}>
            {loginMutation.isPending ? "Iniciando sesión..." : "Iniciar sesión"}
          </Button>

          {formError && <p className="text-sm text-destructive">{formError}</p>}
        </form>

        <p className="mt-6 text-center text-sm text-muted-fg">
          ¿Nuevo miembro? Contacta al administrador
        </p>
      </section>
    </main>
  )
}
