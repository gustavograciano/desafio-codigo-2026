<script setup lang="ts">
interface Props {
  icon?: string
  title: string
  message?: string
  ctaLabel?: string
  ctaIcon?: string
}

withDefaults(defineProps<Props>(), {
  icon: 'mdi-package-variant-closed',
  ctaIcon: 'mdi-plus'
})

defineEmits<{
  cta: []
}>()
</script>

<template>
  <div class="d-flex flex-column align-center justify-center text-center py-12 px-4">
    <div
      class="mb-4 d-flex align-center justify-center"
      style="
        width: 72px;
        height: 72px;
        border-radius: 18px;
        background-color: rgba(var(--v-theme-primary), 0.08);
        color: rgb(var(--v-theme-primary));
      "
    >
      <v-icon :icon="icon" size="36" />
    </div>
    <div class="text-h6 font-weight-bold mb-1">{{ title }}</div>
    <div v-if="message" class="app-subtitle mb-4" style="max-width: 380px;">{{ message }}</div>
    <v-btn
      v-if="ctaLabel"
      color="primary"
      :prepend-icon="ctaIcon"
      @click="$emit('cta')"
    >
      {{ ctaLabel }}
    </v-btn>
  </div>
</template>
