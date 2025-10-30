<script setup lang="ts">
import { ref, watch } from 'vue'
import { UpdateTestdataRequest } from '../../../../composables/useRequest'

interface Props {
  dataBaseTest?: string | null
  requestId?: number | null
}

const props = defineProps<Props>()

const emit = defineEmits<{ (e: 'update', value: string): void }>()

const dbContent = ref(props.dataBaseTest || '')
const editContent = ref(dbContent.value)
const isEditing = ref(false)
const isLoading = ref(false)
const errorMessage = ref('')

watch(() => props.dataBaseTest, (val) => {
  dbContent.value = val || ''
  if (!isEditing.value) editContent.value = dbContent.value
})

const startEdit = () => {
  isEditing.value = true
  editContent.value = dbContent.value
}

const cancelEdit = () => {
  isEditing.value = false
  editContent.value = dbContent.value
}

const saveEdit = async () => {
  if (!props.requestId) {
    errorMessage.value = 'Thiếu dữ liệu requestId.'
    return
  }

  try {
    isLoading.value = true
    errorMessage.value = ''

    // Gọi API mới
    const result = await UpdateTestdataRequest({
      requestId: props.requestId,
      newTestDataContent: editContent.value
    })

    // result là { requestId, newTestDataContent } → thành công
    dbContent.value = editContent.value
    emit('update', dbContent.value)
    isEditing.value = false

  } catch (err: any) {
    // Xử lý lỗi từ API
    console.error('Lưu dataBaseTest thất bại:', err)
    errorMessage.value = err.message || 'Lỗi hệ thống khi lưu Database Test.'
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <div class="h-[360px] p-3 bg-gray-50 border rounded-lg flex flex-col">
    <div class="flex justify-end gap-2 mb-2">
      <button v-if="!isEditing" @click="startEdit"
        class="px-3 py-1 text-xs font-medium text-blue-600 bg-blue-50 rounded hover:bg-blue-100 transition-colors">
        Edit
      </button>
      <div v-else class="flex gap-1">
        <button @click="saveEdit" :disabled="isLoading"
          class="px-3 py-1 text-xs font-medium text-green-600 bg-green-50 rounded hover:bg-green-100 transition-colors">
          {{ isLoading ? 'Saving...' : 'Save' }}
        </button>
        <button @click="cancelEdit" :disabled="isLoading"
          class="px-3 py-1 text-xs font-medium text-gray-600 bg-gray-100 rounded hover:bg-gray-200 transition-colors">
          Cancel
        </button>
      </div>
    </div>

    <div class="flex-1 overflow-hidden">
      <p v-if="errorMessage" class="text-xs text-red-500 mb-1">{{ errorMessage }}</p>

      <pre v-if="!isEditing"
        class="text-xs font-mono text-gray-700 whitespace-pre-wrap break-words h-full overflow-y-auto p-2 bg-white rounded border"> {{ dbContent || 'No DataBaseTest content' }} </pre>

      <textarea v-else id="db-test-editor" v-model="editContent"
        class="w-full h-full p-2 text-xs font-mono text-gray-700 bg-white border rounded resize-none focus:outline-none focus:ring-2 focus:ring-blue-500"
        spellcheck="false" />
    </div>
  </div>
</template>
