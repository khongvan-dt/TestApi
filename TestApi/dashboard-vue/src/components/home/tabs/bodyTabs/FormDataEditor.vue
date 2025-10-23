<script setup lang="ts">
import { ref } from 'vue'

interface FormDataItem {
  id: string
  key: string
  type: 'text' | 'file'
  value: string | File | null
  description: string
  enabled: boolean
}

const formDataItems = ref<FormDataItem[]>([
  { id: '1', key: '', type: 'text', value: '', description: '', enabled: true }
])

const addFormData = () => {
  formDataItems.value.push({
    id: Date.now().toString(),
    key: '',
    type: 'text',
    value: '',
    description: '',
    enabled: true
  })
}

const removeFormData = (id: string) => {
  formDataItems.value = formDataItems.value.filter(i => i.id !== id)
  if (formDataItems.value.length === 0) addFormData()
}

const handleFileChange = (e: Event, item: FormDataItem) => {
  const target = e.target as HTMLInputElement
  if (target.files && target.files.length > 0) item.value = target.files[0]
  else item.value = null
}

// auto add when typing key in last row
const onKeyInput = (item: FormDataItem) => {
  const last = formDataItems.value[formDataItems.value.length - 1]
  if (item.id === last.id && String(item.key).trim() !== '') addFormData()
}

// remove empty middle rows (optional helper)
const compactEmptyRows = () => {
  // keep last row empty always
  const last = formDataItems.value[formDataItems.value.length - 1]
  formDataItems.value = formDataItems.value.filter(i => {
    if (i === last) return true
    return !(i.key.trim() === '' && String(i.value || '').toString().trim() === '')
  })
  if (formDataItems.value.length === 0) addFormData()
}

defineExpose({
  getBody: () => {
    const fd = new FormData()
    formDataItems.value.filter(i => i.enabled && i.key).forEach(i => {
      if (i.type === 'file' && i.value instanceof File) fd.append(i.key, i.value)
      else if (i.type === 'text') fd.append(i.key, String(i.value ?? ''))
    })
    return fd
  },
  getBodyType: () => 'form-data',
  addFormData,
  removeFormData,
  compactEmptyRows
})
</script>

<template>
  <div class="min-h-[310px]">
    <div class="border border-gray-300 rounded-lg overflow-hidden shadow-sm">
      <div class="grid grid-cols-12 gap-2 text-xs font-medium text-gray-600 bg-gray-50 px-2 py-2 border-b border-gray-200">
        <div class="col-span-1"></div>
        <div class="col-span-3">Key</div>
        <div class="col-span-1">Type</div>
        <div class="col-span-4">Value</div>
        <div class="col-span-2">Description</div>
        <div class="col-span-1"></div>
      </div>

      <div class="max-h-[250px] overflow-y-auto divide-y divide-gray-200">
        <div v-for="item in formDataItems" :key="item.id"
             class="grid grid-cols-12 gap-2 items-center group hover:bg-gray-50 p-2 transition-colors">
          <div class="col-span-1 flex items-center justify-center">
            <input v-model="item.enabled" type="checkbox"
                   class="w-3.5 h-3.5 text-blue-600 rounded focus:ring-blue-500 cursor-pointer"/>
          </div>

          <div class="col-span-3">
            <input v-model="item.key" type="text" placeholder="Key" @input="onKeyInput(item)"
                   class="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 placeholder-gray-400"/>
          </div>

          <div class="col-span-1">
            <select v-model="item.type"
                    class="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white">
              <option value="text">Text</option>
              <option value="file">File</option>
            </select>
          </div>

          <div class="col-span-4">
            <input v-if="item.type === 'text'"
                   v-model="item.value"
                   type="text"
                   placeholder="Value"
                   class="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 placeholder-gray-400"/>

            <div v-else
                 class="relative w-full border border-gray-300 rounded px-2 py-1.5 text-sm text-gray-700 bg-white cursor-pointer hover:border-blue-400 focus-within:ring-2 focus-within:ring-blue-300"
                 @click="() => $refs['fileInput' + item.id]?.click()">
              <span class="truncate block max-w-full">
                {{ item.value instanceof File ? item.value.name : 'Choose file...' }}
              </span>
              <input :ref="'fileInput' + item.id" type="file" class="hidden" @change="e => handleFileChange(e, item)"/>
              <button v-if="item.value instanceof File" @click.stop="item.value = null"
                      class="absolute right-2 top-1/2 -translate-y-1/2 text-gray-400 hover:text-red-500">✕</button>
            </div>
          </div>

          <div class="col-span-2">
            <input v-model="item.description" type="text" placeholder="Description"
                   class="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 placeholder-gray-400"/>
          </div>

          <div class="col-span-1 flex items-center justify-center">
            <button @click="removeFormData(item.id)"
                    class="opacity-0 group-hover:opacity-100 p-1 text-gray-400 hover:text-red-600 transition-all">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>
