<script setup lang="ts">
import { useFeedback } from '~/composables/useFeedback'

const route = useRoute()
const { feedback, dismiss } = useFeedback()

const navItems = [
  { title: 'Categorias', to: '/categorias', icon: 'mdi-tag-multiple' },
  { title: 'Produtos', to: '/produtos', icon: 'mdi-package-variant' }
]
</script>

<template>
  <v-app>
    <v-app-bar color="primary" density="comfortable" elevation="2">
      <v-app-bar-title class="font-weight-bold">
        <v-icon start>mdi-cube-outline</v-icon>
        Inventário Comercial
      </v-app-bar-title>

      <template v-for="item in navItems" :key="item.to">
        <v-btn
          :to="item.to"
          :prepend-icon="item.icon"
          :variant="route.path.startsWith(item.to) ? 'tonal' : 'text'"
          class="ml-2"
        >
          {{ item.title }}
        </v-btn>
      </template>
    </v-app-bar>

    <v-main>
      <v-container class="py-6">
        <slot />
      </v-container>
    </v-main>

    <v-snackbar
      v-model="feedback.visible"
      :color="feedback.color"
      :timeout="feedback.timeout"
      location="bottom right"
      multi-line
    >
      <div class="d-flex align-center">
        <v-icon start>{{ feedback.icon }}</v-icon>
        <span>{{ feedback.message }}</span>
      </div>
      <template #actions>
        <v-btn icon="mdi-close" variant="text" @click="dismiss" />
      </template>
    </v-snackbar>
  </v-app>
</template>
