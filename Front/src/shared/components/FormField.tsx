import type { ReactNode } from 'react'
import { CircleXIcon } from 'lucide-react'
import { Label } from '@/components/ui/label'
import { cn } from '@/lib/utils'

/** Etiqueta + control + texto de ayuda o error. Evita repetir este bloque en cada input. */
export function FormField({
  id,
  label,
  required,
  hint,
  error,
  className,
  children,
}: {
  id: string
  label: string
  required?: boolean
  hint?: ReactNode
  error?: string
  className?: string
  children: ReactNode
}) {
  return (
    <div className={cn('space-y-1.5', className)}>
      <Label htmlFor={id} className="text-sm font-medium">
        {label}
        {required && <span className="text-destructive">*</span>}
      </Label>
      {children}
      {error ? (
        <p id={`${id}-error`} className="flex items-center gap-1 text-xs text-destructive" role="alert">
          <CircleXIcon className="size-3.5 shrink-0" />
          {error}
        </p>
      ) : (
        hint && <p className="text-xs text-muted-foreground">{hint}</p>
      )}
    </div>
  )
}
