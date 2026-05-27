import { computed } from 'vue'
import { useTheme } from 'vuetify'

const STORAGE_KEY = 'inventario:theme'
type ThemeName = 'light' | 'dark'

export function useAppTheme() {
  const theme = useTheme()

  function restoreFromStorage() {
    if (typeof window === 'undefined') return
    const stored = window.localStorage.getItem(STORAGE_KEY) as ThemeName | null
    if (stored === 'light' || stored === 'dark') {
      theme.change(stored)
    } else if (window.matchMedia?.('(prefers-color-scheme: dark)').matches) {
      theme.change('dark')
    }
  }

  function setTheme(name: ThemeName) {
    theme.change(name)
    if (typeof window !== 'undefined') {
      window.localStorage.setItem(STORAGE_KEY, name)
    }
  }

  function toggle() {
    setTheme(theme.global.current.value.dark ? 'light' : 'dark')
  }

  const isDark = computed(() => theme.global.current.value.dark)

  return { isDark, toggle, setTheme, restoreFromStorage }
}
