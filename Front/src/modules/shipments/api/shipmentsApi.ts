import { http } from '@/shared/api/http'
import type { CreateShipmentRequest, CreateShipmentResult, Shipment } from './types'

/** Una función por endpoint de ShipmentEndpoints.cs. Sin lógica: solo HTTP. */
export const shipmentsApi = {
  create: (request: CreateShipmentRequest) =>
    http<CreateShipmentResult>('/api/shipments', { method: 'POST', body: JSON.stringify(request) }),

  getByTrackingNumber: (trackingNumber: string) =>
    http<Shipment>(`/api/shipments/tracking/${encodeURIComponent(trackingNumber)}`),

  cancel: (shipmentId: string, reason: string) =>
    http<void>(`/api/shipments/${shipmentId}/cancel`, { method: 'POST', body: JSON.stringify({ reason }) }),
}
