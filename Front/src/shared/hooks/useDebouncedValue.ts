import { useEffect, useState } from 'react'

/**
 * Devuelve `value` solo cuando lleva `delayMs` sin cambiar.
 * Para buscadores: escribir "monterrey" no lanza 9 peticiones (una por letra), sino una.
 */
export function useDebouncedValue<T>(value: T, delayMs = 300): T {
  const [debounced, setDebounced] = useState(value)

  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delayMs)
    return () => clearTimeout(timer) // si cambia antes de tiempo, se cancela el temporizador anterior
  }, [value, delayMs])

  return debounced
}
