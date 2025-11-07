<script setup lang="ts">
import { ref, watch,onMounted } from 'vue'
import RawEditor from './bodyTabs/RawEditor.vue'
import FormDataEditor from './bodyTabs/FormDataEditor.vue'
import DataBaseTest from './bodyTabs/DataBaseTest.vue'
import ParamsTab from './ParamsTab.vue'
 interface Props {
  modelValue?: string
  dataBaseTest?: string | null
  requestId?: number | null
  bodyId?: number
   formDataItems?: Array<any>
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: '{}',
  dataBaseTest: null,
  requestId: null,
  bodyId: 0,
   formDataItems: () => []
})

// State
const currentDataBaseTest = ref('')
const currentBodyId = ref(0)
// const bodyType = ref<'base-data' | 'raw' | 'form-data' | 'none' | 'x-www-form-urlencoded' | 'binary'>('raw')
const bodyType = ref<'base-data' | 'raw' | 'form-data' | 'none' | 'binary'>('raw')

const rawContent = ref('')
 const formUrlEncoded = ref<any[]>([])
const binaryFile = ref<File | null>(null)

// Refs
const rawEditorRef = ref<any>(null)
const formRef = ref<any>(null)
const paramsRef = ref<any>(null)
onMounted(() => {
  console.log('🟩 [BodyTab] Mounted')
  console.log('🟩 [BodyTab] bodyType:', bodyType.value)
  console.log('🟩 [BodyTab] formRef:', formRef.value)
  console.log('🟩 [BodyTab] Has getFormDataItems?:', !!formRef.value?.getFormDataItems)
})
watch(() => props.bodyId, (val) => {
  currentBodyId.value = val || 0
}, { immediate: true, flush: 'sync' })

// Watch dataBaseTest
watch(() => props.dataBaseTest, (val) => {
  currentDataBaseTest.value = val || ''

  if (val && (bodyType.value === 'raw' || bodyType.value === 'none')) {
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
function testGetFormData() {
  console.log('🟩 [BodyTab] testGetFormData called')
  console.log('🟩 [BodyTab] formRef.value:', formRef.value)
  if (formRef.value && formRef.value.getFormDataItems) {
    const items = formRef.value.getFormDataItems()
    console.log('🟩 [BodyTab] Form items:', items)
    return items
  }
  return []
}
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
  return result
}

// BodyTab.vue
function getBody() {
  let result = null

  switch (bodyType.value) {
    case 'form-data':
      // ✅ DÙNG getBody() TỪ FORM REF
      result = formRef.value?.getBody?.() || null
      break
    case 'raw':
      result = normalizeBodyOutput(rawContent.value, 'raw')
      break
    case 'base-data':
      result = normalizeBodyOutput(currentDataBaseTest.value, 'base-data')
      break
    default:
      result = null
  }

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
  currentBodyId.value = id
}
function getDataBaseTest() {
  return currentDataBaseTest.value
}
function setBodyType(type: 'base-data' | 'raw' | 'form-data' | 'none' | 'binary') {
  console.log('🟩 [BodyTab] setBodyType called:', type)
  bodyType.value = type
}
defineExpose({
  getBodyType: () => bodyType.value,
  getBody,
  updateBody,
  setDataBaseTest,
  setBodyId,
  testGetFormData,
  setBodyType,
  getDataBaseTest,
    $refs: {            
    formRef,
    rawEditorRef
  }
})
</script>

<template>
  <div class="bg-white">
    <!-- ✅ THÊM: Debug display -->
    <div class="mb-2 p-2 bg-yellow-50 border border-yellow-200 rounded text-xs">
      🐛 DEBUG: Current bodyType = <strong>{{ bodyType }}</strong>
    </div>

    <!-- Radio buttons -->
    <div class="flex items-center gap-4 mb-4 flex-wrap">
      <label v-for="t in ['base-data', 'none', 'form-data', 'raw', 'binary']" :key="t"
        class="flex items-center gap-2 cursor-pointer select-none"
        :class="bodyType === t ? 'text-blue-600 font-medium' : 'text-gray-600'">
        <!-- ✅ THÊM: Log khi click -->
        <input 
          type="radio" 
          :value="t" 
          v-model="bodyType" 
          @change="console.log('🟩 [BodyTab] Radio changed to:', t)"
          class="w-4 h-4 text-blue-600" />
        <span class="text-sm">{{ t }}</span>
      </label>
    </div>

    <div>
      <!-- ✅ Các body types -->
      <div v-show="bodyType === 'none'" class="text-center py-6 text-gray-400">
        This request does not have a body
      </div>

      <RawEditor v-if="bodyType === 'raw'" v-model="rawContent" ref="rawEditorRef" />

      <FormDataEditor 
        v-show="bodyType === 'form-data'" 
        ref="formRef"
        :initialData="props.formDataItems" />

      <DataBaseTest 
        v-show="bodyType === 'base-data'" 
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