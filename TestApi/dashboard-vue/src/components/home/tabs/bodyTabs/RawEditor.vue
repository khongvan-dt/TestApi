<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{
  modelValue?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const textareaRef = ref<HTMLTextAreaElement | null>(null)
const body = ref(props.modelValue ?? '')

// Watch modelValue từ parent
watch(() => props.modelValue, (newValue) => {
  console.log('📄 RawEditor received:', newValue) // Debug log
  if (newValue !== undefined && newValue !== body.value) {
    body.value = newValue
  }
}, { immediate: true })

const updateBody = () => {
  emit('update:modelValue', body.value)
}

defineExpose({
  updateBody: (newBody: string) => {
    console.log('✏️ RawEditor updateBody:', newBody)  
    body.value = newBody
    emit('update:modelValue', newBody)
  },
  getBody: () => body.value,
  focus: () => textareaRef.value?.focus()
})
</script>

<template>
  <div class="relative rounded-md border border-gray-300 overflow-hidden bg-white">
    <div class="relative h-[360px] overflow-auto">
      <textarea
        ref="textareaRef"
        v-model="body"
        @input="updateBody"
        spellcheck="false"
        placeholder="Enter request body..."
        class="w-full h-full px-3 py-2 text-sm font-mono text-gray-800 bg-white focus:outline-none resize-none"
      ></textarea>
    </div>
  </div>
</template>