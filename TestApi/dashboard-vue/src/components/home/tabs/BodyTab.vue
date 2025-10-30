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
  bodyId?: number
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: '{}',
  dataBaseTest: null,
  requestId: null,
  bodyId: 0
})

// State
const currentDataBaseTest = ref('')
const currentBodyId = ref(0)
const bodyType = ref<'base-data' | 'raw' | 'form-data' | 'none' | 'x-www-form-urlencoded' | 'binary'>('raw')
const rawContent = ref('')
const formData = ref<any[]>([])
const formUrlEncoded = ref<any[]>([])
const binaryFile = ref<File | null>(null)

// Refs
const rawEditorRef = ref<any>(null)
const formRef = ref<any>(null)
const paramsRef = ref<any>(null)

// ✅ FIX: Watch bodyId TRƯỚC - để set currentBodyId ngay lập tức
watch(() => props.bodyId, (val) => {
  console.log('🟡 [BodyTab.vue] Received bodyId prop:', val)
  currentBodyId.value = val || 0
  console.log('🟡 [BodyTab.vue] Set currentBodyId to:', currentBodyId.value)
}, { immediate: true, flush: 'sync' })  // ✅ Thêm flush: 'sync'

// Watch dataBaseTest
watch(() => props.dataBaseTest, (val) => {
  console.log('🟡 [BodyTab.vue] Received dataBaseTest:', val)
  currentDataBaseTest.value = val || ''
  
  // ✅ CHỈ tự động chuyển sang base-data nếu có dataBaseTest VÀ đang ở raw/none
  if (val && (bodyType.value === 'raw' || bodyType.value === 'none')) {
    console.log('🟡 [BodyTab.vue] Auto switching to base-data')
    bodyType.value = 'base-data'
  }
}, { immediate: true })

// Watch modelValue
watch(() => props.modelValue, (newValue) => {
  if (bodyType.value === 'raw' && typeof newValue === 'string') {
    rawContent.value = newValue
    rawEditorRef.value?.updateBody?.(newValue)
  }
}, { immediate: true })

function handleBinaryFileChange(e: Event) {
  const target = e.target as HTMLInputElement
  binaryFile.value = target.files?.[0] ?? null
}

function normalizeBodyOutput(content: any, type: string) {
  const result = {
    id: currentBodyId.value,
    bodyType: type,
    content
  }
  console.log('🟡 [BodyTab.vue] normalizeBodyOutput:', result)
  return result
}

function getBody() {
  console.log('🟡 [BodyTab.vue] getBody called')
  console.log('🟡 [BodyTab.vue] currentBodyId:', currentBodyId.value)
  console.log('🟡 [BodyTab.vue] bodyType:', bodyType.value)
  
  let result = null
  
  switch (bodyType.value) {
    case 'base-data':
      result = normalizeBodyOutput(currentDataBaseTest.value, 'base-data')
      break
    case 'raw':
      result = normalizeBodyOutput(rawContent.value, 'raw')
      break
    case 'form-data':
      result = normalizeBodyOutput(formData.value, 'form-data')
      break
    case 'x-www-form-urlencoded':
      result = normalizeBodyOutput(formUrlEncoded.value, 'x-www-form-urlencoded')
      break
    case 'binary':
      result = normalizeBodyOutput(binaryFile.value, 'binary')
      break
    default:
      result = null
  }
  
  console.log('🟡 [BodyTab.vue] getBody returning:', result)
  return result
}

function updateBody(newBody: string) {
  rawContent.value = newBody
  rawEditorRef.value?.updateBody?.(newBody)
}

function setDataBaseTest(value: string | null) {
  currentDataBaseTest.value = value || ''
  if (value) bodyType.value = 'base-data'
}

function setBodyId(id: number) {
  console.log('🟡 [BodyTab.vue] setBodyId called with:', id)
  currentBodyId.value = id
  console.log('🟡 [BodyTab.vue] currentBodyId updated to:', currentBodyId.value)
}

defineExpose({
  getBodyType: () => bodyType.value,
  getBody,
  updateBody,
  setDataBaseTest,
  setBodyId
})
</script>

<template>
  <div class="bg-white">
    <div class="flex items-center gap-4 mb-4 flex-wrap">
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

      <DataBaseTest v-show="bodyType === 'base-data'" 
        :dataBaseTest="currentDataBaseTest"
        :requestId="props.requestId" />

      <div v-show="bodyType === 'binary'" class="py-6 flex flex-col items-center gap-3">
        <input id="binaryFile" type="file" class="hidden" @change="handleBinaryFileChange" />
        <label for="binaryFile"
          class="cursor-pointer border border-gray-300 rounded-lg px-4 py-2 text-sm text-blue-600 bg-gray-50 hover:bg-blue-100 hover:text-blue-700 transition-colors">
          Choose file
        </label>
        <div v-if="binaryFile" class="text-sm text-gray-600">{{ binaryFile.name }}</div>
        <div v-else class="text-xs text-gray-400">No file selected</div>
      </div>
    </div>
  </div>
</template>