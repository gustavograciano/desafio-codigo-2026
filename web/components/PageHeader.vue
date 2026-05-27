<script setup lang="ts">
interface Props {
  eyebrow?: string
  title: string
  subtitle?: string
}

defineProps<Props>()
</script>

<template>
  <div class="d-flex align-end flex-wrap ga-4 mb-6">
    <div class="flex-grow-1">
      <div
        v-if="eyebrow"
        class="text-uppercase text-caption font-weight-bold mb-1"
        style="letter-spacing: 0.08em; color: rgb(var(--v-theme-primary));"
      >
        {{ eyebrow }}
      </div>
      <h1 class="app-title text-h4">{{ title }}</h1>
      <p v-if="subtitle" class="app-subtitle mt-1 mb-0">{{ subtitle }}</p>
    </div>
    <div class="d-flex ga-2">
      <slot name="actions" />
    </div>
  </div>
</template>
