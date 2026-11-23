/** Tipos que reflejan los DTOs de Tracking.Application (TrackingDto, TrackingEventDto). */

export type TrackingStatus =
  | 'Created'
  | 'PickedUp'
  | 'InTransit'
  | 'AtDistributionCenter'
  | 'OutForDelivery'
  | 'Delivered'
  | 'DeliveryFailed'
  | 'Cancelled'

/** Los únicos que el operador registra a mano (ManualTrackingStatuses en el backend). */
export type ManualTrackingStatus = 'PickedUp' | 'InTransit' | 'AtDistributionCenter'

export interface TrackingEvent {
  id: string
  status: TrackingStatus
  location: string
  description: string
  occurredAt: string
}

/** Respuesta de GET /api/tracking/{trackingNumber}. */
export interface Tracking {
  shipmentId: string
  trackingNumber: string
  currentStatus: TrackingStatus
  currentLocation: string
  lastUpdatedAt: string
  isFinal: boolean
  /** Qué botones mostrar: lo calcula el backend con su máquina de estados. */
  nextManualStatuses: ManualTrackingStatus[]
  /** Del más antiguo al más reciente. */
  events: TrackingEvent[]
}

/** Cuerpo de POST /api/tracking/{trackingNumber}/events. */
export interface RegisterTrackingEventRequest {
  status: ManualTrackingStatus
  location: string
  description: string
  occurredAt?: string
}
