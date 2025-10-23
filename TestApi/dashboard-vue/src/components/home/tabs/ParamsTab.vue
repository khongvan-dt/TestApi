<script setup lang="ts">
import { ref } from 'vue'

interface Param {
  key: string
  value: string
  enabled: boolean
}

const params = ref<Param[]>([
  { key: '', value: '', enabled: true }
])

const addParam = () => {
  params.value.push({ key: '', value: '', enabled: true })
}

const removeParam = (index: number) => {
  params.value.splice(index, 1)
  if (params.value.length === 0) {
    addParam()
  }
}

defineExpose({
  getParams: () => params.value.filter(p => p.key.trim() !== '')
})
</script>

<template>
  <div class="space-y-2">
    <div class="text-xs text-gray-500 mb-3">Query parameters are appended to the URL</div>
    
    <div 
      v-for="(param, index) in params" 
      :key="index"
      class="flex items-center gap-2"
    >
      <input 
        type="checkbox" 
        v-model="param.enabled"
        class="w-4 h-4 text-blue-600 rounded"
      />
      <input 
        v-model="param.key"
        type="text"
        placeholder="Key"
        class="flex-1 px-3 py-2 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
      />
      <input 
        v-model="param.value"
        type="text"
        placeholder="Value"
        class="flex-1 px-3 py-2 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
      />
      <button 
        @click="removeParam(index)"
        class="px-2 py-2 text-sm text-red-600 hover:bg-red-50 rounded-md transition-colors"
        title="Remove parameter"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>
    </div>
    
    <button 
      @click="addParam"
      class="mt-2 text-sm text-blue-600 hover:text-blue-700 font-medium flex items-center gap-1"
    >
      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
      </svg>
      Add Parameter
    </button>
  </div>
</template>