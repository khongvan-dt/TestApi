<script setup lang="ts">
import { ref } from 'vue'
import { useUserData, type UserData, type ImportResult } from '../../../composables/useUserData'

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'imported'): void
}>()

// ✅ Sử dụng composable đã có
const { 
  loading, 
  error, 
  exportUserData,
  importUserData,
  downloadAsFile, 
  readFile 
} = useUserData()

const activeTab = ref<'export' | 'import'>('export')
const exportedData = ref<UserData | null>(null)
const importResult = ref<ImportResult | null>(null)
const fileInput = ref<HTMLInputElement>()
const selectedFile = ref<File | null>(null)

// Export
const handleExport = async () => {
  exportedData.value = null
  const data = await exportUserData()
  
  if (data) {
    exportedData.value = data
  }
}

const handleDownload = () => {
  if (!exportedData.value) return
  
  const timestamp = new Date().toISOString().split('T')[0]
  const filename = `api-collections-${timestamp}.json`
  downloadAsFile(exportedData.value, filename)
}

// Import
const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  
  if (file) {
    selectedFile.value = file
    importResult.value = null
  }
}

const handleImport = async () => {
  if (!selectedFile.value) return
  
  importResult.value = null
  
  const data = await readFile(selectedFile.value)
  
  if (!data) return
  
  const result = await importUserData(data)
  
  if (result && result.success) {
    importResult.value = result
    
    // Emit để refresh list
    setTimeout(() => {
      emit('imported')
      emit('close')
    }, 2000)
  }
}

const triggerFileInput = () => {
  fileInput.value?.click()
}
</script>

<template>
  <div class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
    <div class="bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] flex flex-col">
      <!-- Header -->
      <div class="flex items-center justify-between p-6 border-b border-gray-200">
        <h2 class="text-xl font-semibold text-gray-900">Export / Import Collections</h2>
        <button 
          @click="emit('close')"
          class="text-gray-400 hover:text-gray-600 transition-colors"
        >
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <!-- Tabs -->
      <div class="border-b border-gray-200">
        <div class="flex">
          <button
            @click="activeTab = 'export'"
            :class="[
              'flex-1 px-6 py-3 text-sm font-medium transition-colors',
              activeTab === 'export'
                ? 'text-blue-600 border-b-2 border-blue-600 bg-blue-50'
                : 'text-gray-600 hover:text-gray-900 hover:bg-gray-50'
            ]"
          >
            <div class="flex items-center justify-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
              </svg>
              Export Data
            </div>
          </button>
          
          <button
            @click="activeTab = 'import'"
            :class="[
              'flex-1 px-6 py-3 text-sm font-medium transition-colors',
              activeTab === 'import'
                ? 'text-blue-600 border-b-2 border-blue-600 bg-blue-50'
                : 'text-gray-600 hover:text-gray-900 hover:bg-gray-50'
            ]"
          >
            <div class="flex items-center justify-center gap-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
              </svg>
              Import Data
            </div>
          </button>
        </div>
      </div>

      <!-- Content -->
      <div class="flex-1 overflow-y-auto p-6">
        <!-- Error Message -->
        <div v-if="error" class="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg">
          <div class="flex items-start gap-3">
            <svg class="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div class="flex-1">
              <p class="text-sm font-medium text-red-800">Error</p>
              <p class="text-sm text-red-600 mt-1">{{ error }}</p>
            </div>
          </div>
        </div>

        <!-- Export Tab -->
        <div v-if="activeTab === 'export'">
          <div class="text-center">
            <div class="mb-6">
              <div class="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <svg class="w-8 h-8 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
              </div>
              <h3 class="text-lg font-semibold text-gray-900 mb-2">Export Your Collections</h3>
              <p class="text-sm text-gray-600 max-w-md mx-auto">
                Download all your collections, requests, headers, and parameters as a JSON file. 
                You can import this file later or share it with others.
              </p>
            </div>

            <!-- Export Button -->
            <button
              @click="handleExport"
              :disabled="loading"
              class="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors font-medium"
            >
              <span v-if="loading" class="flex items-center gap-2">
                <svg class="animate-spin w-5 h-5" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Exporting...
              </span>
              <span v-else>Export Data</span>
            </button>

            <!-- Export Result -->
            <div v-if="exportedData" class="mt-6 p-4 bg-green-50 border border-green-200 rounded-lg">
              <div class="flex items-start gap-3">
                <svg class="w-5 h-5 text-green-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <div class="flex-1 text-left">
                  <p class="text-sm font-medium text-green-800">Export Successful!</p>
                  <p class="text-sm text-green-700 mt-1">
                    {{ exportedData.summary.totalCollections }} collections with {{ exportedData.summary.totalRequests }} requests
                  </p>
                  <button
                    @click="handleDownload"
                    class="mt-3 px-4 py-2 bg-green-600 text-white text-sm rounded-md hover:bg-green-700 transition-colors font-medium"
                  >
                    Download JSON File
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Import Tab -->
        <div v-if="activeTab === 'import'">
          <div class="text-center">
            <div class="mb-6">
              <div class="w-16 h-16 bg-purple-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <svg class="w-8 h-8 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                </svg>
              </div>
              <h3 class="text-lg font-semibold text-gray-900 mb-2">Import Collections</h3>
              <p class="text-sm text-gray-600 max-w-md mx-auto">
                Upload a JSON file to import collections. Existing collections with the same name will be updated.
              </p>
            </div>

            <!-- File Input (Hidden) -->
            <input
              ref="fileInput"
              type="file"
              accept=".json"
              @change="handleFileSelect"
              class="hidden"
            />

            <!-- Upload Button -->
            <button
              v-if="!selectedFile"
              @click="triggerFileInput"
              class="px-6 py-3 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition-colors font-medium"
            >
              Choose JSON File
            </button>

            <!-- Selected File -->
            <div v-if="selectedFile && !importResult" class="mt-4 p-4 bg-gray-50 border border-gray-200 rounded-lg">
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-3">
                  <svg class="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z" />
                  </svg>
                  <div class="text-left">
                    <p class="text-sm font-medium text-gray-900">{{ selectedFile.name }}</p>
                    <p class="text-xs text-gray-500">{{ (selectedFile.size / 1024).toFixed(2) }} KB</p>
                  </div>
                </div>
                <button
                  @click="selectedFile = null"
                  class="text-gray-400 hover:text-red-600 transition-colors"
                >
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>

              <button
                @click="handleImport"
                :disabled="loading"
                class="mt-4 w-full px-6 py-3 bg-purple-600 text-white rounded-lg hover:bg-purple-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors font-medium"
              >
                <span v-if="loading" class="flex items-center justify-center gap-2">
                  <svg class="animate-spin w-5 h-5" fill="none" viewBox="0 0 24 24">
                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  Importing...
                </span>
                <span v-else>Import Now</span>
              </button>
            </div>

            <!-- Import Result -->
            <div v-if="importResult" class="mt-6 p-4 bg-green-50 border border-green-200 rounded-lg">
              <div class="flex items-start gap-3">
                <svg class="w-5 h-5 text-green-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <div class="flex-1 text-left">
                  <p class="text-sm font-medium text-green-800">Import Successful!</p>
                  <div class="text-sm text-green-700 mt-2 space-y-1">
                    <p>✓ {{ importResult.importedCollections }} new collection(s) added</p>
                    <p v-if="importResult.updatedCollections > 0">✓ {{ importResult.updatedCollections }} collection(s) updated</p>
                    <p>✓ {{ importResult.importedRequests + importResult.updatedRequests }} total request(s) processed</p>
                  </div>
                  <p class="text-xs text-green-600 mt-2">Refreshing in 2 seconds...</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div class="border-t border-gray-200 p-4 bg-gray-50">
        <button
          @click="emit('close')"
          class="w-full px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors font-medium"
        >
          Close
        </button>
      </div>
    </div>
  </div>
</template>