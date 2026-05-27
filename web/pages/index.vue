<script setup lang="ts">
definePageMeta({
  middleware: [
    function () {
      return navigateTo('/categorias', { replace: true })
    }
  ]
})
</script>

<template>
  <div />
</template>
