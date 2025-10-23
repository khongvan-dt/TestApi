<script setup lang="ts">
import { ref } from 'vue'

interface Header {
  id: string
  key: string
  value: string
  description: string
  enabled: boolean
}

const headers = ref<Header[]>([
  { id: '1', key: '', value: '', description: '', enabled: true },
])

const headerSuggestions = [
  'Content-Type',
  'Accept',
  'Authorization',
  'User-Agent',
  'Cache-Control',
  'Accept-Encoding',
  'Accept-Language',
  'Connection',
  'Host',
  'Referer',
  'Origin',
  'Cookie',
  'X-Requested-With',
  'X-API-Key',
  'Access-Control-Allow-Origin'
]

const availableSuggestions = (currentId: string) => {
  const usedKeys = headers.value
    .filter(h => h.key && h.id !== currentId)
    .map(h => h.key)
  return headerSuggestions.filter(s => !usedKeys.includes(s))
}

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
  if (headers.value.length === 0) addHeader()
}

 const onKeyInput = (header: Header) => {
  const isLast = headers.value[headers.value.length - 1].id === header.id
  if (isLast && header.key.trim() !== '') {
    addHeader()
  }
}

defineExpose({
  getHeaders: () => headers.value.filter(h => h.enabled && h.key)
})
</script>


<template>
  <div class="min-h-[310px]">


    <div class="border border-gray-300 rounded-lg overflow-hidden shadow-sm">
      <div
        class="grid grid-cols-12 gap-2 text-xs font-medium text-gray-600 bg-gray-50 px-2 py-2 border-b border-gray-200">
        <div class="col-span-1"></div>
        <div class="col-span-4">Key</div>
        <div class="col-span-4">Value</div>
        <div class="col-span-2">Description</div>
        <div class="col-span-1"></div>
      </div>
      <div class="max-h-[250px] overflow-y-auto divide-y divide-gray-200">
        <div v-for="header in headers" :key="header.id"
          class="grid grid-cols-12 gap-2 items-center group hover:bg-gray-50 p-2 transition-colors">

          <!-- Checkbox -->
          <div class="col-span-1 flex items-center justify-center">
            <input v-model="header.enabled" type="checkbox"
              class="w-4 h-4 text-blue-600 rounded focus:ring-blue-500 cursor-pointer" />

          </div>

          <!-- Key Input -->
          <div class="col-span-4">
            <input v-model="header.key" type="text" placeholder="Key" @input="onKeyInput(header)"
              :list="`header-suggestions-${header.id}`" :class="[
                'w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent placeholder-gray-400',
                header.enabled ? 'text-gray-900' : 'text-gray-500'
              ]" />
            <datalist :id="`header-suggestions-${header.id}`">
              <option v-for="item in availableSuggestions(header.id)" :key="item" :value="item" />
            </datalist>
          </div>



          <!-- Value Input -->
          <div class="col-span-4">
            <input v-model="header.value" type="text" placeholder="Value" :class="[
              'w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent placeholder-gray-400',
              header.enabled ? 'text-gray-900' : 'text-gray-500'
            ]" />
          </div>

          <!-- Description Input -->
          <div class="col-span-2">
            <input v-model="header.description" type="text" placeholder="Description" :class="[
              'w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent placeholder-gray-400',
              header.enabled ? 'text-gray-900' : 'text-gray-500'
            ]" />
          </div>

          <!-- Delete Button -->
          <div class="col-span-1 flex items-center justify-center">
            <button @click="removeHeader(header.id)"
              class="opacity-0 group-hover:opacity-100 p-1 text-gray-400 hover:text-red-600 transition-all">
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
