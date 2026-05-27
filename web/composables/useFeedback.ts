import { reactive } from 'vue'

type FeedbackType = 'success' | 'error' | 'warning' | 'info'

interface FeedbackState {
  visible: boolean
  message: string
  color: FeedbackType
  icon: string
  timeout: number
}

const state = reactive<FeedbackState>({
  visible: false,
  message: '',
  color: 'success',
  icon: 'mdi-check-circle',
  timeout: 3500
})

const ICONS: Record<FeedbackType, string> = {
  success: 'mdi-check-circle',
  error: 'mdi-alert-circle',
  warning: 'mdi-alert',
  info: 'mdi-information'
}

export function useFeedback() {
  function notify(message: string, color: FeedbackType = 'success', timeout = 3500) {
    state.message = message
    state.color = color
    state.icon = ICONS[color]
    state.timeout = timeout
    state.visible = true
  }

  function dismiss() {
    state.visible = false
  }

  return {
    feedback: state,
    notify,
    dismiss,
    success: (m: string) => notify(m, 'success'),
    error: (m: string) => notify(m, 'error', 5000),
    warning: (m: string) => notify(m, 'warning', 4500),
    info: (m: string) => notify(m, 'info')
  }
}
