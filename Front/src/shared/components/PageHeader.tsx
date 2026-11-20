import type { ReactNode } from 'react'
import { Link } from 'react-router'

interface Crumb {
  label: string
  to?: string
}

/** Migas de pan + título + subtítulo + acciones a la derecha. Lo usan todas las páginas. */
export function PageHeader({
  title,
  description,
  breadcrumbs,
  actions,
}: {
  title: ReactNode
  description?: ReactNode
  breadcrumbs?: Crumb[]
  actions?: ReactNode
}) {
  return (
    <header className="mb-6 space-y-1">
      {breadcrumbs && (
        <nav className="text-sm text-muted-foreground" aria-label="Migas de pan">
          {breadcrumbs.map((crumb, i) => (
            <span key={crumb.label}>
              {i > 0 && <span className="mx-1.5">/</span>}
              {crumb.to ? (
                <Link to={crumb.to} className="hover:text-foreground">
                  {crumb.label}
                </Link>
              ) : (
                <span className="text-foreground">{crumb.label}</span>
              )}
            </span>
          ))}
        </nav>
      )}
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">{title}</h1>
          {description && <p className="mt-1 text-muted-foreground">{description}</p>}
        </div>
        {actions && <div className="flex items-center gap-2">{actions}</div>}
      </div>
    </header>
  )
}
