import { http } from '@/shared/api/http'
import type { RegisterTrackingEventRequest, Tracking } from './types'

/** Una función por endpoint de TrackingEndpoints.cs. */
export const trackingApi = {
  get: (trackingNumber: string) => http<Tracking>(`/api/tracking/${encodeURIComponent(trackingNumber)}`),

  registerEvent: (trackingNumber: string, request: RegisterTrackingEventRequest) =>
    http<void>(`/api/tracking/${encodeURIComponent(trackingNumber)}/events`, {
      method: 'POST',
      body: JSON.stringify(request),
    }),
}
