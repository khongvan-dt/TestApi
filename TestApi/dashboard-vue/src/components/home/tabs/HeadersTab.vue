<script setup lang="ts">
import { ref } from 'vue'

interface Header {
  key: string
  value: string
  enabled: boolean
}

const headers = ref<Header[]>([
  { key: '', value: '', enabled: true }
])

const commonHeaders = [
  { key: 'Content-Type', value: 'application/json' },
  { key: 'Authorization', value: 'Bearer ' },
  { key: 'Accept', value: 'application/json' },
  { key: 'User-Agent', value: 'API-Tester/1.0' },
  { key: 'Accept-Language', value: 'en-US,en;q=0.9' }
]

const addHeader = () => {
  headers.value.push({ key: '', value: '', enabled: true })
}

const addCommonHeader = (header: { key: string; value: string }) => {
  headers.value.push({ ...header, enabled: true })
}

const removeHeader = (index: number) => {
  headers.value.splice(index, 1)
  if (headers.value.length === 0) {
    addHeader()
  }
}

defineExpose({
  getHeaders: () => headers.value.filter(h => h.key.trim() !== '')
})
</script>

<template>
  <div class="space-y-2">
    <div class="text-xs text-gray-500 mb-3">
      Headers are sent with the request
      
      <details class="mt-2">
        <summary class="cursor-pointer text-blue-600 hover:text-blue-700">Common headers</summary>
        <div class="mt-2 space-y-1 pl-2">
          <button
            v-for="(header, idx) in commonHeaders"
            :key="idx"
            @click="addCommonHeader(header)"
            class="block text-xs text-gray-600 hover:text-blue-600 hover:bg-blue-50 px-2 py-1 rounded w-full text-left"
          >
            {{ header.key }}: {{ header.value }}
          </button>
        </div>
      </details>
    </div>
    
    <div 
      v-for="(header, index) in headers" 
      :key="index"
      class="flex items-center gap-2"
    >
      <input 
        type="checkbox" 
        v-model="header.enabled"
        class="w-4 h-4 text-blue-600 rounded"
      />
      <input 
        v-model="header.key"
        type="text"
        placeholder="Header Key"
        class="flex-1 px-3 py-2 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
      />
      <input 
        v-model="header.value"
        type="text"
        placeholder="Header Value"
        class="flex-1 px-3 py-2 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
      />
      <button 
        @click="removeHeader(index)"
        class="px-2 py-2 text-sm text-red-600 hover:bg-red-50 rounded-md transition-colors"
        title="Remove header"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>
    </div>
    
    <button 
      @click="addHeader"
      class="mt-2 text-sm text-blue-600 hover:text-blue-700 font-medium flex items-center gap-1"
    >
      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
      </svg>
      Add Header
    </button>
  </div>
</template>