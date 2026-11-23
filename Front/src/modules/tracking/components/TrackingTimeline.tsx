import { MapPinIcon } from 'lucide-react'
import { cn } from '@/lib/utils'
import { StatusBadge } from '@/shared/components/StatusBadge'
import { formatDateTime } from '@/shared/lib/format'
import type { TrackingEvent } from '../api/types'
import { TRACKING_STATUS_ICON } from '../lib/statusInfo'

/**
 * Línea de tiempo vertical del historial (requisito del diseño §8.2 Tracking).
 * La API lo manda en orden cronológico; aquí se muestra al revés: lo más reciente arriba,
 * que es lo que el usuario quiere ver primero.
 */
export function TrackingTimeline({ events, limit }: { events: TrackingEvent[]; limit?: number }) {
  const ordered = [...events].reverse().slice(0, limit)

  return (
    <ol className="relative">
      {ordered.map((event, index) => {
        const { icon: Icon, className } = TRACKING_STATUS_ICON[event.status]
        const isLatest = index === 0
        const isLast = index === ordered.length - 1

        return (
          <li key={event.id} className="relative flex gap-4 pb-6 last:pb-0">
            {/* La línea que une los puntos (no se pinta debajo del último). */}
            {!isLast && <span className="absolute top-10 bottom-0 left-5 w-px bg-border" aria-hidden />}

            <div
              className={cn(
                'relative z-[1] flex size-10 shrink-0 items-center justify-center rounded-full',
                className,
                isLatest && 'ring-4 ring-primary/15',
              )}
            >
              <Icon className="size-5" />
            </div>

            <div className="min-w-0 flex-1 pt-1">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <StatusBadge status={event.status} />
                <time className="text-xs text-muted-foreground tabular" dateTime={event.occurredAt}>
                  {formatDateTime(event.occurredAt)}
                </time>
              </div>
              <p className={cn('mt-1.5 text-sm', isLatest ? 'font-medium' : 'text-muted-foreground')}>{event.description}</p>
              <p className="mt-0.5 flex items-center gap-1 text-xs text-muted-foreground">
                <MapPinIcon className="size-3.5" /> {event.location}
              </p>
            </div>
          </li>
        )
      })}
    </ol>
  )
}
