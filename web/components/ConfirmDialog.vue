<script setup lang="ts">
interface Props {
  modelValue: boolean
  title?: string
  message: string
  confirmText?: string
  cancelText?: string
  confirmColor?: string
  icon?: string
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  title: 'Confirmar ação',
  confirmText: 'Confirmar',
  cancelText: 'Cancelar',
  confirmColor: 'error',
  icon: 'mdi-alert-outline',
  loading: false
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirm: []
  cancel: []
}>()

function close(value: boolean) {
  emit('update:modelValue', value)
}

function handleConfirm() {
  emit('confirm')
}

function handleCancel() {
  emit('cancel')
  close(false)
}

const iconBgStyle = computed(() => ({
  backgroundColor: `rgba(var(--v-theme-${props.confirmColor}), 0.12)`,
  color: `rgb(var(--v-theme-${props.confirmColor}))`
}))
</script>

<template>
  <v-dialog
    :model-value="modelValue"
    max-width="480"
    persistent
    @update:model-value="close"
  >
    <v-card class="pa-2">
      <div class="d-flex align-start ga-4 pa-4">
        <div
          class="d-flex align-center justify-center flex-shrink-0"
          :style="{
            ...iconBgStyle,
            width: '48px',
            height: '48px',
            borderRadius: '14px'
          }"
        >
          <v-icon :icon="icon" size="26" />
        </div>
        <div class="flex-grow-1 pt-1">
          <div class="text-h6 font-weight-bold mb-1">{{ title }}</div>
          <div class="text-body-2" style="color: rgb(var(--v-theme-on-surface-variant));">
            {{ message }}
          </div>
        </div>
      </div>

      <v-card-actions class="px-4 pb-4 pt-2">
        <v-spacer />
        <v-btn
          variant="text"
          :disabled="loading"
          @click="handleCancel"
        >
          {{ cancelText }}
        </v-btn>
        <v-btn
          :color="confirmColor"
          :loading="loading"
          @click="handleConfirm"
        >
          {{ confirmText }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>
