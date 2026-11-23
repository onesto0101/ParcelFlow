import type { ReactNode } from 'react'

/** Bloque centrado para "no hay resultados" / "no se pudo cargar". */
export function EmptyState({ icon, title, children }: { icon: ReactNode; title: string; children?: ReactNode }) {
  return (
    <div className="flex flex-col items-center gap-3 px-6 py-14 text-center">
      <div className="flex size-12 items-center justify-center rounded-full bg-muted text-muted-foreground">{icon}</div>
      <h2 className="text-lg font-semibold">{title}</h2>
      {children && <div className="max-w-md text-muted-foreground">{children}</div>}
    </div>
  )
}
