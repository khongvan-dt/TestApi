<script setup lang="ts">
import { ref, computed,watch } from 'vue'

interface Header {
  id: string
  key: string
  value: string
  description: string
  enabled: boolean
}

const headers = ref<Header[]>([
  { id: '1', key: '', value: '', description: '', enabled: true }
])

// Common headers suggestions
const commonHeaders = [
  { key: 'Content-Type', values: ['application/json', 'application/xml', 'text/html', 'multipart/form-data', 'application/x-www-form-urlencoded'] },
  { key: 'Accept', values: ['application/json', 'application/xml', 'text/html', '*/*'] },
  { key: 'Authorization', values: ['Bearer ', 'Basic '] },
  { key: 'User-Agent', values: ['Mozilla/5.0', 'PostmanRuntime/7.26.8'] },
  { key: 'Cache-Control', values: ['no-cache', 'no-store', 'max-age=0'] },
  { key: 'Accept-Language', values: ['en-US', 'en', 'vi-VN'] },
  { key: 'Accept-Encoding', values: ['gzip, deflate, br'] },
  { key: 'Connection', values: ['keep-alive', 'close'] },
  { key: 'Cookie', values: [''] },
  { key: 'Referer', values: [''] }
]

const activeHeaderSuggestions = ref<string[]>([])
const activeValueSuggestions = ref<string[]>([])
const showHeaderSuggestions = ref(false)
const showValueSuggestions = ref(false)
const activeHeaderIndex = ref(-1)
const blurTimeout = ref<number | null>(null)
const props = defineProps<{
  headersData: Array<{ key: string; value: string }> | null
}>();

const usedHeaderKeys = computed(() =>
  headers.value
    .filter(h => h.key && h.enabled)
    .map(h => h.key)
)

const availableHeaders = computed(() =>
  commonHeaders
    .map(h => h.key)
    .filter(key => !usedHeaderKeys.value.includes(key))
)

const addHeader = () => {
  headers.value.push({
    id: Date.now().toString(),
    key: '',
    value: '',
    description: '',
    enabled: true
  })
}

const removeHeader = (id: string) => {
  headers.value = headers.value.filter(h => h.id !== id)
  if (headers.value.length === 0) {
    addHeader()
  }
}

const onKeyInput = (header: Header, index: number) => {
  activeHeaderIndex.value = index
  const searchTerm = header.key.toLowerCase()

  if (searchTerm.length > 0) {
    activeHeaderSuggestions.value = availableHeaders.value
      .filter(key => key.toLowerCase().includes(searchTerm))
    showHeaderSuggestions.value = activeHeaderSuggestions.value.length > 0
  } else {
    activeHeaderSuggestions.value = availableHeaders.value
    showHeaderSuggestions.value = true
  }

  const isLast = headers.value[headers.value.length - 1].id === header.id
  if (isLast && header.key.trim() !== '') {
    addHeader()
  }
}

const onValueInput = (header: Header, index: number) => {
  activeHeaderIndex.value = index
  const matchedHeader = commonHeaders.find(h => h.key === header.key)

  if (matchedHeader) {
    const searchTerm = header.value.toLowerCase()
    activeValueSuggestions.value = matchedHeader.values.filter(v =>
      v.toLowerCase().includes(searchTerm)
    )
    showValueSuggestions.value = activeValueSuggestions.value.length > 0
  } else {
    showValueSuggestions.value = false
  }
}

const selectHeaderSuggestion = (suggestion: string, index: number) => {
  headers.value[index].key = suggestion
  showHeaderSuggestions.value = false

  // Auto-fill common value if exists
  const matchedHeader = commonHeaders.find(h => h.key === suggestion)
  if (matchedHeader && matchedHeader.values.length > 0) {
    headers.value[index].value = matchedHeader.values[0]
  }
}

const selectValueSuggestion = (suggestion: string, index: number) => {
  headers.value[index].value = suggestion
  showValueSuggestions.value = false
}

const handleBlur = (type: 'header' | 'value') => {
  if (blurTimeout.value) {
    clearTimeout(blurTimeout.value)
  }
  blurTimeout.value = window.setTimeout(() => {
    if (type === 'header') {
      showHeaderSuggestions.value = false
    } else {
      showValueSuggestions.value = false
    }
  }, 200)
}


const enabledCount = computed(() => headers.value.filter(h => h.enabled && h.key).length)

defineExpose({
  getHeaders: () => {
    const result = headers.value.filter(h => h.enabled && h.key)
    return result
  }
})
watch(
  () => props.headersData,
  (newHeaders) => {
    if (newHeaders && newHeaders.length > 0) {
      headers.value = newHeaders.map((h, idx) => ({
        id: Date.now().toString() + idx,
        key: h.key,
        value: h.value,
        description: '',
        enabled: true
      }));
    } else {
      headers.value = [{ id: '1', key: '', value: '', description: '', enabled: true }];
    }
  },
  { immediate: true } // cập nhật ngay khi mount
)

</script>

<template>
  <div class="min-h-[310px]">


    <div class="border border-gray-300 rounded-lg overflow-hidden shadow-sm">
      <!-- Header Row -->
      <div
        class="grid grid-cols-12 gap-2 text-xs font-semibold text-gray-700 bg-gray-50 px-2 py-2 border-b border-gray-200">
        <div class="col-span-1"></div>
        <div class="col-span-4">KEY</div>
        <div class="col-span-4">VALUE</div>
        <div class="col-span-2">DESCRIPTION</div>
        <div class="col-span-1"></div>
      </div>

      <!-- Headers List -->
      <div class="max-h-[300px] overflow-y-auto divide-y divide-gray-200">
        <div v-for="(header, index) in headers" :key="header.id"
          class="grid grid-cols-12 gap-2 items-center group hover:bg-blue-50/30 p-2 transition-colors relative">
          <!-- Checkbox -->
          <div class="col-span-1 flex items-center justify-center">
            <input v-model="header.enabled" type="checkbox"
              class="w-4 h-4 text-blue-600 rounded focus:ring-blue-500 cursor-pointer" />
          </div>

          <!-- Key Input with Autocomplete -->
          <div class="col-span-4 relative">
            <input v-model="header.key" type="text" placeholder="Header name" @input="onKeyInput(header, index)"
              @focus="onKeyInput(header, index)" @blur="handleBlur('header')"
              class="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent font-mono"
              :class="{ 'bg-gray-50': !header.enabled }" :disabled="!header.enabled" />

            <!-- Header Suggestions Dropdown -->
            <div v-if="showHeaderSuggestions && activeHeaderIndex === index && activeHeaderSuggestions.length > 0"
              class="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg max-h-48 overflow-y-auto">
              <button v-for="suggestion in activeHeaderSuggestions" :key="suggestion"
                @click="selectHeaderSuggestion(suggestion, index)"
                class="w-full text-left px-3 py-2 text-sm hover:bg-blue-50 transition-colors font-mono border-b border-gray-100 last:border-b-0">
                {{ suggestion }}
              </button>
            </div>
          </div>

          <!-- Value Input with Autocomplete -->
          <div class="col-span-4 relative">
            <input v-model="header.value" type="text" placeholder="Value" @input="onValueInput(header, index)"
              @focus="onValueInput(header, index)" @blur="handleBlur('value')"
              class="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent font-mono"
              :class="{ 'bg-gray-50': !header.enabled }" :disabled="!header.enabled" />

            <!-- Value Suggestions Dropdown -->
            <div v-if="showValueSuggestions && activeHeaderIndex === index && activeValueSuggestions.length > 0"
              class="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg max-h-48 overflow-y-auto">
              <button v-for="suggestion in activeValueSuggestions" :key="suggestion"
                @click="selectValueSuggestion(suggestion, index)"
                class="w-full text-left px-3 py-2 text-sm hover:bg-blue-50 transition-colors font-mono border-b border-gray-100 last:border-b-0">
                {{ suggestion }}
              </button>
            </div>
          </div>

          <!-- Description Input -->
          <div class="col-span-2">
            <input v-model="header.description" type="text" placeholder="Description"
              class="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              :class="{ 'bg-gray-50': !header.enabled }" :disabled="!header.enabled" />
          </div>

          <!-- Delete Button -->
          <div class="col-span-1 flex items-center justify-center">
            <button @click="removeHeader(header.id)"
              class="opacity-0 group-hover:opacity-100 p-1 text-gray-400 hover:text-red-600 transition-all"
              :class="{ 'opacity-50': !header.enabled }">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>


  </div>
</template>