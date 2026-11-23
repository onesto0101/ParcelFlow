import { useQuery } from '@tanstack/react-query'
import { http } from '@/shared/api/http'
import { retryUnlessClientError } from '@/shared/api/retry'

/** Refleja DashboardDto (DashboardEndpoints.cs): 5 indicadores + el desglose de cada módulo. */
export interface Dashboard {
  totalShipments: number
  inTransit: number
  outForDelivery: number
  delivered: number
  withDeliveryProblems: number
  shipments: { total: number; registered: number; delivered: number; cancelled: number }
  tracking: Record<
    'created' | 'pickedUp' | 'inTransit' | 'atDistributionCenter' | 'outForDelivery' | 'delivered' | 'deliveryFailed' | 'cancelled',
    number
  >
  deliveries: { pending: number; assigned: number; delivered: number; failed: number; cancelled: number }
}

/** Módulo pequeño: API y hook en el mismo archivo (no merece tres). */
export function useDashboard() {
  return useQuery({
    queryKey: ['dashboard'],
    queryFn: () => http<Dashboard>('/api/dashboard'),
    retry: retryUnlessClientError,
    // Un panel de control debe refrescarse solo: cada 30 s mientras la pestaña está abierta.
    refetchInterval: 30_000,
  })
}
