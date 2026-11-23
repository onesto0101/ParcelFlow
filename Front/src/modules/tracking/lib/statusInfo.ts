import {
  BanIcon,
  CircleCheckIcon,
  CircleXIcon,
  PackageIcon,
  PackageOpenIcon,
  TruckIcon,
  WarehouseIcon,
  type LucideIcon,
} from 'lucide-react'
import type { ManualTrackingStatus, TrackingStatus } from '../api/types'

/** Icono y color del punto de la línea de tiempo para cada estado. */
export const TRACKING_STATUS_ICON: Record<TrackingStatus, { icon: LucideIcon; className: string }> = {
  Created: { icon: PackageIcon, className: 'bg-slate-100 text-slate-600' },
  PickedUp: { icon: PackageOpenIcon, className: 'bg-sky-100 text-sky-700' },
  InTransit: { icon: TruckIcon, className: 'bg-blue-100 text-blue-700' },
  AtDistributionCenter: { icon: WarehouseIcon, className: 'bg-purple-100 text-purple-700' },
  OutForDelivery: { icon: TruckIcon, className: 'bg-amber-100 text-amber-700' },
  Delivered: { icon: CircleCheckIcon, className: 'bg-emerald-100 text-emerald-700' },
  DeliveryFailed: { icon: CircleXIcon, className: 'bg-rose-100 text-rose-700' },
  Cancelled: { icon: BanIcon, className: 'bg-slate-200 text-slate-600' },
}

/** Texto de los botones de "Registrar movimiento" y sugerencia de descripción. */
export const MANUAL_STATUS_INFO: Record<ManualTrackingStatus, { action: string; suggestion: string }> = {
  PickedUp: { action: 'Recolectado', suggestion: 'Paquete recolectado en el domicilio del remitente.' },
  InTransit: { action: 'En tránsito', suggestion: 'Salió hacia el siguiente centro de distribución.' },
  AtDistributionCenter: { action: 'Llegó a centro de distribución', suggestion: 'Recibido en el centro de distribución.' },
}
