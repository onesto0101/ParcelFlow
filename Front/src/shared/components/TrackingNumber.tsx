import { useState } from 'react'
import { CheckIcon, CopyIcon } from 'lucide-react'
import { cn } from '@/lib/utils'

/** "Guía Tracking Pill" del DESIGN.md: número de guía en monoespaciada con botón de copiar. */
export function TrackingNumber({ value, size = 'md' }: { value: string; size?: 'md' | 'lg' }) {
  const [copied, setCopied] = useState(false)

  async function copy() {
    await navigator.clipboard.writeText(value)
    setCopied(true)
    setTimeout(() => setCopied(false), 1500)
  }

  return (
    <span
      className={cn(
        'group inline-flex items-center gap-2 rounded-lg border bg-muted/60 font-mono font-semibold tabular',
        size === 'lg' ? 'px-3 py-1.5 text-2xl tracking-wider' : 'px-2 py-0.5 text-[13px]',
      )}
    >
      {value}
      <button
        type="button"
        onClick={copy}
        className="text-muted-foreground hover:text-foreground"
        aria-label="Copiar número de guía"
        title="Copiar"
      >
        {copied ? <CheckIcon className="size-4 text-emerald-600" /> : <CopyIcon className="size-4" />}
      </button>
    </span>
  )
}
