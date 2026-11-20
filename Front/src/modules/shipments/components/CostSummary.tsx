import type { ReactNode } from 'react'
import { InfoIcon } from 'lucide-react'
import { Separator } from '@/components/ui/separator'
import { formatKg, formatMoney } from '@/shared/lib/format'
import { BASE_FEE, RATE_PER_KG, type CostEstimate } from '../lib/shippingCost'

/** Desglose del costo: peso real vs volumétrico → peso cobrado → tarifa. */
export function CostSummary({ estimate }: { estimate: CostEstimate | null }) {
  if (!estimate) {
    return <p className="text-sm text-muted-foreground">Captura peso y dimensiones para ver la estimación.</p>
  }

  return (
    <div className="space-y-4 text-sm">
      <div className="space-y-2 rounded-lg bg-muted/60 p-3">
        <Row label="Peso real" value={formatKg(estimate.realKg)} />
        <Row
          label={
            <span className="inline-flex items-center gap-1" title="Largo × Ancho × Alto ÷ 5000">
              Peso volumétrico <InfoIcon className="size-3.5 text-muted-foreground" />
            </span>
          }
          value={formatKg(estimate.volumetricKg)}
        />
        <Row
          label={<span className="font-semibold text-primary">Peso a cobrar</span>}
          value={<span className="font-semibold text-primary">{formatKg(estimate.billableKg)}</span>}
          hint="El mayor de los dos"
        />
      </div>

      <div className="space-y-2">
        <Row label="Tarifa base" value={formatMoney(BASE_FEE)} />
        <Row label={`${RATE_PER_KG} × ${formatKg(estimate.billableKg)}`} value={formatMoney(estimate.weightFee)} />
      </div>

      <Separator />

      <div className="flex items-baseline justify-between">
        <span className="text-xs font-semibold tracking-wider text-muted-foreground uppercase">Total estimado</span>
        <span className="text-xl font-bold text-primary tabular">{formatMoney(estimate.total)}</span>
      </div>
      <p className="text-xs text-muted-foreground">El costo final lo calcula el sistema al registrar.</p>
    </div>
  )
}

function Row({ label, value, hint }: { label: ReactNode; value: ReactNode; hint?: string }) {
  return (
    <div className="flex items-start justify-between gap-4">
      <div>
        <div>{label}</div>
        {hint && <div className="text-xs text-muted-foreground">{hint}</div>}
      </div>
      <div className="tabular">{value}</div>
    </div>
  )
}
