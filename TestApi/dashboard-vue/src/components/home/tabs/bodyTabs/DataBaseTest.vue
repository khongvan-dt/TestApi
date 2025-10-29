<!-- DataBaseTest.vue -->
<script setup lang="ts">
import { ref, watch, nextTick } from 'vue'

const props = defineProps<{
  dataBaseTest?: string
}>()

const body = ref<string>('')
const isEditing = ref(false)
const editContent = ref<string>('')

// Khởi tạo nội dung
watch(() => props.dataBaseTest, (newVal) => {
  const content = newVal ?? ''
  body.value = content
  editContent.value = content
}, { immediate: true })

// Bắt đầu chỉnh sửa
const startEdit = () => {
  isEditing.value = true
  editContent.value = body.value
  nextTick(() => {
    const textarea = document.getElementById('db-test-editor') as HTMLTextAreaElement
    textarea?.focus()
  })
}

// Lưu
const saveEdit = () => {
  body.value = editContent.value
  isEditing.value = false
  // Gửi lên BodyTab
  if (typeof defineExpose === 'function') {
    // Sẽ expose getBody ở dưới
  }
}

// Hủy
const cancelEdit = () => {
  editContent.value = body.value
  isEditing.value = false
}

// Expose để BodyTab lấy dữ liệu
defineExpose({
  getBody: () => body.value,
  getBodyType: () => 'base-data',
  setDataBaseTest: (val: string) => {
    body.value = val
    editContent.value = val
  }
})
</script>

<template>
  <div class="h-[360px] p-3 bg-gray-50 border rounded-lg flex flex-col">
    <!-- Header: Nút Edit / Save / Cancel -->
    <div class="flex justify-end gap-2 mb-2">
      <button
        v-if="!isEditing"
        @click="startEdit"
        class="px-3 py-1 text-xs font-medium text-blue-600 bg-blue-50 rounded hover:bg-blue-100 transition-colors"
      >
        Edit
      </button>

      <div v-else class="flex gap-1">
        <button
          @click="saveEdit"
          class="px-3 py-1 text-xs font-medium text-green-600 bg-green-50 rounded hover:bg-green-100 transition-colors"
        >
          Save
        </button>
        <button
          @click="cancelEdit"
          class="px-3 py-1 text-xs font-medium text-gray-600 bg-gray-100 rounded hover:bg-gray-200 transition-colors"
        >
          Cancel
        </button>
      </div>
    </div>

    <!-- Nội dung -->
    <div class="flex-1 overflow-hidden">
      <!-- Chế độ Xem -->
      <pre
        v-if="!isEditing"
        class="text-xs font-mono text-gray-700 whitespace-pre-wrap break-words h-full overflow-y-auto p-2 bg-white rounded border"
      >{{ body || 'No DataBaseTest content' }}</pre>

      <!-- Chế độ Sửa -->
      <textarea
        v-else
        id="db-test-editor"
        v-model="editContent"
        class="w-full h-full p-2 text-xs font-mono text-gray-700 bg-white border rounded resize-none focus:outline-none focus:ring-2 focus:ring-blue-500"
        spellcheck="false"
      />
    </div>
  </div>
</template>

<style scoped>
textarea {
  line-height: 1.4;
}
</style>