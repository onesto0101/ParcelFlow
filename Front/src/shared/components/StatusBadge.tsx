import { cn } from '@/lib/utils'

/**
 * Badge de estado con los colores del DESIGN.md. Recibe el valor TAL CUAL lo manda la API
 * ("Registered", "InTransit"...) y lo traduce. Incluye ya los estados de Tracking y Deliveries
 * para cuando existan esas APIs.
 */
const STATUS_STYLES: Record<string, { label: string; className: string }> = {
  // Shipments
  Registered: { label: 'Registrado', className: 'bg-slate-100 text-slate-700' },
  Delivered: { label: 'Entregado', className: 'bg-emerald-100 text-emerald-700' },
  Cancelled: { label: 'Cancelado', className: 'bg-slate-200 text-slate-600 line-through' },
  // Tracking
  Created: { label: 'Registrado', className: 'bg-slate-100 text-slate-700' },
  PickedUp: { label: 'Recolectado', className: 'bg-sky-100 text-sky-700' },
  InTransit: { label: 'En tránsito', className: 'bg-blue-100 text-blue-700' },
  AtDistributionCenter: { label: 'En centro de distribución', className: 'bg-purple-100 text-purple-700' },
  OutForDelivery: { label: 'En reparto', className: 'bg-amber-100 text-amber-700' },
  DeliveryFailed: { label: 'Entrega fallida', className: 'bg-rose-100 text-rose-700' },
  // Deliveries
  Pending: { label: 'Pendiente', className: 'bg-yellow-100 text-yellow-800' },
  Assigned: { label: 'Asignada', className: 'bg-indigo-100 text-indigo-700' },
  Failed: { label: 'Fallida', className: 'bg-rose-100 text-rose-700' },
}

export function StatusBadge({ status, className }: { status: string; className?: string }) {
  const style = STATUS_STYLES[status] ?? { label: status, className: 'bg-slate-100 text-slate-700' }

  return (
    <span
      className={cn(
        'inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-[11px] font-semibold tracking-wide',
        style.className,
        className,
      )}
    >
      <span className="size-1.5 rounded-full bg-current opacity-70" aria-hidden />
      {style.label}
    </span>
  )
}
