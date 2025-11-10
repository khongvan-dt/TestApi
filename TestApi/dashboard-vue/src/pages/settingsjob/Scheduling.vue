<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useUserData } from '../../composables/useUserData'
import {
  upsertJobSchedule,
  getJobScheduleList,
  getJobScheduleDetail,
  toggleJobScheduleStatus,
  JobSchedulePayload,
  JobApiTestSuiteDto,
  JobScheduleListItem,
  JobScheduleDetailItem
} from '../../composables/userJobApiTestSuite'
import { useToast } from "primevue/usetoast"

const { data, loading, error, fetchUserData } = useUserData()
const toast = useToast()

// === STATE ===
const showDialog = ref(false)
const jobSchedules = ref<JobScheduleListItem[]>([])
const loadingList = ref(false)
const currentStep = ref(1)
const totalSteps = 4
const selectedCollectionId = ref<number | null>(null)
const submitting = ref(false)

// Trạng thái cho View Detail và Toggle Status
const showDetailModal = ref(false)
const detailJob = ref<JobScheduleDetailItem | null>(null)
const togglingJobId = ref<number | null>(null) // ID Job đang được bật/tắt

interface RequestItem {
  id: number
  name: string
  method: 'GET' | 'POST' | 'PUT' | 'DELETE'
  url: string
  selected: boolean
  headers?: any[]
  body?: any
  dataBaseTest?: string
}

const selectedRequests = reactive<RequestItem[]>([])
const job = reactive({
  name: '',
  description: '',
  scheduleType: 'daily' as 'daily' | 'interval',
  dailyTime: '09:00',
  intervalValue: 5 as number | undefined,
  intervalUnit: 'minutes' as 'minutes' | 'hours' | undefined,
})

const stepLabels = ['Chọn collection', 'Chọn Requests', 'Cấu hình Job', 'Xác nhận']

// === LOAD DATA (Giữ nguyên logic) ===
onMounted(async () => {
  await Promise.all([
    fetchUserData(),
    loadJobSchedules()
  ])

  if (!data.value?.collections) return

  const allRequests: RequestItem[] = []
  data.value.collections.forEach((collection: any) => {
    collection.requests.forEach((req: any) => {
      allRequests.push({
        id: req.id,
        name: req.name,
        method: req.method,
        url: req.url,
        selected: false,
        headers: req.headers,
        body: req.body,
        dataBaseTest: req.dataBaseTest
      })
    })
  })
  selectedRequests.splice(0, selectedRequests.length, ...allRequests)
})

const loadJobSchedules = async () => {
  loadingList.value = true
  try {
    jobSchedules.value = await getJobScheduleList()
  } catch (err: any) {
    console.error('Error loading job schedules:', err)
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Không thể tải danh sách Job',
      life: 3000
    })
  } finally {
    loadingList.value = false
  }
}

// === COMPUTED (Giữ nguyên logic) ===
const selectedCollection = computed(() => {
  return data.value?.collections?.find((c: any) => c.id === selectedCollectionId.value) || null
})

const currentCollectionRequests = computed(() => {
  if (!selectedCollection.value) return []
  return selectedRequests.filter(r =>
    selectedCollection.value!.requests.some((req: any) => req.id === r.id)
  )
})

const isNextDisabled = computed(() => {
  if (currentStep.value === 1) return !selectedCollectionId.value
  if (currentStep.value === 2) return selectedRequests.filter(r => r.selected).length === 0
  if (currentStep.value === 3) {
    if (!job.name.trim()) return true
    if (!job.scheduleType) return true
    if (job.scheduleType === 'daily') {
      return !job.dailyTime
    } else if (job.scheduleType === 'interval') {
      return !job.intervalValue || job.intervalValue <= 0 || !job.intervalUnit
    }
  }
  return false
})

// === FUNCTIONS HÀNH ĐỘNG ===

const handleToggleStatus = async (job: JobScheduleListItem) => {
  togglingJobId.value = job.id

  try {
    const result = await toggleJobScheduleStatus(job.id)

    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: result.message || `Job đã được ${result.isActive ? 'kích hoạt' : 'tắt'}`,
      life: 3000
    })

    await loadJobSchedules()
  } catch (err: any) {
    console.error('Error toggling job status:', err)
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: err?.response?.data?.message || 'Không thể thay đổi trạng thái Job',
      life: 3000
    })
  } finally {
    togglingJobId.value = null
  }
}

const viewDetail = async (jobId: number) => {
  loadingList.value = true
  try {
    const detail: JobScheduleDetailItem = await getJobScheduleDetail(jobId);
    detailJob.value = detail;
    showDetailModal.value = true;
  } catch (err) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: 'Không thể tải chi tiết Job',
      life: 3000
    })
  } finally {
    loadingList.value = false
  }
}

const openDialog = () => {
  resetForm()
  showDialog.value = true
}

const closeDialog = () => {
  if (submitting.value) return
  showDialog.value = false
  resetForm()
}

const resetForm = () => {
  currentStep.value = 1
  selectedCollectionId.value = null
  selectedRequests.forEach(r => (r.selected = false))
  job.name = ''
  job.description = ''
  job.scheduleType = 'daily'
  job.dailyTime = '09:00'
  job.intervalValue = 5
  job.intervalUnit = 'minutes'
}

const formatSchedule = (item: JobScheduleListItem) => {
  if (item.scheduleType === 'daily' && item.runAtTime) {
    const time = item.runAtTime.substring(0, 5)
    return `Hằng ngày lúc ${time}`
  }
  if (item.scheduleType === 'interval' && item.intervalMinutes) {
    const hours = Math.floor(item.intervalMinutes / 60)
    const minutes = item.intervalMinutes % 60
    if (hours > 0) return `Mỗi ${hours} giờ ${minutes > 0 ? minutes + ' phút' : ''}`
    return `Mỗi ${minutes} phút`
  }
  return 'Chưa cấu hình'
}

const formatScheduleForConfirm = () => {
  if (job.scheduleType === 'daily') {
    return `Hằng ngày lúc <strong>${job.dailyTime || '[Chưa nhập giờ]'}</strong>`
  }
  if (job.scheduleType === 'interval') {
    if (!job.intervalValue || job.intervalValue <= 0) {
      return `Lặp lại mỗi <strong>[Chưa nhập giá trị]</strong>`
    }
    const unitText = job.intervalUnit === 'minutes' ? 'phút' : 'giờ'
    return `Lặp lại mỗi <strong>${job.intervalValue} ${unitText}</strong>`
  }
  return '[Chưa cấu hình lịch]'
}

const nextStep = () => {
  if (isNextDisabled.value) return
  currentStep.value++
}

const prevStep = () => {
  if (currentStep.value > 1) {
    currentStep.value--
  }
}

const saveJob = async () => {
  if (isNextDisabled.value) {
    toast.add({
      severity: 'warn',
      summary: 'Validation Error',
      detail: 'Vui lòng hoàn thành đầy đủ thông tin.',
      life: 3000
    })
    return
  }

  const selected = selectedRequests.filter(r => r.selected)
  submitting.value = true

  try {
    const suiteDtos: JobApiTestSuiteDto[] = selected.map(r => {
      const headersObj = (r.headers || []).reduce((acc: Record<string, string>, h: any) => {
        if (h.key && h.value) acc[h.key] = h.value
        return acc
      }, {})

      let parsedDataBase = {}
      try { parsedDataBase = r.dataBaseTest ? JSON.parse(r.dataBaseTest) : {} } catch { }
      let parsedBody = {}
      try { parsedBody = r.body?.content ? JSON.parse(r.body.content) : {} } catch { }

      const dataBase = Object.keys(parsedDataBase).length ? parsedDataBase : parsedBody

      return {
        endpoint: r.url,
        method: r.method,
        headers: headersObj,
        dataBase,
        description: r.name,
        testCases: [
          {
            caseName: r.name,
            testData: parsedBody,
            expectedStatus: 200
          }
        ]
      }
    })

    const jobPayload: JobSchedulePayload = {
      name: job.name,
      description: job.description ?? undefined,
      scheduleType: job.scheduleType as 'daily' | 'interval',
      dailyTime: job.scheduleType === 'daily' ? job.dailyTime : undefined,
      intervalValue: job.scheduleType === 'interval' ? job.intervalValue : undefined,
      intervalUnit: job.scheduleType === 'interval'
        ? (job.intervalUnit as 'minutes' | 'hours' | undefined)
        : undefined,
      testSuites: suiteDtos,
    }

    await upsertJobSchedule(jobPayload)

    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: `Lưu Job thành công!`,
      life: 3000
    })

    closeDialog()
    await loadJobSchedules()
  } catch (err: any) {
    console.error('Upsert error', err)
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: err?.response?.data?.message ?? 'Có lỗi khi lưu Job',
      life: 5000
    })
  } finally {
    submitting.value = false
  }
}

const getMethodClasses = (method: string) => {
  const map: Record<string, string> = {
    GET: 'bg-blue-500',
    POST: 'bg-green-500',
    PUT: 'bg-yellow-500',
    DELETE: 'bg-red-500'
  }
  return map[method] || 'bg-gray-500'
}
</script>

<template>
  <div class="w-full">
    <div class="max-w-7xl mx-auto">
      <div class="flex items-center justify-between mb-6">
        <div>
          <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Job Schedule Management</h1>
          <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
            Quản lý lịch chạy tự động và cấu hình kiểm thử API
          </p>
        </div>
        <button @click="openDialog"
          class="bg-blue-600 hover:bg-blue-700 text-white px-5 py-2.5 rounded-lg font-semibold flex items-center gap-2 transition-all shadow-sm hover:shadow-md">
          <i class="pi pi-plus"></i>
          <span>Thêm Job mới</span>
        </button>
      </div>

      <div v-if="loadingList && jobSchedules.length === 0"
        class="bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700 p-12">
        <div class="flex flex-col items-center justify-center">
          <i class="pi pi-spin pi-spinner text-4xl text-blue-600 mb-4"></i>
          <span class="text-gray-600 dark:text-gray-400 font-medium">Đang tải danh sách Job Schedules...</span>
        </div>
      </div>

      <div v-else-if="jobSchedules.length === 0 && !loadingList"
        class="bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
        <div class="text-center py-20 px-6">
          <div class="flex justify-center mb-6">
            <div class="p-5 bg-blue-50 dark:bg-blue-900/20 rounded-full">
              <i class="pi pi-calendar-clock text-6xl text-blue-500 dark:text-blue-400"></i>
            </div>
          </div>
          <h3 class="text-xl font-bold text-gray-900 dark:text-white mb-2">Chưa có Job Schedule nào</h3>
          <p class="text-gray-600 dark:text-gray-400 mb-6 max-w-md mx-auto">
            Tạo Job Schedule đầu tiên để tự động hóa việc kiểm thử API của bạn
          </p>
          <button @click="openDialog"
            class="inline-flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-6 py-3 rounded-lg font-semibold transition-all shadow-sm hover:shadow-md">
            <i class="pi pi-plus"></i>
            <span>Tạo Job đầu tiên</span>
          </button>
        </div>
      </div>

      <div v-else class="grid grid-cols-1 gap-4">
        <div v-for="job in jobSchedules" :key="job.id"
          class="bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700 hover:shadow-lg hover:border-blue-300 dark:hover:border-blue-600 transition-all duration-200 overflow-hidden group">

          <div class="p-6 cursor-pointer" @click="viewDetail(job.id)">
            <div class="flex items-start justify-between gap-4">
              <div class="flex-1 min-w-0">
                <div class="flex items-start gap-4 mb-3">
                  <div class="flex-shrink-0 p-3 bg-gradient-to-br from-blue-500 to-blue-600 rounded-lg shadow-md">
                    <i class="pi pi-calendar-clock text-white text-xl"></i>
                  </div>

                  <div class="flex-1 min-w-0">
                    <div class="flex items-center gap-3 mb-2">
                      <h3 class="text-lg font-bold text-gray-900 dark:text-white truncate">
                        {{ job.name }}
                      </h3>
                      <span :class="[
                        'inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold',
                        job.isActive
                          ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300'
                          : 'bg-gray-100 text-gray-600 dark:bg-gray-700 dark:text-gray-400'
                      ]">
                        <span :class="[
                          'w-2 h-2 rounded-full',
                          job.isActive ? 'bg-green-500 animate-pulse' : 'bg-gray-400'
                        ]"></span>
                        {{ job.isActive ? 'Đang hoạt động' : 'Tạm dừng' }}
                      </span>
                    </div>

                    <p class="text-sm text-gray-600 dark:text-gray-400 mb-3 line-clamp-2">
                      {{ job.description || 'Không có mô tả' }}
                    </p>

                    <div class="flex flex-wrap items-center gap-x-5 gap-y-2 text-xs text-gray-500 dark:text-gray-400">
                      <div class="flex items-center gap-1.5">
                        <i class="pi pi-clock text-blue-500"></i>
                        <span class="font-medium">{{ formatSchedule(job) }}</span>
                      </div>
                      <div class="flex items-center gap-1.5">
                        <i class="pi pi-calendar text-purple-500"></i>
                        <span>Tạo: {{ new Date(job.createdAt).toLocaleDateString('vi-VN', {
                          day: '2-digit',
                          month: '2-digit',
                          year: 'numeric'
                        }) }}</span>
                      </div>
                      <div v-if="job.lastRunAt" class="flex items-center gap-1.5">
                        <i class="pi pi-play-circle text-green-500"></i>
                        <span>Chạy lần cuối: {{ new Date(job.lastRunAt).toLocaleString('vi-VN', {
                          day: '2-digit',
                          month: '2-digit',
                          hour: '2-digit',
                          minute: '2-digit'
                        }) }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div class="flex-shrink-0 flex items-center gap-1.5">
                <button @click.stop="viewDetail(job.id)"
                  class="p-2.5 text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-900/30 rounded-lg transition-colors"
                  title="Xem chi tiết">
                  <i class="pi pi-eye text-base"></i>
                </button>

                <button @click.stop="() => { }"
                  class="p-2.5 text-green-600 hover:bg-green-50 dark:hover:bg-green-900/30 rounded-lg transition-colors"
                  title="Chỉnh sửa">
                  <i class="pi pi-pencil text-base"></i>
                </button>

                <button :class="[
                  'p-2.5 rounded-lg transition-all relative',
                  togglingJobId === job.id ? 'opacity-50 cursor-not-allowed' : '',
                  job.isActive
                    ? 'text-yellow-600 hover:bg-yellow-50 dark:hover:bg-yellow-900/30'
                    : 'text-green-600 hover:bg-green-50 dark:hover:bg-green-900/30'
                ]" @click.stop="handleToggleStatus(job)" :disabled="togglingJobId === job.id"
                  :title="job.isActive ? 'Tạm dừng Job' : 'Kích hoạt Job'">
                  <i v-if="togglingJobId === job.id" class="pi pi-spin pi-spinner text-base"></i>
                  <i v-else :class="[
                    'text-base',
                    job.isActive ? 'pi pi-pause-circle' : 'pi pi-play-circle'
                  ]"></i>
                </button>
                <button @click.stop="() => { }"
                  class="p-2.5 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/30 rounded-lg transition-colors"
                  title="Xóa">
                  <i class="pi pi-trash text-base"></i>
                </button>
              </div>
            </div>
          </div>
          <div
            class="h-1 bg-gradient-to-r from-blue-500 to-purple-500 transform scale-x-0 group-hover:scale-x-100 transition-transform duration-300 origin-left">
          </div>
        </div>
      </div>
    </div>

    <Teleport to="body">
      <Transition enter-active-class="transition-all duration-300 ease-out"
        leave-active-class="transition-all duration-200 ease-in" enter-from-class="opacity-0 scale-95"
        enter-to-class="opacity-100 scale-100" leave-from-class="opacity-100 scale-100"
        leave-to-class="opacity-0 scale-95">
        <div v-if="showDialog"
          class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm"
          @click.self="closeDialog">
          <div
            class="relative w-full max-w-4xl bg-white dark:bg-gray-900 rounded-2xl shadow-2xl max-h-[90vh] overflow-hidden transform transition-all"
            @click.stop>
            <div
              class="flex items-center justify-between p-6 border-b border-gray-200 dark:border-gray-800 bg-gradient-to-r from-blue-50 to-purple-50 dark:from-gray-800 dark:to-gray-800">
              <div class="flex items-center gap-3">
                <div class="p-2 bg-blue-600 rounded-lg">
                  <i class="pi pi-calendar-plus text-white text-xl"></i>
                </div>
                <div>
                  <h3 class="text-xl font-bold text-gray-900 dark:text-white">
                    Tạo Job Schedule mới
                  </h3>
                  <p class="text-xs text-gray-600 dark:text-gray-400 mt-0.5">
                    Cấu hình lịch chạy tự động cho API testing
                  </p>
                </div>
              </div>
              <button @click="closeDialog" :disabled="submitting"
                class="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed">
                <i class="pi pi-times text-xl"></i>
              </button>
            </div>

            <div class="px-8 py-6 bg-white dark:bg-gray-900 border-b border-gray-100 dark:border-gray-800">
              <div class="flex items-center justify-between relative">
                <div v-for="(label, idx) in stepLabels" :key="idx"
                  class="flex-1 flex flex-col items-center relative z-10">
                  <div :class="[
                    'w-11 h-11 rounded-full flex items-center justify-center text-white font-bold text-base transition-all duration-300 shadow-lg',
                    currentStep > idx + 1 ? 'bg-gradient-to-br from-green-500 to-green-600 scale-95' :
                      currentStep === idx + 1 ? 'bg-gradient-to-br from-blue-600 to-blue-700 scale-110 ring-4 ring-blue-100 dark:ring-blue-900' :
                        'bg-gray-300 dark:bg-gray-700 scale-90'
                  ]">
                    <i v-if="currentStep > idx + 1" class="pi pi-check font-bold"></i>
                    <span v-else>{{ idx + 1 }}</span>
                  </div>
                  <p class="mt-2 text-xs font-semibold text-center max-w-[100px]"
                    :class="currentStep >= idx + 1 ? 'text-blue-600 dark:text-blue-400' : 'text-gray-400 dark:text-gray-600'">
                    {{ label }}
                  </p>
                </div>

                <div v-for="n in totalSteps - 1" :key="'line-' + n"
                  class="absolute top-[22px] h-1 z-0 transition-all duration-500 rounded-full" :style="{
                    left: `calc(${(n - 1) * (100 / totalSteps)}% + 22px)`,
                    width: `calc(${100 / totalSteps}% - 44px)`
                  }"
                  :class="currentStep > n ? 'bg-gradient-to-r from-green-500 to-green-600' : 'bg-gray-200 dark:bg-gray-700'" />
              </div>
            </div>

            <div class="p-8 overflow-y-auto bg-gray-50 dark:bg-gray-900" style="max-height: calc(90vh - 280px);">
              <div v-if="loading" class="flex flex-col items-center justify-center py-16">
                <i class="pi pi-spin pi-spinner text-4xl text-blue-600 mb-4"></i>
                <p class="text-gray-600 dark:text-gray-400 font-medium">Đang tải dữ liệu...</p>
              </div>

              <div v-else-if="error" class="flex justify-center py-12">
                <div
                  class="bg-red-50 dark:bg-red-900/20 border-2 border-red-200 dark:border-red-800 text-red-700 dark:text-red-300 p-6 rounded-xl max-w-md text-center">
                  <i class="pi pi-exclamation-triangle text-3xl mb-3"></i>
                  <p class="font-bold text-base mb-1">Lỗi kết nối</p>
                  <p class="text-sm">{{ error }}</p>
                </div>
              </div>

              <div v-else-if="data?.collections?.length"
                class="bg-white dark:bg-gray-800 rounded-xl p-6 shadow-sm border border-gray-200 dark:border-gray-700">
                <div v-if="currentStep === 1" class="space-y-4">
                  <div class="mb-4">
                    <h4 class="text-base font-bold text-gray-900 dark:text-white mb-1">Chọn Collection
                    </h4>
                    <p class="text-sm text-gray-600 dark:text-gray-400">Chọn collection chứa các API
                      bạn muốn kiểm tra
                    </p>
                  </div>
                  <div class="grid grid-cols-2 gap-4">
                    <label v-for="c in data.collections" :key="c.id" class="cursor-pointer group">
                      <input type="radio" :value="c.id" v-model="selectedCollectionId" class="sr-only" />
                      <div :class="[
                        'border-2 rounded-xl p-5 transition-all h-full',
                        selectedCollectionId === c.id
                          ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20 shadow-md scale-105'
                          : 'border-gray-200 dark:border-gray-700 hover:border-gray-300 hover:shadow-sm bg-white dark:bg-gray-800'
                      ]">
                        <div class="flex justify-between items-start gap-3">
                          <div class="flex-1 min-w-0">
                            <h4 class="font-bold text-gray-900 dark:text-white text-base mb-2 truncate">
                              {{ c.name }}
                            </h4>
                            <p class="text-xs text-gray-600 dark:text-gray-400 line-clamp-2 mb-3">
                              {{ c.description }}
                            </p>
                            <div class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-gray-500">
                              <i class="pi pi-list"></i>
                              <span class="font-medium">{{ c.requests.length }} request(s)</span>
                            </div>
                          </div>
                          <div v-if="selectedCollectionId === c.id"
                            class="flex-shrink-0 w-8 h-8 bg-blue-600 rounded-full flex items-center justify-center">
                            <i class="pi pi-check text-white font-bold"></i>
                          </div>
                        </div>
                      </div>
                    </label>
                  </div>
                </div>

                <div v-else-if="currentStep === 2 && selectedCollection" class="space-y-4">
                  <div class="mb-4">
                    <h4 class="text-base font-bold text-gray-900 dark:text-white mb-1">Chọn API Requests
                    </h4>
                    <div
                      class="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-3 mt-3">
                      <p class="text-sm text-blue-800 dark:text-blue-300">
                        <strong>Collection:</strong> {{ selectedCollection.name }}
                        <span class="ml-2 font-bold">({{ currentCollectionRequests.length }}
                          request(s))</span>
                      </p>
                    </div>
                  </div>
                  <div class="space-y-2.5 max-h-96 overflow-y-auto pr-2">
                    <label v-for="r in currentCollectionRequests" :key="r.id"
                      class="flex items-center gap-3 p-4 border-2 rounded-xl cursor-pointer transition-all group"
                      :class="r.selected
                        ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20 shadow-sm'
                        : 'border-gray-200 dark:border-gray-700 hover:border-gray-300 hover:shadow-sm bg-white dark:bg-gray-800'">
                      <input type="checkbox" v-model="r.selected"
                        class="w-5 h-5 text-blue-600 border-gray-300 rounded focus:ring-2 focus:ring-blue-500" />
                      <span
                        :class="['px-3 py-1 rounded-lg text-white font-bold text-xs shadow-sm', getMethodClasses(r.method)]">
                        {{ r.method }}
                      </span>
                      <div class="flex-1 min-w-0">
                        <p class="font-semibold text-gray-900 dark:text-white text-sm truncate mb-1">
                          {{ r.name }}</p>
                        <p class="text-xs text-gray-500 dark:text-gray-400 truncate font-mono">{{
                          r.url }}</p>
                      </div>
                    </label>
                  </div>
                </div>

                <div v-else-if="currentStep === 3" class="space-y-5">
                  <div class="mb-4">
                    <h4 class="text-base font-bold text-gray-900 dark:text-white mb-1">Cấu hình Job
                    </h4>
                    <p class="text-sm text-gray-600 dark:text-gray-400">Thiết lập thông tin và lịch chạy
                      cho Job</p>
                  </div>

                  <div>
                    <label class="block text-sm font-bold text-gray-900 dark:text-white mb-2">
                      Tên Job <span class="text-red-500">*</span>
                    </label>
                    <input v-model="job.name" type="text" placeholder="VD: Kiểm tra API hàng ngày"
                      class="w-full px-4 py-3 text-sm border-2 border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-white rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition" />
                    <p v-if="!job.name.trim()" class="text-xs text-red-500 mt-1.5 flex items-center gap-1">
                      <i class="pi pi-exclamation-circle"></i>
                      <span>Vui lòng nhập tên job</span>
                    </p>
                  </div>

                  <fieldset
                    class="border-2 border-gray-300 dark:border-gray-600 p-5 rounded-xl space-y-4 bg-gradient-to-br from-gray-50 to-white dark:from-gray-800 dark:to-gray-800">
                    <legend class="text-sm font-bold text-gray-900 dark:text-white px-2">
                      Lịch chạy Job <span class="text-red-500">*</span>
                    </legend>

                    <div class="flex items-center gap-6">
                      <label class="flex items-center cursor-pointer group">
                        <input type="radio" v-model="job.scheduleType" value="daily"
                          class="w-5 h-5 text-blue-600 border-gray-300 focus:ring-2 focus:ring-blue-500">
                        <span
                          class="ml-2 text-sm font-semibold text-gray-700 dark:text-gray-300 group-hover:text-blue-600">
                          Chạy hằng ngày
                        </span>
                      </label>
                      <label class="flex items-center cursor-pointer group">
                        <input type="radio" v-model="job.scheduleType" value="interval"
                          class="w-5 h-5 text-blue-600 border-gray-300 focus:ring-2 focus:ring-blue-500">
                        <span
                          class="ml-2 text-sm font-semibold text-gray-700 dark:text-gray-300 group-hover:text-blue-600">
                          Chạy lặp lại
                        </span>
                      </label>
                    </div>

                    <div v-if="job.scheduleType === 'daily'"
                      class="space-y-3 bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700">
                      <label class="block text-sm font-bold text-gray-900 dark:text-white">
                        Khung giờ chạy <span class="text-red-500">*</span>
                      </label>
                      <input v-model="job.dailyTime" type="time"
                        class="px-4 py-3 text-base border-2 border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 font-semibold" />
                      <p class="text-xs text-gray-600 dark:text-gray-400 flex items-center gap-1.5">
                        <i class="pi pi-info-circle text-blue-500"></i>
                        <span>Job sẽ chạy vào <strong class="text-blue-600">{{ job.dailyTime }}</strong> mỗi
                          ngày</span>
                      </p>
                    </div>

                    <div v-else-if="job.scheduleType === 'interval'"
                      class="space-y-3 bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700">
                      <label class="block text-sm font-bold text-gray-900 dark:text-white mb-2">
                        Khoảng thời gian lặp lại
                      </label>
                      <div class="flex gap-3">
                        <input v-model.number="job.intervalValue" type="number" min="1" placeholder="5"
                          class="flex-1 px-4 py-3 text-base border-2 border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 font-semibold" />
                        <select v-model="job.intervalUnit"
                          class="px-4 py-3 text-base border-2 border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-white rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 font-semibold">
                          <option value="minutes">Phút</option>
                          <option value="hours">Giờ</option>
                        </select>
                      </div>
                      <p v-if="job.intervalValue && job.intervalValue > 0"
                        class="text-xs text-gray-600 dark:text-gray-400 flex items-center gap-1.5">
                        <i class="pi pi-info-circle text-blue-500"></i>
                        <span>Job sẽ lặp lại mỗi <strong class="text-blue-600">{{ job.intervalValue }}
                            {{ job.intervalUnit === 'minutes' ? 'phút' : 'giờ' }}</strong></span>
                      </p>
                    </div>
                  </fieldset>

                  <div>
                    <label class="block text-sm font-bold text-gray-900 dark:text-white mb-2">Mô tả Job</label>
                    <textarea v-model="job.description" rows="3" placeholder="Mô tả chi tiết về mục đích của job này..."
                      class="w-full px-4 py-3 text-sm border-2 border-gray-300 dark:border-gray-600 dark:bg-gray-800 dark:text-white rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"></textarea>
                  </div>
                </div>

                <div v-else-if="currentStep === 4" class="space-y-5">
                  <div class="mb-4">
                    <h4 class="text-base font-bold text-gray-900 dark:text-white mb-1">Xác nhận thông tin
                    </h4>
                    <p class="text-sm text-gray-600 dark:text-gray-400">Kiểm tra lại thông tin trước khi
                      lưu</p>
                  </div>

                  <div
                    class="bg-gradient-to-br from-blue-50 to-purple-50 dark:from-blue-900/20 dark:to-purple-900/20 border-2 border-blue-200 dark:border-blue-800 rounded-xl p-5 space-y-3">
                    <h4
                      class="font-bold text-blue-900 dark:text-blue-300 text-base flex items-center gap-2 pb-3 border-b-2 border-blue-200 dark:border-blue-700">
                      <i class="pi pi-info-circle"></i>
                      <span>Thông tin Job</span>
                    </h4>
                    <div class="space-y-2.5 text-sm">
                      <div class="flex gap-2">
                        <strong class="text-gray-700 dark:text-gray-300 min-w-[80px]">Tên:</strong>
                        <span class="text-gray-900 dark:text-white font-semibold">{{ job.name }}</span>
                      </div>
                      <div class="flex gap-2">
                        <strong class="text-gray-700 dark:text-gray-300 min-w-[80px]">Lịch:</strong>
                        <span class="text-gray-900 dark:text-white font-semibold"
                          v-html="formatScheduleForConfirm()"></span>
                      </div>
                      <div class="flex gap-2">
                        <strong class="text-gray-700 dark:text-gray-300 min-w-[80px]">Mô tả:</strong>
                        <span class="text-gray-900 dark:text-white">{{ job.description || 'Không có' }}</span>
                      </div>
                    </div>
                  </div>

                  <div class="bg-gray-50 dark:bg-gray-800 border-2 border-gray-300 dark:border-gray-600 rounded-xl p-5">
                    <h4 class="font-bold text-gray-900 dark:text-white mb-4 text-sm flex items-center gap-2">
                      <i class="pi pi-list"></i>
                      <span>APIs đã chọn ({{selectedRequests.filter(r => r.selected).length}})</span>
                    </h4>
                    <div class="space-y-2.5 max-h-60 overflow-y-auto pr-2">
                      <div v-for="r in selectedRequests.filter(r => r.selected)" :key="r.id"
                        class="bg-white dark:bg-gray-700 border border-gray-200 dark:border-gray-600 rounded-lg p-3 flex items-center gap-3 shadow-sm">
                        <span
                          :class="['px-3 py-1 rounded-lg text-white font-bold text-xs', getMethodClasses(r.method)]">
                          {{ r.method }}
                        </span>
                        <div class="flex-1 min-w-0">
                          <p class="font-semibold text-gray-900 dark:text-white text-sm truncate">
                            {{ r.name }}</p>
                          <p class="text-xs text-gray-500 dark:text-gray-400 truncate font-mono">{{
                            r.url }}</p>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div v-else class="flex flex-col items-center justify-center py-16 text-center">
                <i class="pi pi-inbox text-5xl text-gray-300 dark:text-gray-600 mb-4"></i>
                <p class="text-gray-500 dark:text-gray-400 font-medium">Không tìm thấy collection nào</p>
              </div>
            </div>

            <div
              class="flex items-center justify-between p-6 border-t-2 border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-900">
              <button @click="closeDialog" :disabled="submitting"
                class="px-5 py-2.5 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed">
                Hủy
              </button>

              <div class="flex gap-3">
                <button v-if="currentStep > 1" @click="prevStep" :disabled="submitting"
                  class="px-5 py-2.5 text-sm font-semibold bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-300 dark:hover:bg-gray-600 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2">
                  <i class="pi pi-arrow-left"></i>
                  <span>Quay lại</span>
                </button>

                <button v-if="currentStep < totalSteps" @click="nextStep" :disabled="isNextDisabled"
                  class="px-6 py-2.5 text-sm font-semibold bg-gradient-to-r from-blue-600 to-blue-700 text-white hover:from-blue-700 hover:to-blue-800 disabled:from-gray-300 disabled:to-gray-400 disabled:cursor-not-allowed rounded-lg transition-all shadow-md hover:shadow-lg flex items-center gap-2">
                  <span>Tiếp theo</span>
                  <i class="pi pi-arrow-right"></i>
                </button>

                <button v-else @click="saveJob" :disabled="isNextDisabled || submitting"
                  class="px-7 py-2.5 text-sm font-semibold bg-gradient-to-r from-green-600 to-green-700 text-white hover:from-green-700 hover:to-green-800 disabled:from-gray-300 disabled:to-gray-400 disabled:cursor-not-allowed rounded-lg transition-all shadow-md hover:shadow-lg flex items-center gap-2">
                  <i :class="submitting ? 'pi pi-spin pi-spinner' : 'pi pi-save'"></i>
                  <span>{{ submitting ? 'Đang lưu...' : 'Lưu Job' }}</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <Teleport to="body">
      <Transition enter-active-class="transition-all duration-300 ease-out"
        leave-active-class="transition-all duration-200 ease-in" enter-from-class="opacity-0 scale-95"
        leave-to-class="opacity-0 scale-95">
        <div v-if="showDetailModal && detailJob"
          class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm"
          @click.self="showDetailModal = false">
          <div
            class="relative w-full max-w-4xl bg-white dark:bg-gray-900 rounded-2xl shadow-2xl max-h-[90vh] overflow-hidden transform transition-all"
            @click.stop>
            <div class="p-6 border-b border-gray-200 dark:border-gray-800 flex justify-between items-center">
              <h3 class="text-xl font-bold text-blue-600 dark:text-blue-400">
                Chi tiết Job: {{ detailJob.name }}
              </h3>
              <button @click="showDetailModal = false"
                class="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300">
                <i class="pi pi-times text-xl"></i>
              </button>
            </div>

            <div class="p-6 space-y-6 overflow-y-auto" style="max-height: calc(90vh - 120px);">

              <h4 class="font-bold text-gray-900 dark:text-white mb-2">Thông tin chung</h4>
              <div class="grid grid-cols-2 gap-4 text-sm">
                <p><strong class="text-gray-700 dark:text-gray-300">Tên Job:</strong> {{ detailJob.name }}</p>
                <p><strong class="text-gray-700 dark:text-gray-300">Trạng thái:</strong>
                  <span :class="detailJob.isActive ? 'text-green-600' : 'text-gray-600'">
                    {{ detailJob.isActive ? 'Đang hoạt động' : 'Tạm dừng' }}
                  </span>
                </p>
                <p><strong class="text-gray-700 dark:text-gray-300">Lịch chạy:</strong> {{ formatSchedule(detailJob) }}
                </p>
                <p><strong class="text-gray-700 dark:text-gray-300">Ngày tạo:</strong> {{ new
                  Date(detailJob.createdAt).toLocaleString('vi-VN') }}</p>
                <p class="col-span-2"><strong class="text-gray-700 dark:text-gray-300">Mô tả:</strong> {{
                  detailJob.description || 'N/A' }}</p>
              </div>

              <h4 class="font-bold text-gray-900 dark:text-white pt-4 border-t border-gray-200 dark:border-gray-800">
                APIs/Test Suites ({{ detailJob.jobApiTestSuites?.length || 0 }})
              </h4>
              <div v-if="detailJob.jobApiTestSuites?.length" class="space-y-3">
                <div v-for="suite in detailJob.jobApiTestSuites" :key="suite.id"
                  class="p-3 bg-gray-50 dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700">
                  <span
                    :class="['px-2 py-1 rounded text-white font-bold text-xs mr-2', getMethodClasses(suite.method)]">
                    {{ suite.method }}
                  </span>
                  <span class="font-semibold text-gray-900 dark:text-white">{{ suite.description }}</span>
                  <p class="text-xs text-gray-500 dark:text-gray-400 truncate mt-1">{{ suite.endpoint }}</p>
                </div>
              </div>
              <div v-else class="text-sm text-gray-500 italic">Không có Test Suite nào được liên kết.</div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* Smooth transitions */
.hover\:shadow-lg {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

/* Custom scrollbar */
::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

::-webkit-scrollbar-track {
  background: transparent;
}

::-webkit-scrollbar-thumb {
  background: #cbd5e0;
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background: #a0aec0;
}

/* Dark mode scrollbar */
.dark ::-webkit-scrollbar-thumb {
  background: #4a5568;
}

.dark ::-webkit-scrollbar-thumb:hover {
  background: #718096;
}
</style>