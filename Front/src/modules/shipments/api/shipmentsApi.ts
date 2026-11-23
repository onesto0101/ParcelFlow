import { http } from '@/shared/api/http'
import { toQueryString, type PagedResult } from '@/shared/api/paging'
import type { CreateShipmentRequest, CreateShipmentResult, Shipment, ShipmentSummary, ShipmentsFilter } from './types'

/** Una función por endpoint de ShipmentEndpoints.cs. Sin lógica: solo HTTP. */
export const shipmentsApi = {
  list: (filter: ShipmentsFilter) =>
    http<PagedResult<ShipmentSummary>>(`/api/shipments${toQueryString({ ...filter })}`),

  create: (request: CreateShipmentRequest) =>
    http<CreateShipmentResult>('/api/shipments', { method: 'POST', body: JSON.stringify(request) }),

  getByTrackingNumber: (trackingNumber: string) =>
    http<Shipment>(`/api/shipments/tracking/${encodeURIComponent(trackingNumber)}`),

  cancel: (shipmentId: string, reason: string) =>
    http<void>(`/api/shipments/${shipmentId}/cancel`, { method: 'POST', body: JSON.stringify({ reason }) }),
}
