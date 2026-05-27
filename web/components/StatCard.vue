<script setup lang="ts">
interface Props {
  label: string
  value: string | number
  hint?: string
  icon: string
  color?: string
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  color: 'primary',
  loading: false
})

const iconBgStyle = computed(() => ({
  backgroundColor: `rgba(var(--v-theme-${props.color}), 0.12)`,
  color: `rgb(var(--v-theme-${props.color}))`
}))
</script>

<template>
  <v-card class="stat-card card-interactive">
    <div class="stat-card__icon" :style="iconBgStyle">
      <v-icon :icon="icon" size="22" />
    </div>

    <div class="stat-card__label">{{ label }}</div>

    <div class="stat-card__value">
      <v-progress-circular
        v-if="loading"
        indeterminate
        size="20"
        width="2"
        :color="color"
      />
      <template v-else>{{ value }}</template>
    </div>

    <div v-if="hint" class="stat-card__hint">{{ hint }}</div>
  </v-card>
</template>
