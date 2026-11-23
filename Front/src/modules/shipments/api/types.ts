/**
 * Tipos TypeScript que reflejan los DTOs de la API (records de C# en Shipments.Application).
 * Si cambia un DTO en el backend, este archivo es el que hay que actualizar.
 * (Más adelante se pueden generar automáticamente desde /openapi/v1.json.)
 */

export type ShipmentStatus = 'Registered' | 'Delivered' | 'Cancelled'

export interface AddressInput {
  street: string
  city: string
  state: string
  postalCode: string
  country: string
}

export interface ContactInput {
  name: string
  phone: string
  address: AddressInput
}

/** Cuerpo de POST /api/shipments (CreateShipmentCommand). */
export interface CreateShipmentRequest {
  sender: ContactInput
  recipient: ContactInput
  weightKg: number
  lengthCm: number
  widthCm: number
  heightCm: number
}

/** Respuesta 201 de POST /api/shipments (CreateShipmentResult). */
export interface CreateShipmentResult {
  shipmentId: string
  trackingNumber: string
  cost: number
  currency: string
}

/** Una fila de GET /api/shipments (ShipmentSummaryDto). */
export interface ShipmentSummary {
  id: string
  trackingNumber: string
  status: ShipmentStatus
  senderName: string
  originCity: string
  recipientName: string
  destinationCity: string
  cost: number
  currency: string
  createdAt: string
}

/** Filtros de GET /api/shipments (GetShipmentsQuery). */
export interface ShipmentsFilter {
  status?: ShipmentStatus
  search?: string
  page?: number
  pageSize?: number
}

/** Respuesta de GET /api/shipments/tracking/{trackingNumber} (ShipmentDto). */
export interface Shipment {
  id: string
  trackingNumber: string
  status: ShipmentStatus
  sender: ContactInput
  recipient: ContactInput
  weightKg: number
  lengthCm: number
  widthCm: number
  heightCm: number
  cost: number
  currency: string
  createdAt: string
  cancelledAt: string | null
  cancellationReason: string | null
  deliveredAt: string | null
}
