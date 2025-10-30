<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{ modelValue?: string }>()
const emit = defineEmits<{ (e: 'update:modelValue', value: string): void }>()

const body = ref('')

watch(() => props.modelValue, (newValue) => {
  if (newValue !== undefined && newValue !== body.value) {
    body.value = newValue
  }
}, { immediate: true })

function updateBody() {
  emit('update:modelValue', body.value)
}

function setBody(newBody: string) {
  body.value = newBody
  emit('update:modelValue', newBody)
}

defineExpose({
  updateBody: setBody,
  getBody: () => body.value,
  getBodyType: () => 'raw'
})
</script>

<template>
  <textarea
    v-model="body"
    @input="updateBody"
    placeholder="Enter request body..."
    class="w-full h-[360px] px-3 py-2 font-mono text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
  />
</template>