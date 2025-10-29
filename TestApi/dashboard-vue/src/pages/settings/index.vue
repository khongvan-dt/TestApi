<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useUserData } from '../../composables/useUserData'

const { data, loading, error, fetchUserData } = useUserData()

// === API DATA ===
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


// === STEP LOGIC ===
const currentStep = ref(1)
const totalSteps = 4  // ← 4 bước, 4 vòng tròn

// Bước 1: Collection
const selectedCollectionId = ref<number | null>(null)

// Bước 2: Requests
interface RequestItem {
  id: number
  name: string
  method: 'GET' | 'POST' | 'PUT' | 'DELETE'
  url: string
  selected: boolean
}
const selectedRequests = reactive<RequestItem[]>([])

// Bước 3: Job
const job = reactive({
  name: '',
  description: '',
  schedule: 'daily'
})

// === COMPUTED ===
const selectedCollection = computed(() => {
  return data.value?.collections?.find((c: any) => c.id === selectedCollectionId.value) || null
})

const currentCollectionRequests = computed(() => {
  if (!selectedCollection.value) return []
  return selectedRequests.filter(r =>
    selectedCollection.value.requests.some((req: any) => req.id === r.id)
  )
})

const confirmData = computed(() => ({
  ...job,
  collection: selectedCollection.value,
  requests: selectedRequests.filter(r => r.selected)
}))

const isNextDisabled = computed(() => {
  if (currentStep.value === 1) return !selectedCollectionId.value
  if (currentStep.value === 2) return selectedRequests.filter(r => r.selected).length === 0
  if (currentStep.value === 3) return !job.name.trim()
  return false
})

// === NAVIGATION ===
const nextStep = () => {
  if (isNextDisabled.value) return
  if (currentStep.value < totalSteps) currentStep.value++
}

const prevStep = () => {
  if (currentStep.value > 1) currentStep.value--
}

const saveJob = () => {
  console.log('Job saved:', confirmData.value)
  alert('Lưu job thành công!')
}

// === HELPER ===
const getMethodClasses = (method: string) => {
  const map: Record<string, string> = {
    GET: 'bg-blue-500',
    POST: 'bg-green-500',
    PUT: 'bg-yellow-500',
    DELETE: 'bg-red-500'
  }
  return map[method] || 'bg-gray-500'
}

const formatSchedule = (value: string) => {
  return value === 'daily' ? 'Hàng ngày' : value === 'weekly' ? 'Hàng tuần' : 'Hàng tháng'
}

// === INIT ===
onMounted(async () => {
  await fetchUserData()

  if (data.value?.collections) {
    const allRequests: RequestItem[] = []
    data.value.collections.forEach((collection: any) => {
      collection.requests.forEach((req: any) => {
        allRequests.push({
          id: req.id,
          name: req.name,
          method: req.method,
          url: req.url,
          selected: false
        })
      })
    })
    selectedRequests.splice(0, selectedRequests.length, ...allRequests)
  }
})

// === PROGRESS BAR LABELS – 4 BƯỚC ===
const stepLabels = ['Chọn collection & API', 'Chọn Requests', 'Cấu hình Job', 'Xác nhận']
</script>

<template>
  <div class="">

    <!-- HEADER + PROGRESS BAR (4 BƯỚC) -->
    <div class="mb-10">

      <!-- 4 VÒNG TRÒN -->
      <div class="flex items-center justify-between">
        <div
          v-for="(label, idx) in stepLabels"
          :key="idx"
          class="flex-1 flex flex-col items-center relative"
        >
          <!-- Circle -->
          <div
             :class="[
              'w-10 h-10 rounded-full flex items-center justify-center text-white font-bold text-lg transition-all duration-300 shadow-md ',
              currentStep > idx + 1 ? 'bg-green-500 ' :
              currentStep === idx + 1 ? 'bg-blue-600' : 'bg-gray-300'
            ]"
          >
            <span v-if="currentStep > idx + 1">{{ idx + 1 }}</span>
            <span v-else>{{ idx + 1 }}</span>
          </div>

          <!-- Label -->
          <p class="mt-2 text-xs font-medium"
             :class="currentStep >= idx + 1 ? 'text-blue-600' : 'text-gray-400'">
            {{ label }}
          </p>

          <!-- Line -->
          <div
            v-if="idx < totalSteps - 1"
            class="absolute top-5 left-full w-full h-1 -translate-x-1/2 z-0 "
            :class="currentStep > idx + 1 ? 'bg-green-500' : 'bg-gray-300'"
          ></div>
        </div>
      </div>

      
    </div>

    <!-- MAIN CARD -->
    <div class="bg-white rounded-2xl shadow-md border border-gray-200 p-8">

      <!-- Loading -->
      <div v-if="loading" class="space-y-6">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div v-for="n in 4" :key="n" class="border rounded-xl p-5 bg-gray-50 animate-pulse">
            <div class="h-6 bg-gray-200 rounded w-3/4 mb-3"></div>
            <div class="h-4 bg-gray-200 rounded w-full mb-2"></div>
            <div class="h-4 bg-gray-200 rounded w-1/2"></div>
          </div>
        </div>
      </div>

      <!-- Error -->
      <div v-else-if="error" class="text-center py-12">
        <div class="bg-red-50 border border-red-200 text-red-700 p-6 rounded-xl max-w-md mx-auto">
          <p class="font-semibold">Lỗi kết nối</p>
          <p class="text-sm mt-1">{{ error }}</p>
        </div>
      </div>

      <!-- 4 BƯỚC -->
      <div v-else-if="data?.collections?.length">

        <!-- BƯỚC 1: Chọn Collection -->
        <div v-if="currentStep === 1" class="space-y-6">
           <p class="text-sm text-gray-600">Chọn collection chứa các API bạn muốn kiểm tra</p>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <label v-for="c in data.collections" :key="c.id" class="cursor-pointer">
              <input :id="'c'+c.id" type="radio" name="col" :value="c.id" v-model="selectedCollectionId" class="sr-only" />
              <div :class="[
                'border-2 rounded-xl p-5 transition-all hover:shadow-md',
                selectedCollectionId === c.id ? 'border-blue-500 bg-blue-50 shadow-md' : 'border-gray-200 bg-white'
              ]">
                <div class="flex justify-between items-start">
                  <div>
                    <h4 class="font-semibold text-gray-800">{{ c.name }}</h4>
                    <p class="text-sm text-gray-600 line-clamp-1">{{ c.description }}</p>
                    <p class="text-xs text-gray-500 mt-2">{{ c.requests.length }} request(s)</p>
                  </div>
                  <svg v-if="selectedCollectionId === c.id" class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7" />
                  </svg>
                </div>
              </div>
            </label>
          </div>
        </div>

        <!-- BƯỚC 2: Chọn Requests -->
        <div v-else-if="currentStep === 2 && selectedCollection" class="space-y-6">
           <p class="text-sm text-gray-600">
            Collection: <strong>{{ selectedCollection.name }}</strong>
            <span class="text-gray-500 ml-2">({{ currentCollectionRequests.length }} request(s))</span>
          </p>
          <div class="space-y-3">
            <label v-for="r in currentCollectionRequests" :key="r.id"
                   class="flex items-center gap-3 p-3 border rounded-lg cursor-pointer hover:shadow-sm"
                   :class="r.selected ? 'border-blue-500 bg-blue-50' : 'border-gray-200 bg-white'">
              <input :id="'r'+r.id" type="checkbox" v-model="r.selected" class="w-5 h-5 text-blue-600 rounded" />
              <span :class="['px-2 py-1 rounded text-white font-bold text-xs', getMethodClasses(r.method)]">
                {{ r.method }}
              </span>
              <div class="flex-1">
                <p class="font-medium text-gray-700">{{ r.name }}</p>
                <p class="text-xs text-gray-500 truncate">{{ r.url }}</p>
              </div>
            </label>
          </div>
        </div>

        <!-- BƯỚC 3: Cấu hình Job -->
        <div v-else-if="currentStep === 3" class="space-y-6">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
            
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Tên Job <span class="text-red-500">*</span></label>
              <input v-model="job.name" type="text" placeholder="VD: Kiểm tra API hàng ngày"
                     class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500" />
              <p v-if="!job.name.trim()" class="text-xs text-red-500 mt-1">Vui lòng nhập tên job</p>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Lịch chạy</label>
              <select v-model="job.schedule" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg bg-white focus:ring-2 focus:ring-blue-500">
                <option value="daily">Hàng ngày</option>
                <option value="weekly">Hàng tuần</option>
                <option value="monthly">Hàng tháng</option>
              </select>
            </div>
            <div class="md:col-span-2">
              <label class="block text-sm font-medium text-gray-700 mb-1">Mô tả</label>
              <textarea v-model="job.description" placeholder="Mô tả ngắn gọn về job..."
                        class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 resize-none h-24" />
            </div>
          </div>
        </div>

        <!-- BƯỚC 4: Xác nhận -->
        <div v-else-if="currentStep === 4" class="space-y-6">
          <h3 class="text-xl font-bold text-gray-800">Xem lại thông tin trước khi lưu</h3>
 
          <div class="bg-blue-50 border border-blue-200 rounded-xl p-5 space-y-3">
            <h4 class="font-bold text-blue-800 text-lg border-b border-blue-300 pb-2">Thông tin Job</h4>
            <p><strong>Tên:</strong> {{ job.name || '[Chưa đặt]' }}</p>
            <p><strong>Lịch:</strong> {{ formatSchedule(job.schedule) }}</p>
            <p><strong>Mô tả:</strong> {{ job.description || '[Không có]' }}</p>
          </div>

          <div class="bg-gray-50 border border-gray-300 rounded-xl p-5">
            <h4 class="font-bold text-gray-800 mb-3">APIs đã chọn ({{ confirmData.requests.length }})</h4>
            <div class="space-y-2">
              <div v-for="r in confirmData.requests" :key="r.id"
                   class="bg-white border border-gray-200 rounded-lg p-3 flex items-center gap-3">
                <span :class="['px-2 py-1 rounded text-white font-bold text-xs', getMethodClasses(r.method)]">
                  {{ r.method }}
                </span>
                <div>
                  <p class="font-medium text-gray-700 text-sm">{{ r.name }}</p>
                  <p class="text-xs text-gray-500">{{ r.url }}</p>
                </div>
              </div>
              <p v-if="confirmData.requests.length === 0" class="text-red-500 italic text-center py-3">
                Chưa chọn API nào
              </p>
            </div>
          </div>
        </div>

      </div>

      <!-- Không có dữ liệu -->
      <div v-else class="text-center py-16 text-gray-500">
        Không tìm thấy collection nào.
      </div>

      <!-- NAVIGATION -->
      <div class="mt-8 pt-6 border-t border-gray-200 flex justify-between items-center">
        <button @click="prevStep" :disabled="currentStep === 1"
                :class="currentStep === 1 ? 'bg-gray-100 text-gray-400 cursor-not-allowed' : 'bg-gray-200 text-gray-700 hover:bg-gray-300'"
                class="px-5 py-2.5 rounded-lg font-medium flex items-center gap-2 transition">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
          </svg>
          Quay lại
        </button>

        <div>
          <button v-if="currentStep < totalSteps" @click="nextStep" :disabled="isNextDisabled"
                  class="px-6 py-2.5 rounded-lg font-medium bg-blue-600 text-white hover:bg-blue-700 disabled:bg-blue-300 disabled:cursor-not-allowed transition">
            Tiếp theo
          </button>

          <button v-else @click="saveJob"
                  class="px-8 py-2.5 bg-green-600 text-white rounded-lg font-medium hover:bg-green-700 flex items-center gap-2 transition">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V8"/>
            </svg>
            Lưu Job
          </button>
        </div>
      </div>
    </div>
  </div>
</template>