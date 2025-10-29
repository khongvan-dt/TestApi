<script setup lang="ts">
import { ref,watch } from 'vue'

interface Param {
  id: string
  key: string
  value: string
  description: string
  enabled: boolean
}

const params = ref<Param[]>([
  { id: '1', key: '', value: '', description: '', enabled: true }
])
const props = defineProps<{
  paramsData: Array<{ key: string; value: string }> | null
}>();


const addParam = () => {
  params.value.push({
    id: Date.now().toString(),
    key: '',
    value: '',
    description: '',
    enabled: true
  })
}

const removeParam = (id: string) => {
  params.value = params.value.filter(p => p.id !== id)
  if (params.value.length === 0) {
    addParam()
  }
}

const onKeyInput = (param: Param) => {
  const isLast = params.value[params.value.length - 1].id === param.id
  if (isLast && param.key.trim() !== '') {
    addParam()
  }
}

defineExpose({
  getParams: () => params.value.filter(p => p.enabled && p.key)
})
watch(
  () => props.paramsData,
  (newParams) => {
    if (newParams && newParams.length > 0) {
      params.value = newParams.map((p, idx) => ({
        id: Date.now().toString() + idx,
        key: p.key,
        value: p.value,
        description: '',
        enabled: true
      }));
    } else {
      params.value = [{ id: '1', key: '', value: '', description: '', enabled: true }];
    }
  },
  { immediate: true }
)

</script>


<template>
  <div class="min-h-[310px]">

    <div class="border border-gray-300 rounded-lg overflow-hidden shadow-sm">
      <!-- Header Row -->
      <div
        class="grid grid-cols-12 gap-2 text-xs font-medium text-gray-600 bg-gray-50 px-2 py-2 border-b border-gray-200">
        <div class="col-span-1"></div>
        <div class="col-span-4">Key</div>
        <div class="col-span-4">Value</div>
        <div class="col-span-2">Description</div>
        <div class="col-span-1"></div>
      </div>

      <!-- Params List -->
      <div class="max-h-[300px] overflow-y-auto divide-y divide-gray-200">
        <div v-for="param in params" :key="param.id"
          class="grid grid-cols-12 gap-2 items-center group hover:bg-gray-50 p-2 transition-colors">
          <div class="col-span-1 flex items-center justify-center">

            <input v-model="param.enabled" type="checkbox"
              class="w-4 h-4 text-blue-600 rounded focus:ring-blue-500 cursor-pointer" />
          </div>

          <!-- Key Input -->
          <div class="col-span-4">
            <input v-model="param.key" type="text" placeholder="Key" @input="onKeyInput(param)"
              class="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent" />
          </div>


          <!-- Value Input -->
          <div class="col-span-4">
            <input v-model="param.value" type="text" placeholder="Value"
              class="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent" />
          </div>

          <!-- Description Input -->
          <div class="col-span-2">
            <input v-model="param.description" type="text" placeholder="Description"
              class="w-full px-2 py-1.5 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent" />
          </div>

          <!-- Delete Button -->
          <div class="col-span-1 flex items-center justify-center">
            <button @click="removeParam(param.id)"
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