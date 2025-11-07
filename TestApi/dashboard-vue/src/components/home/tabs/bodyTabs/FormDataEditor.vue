<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getMySQLConnections } from '../../../../composables/sqlConnectionService'

// ==================== INTERFACE ====================
interface FormDataItem {
  id: string
  key: string
  type: 'text' | 'sql'
  sqlConnectionId?: number | null
  value: string | File | null
  description: string
  enabled: boolean
}

interface Props {
  initialData?: FormDataItem[]
}

// ==================== PROPS ====================
const props = withDefaults(defineProps<Props>(), {
  initialData: () => []
})

// ==================== REACTIVE STATE ====================
const formDataItems = ref<FormDataItem[]>(
  props.initialData && props.initialData.length > 0
    ? [...props.initialData]
    : [{
      id: '1',
      key: '',
      type: 'text' as const,
      value: '',
      description: '',
      enabled: true
    }]
)

const sqlConnections = ref<any[]>([])

// ==================== LIFECYCLE ====================
onMounted(async () => {
  const res = await getMySQLConnections()
  if (res?.success) {
    sqlConnections.value = res.data
  }
})

// ==================== METHODS ====================
function addFormData() {
  formDataItems.value.push({
    id: Date.now().toString(),
    key: '',
    type: 'text' as const,
    sqlConnectionId: null,
    value: '',
    description: '',
    enabled: true
  })
}

function removeFormData(id: string) {
  formDataItems.value = formDataItems.value.filter((i: FormDataItem) => i.id !== id)
  if (formDataItems.value.length === 0) addFormData()
}

function onKeyInput(item: FormDataItem) {
  const last = formDataItems.value[formDataItems.value.length - 1]
  if (item.id === last.id && item.key.trim()) {
    addFormData()
  }
}

function compactEmptyRows() {
  const last = formDataItems.value[formDataItems.value.length - 1]
  formDataItems.value = formDataItems.value.filter((i: FormDataItem) => {
    if (i === last) return true
    return !(i.key.trim() === '' && String(i.value || '').trim() === '')
  })
  if (formDataItems.value.length === 0) addFormData()
}

function getBody(): FormData {
  const fd = new FormData()
  formDataItems.value
    .filter((i: FormDataItem) => i.enabled && i.key)
    .forEach((i: FormDataItem) => {
      if (i.value != null) {
        fd.append(i.key, i.value)
      }
    })
  return fd
}

function getFormDataItems() {

  return formDataItems.value
}

function getBodyTypeWithType() {
  return {
    bodyType: 'form-data',
    type: formDataItems.value[0]?.type
  }
}

function getSQLItems() {
  return formDataItems.value
    .filter((i: FormDataItem) => i.enabled && i.type === 'sql' && i.sqlConnectionId && i.value)
    .map((i: FormDataItem) => ({
      key: i.key,
      connectionId: i.sqlConnectionId!,
      query: i.value as string
    }))
}

function updateFormData(newData: FormDataItem[]) {
  if (newData && newData.length > 0) {
    formDataItems.value = [...newData]
  }
}

// ==================== EXPOSE ====================
defineExpose({
  getBody,
  getBodyType: getBodyTypeWithType,
  addFormData,
  removeFormData,
  compactEmptyRows,
  getSQLItems,
  getFormDataItems,
  updateFormData
})
</script>

<template>
  <div class="min-h-[310px]">
    <div class="border border-gray-300 rounded-lg overflow-hidden shadow-sm">
      <!-- Header -->
      <div
        class="grid grid-cols-12 gap-2 text-xs font-medium text-gray-600 bg-gray-50 px-2 py-2 border-b border-gray-200">
        <div class="col-span-1"></div>
        <div class="col-span-3">Key</div>
        <div class="col-span-1">Type</div>
        <div class="col-span-4">Value</div>
        <div class="col-span-2">Description</div>
        <div class="col-span-1"></div>
      </div>

      <!-- Body -->
      <div class="max-h-[250px] overflow-y-auto divide-y divide-gray-200">
        <div v-for="item in formDataItems" :key="item.id"
          class="grid grid-cols-12 gap-2 items-center group hover:bg-gray-50 p-2 transition-colors">
          <!-- Enabled -->
          <div class="col-span-1 flex items-center justify-center">
            <input v-model="item.enabled" type="checkbox"
              class="w-3.5 h-3.5 text-blue-600 rounded focus:ring-blue-500 cursor-pointer" />
          </div>

          <!-- Key -->
          <div class="col-span-3">
            <input v-model="item.key" type="text" placeholder="Key" @input="onKeyInput(item)"
              class="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 placeholder-gray-400" />
          </div>

          <!-- Type -->
          <div class="col-span-1">
            <select v-model="item.type"
              class="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-2 focus:ring-blue-500 bg-white">
              <option value="text">Text</option>
              <option value="sql">SQL</option>
            </select>
          </div>

          <!-- Value -->
          <div class="col-span-4 flex gap-1">
            <template v-if="item.type === 'sql'">
              <select v-model="item.sqlConnectionId"
                class="w-1/2 px-2 py-1.5 text-sm border border-gray-300 rounded bg-white">
                <option disabled value="">-- Connection --</option>
                <option v-for="conn in sqlConnections" :key="conn.id" :value="conn.id">
                  {{ conn.name }}
                </option>
              </select>
              <input v-model="item.value" placeholder="SQL Query" type="text"
                class="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-2 focus:ring-blue-500" />
            </template>
            <input v-else v-model="item.value" type="text" placeholder="Value"
              class="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-2 focus:ring-blue-500 placeholder-gray-400" />
          </div>

          <!-- Description -->
          <div class="col-span-2">
            <input v-model="item.description" type="text" placeholder="Description"
              class="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:ring-2 focus:ring-blue-500 placeholder-gray-400" />
          </div>

          <!-- Remove -->
          <div class="col-span-1 flex items-center justify-center">
            <button @click="removeFormData(item.id)"
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