import { useState, type FormEvent } from 'react'
import { NavLink, Outlet, useNavigate } from 'react-router'
import { CompassIcon, LayoutGridIcon, PackageIcon, SearchIcon, TruckIcon, ArchiveIcon } from 'lucide-react'
import { Input } from '@/components/ui/input'
import { cn } from '@/lib/utils'
import { normalizeTrackingNumber } from '@/shared/lib/format'

const NAV_ITEMS = [
  { to: '/', label: 'Dashboard', icon: LayoutGridIcon, end: true },
  { to: '/envios', label: 'Envíos', icon: TruckIcon, end: false },
  { to: '/rastreo', label: 'Rastreo', icon: CompassIcon, end: false },
  { to: '/entregas', label: 'Entregas', icon: ArchiveIcon, end: false },
]

/**
 * Estructura que comparten todas las páginas: barra lateral + barra superior.
 * <Outlet /> es el "hueco" donde React Router pinta la página de la ruta actual.
 */
export function AppLayout() {
  return (
    <div className="flex min-h-svh">
      <Sidebar />
      <div className="flex min-w-0 flex-1 flex-col">
        <Topbar />
        <main className="mx-auto w-full max-w-[1600px] flex-1 px-8 py-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}

function Sidebar() {
  return (
    <aside className="sticky top-0 flex h-svh w-60 shrink-0 flex-col border-r bg-sidebar">
      <div className="flex h-16 items-center gap-2.5 border-b px-5">
        <div className="flex size-9 items-center justify-center rounded-lg bg-primary text-primary-foreground">
          <PackageIcon className="size-5" />
        </div>
        <div className="leading-tight">
          <div className="font-bold">ParcelFlow</div>
          <div className="text-[11px] font-semibold tracking-wider text-muted-foreground uppercase">Logística MX</div>
        </div>
      </div>

      <nav className="flex flex-col gap-1 p-3">
        {NAV_ITEMS.map(({ to, label, icon: Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                isActive
                  ? 'bg-sidebar-accent text-sidebar-accent-foreground'
                  : 'text-muted-foreground hover:bg-muted hover:text-foreground',
              )
            }
          >
            <Icon className="size-4" />
            {label}
          </NavLink>
        ))}
      </nav>
    </aside>
  )
}

function Topbar() {
  const navigate = useNavigate()
  const [query, setQuery] = useState('')

  // Buscador global: escribir una guía y Enter lleva directo al detalle del envío.
  function onSubmit(event: FormEvent) {
    event.preventDefault()
    const trackingNumber = normalizeTrackingNumber(query)
    if (!trackingNumber) return
    navigate(`/envios/${trackingNumber}`)
    setQuery('')
  }

  return (
    <header className="sticky top-0 z-10 flex h-16 items-center justify-between gap-6 border-b bg-white/90 px-8 backdrop-blur">
      <form onSubmit={onSubmit} className="relative w-full max-w-md" role="search">
        <SearchIcon className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Buscar por número de guía…"
          className="h-9 bg-muted/50 pl-9"
          aria-label="Buscar por número de guía"
        />
      </form>

      <div className="flex items-center gap-3">
        <div className="flex size-9 items-center justify-center rounded-full bg-secondary text-sm font-semibold text-secondary-foreground">
          OP
        </div>
        <div className="text-sm leading-tight">
          <div className="font-semibold">Operador</div>
          <div className="text-muted-foreground">ParcelFlow</div>
        </div>
      </div>
    </header>
  )
}
