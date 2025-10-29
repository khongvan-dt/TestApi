<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useUserData } from '../../composables/useUserData'

const { data, loading, error, fetchUserData } = useUserData()

// Step state
const currentStep = ref(1)

// Step 1: Chọn Collection
const selectedCollectionId = ref<number | null>(null)

// Step 2: Chọn Requests
interface RequestItem {
  id: number
  name: string
  method: 'GET' | 'POST' | 'PUT' | 'DELETE'
  url: string
  selected: boolean
}

const selectedRequests = reactive<RequestItem[]>([])

// Step 3: Cấu hình Job
const job = reactive({
  name: '',
  description: '',
  schedule: 'daily'
})

// Step 4: Xác nhận
const confirmData = computed(() => ({
  ...job,
  collection: data.value?.collections.find(c => c.id === selectedCollectionId.value) || null,
  requests: selectedRequests.filter(r => r.selected)
}))

// Navigation
const nextStep = () => {
  if (currentStep.value === 1 && !selectedCollectionId.value) return
  if (currentStep.value === 2 && selectedRequests.filter(r => r.selected).length === 0) return
  if (currentStep.value === 3 && !job.name.trim()) return
  if (currentStep.value < 4) currentStep.value++
}

const prevStep = () => {
  if (currentStep.value > 1) currentStep.value--
}

const saveJob = () => {
  alert('Job saved successfully! Check console for data.')
  console.log('Final Job Data:', confirmData.value)
}

// Helper classes
const getMethodClasses = (method: string) => {
  const map: Record<string, string> = {
    GET: 'bg-blue-500',
    POST: 'bg-green-500',
    PUT: 'bg-yellow-500',
    DELETE: 'bg-red-500'
  }
  return map[method] || 'bg-gray-500'
}

// Load data + khởi tạo requests
onMounted(async () => {
  await fetchUserData()

  if (!data.value?.collections) return

  // Khởi tạo danh sách requests có thể chọn
  const allRequests: RequestItem[] = []
  data.value.collections.forEach(collection => {
    collection.requests.forEach(req => {
      allRequests.push({
        id: req.id,
        name: req.name,
        method: req.method as 'GET' | 'POST' | 'PUT' | 'DELETE',
        url: req.url,
        selected: false
      })
    })
  })

  // Gán vào reactive array
  selectedRequests.splice(0, selectedRequests.length, ...allRequests)
})

// Computed
const selectedCollection = computed(() => {
  return data.value?.collections.find(c => c.id === selectedCollectionId.value) || null
})

const currentCollectionRequests = computed(() => {
  if (!selectedCollection.value) return []
  return selectedRequests.filter(r =>
    selectedCollection.value!.requests.some(req => req.id === r.id)
  )
})
</script>
<template>
  <div class="">

    

    <!-- Loading Skeleton -->
    <div v-if="loading" class="space-y-6">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div v-for="n in 4" :key="n" class="border rounded-xl p-5 bg-white shadow-sm animate-pulse">
          <div class="h-6 bg-gray-200 rounded w-3/4 mb-3"></div>
          <div class="h-4 bg-gray-200 rounded w-full mb-2"></div>
          <div class="h-4 bg-gray-200 rounded w-1/2"></div>
        </div>
      </div>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="text-center py-16">
      <div class="bg-red-50 border border-red-200 text-red-700 p-6 rounded-xl max-w-md mx-auto">
        <p class="font-semibold">Lỗi kết nối</p>
        <p class="text-sm mt-1">{{ error }}</p>
      </div>
    </div>

    <!-- Main Content -->
    <div v-else-if="data?.collections" >

      <!-- Bước 1: Chọn Collection -->
      <div v-if="currentStep === 1" class="space-y-5">
        <h3 class="text-2xl font-bold text-gray-800 flex items-center gap-2">
          <span class="w-8 h-8 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center text-sm font-bold">1</span>
          Chọn Collection
        </h3>
        <p class="text-gray-600">Chọn một collection để bắt đầu cấu hình job.</p>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
          <label
            v-for="collection in data.collections"
            :key="collection.id"
            :for="'collection-' + collection.id"
            class="group cursor-pointer"
          >
            <input
              :id="'collection-' + collection.id"
              type="radio"
              name="collection"
              :value="collection.id"
              v-model="selectedCollectionId"
              class="sr-only"
            />
            <div
              :class="[
                'border-2 rounded-xl p-5 transition-all duration-300 shadow-sm hover:shadow-md',
                selectedCollectionId === collection.id
                  ? 'border-blue-500 bg-blue-50 shadow-lg scale-105'
                  : 'border-gray-200 bg-white group-hover:border-gray-300'
              ]"
            >
              <div class="flex justify-between items-start">
                <div class="flex-1">
                  <h4 class="font-semibold text-gray-800 text-lg">{{ collection.name }}</h4>
                  <p class="text-sm text-gray-600 mt-1 line-clamp-2">{{ collection.description }}</p>
                  <div class="flex items-center gap-3 mt-3 text-xs text-gray-500">
                    <span class="flex items-center gap-1">
                      <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20"><path d="M10 2a8 8 0 100 16 8 8 0 000-16zM9 13a1 1 0 112 0 1 1 0 010 0zm1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"/></svg>
                      {{ collection.requests.length }} request(s)
                    </span>
                  </div>
                </div>
                <div v-if="selectedCollectionId === collection.id" class="text-blue-600 ml-3">
                  <svg class="w-7 h-7" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7" />
                  </svg>
                </div>
              </div>
            </div>
          </label>
        </div>
      </div>

      <!-- Bước 2: Chọn Requests -->
      <div v-if="currentStep === 2 && selectedCollection" class="space-y-5">
        <h3 class="text-2xl font-bold text-gray-800 flex items-center gap-2">
          <span class="w-8 h-8 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center text-sm font-bold">2</span>
          Chọn Requests
        </h3>
        <p class="text-gray-600">
          Collection: <strong class="text-blue-600">{{ selectedCollection.name }}</strong>
          <span class="text-sm text-gray-500 ml-2">({{ currentCollectionRequests.length }} request(s))</span>
        </p>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <label
            v-for="request in currentCollectionRequests"
            :key="request.id"
            :for="'request-' + request.id"
            class="group cursor-pointer"
          >
            <input
              :id="'request-' + request.id"
              type="checkbox"
              v-model="request.selected"
              class="sr-only"
            />
            <div
              :class="[
                'border-2 rounded-lg p-4 transition-all duration-200 shadow-sm hover:shadow-md',
                request.selected
                  ? 'border-blue-500 bg-blue-50 shadow-md'
                  : 'border-gray-200 bg-white group-hover:border-gray-300'
              ]"
            >
              <div class="flex items-center gap-3">
                <div class="w-5 h-5 rounded border-2 flex items-center justify-center transition-colors"
                     :class="request.selected ? 'bg-blue-600 border-blue-600' : 'border-gray-300'">
                  <svg v-if="request.selected" class="w-3 h-3 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7"/>
                  </svg>
                </div>

                <span :class="['px-2.5 py-1 rounded text-white font-bold text-xs', getMethodClasses(request.method)]">
                  {{ request.method }}
                </span>

                <div class="flex-1 min-w-0">
                  <p class="font-medium text-gray-800 truncate">{{ request.name }}</p>
                  <p class="text-xs text-gray-500 truncate">{{ request.url }}</p>
                </div>
              </div>
            </div>
          </label>
        </div>

        <p v-if="currentCollectionRequests.length === 0" class="text-gray-500 italic text-center py-8">
          Không có request nào trong collection này.
        </p>
      </div>

      <!-- Bước 3: Cấu hình Job -->
      <div v-if="currentStep === 3" class="space-y-6">
        <h3 class="text-2xl font-bold text-gray-800 flex items-center gap-2">
          <span class="w-8 h-8 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center text-sm font-bold">3</span>
          Cấu hình Job
        </h3>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label class="block font-semibold text-gray-700 mb-2">
              Tên Job <span class="text-red-500">*</span>
            </label>
            <input
              v-model="job.name"
              type="text"
              placeholder="VD: Kiểm tra API hàng ngày"
              class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
            />
            <p v-if="!job.name.trim()" class="text-xs text-red-500 mt-1">Vui lòng nhập tên job.</p>
          </div>

          <div>
            <label class="block font-semibold text-gray-700 mb-2">Lịch chạy</label>
            <select
              v-model="job.schedule"
              class="w-full px-4 py-3 border border-gray-300 rounded-lg bg-white focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
            >
              <option value="daily">Hàng ngày</option>
              <option value="weekly">Hàng tuần</option>
              <option value="monthly">Hàng tháng</option>
            </select>
          </div>

          <div class="md:col-span-2">
            <label class="block font-semibold text-gray-700 mb-2">Mô tả</label>
            <textarea
              v-model="job.description"
              placeholder="Mô tả ngắn gọn về mục đích của job..."
              class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition resize-none h-28"
            />
          </div>
        </div>
      </div>

      <!-- Bước 4: Xác nhận -->
      <div v-if="currentStep === 4" class="space-y-6">
        <h3 class="text-2xl font-bold text-gray-800 flex items-center gap-2">
          <span class="w-8 h-8 bg-green-100 text-green-600 rounded-full flex items-center justify-center text-sm font-bold">✓</span>
          Xác nhận & Lưu
        </h3>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <!-- Job Info -->
          <div class="bg-blue-50 border border-blue-200 rounded-xl p-5 space-y-3">
            <h4 class="font-bold text-blue-800 text-lg border-b border-blue-300 pb-2">Thông tin Job</h4>
            <p><strong>Tên:</strong> <span class="text-gray-700">{{ job.name || '[Chưa đặt]' }}</span></p>
            <p><strong>Lịch:</strong> <span class="text-gray-700 capitalize">{{ formatSchedule(job.schedule) }}</span></p>
            <p><strong>Mô tả:</strong> <span class="text-gray-700">{{ job.description || '[Không có]' }}</span></p>
            <p><strong>Collection:</strong> <span class="text-gray-700">{{ confirmData.collection?.name || 'N/A' }}</span></p>
          </div>

          <!-- Requests -->
          <div class="bg-gray-50 border border-gray-300 rounded-xl p-5">
            <h4 class="font-bold text-gray-800 mb-3">Requests đã chọn ({{ confirmData.requests.length }})</h4>
            <div class="max-h-48 overflow-y-auto space-y-2 pr-2">
              <div v-for="req in confirmData.requests" :key="req.id"
                   class="bg-white border border-gray-200 rounded-lg p-3 flex items-center gap-3 shadow-sm">
                <span :class="['px-2.5 py-1 rounded text-white font-bold text-xs', getMethodClasses(req.method)]">
                  {{ req.method }}
                </span>
                <span class="text-sm text-gray-700 truncate">{{ req.name }}</span>
              </div>
              <p v-if="confirmData.requests.length === 0" class="text-red-500 text-sm italic text-center py-4">
                Chưa chọn request nào.
              </p>
            </div>
          </div>
        </div>
      </div>

    </div>

    <!-- Không có dữ liệu -->
    <div v-else-if="!data?.collections && !loading && !error" class="text-center py-20">
      <p class="text-gray-500 text-lg">Không tìm thấy collection nào.</p>
    </div>

    <!-- Navigation -->
    <div class="mt-10 pt-6 border-t border-gray-200 flex flex-col sm:flex-row justify-between gap-4">
      <button
        @click="prevStep"
        :disabled="currentStep === 1"
        :class="[
          'px-6 py-3 rounded-lg font-semibold transition-all duration-200 flex items-center gap-2',
          currentStep === 1
            ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
            : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
        ]"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
        </svg>
        Quay lại
      </button>

      <div class="flex gap-3">
        <button
          v-if="currentStep < 4"
          @click="nextStep"
          :disabled="isNextDisabled"
          :class="[
            'px-6 py-3 rounded-lg font-semibold transition-all duration-200 flex items-center gap-2',
            isNextDisabled
              ? 'bg-blue-300 text-white cursor-not-allowed'
              : 'bg-blue-600 text-white hover:bg-blue-700 shadow-lg'
          ]"
        >
          Tiếp theo
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
          </svg>
        </button>

        <button
          v-else
          @click="saveJob"
          class="px-8 py-3 bg-green-600 text-white rounded-lg font-semibold hover:bg-green-700 transition-all duration-200 shadow-lg flex items-center gap-2"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V8"/>
          </svg>
          Lưu Job
        </button>
      </div>
    </div>
  </div>
</template>