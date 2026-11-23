import { http } from '@/shared/api/http'
import { toQueryString, type PagedResult } from '@/shared/api/paging'
import type { DeliveriesFilter, Delivery, DeliverySummary, FailureCode } from './types'

const post = (url: string, body: unknown) => http<void>(url, { method: 'POST', body: JSON.stringify(body) })

/** Una función por endpoint de DeliveryEndpoints.cs. */
export const deliveriesApi = {
  list: (filter: DeliveriesFilter) => http<PagedResult<DeliverySummary>>(`/api/deliveries${toQueryString({ ...filter })}`),

  get: (id: string) => http<Delivery>(`/api/deliveries/${id}`),

  assign: (id: string, courierId: string) => post(`/api/deliveries/${id}/assign`, { courierId }),

  confirm: (id: string, receivedBy: string, notes?: string) => post(`/api/deliveries/${id}/confirm`, { receivedBy, notes }),

  fail: (id: string, failureCode: FailureCode, details?: string) => post(`/api/deliveries/${id}/fail`, { failureCode, details }),
}
