<script setup lang="ts">
import { ref, watch } from 'vue'
import RawEditor from './bodyTabs/RawEditor.vue'
import FormDataEditor from './bodyTabs/FormDataEditor.vue'
import ParamsTab from './ParamsTab.vue'

interface Props {
  modelValue?: string
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: '{}'
})

const bodyType = ref<'raw' | 'form-data' | 'none' | 'x-www-form-urlencoded' | 'binary'>('raw')
const currentBody = ref(props.modelValue)

// refs to children
const rawRef = ref<any>(null)
const formRef = ref<any>(null)
const paramsRef = ref<any>(null)

// Watch modelValue changes
watch(() => props.modelValue, (newValue) => {
  console.log('📝 BodyTab received:', newValue) // Debug log
  currentBody.value = newValue
  
  if (rawRef.value?.updateBody) {
    console.log('✍️ Updating RawEditor...') // Debug log
    rawRef.value.updateBody(newValue)
  }
}, { immediate: true })

const binaryFile = ref<File | null>(null)

const handleBinaryFileChange = (e: Event) => {
  const target = e.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    binaryFile.value = target.files[0]
  } else {
    binaryFile.value = null
  }
}

// Parent expose
defineExpose({
  getBody: () => {
    if (bodyType.value === 'raw' && rawRef.value) return rawRef.value.getBody?.()
    if (bodyType.value === 'form-data' && formRef.value) return formRef.value.getBody?.()
    if (bodyType.value === 'x-www-form-urlencoded' && paramsRef.value) return paramsRef.value.getParams?.()
    return null
  },
  getBodyType: () => bodyType.value,
  updateBody: (newBody: string) => {
    console.log('🔄 BodyTab updateBody called with:', newBody) // Debug log
    currentBody.value = newBody
    if (rawRef.value?.updateBody) {
      rawRef.value.updateBody(newBody)
    }
  },
  focus: () => {
    if (bodyType.value === 'raw' && rawRef.value) {
      rawRef.value.focus?.()
    }
  }
})
</script>

<template>
  <div class="bg-white">
    <!-- Tabs -->
    <div class="flex items-center gap-4 mb-4">
      <label 
        v-for="t in ['none', 'form-data', 'x-www-form-urlencoded', 'raw', 'binary']" 
        :key="t"
        class="flex items-center gap-2 cursor-pointer select-none"
        :class="bodyType === t ? 'text-blue-600 font-medium' : 'text-gray-600'"
      >
        <input type="radio" :value="t" v-model="bodyType" class="w-4 h-4 text-blue-600" />
        <span class="text-sm">{{ t }}</span>
      </label>
    </div>

    <!-- content -->
    <div>
      <div v-show="bodyType === 'none'" class="text-center py-6 text-gray-400">
        This request does not have a body
      </div>
      
      <RawEditor 
        v-show="bodyType === 'raw'" 
        ref="rawRef" 
        v-model="currentBody"
      />
      
      <FormDataEditor v-show="bodyType === 'form-data'" ref="formRef" />
      
      <ParamsTab v-show="bodyType === 'x-www-form-urlencoded'" ref="paramsRef" />
      
      <div v-show="bodyType === 'binary'" class="py-6 flex flex-col items-center gap-3">
        <input id="binaryFile" type="file" class="hidden" @change="handleBinaryFileChange" />

        <label 
          for="binaryFile"
          class="cursor-pointer border border-gray-300 rounded-lg px-4 py-2 text-sm text-blue-600 bg-gray-50 hover:bg-blue-100 hover:text-blue-700 transition-colors"
        >
          Chọn file
        </label>

        <div v-if="binaryFile" class="text-sm text-gray-600">
          📄 {{ binaryFile.name }}
        </div>

        <div v-else class="text-xs text-gray-400">
          Chưa có file nào được chọn
        </div>
      </div>
    </div>
  </div>
</template>