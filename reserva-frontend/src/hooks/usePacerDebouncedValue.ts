import { Debouncer } from "@tanstack/pacer"
import { useCallback, useEffect, useMemo, useState } from "react"

interface UsePacerDebouncedValueOptions {
  wait?: number
}

interface UsePacerDebouncedValueResult<T> {
  debouncedValue: T
  schedule: (next: T) => void
  cancel: () => void
  setImmediate: (next: T) => void
}

export function usePacerDebouncedValue<T>(
  initialValue: T,
  options: UsePacerDebouncedValueOptions = {},
): UsePacerDebouncedValueResult<T> {
  const { wait = 350 } = options
  const [debouncedValue, setDebouncedValue] = useState<T>(initialValue)

  const debouncer = useMemo(
    () =>
      new Debouncer(
        (next: T) => {
          setDebouncedValue(next)
        },
        { wait },
      ),
    [wait],
  )

  const schedule = useCallback(
    (next: T) => {
      debouncer.maybeExecute(next)
    },
    [debouncer],
  )

  const cancel = useCallback(() => {
    debouncer.cancel()
  }, [debouncer])

  const setImmediate = useCallback(
    (next: T) => {
      debouncer.cancel()
      setDebouncedValue(next)
    },
    [debouncer],
  )

  useEffect(() => {
    return () => {
      debouncer.cancel()
    }
  }, [debouncer])

  return {
    debouncedValue,
    schedule,
    cancel,
    setImmediate,
  }
}
