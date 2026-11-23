import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { retryUnlessClientError } from '@/shared/api/retry'
import { deliveriesApi } from './deliveriesApi'
import type { DeliveriesFilter, FailureCode } from './types'

export const deliveryKeys = {
  all: ['deliveries'] as const,
  list: (filter: DeliveriesFilter) => ['deliveries', 'list', filter] as const,
  detail: (id: string) => ['deliveries', 'detail', id] as const,
}

export function useDeliveries(filter: DeliveriesFilter) {
  return useQuery({
    queryKey: deliveryKeys.list(filter),
    queryFn: () => deliveriesApi.list(filter),
    placeholderData: keepPreviousData,
    retry: retryUnlessClientError,
  })
}

export function useDelivery(id: string) {
  return useQuery({
    queryKey: deliveryKeys.detail(id),
    queryFn: () => deliveriesApi.get(id),
    retry: retryUnlessClientError,
  })
}

export type DeliveryAction =
  | { type: 'assign'; courierId: string }
  | { type: 'confirm'; receivedBy: string; notes?: string }
  | { type: 'fail'; failureCode: FailureCode; details?: string }

/**
 * Una sola mutación para las tres acciones: las tres invalidan lo mismo.
 *
 * Cada acción cambia OTROS módulos por evento (Tracking pasa a "En reparto", Shipments a "Entregado"...).
 * Esos cambios llegan un instante después (outbox), así que esas cachés se invalidan también un poco
 * más tarde. Es un parche de UI razonable; lo "de verdad" sería que el servidor avise (SSE / SignalR).
 */
export function useDeliveryAction(id: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (action: DeliveryAction) => {
      switch (action.type) {
        case 'assign':
          return deliveriesApi.assign(id, action.courierId)
        case 'confirm':
          return deliveriesApi.confirm(id, action.receivedBy, action.notes)
        case 'fail':
          return deliveriesApi.fail(id, action.failureCode, action.details)
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: deliveryKeys.all })
      setTimeout(() => {
        for (const key of [['tracking'], ['shipments'], ['dashboard']]) queryClient.invalidateQueries({ queryKey: key })
      }, 1000)
    },
  })
}
