<script setup lang="ts">
import { ref } from 'vue'

interface TestData {
  id: string
  name: string
  content: string
}

interface RequestItem {
  id: string
  name: string
  method: string
  url: string
  testData: TestData[]
}

interface Collection {
  id: string
  name: string
  requests: RequestItem[]
}

// ==== EMIT ====
const emit = defineEmits<{
  (e: 'selectTestData', payload: { content: string; name: string }): void
  (e: 'selectRequest', payload: { url: string; method: string; name: string }): void // ← THÊM EVENT NÀY
}>()

// ==== DATA với URL ====
const collections = ref<Collection[]>([
  {
    id: '1',
    name: 'SFIN-INVOICE',
    requests: [
      {
        id: 'r1',
        name: 'Lấy mã truy cập',
        method: 'POST',
        url: 'https://api.sfin.vn/v1/auth/token', // ← THÊM URL
        testData: [
          {
            id: 't1',
            name: 'data_1.json',
            content: `{
  "username": "test_user_001",
  "password": "SecurePass123!",
  "grant_type": "password",
  "client_id": "sfin-invoice-app",
  "scope": "read write"
}`
          },
          {
            id: 't2',
            name: 'data_2.json',
            content: `{
  "username": "admin@sfin.vn",
  "password": "Admin@2024",
  "grant_type": "client_credentials",
  "client_id": "sfin-admin-portal",
  "client_secret": "xK9mP2nQ5rT8wV3yZ",
  "scope": "admin"
}`
          }
        ]
      },
      {
        id: 'r2',
        name: 'Danh sách hóa đơn',
        method: 'GET',
        url: 'https://api.sfin.vn/v1/invoices',
        testData: [
          {
            id: 't3',
            name: 'default.json',
            content: `{
  "page": 1,
  "pageSize": 20,
  "sortBy": "createdDate",
  "sortOrder": "desc"
}`
          },
          {
            id: 't4',
            name: 'filter.json',
            content: `{
  "page": 1,
  "pageSize": 50,
  "filters": {
    "status": ["APPROVED", "PENDING"],
    "fromDate": "2024-01-01",
    "toDate": "2024-12-31",
    "minAmount": 1000000,
    "maxAmount": 50000000,
    "customerName": "Công ty TNHH ABC"
  },
  "sortBy": "invoiceDate",
  "sortOrder": "desc"
}`
          },
          {
            id: 't5',
            name: 'search_by_number.json',
            content: `{
  "invoiceNumber": "INV-2024-001234",
  "includeDetails": true,
  "includePayments": true
}`
          }
        ]
      },
      {
        id: 'r3',
        name: 'Tạo hóa đơn mới',
        method: 'POST',
        url: 'https://api.sfin.vn/v1/invoices',
        testData: [
          {
            id: 't6',
            name: 'create_invoice.json',
            content: `{
  "customer": {
    "name": "Công ty TNHH Thương Mại XYZ",
    "taxCode": "0123456789",
    "address": "123 Đường ABC, Quận 1, TP.HCM",
    "phone": "0901234567",
    "email": "contact@xyz.com"
  },
  "invoiceDate": "2024-10-23",
  "dueDate": "2024-11-23",
  "items": [
    {
      "productCode": "PROD-001",
      "productName": "Laptop Dell Inspiron 15",
      "unit": "Chiếc",
      "quantity": 5,
      "unitPrice": 15000000,
      "vatRate": 10,
      "discount": 5
    },
    {
      "productCode": "PROD-002",
      "productName": "Chuột không dây Logitech",
      "unit": "Chiếc",
      "quantity": 10,
      "unitPrice": 350000,
      "vatRate": 10,
      "discount": 0
    }
  ],
  "notes": "Giao hàng trong vòng 7 ngày",
  "paymentMethod": "BANK_TRANSFER"
}`
          }
        ]
      }
    ]
  },
  {
    id: '2',
    name: 'USER-MANAGER',
    requests: [
      {
        id: 'r4',
        name: 'Đăng nhập',
        method: 'POST',
        url: 'https://bhl.dev.tlsoft.com.vn/api/auth/login',
        testData: [
          {
            id: 't7',
            name: 'login_success.json',
            content: `{
                "email": "admin",
                "password": "adminBhl12345!",
                "rememberMe": true
              }`
          },
          {
            id: 't8',
            name: 'login_fail.json',
            content: `{
  "email": "wrong@example.com",
  "password": "WrongPassword",
  "rememberMe": false
}`
          },
          {
            id: 't9',
            name: 'login_mobile.json',
            content: `{
  "phoneNumber": "+84901234567",
  "otp": "123456",
  "deviceInfo": {
    "deviceId": "mobile-ios-iphone14",
    "platform": "iOS",
    "appVersion": "1.2.3"
  }
}`
          }
        ]
      },
      {
        id: 'r5',
        name: 'Đăng ký người dùng',
        method: 'POST',
        url: 'https://api.example.com/v1/users/register',
        testData: [
          {
            id: 't10',
            name: 'register_user.json',
            content: `{
  "firstName": "Nguyễn Văn",
  "lastName": "An",
  "email": "nguyenvanan@example.com",
  "phoneNumber": "+84909876543",
  "password": "SecurePass@2024",
  "confirmPassword": "SecurePass@2024",
  "dateOfBirth": "1990-05-15",
  "gender": "male",
  "address": {
    "street": "456 Đường Lê Lợi",
    "ward": "Phường Bến Thành",
    "district": "Quận 1",
    "city": "TP. Hồ Chí Minh",
    "country": "Vietnam"
  },
  "agreeToTerms": true,
  "newsletter": true
}`
          }
        ]
      },
      {
        id: 'r6',
        name: 'Cập nhật thông tin',
        method: 'PUT',
        url: 'https://api.example.com/v1/users/profile',
        testData: [
          {
            id: 't11',
            name: 'update_profile.json',
            content: `{
  "userId": "user-12345",
  "firstName": "Trần Thị",
  "lastName": "Bình",
  "phoneNumber": "+84912345678",
  "avatar": "https://example.com/avatars/user-12345.jpg",
  "bio": "Software Developer | Tech Enthusiast",
  "preferences": {
    "language": "vi",
    "timezone": "Asia/Ho_Chi_Minh",
    "emailNotifications": true,
    "smsNotifications": false
  }
}`
          }
        ]
      }
    ]
  },
  {
    id: '3',
    name: 'PRODUCT-SERVICE',
    requests: [
      {
        id: 'r7',
        name: 'Danh sách sản phẩm',
        method: 'GET',
        url: 'https://api.shop.vn/v1/products',
        testData: [
          {
            id: 't12',
            name: 'list_all.json',
            content: `{
  "page": 1,
  "limit": 20,
  "categoryId": null,
  "inStock": true
}`
          },
          {
            id: 't13',
            name: 'filter_by_category.json',
            content: `{
  "page": 1,
  "limit": 50,
  "categoryId": "cat-electronics",
  "priceRange": {
    "min": 1000000,
    "max": 20000000
  },
  "sortBy": "price",
  "sortOrder": "asc",
  "inStock": true,
  "brands": ["Apple", "Samsung", "Dell"]
}`
          }
        ]
      },
      {
        id: 'r8',
        name: 'Tạo sản phẩm mới',
        method: 'POST',
        url: 'https://api.shop.vn/v1/products',
        testData: [
          {
            id: 't14',
            name: 'create_product.json',
            content: `{
  "name": "iPhone 15 Pro Max 256GB",
  "sku": "IPHONE-15-PM-256-BLK",
  "categoryId": "cat-smartphones",
  "brand": "Apple",
  "price": 34990000,
  "compareAtPrice": 36990000,
  "costPrice": 30000000,
  "description": "iPhone 15 Pro Max với chip A17 Pro, camera 48MP, titanium design",
  "specifications": {
    "screen": "6.7 inch Super Retina XDR",
    "chip": "A17 Pro",
    "ram": "8GB",
    "storage": "256GB",
    "camera": "48MP + 12MP + 12MP",
    "battery": "4422mAh",
    "os": "iOS 17"
  },
  "images": [
    "https://example.com/products/iphone15pm-1.jpg",
    "https://example.com/products/iphone15pm-2.jpg"
  ],
  "stock": 50,
  "weight": 221,
  "dimensions": {
    "length": 160.9,
    "width": 77.8,
    "height": 8.25
  },
  "tags": ["flagship", "premium", "5g"],
  "status": "active"
}`
          }
        ]
      }
    ]
  }
])

// ==== STATE ====
const selectedCollection = ref<string | null>(null)
const selectedRequest = ref<string | null>(null)
const selectedTestData = ref<string | null>(null)

// ==== METHODS ====
const toggleCollection = (id: string) => {
  selectedCollection.value = selectedCollection.value === id ? null : id
  selectedRequest.value = null
  selectedTestData.value = null
}

const toggleRequest = (id: string, request: RequestItem) => {
  selectedRequest.value = selectedRequest.value === id ? null : id
  selectedTestData.value = null

  // Emit request info khi click vào request
  if (selectedRequest.value === id) {
    console.log('📡 List: emitting selectRequest', request)
    emit('selectRequest', {
      url: request.url,
      method: request.method,
      name: request.name
    })
  }
}

const selectTestData = (data: TestData) => {
  selectedTestData.value = data.id

  const payload = { content: data.content, name: data.name }

  emit('selectTestData', payload)
}
</script>

<template>
  <div class="h-full flex flex-col bg-white overflow-hidden">
    <div class="flex-1 overflow-y-auto overflow-x-hidden p-2 text-sm">
      <div v-for="col in collections" :key="col.id" class="py-2">
        <!-- CẤP 1: COLLECTION -->
        <div
          class="flex items-center justify-between cursor-pointer hover:bg-gray-50 px-2 py-2 rounded-md border-l-2 transition-colors"
          :class="selectedCollection === col.id ? 'border-primary bg-primary/10' : 'border-transparent'"
          @click="toggleCollection(col.id)">
          <div class="font-semibold text-gray-800 text-xs truncate flex-1 mr-2">
            📁 {{ col.name }}
          </div>
          <span class="text-gray-400 text-xs whitespace-nowrap flex-shrink-0">
            {{ col.requests.length }}
          </span>
        </div>

        <!-- CẤP 2: REQUEST -->
        <div v-if="selectedCollection === col.id" class="pl-4 border-l border-gray-200 ml-2 mt-1 space-y-1">
          <div v-for="req in col.requests" :key="req.id">
            <div class="cursor-pointer px-2 py-1.5 hover:bg-gray-50 rounded-md"
              :class="selectedRequest === req.id ? 'bg-blue-50' : ''" @click="toggleRequest(req.id, req)">
              <div class="flex items-center gap-1 mb-0.5">
                <span class="text-xs font-bold px-1 rounded flex-shrink-0"
                  :class="req.method === 'POST' ? 'text-green-600' : req.method === 'GET' ? 'text-blue-600' : req.method === 'PUT' ? 'text-orange-600' : 'text-red-600'">
                  {{ req.method }}
                </span>
                <span class="text-gray-800 text-xs truncate flex-1">{{ req.name }}</span>
              </div>
              <div class="text-gray-400 text-xs pl-1">
                {{ req.testData.length }} data
              </div>
            </div>

            <!-- CẤP 3: TEST DATA -->
            <div v-if="selectedRequest === req.id" class="pl-4 mt-1 space-y-0.5">
              <div v-for="data in req.testData" :key="data.id"
                class="text-gray-600 hover:text-primary cursor-pointer py-1 px-2 rounded hover:bg-gray-50 text-xs truncate"
                :class="selectedTestData === data.id ? 'font-semibold text-blue-600 bg-blue-50' : ''"
                @click.stop="selectTestData(data)" :title="data.name">
                📄 {{ data.name }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>