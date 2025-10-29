<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{ modelValue?: string }>()
const emit = defineEmits<{ 'update:modelValue': [value: string] }>()

const body = ref(props.modelValue ?? '')

watch(() => props.modelValue, (newValue) => {
  if (newValue !== undefined && newValue !== body.value) {
    body.value = newValue
  }
}, { immediate: true })

const updateBody = () => {
   emit('update:modelValue', body.value)
}

defineExpose({
  updateBody: (newBody: string) => {
    body.value = newBody
    emit('update:modelValue', newBody)
  },
  getBody: () => body.value,
  getBodyType: () => 'raw'
})
</script>

<template>
  <textarea
    v-model="body"
    @input="updateBody"
    placeholder="Enter request body..."
    class="w-full h-[360px] px-3 py-2 font-mono"
  />
</template>
