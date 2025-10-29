<script setup lang="ts">
import { ref, watch } from 'vue'
import RawEditor from './bodyTabs/RawEditor.vue'
import FormDataEditor from './bodyTabs/FormDataEditor.vue'
import DataBaseTest from './bodyTabs/DataBaseTest.vue'
import ParamsTab from './ParamsTab.vue'

interface Props {
  modelValue?: string
  dataBaseTest?: string | null  
  requestId?: number | null   
}
const props = withDefaults(defineProps<Props>(), {
  modelValue: '{}',
  dataBaseTest: null,
  requestId: null
})

const currentDataBaseTest = ref<string>('')
 
// Watch prop dataBaseTest từ Card.vue
watch(() => props.dataBaseTest, (val) => {
  currentDataBaseTest.value = val || ''
}, { immediate: true })

// Loại body
const bodyType = ref<'base-data' | 'raw' | 'form-data' | 'none' | 'x-www-form-urlencoded' | 'binary'>('raw')

const rawContent = ref<string>('')
const formData = ref<any[]>([])
const formUrlEncoded = ref<any[]>([])
const binaryFile = ref<File | null>(null)

const rawEditorRef = ref<any>(null)
const formRef = ref<any>(null)
const paramsRef = ref<any>(null)

watch(() => props.modelValue, (newValue) => {
  if (bodyType.value === 'raw' && typeof newValue === 'string') {
    rawContent.value = newValue
    rawEditorRef.value?.updateBody?.(newValue)
  }
}, { immediate: true })

const handleBinaryFileChange = (e: Event) => {
  const target = e.target as HTMLInputElement
  binaryFile.value = target.files?.[0] ?? null
}

// Tự động chọn tab base-data nếu có dataBaseTest
watch(currentDataBaseTest, (val) => {
  if (val && bodyType.value !== 'base-data') {
    bodyType.value = 'base-data'
  }
  
}, { immediate: true })

defineExpose({
  getBodyType: () => bodyType.value,

  getBody: () => {
    if (bodyType.value === 'base-data') {
      return {
        bodyType: 'base-data',
        content: currentDataBaseTest.value
      }
    }
    if (bodyType.value === 'raw') {
      return { bodyType: 'raw', content: rawContent.value }
    }
    if (bodyType.value === 'form-data') {
      return { bodyType: 'form-data', content: formData.value }
    }
    if (bodyType.value === 'x-www-form-urlencoded') {
      return { bodyType: 'x-www-form-urlencoded', content: formUrlEncoded.value }
    }
    if (bodyType.value === 'binary') {
      return { bodyType: 'binary', content: binaryFile.value }
    }
    return null
  },

  updateBody: (newBody: string) => {
    rawContent.value = newBody
    rawEditorRef.value?.updateBody?.(newBody)
  },

  setDataBaseTest: (value: string | null) => {
    currentDataBaseTest.value = value || ''
    if (value) bodyType.value = 'base-data'
  }
})
</script>

<template>
  <div class="bg-white">
    <div class="flex items-center gap-4 mb-4">
      <label v-for="t in ['base-data', 'none', 'form-data', 'x-www-form-urlencoded', 'raw', 'binary']" :key="t"
        class="flex items-center gap-2 cursor-pointer select-none"
        :class="bodyType === t ? 'text-blue-600 font-medium' : 'text-gray-600'">
        <input type="radio" :value="t" v-model="bodyType" class="w-4 h-4 text-blue-600" />
        <span class="text-sm">{{ t }}</span>
      </label>
    </div>

    <div>
      <div v-show="bodyType === 'none'" class="text-center py-6 text-gray-400">
        This request does not have a body
      </div>

      <RawEditor v-if="bodyType === 'raw'" v-model="rawContent" ref="rawEditorRef" />
      <FormDataEditor v-show="bodyType === 'form-data'" ref="formRef" />
      <ParamsTab v-show="bodyType === 'x-www-form-urlencoded'" ref="paramsRef" :paramsData="formUrlEncoded" />

<!-- BodyTab.vue -->
<DataBaseTest 
  v-show="bodyType === 'base-data'" 
  :dataBaseTest="currentDataBaseTest"
  :requestId="props.requestId"    
/>
      <div v-show="bodyType === 'binary'" class="py-6 flex flex-col items-center gap-3">
        <input id="binaryFile" type="file" class="hidden" @change="handleBinaryFileChange" />
        <label for="binaryFile"
          class="cursor-pointer border border-gray-300 rounded-lg px-4 py-2 text-sm text-blue-600 bg-gray-50 hover:bg-blue-100 hover:text-blue-700 transition-colors">
          Chọn file
        </label>
        <div v-if="binaryFile" class="text-sm text-gray-600">{{ binaryFile.name }}</div>
        <div v-else class="text-xs text-gray-400">Chưa có file nào được chọn</div>
      </div>
    </div>
  </div>
</template>