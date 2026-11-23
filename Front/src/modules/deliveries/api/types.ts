/** Tipos que reflejan los DTOs de Deliveries.Application. */

export type DeliveryStatus = 'Pending' | 'Assigned' | 'Delivered' | 'Failed' | 'Cancelled'

export type FailureCode = 'RecipientAbsent' | 'WrongAddress' | 'Refused' | 'Inaccessible' | 'Other'

/** Una fila de GET /api/deliveries (DeliverySummaryDto). */
export interface DeliverySummary {
  id: string
  shipmentId: string
  trackingNumber: string
  status: DeliveryStatus
  courierId: string | null
  attemptCount: number
  createdAt: string
  assignedAt: string | null
  completedAt: string | null
}

export interface DeliveriesFilter {
  status?: DeliveryStatus
  courierId?: string
  search?: string
  page?: number
  pageSize?: number
}

export interface DeliveryAttempt {
  attemptNumber: number
  courierId: string
  attemptedAt: string
  succeeded: boolean
  receivedBy: string | null
  notes: string | null
  failureCode: FailureCode | null
  failureDetails: string | null
}

/** Respuesta de GET /api/deliveries/{id} (DeliveryDto). */
export interface Delivery {
  id: string
  shipmentId: string
  trackingNumber: string
  status: DeliveryStatus
  courierId: string | null
  createdAt: string
  assignedAt: string | null
  completedAt: string | null
  maxAttempts: number
  remainingAttempts: number
  /** Calculados por el backend con las reglas del dominio: el front solo decide qué botones mostrar. */
  canAssign: boolean
  canRegisterAttempt: boolean
  attempts: DeliveryAttempt[]
}
