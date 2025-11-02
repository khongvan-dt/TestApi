<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useToast } from "primevue/usetoast"
import {
  getMySQLConnections,
  saveSQLConnection,
  testSQLConnection,
  deleteSQLConnection,
  toggleSQLConnectionStatus,
  type SQLConnection,
  type CreateSQLConnectionDto
} from '../../composables/sqlConnectionService'

// ==================== STATE ====================
const toast = useToast()
const connections = ref<SQLConnection[]>([])
const loading = ref(false)
const showModal = ref(false)
const isEditing = ref(false)
const testingConnection = ref(false)
const savingConnection = ref(false)

const formData = ref<CreateSQLConnectionDto>({
  name: '',
  connectString: '',
  isActive: true
})

// ==================== API FUNCTIONS ====================
async function fetchConnections() {
  loading.value = true

  const result = await getMySQLConnections()

  if (result.success) {
    connections.value = result.data
  } else {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: result.message || 'Failed to fetch connections',
      life: 3000
    })
  }

  loading.value = false
}


async function saveConnection() {
  if (!formData.value.name || !formData.value.connectString) {
    toast.add({
      severity: 'warn',
      summary: 'Validation Error',
      detail: 'Name and Connection String are required',
      life: 3000
    })
    return
  }

  if (formData.value.name.length > 200) {
    toast.add({
      severity: 'warn',
      summary: 'Validation Error',
      detail: 'Name must not exceed 200 characters',
      life: 3000
    })
    return
  }

  if (formData.value.connectString.length > 1000) {
    toast.add({
      severity: 'warn',
      summary: 'Validation Error',
      detail: 'Connection string must not exceed 1000 characters',
      life: 3000
    })
    return
  }

  savingConnection.value = true
  try {
    await saveSQLConnection(formData.value)
    
    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: isEditing.value ? 'Connection updated successfully' : 'Connection created successfully',
      life: 3000
    })

    showModal.value = false
    resetForm()
    await fetchConnections()
  } catch (error: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: error.message || 'Failed to save connection',
      life: 3000
    })
  } finally {
    savingConnection.value = false
  }
}

async function handleTestConnection(connectionString?: string) {
  const testString = connectionString || formData.value.connectString

  if (!testString) {
    toast.add({
      severity: 'warn',
      summary: 'Validation Error',
      detail: 'Connection String is required',
      life: 3000
    })
    return
  }

  testingConnection.value = true
  try {
    const result = await testSQLConnection(testString)
    
    toast.add({
      severity: result.success ? 'success' : 'error',
      summary: result.success ? 'Success' : 'Connection Failed',
      detail: result.message,
      life: 3000
    })
  } catch (error: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: error.message || 'Failed to test connection',
      life: 3000
    })
  } finally {
    testingConnection.value = false
  }
}

async function handleDeleteConnection(id: number) {
  if (!confirm('Are you sure you want to delete this connection?')) {
    return
  }

  loading.value = true
  try {
    await deleteSQLConnection(id)
    
    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: 'Connection deleted successfully',
      life: 3000
    })

    await fetchConnections()
  } catch (error: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: error.message || 'Failed to delete connection',
      life: 3000
    })
  } finally {
    loading.value = false
  }
}

async function handleToggleActive(connection: SQLConnection) {
  loading.value = true
  try {
    await toggleSQLConnectionStatus(connection)
    
    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: `Connection ${!connection.isActive ? 'activated' : 'deactivated'} successfully`,
      life: 3000
    })

    await fetchConnections()
  } catch (error: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: error.message || 'Failed to update connection',
      life: 3000
    })
  } finally {
    loading.value = false
  }
}

// ==================== HELPERS ====================
function openCreateModal() {
  isEditing.value = false
  resetForm()
  showModal.value = true
}

function openEditModal(connection: SQLConnection) {
  isEditing.value = true
  formData.value = {
    id: connection.id,
    name: connection.name,
    connectString: connection.connectString,
    isActive: connection.isActive
  }
  showModal.value = true
}

function resetForm() {
  formData.value = {
    name: '',
    connectString: '',
    isActive: true
  }
}

function closeModal() {
  showModal.value = false
  resetForm()
}

function formatDate(dateString?: string) {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// ==================== LIFECYCLE ====================
onMounted(() => {
  fetchConnections()
})
</script>

<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <div class="max-w-7xl mx-auto p-6">
      <!-- ==================== HEADER ==================== -->
      <div class="flex items-center justify-between mb-6">
        <div>
          <h1 class="text-2xl font-bold text-gray-900 dark:text-white">SQL Database Connections</h1>
          <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">Manage your SQL Server database connections</p>
        </div>

        <UButton 
          icon="i-heroicons-plus" 
          size="lg"
          @click="openCreateModal">
          Add Connection
        </UButton>
      </div>

      <!-- ==================== LOADING STATE ==================== -->
      <UCard v-if="loading && connections.length === 0">
        <div class="flex items-center justify-center py-12">
          <UIcon name="i-heroicons-arrow-path" class="w-8 h-8 animate-spin text-primary" />
          <span class="ml-3 text-gray-600 dark:text-gray-400">Loading connections...</span>
        </div>
      </UCard>

      <!-- ==================== EMPTY STATE ==================== -->
      <UCard v-else-if="connections.length === 0 && !loading">
        <div class="text-center py-12">
          <div class="flex justify-center mb-4">
            <div class="p-4 bg-gray-100 dark:bg-gray-800 rounded-full">
              <UIcon name="i-heroicons-circle-stack" class="w-16 h-16 text-gray-400" />
            </div>
          </div>
          <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">No connections yet</h3>
          <p class="text-gray-600 dark:text-gray-400 mb-6">Get started by creating your first database connection</p>
          
        </div>
      </UCard>

      <!-- ==================== CONNECTIONS LIST ==================== -->
      <div v-else class="space-y-4">
        <UCard 
          v-for="connection in connections" 
          :key="connection.id"
          class="hover:shadow-lg transition-shadow">

          <div class="flex items-start justify-between">
            <!-- Left: Connection Info -->
            <div class="flex-1">
              <div class="flex items-center gap-3 mb-3">
                <div class="p-2 bg-primary-50 dark:bg-primary-900 rounded-lg">
                  <UIcon name="i-heroicons-circle-stack" class="w-6 h-6 text-primary" />
                </div>
                <div>
                  <h3 class="text-lg font-semibold text-gray-900 dark:text-white">{{ connection.name }}</h3>
                  <UBadge 
                    variant="subtle"
                    :class="connection.isActive ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200' : 'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200'"
                    class="mt-1">
                    {{ connection.isActive ? 'Active' : 'Inactive' }}
                  </UBadge>
                </div>
              </div>

              <div class="ml-14 space-y-2">
                <div class="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400">
                  <UIcon name="i-heroicons-link" class="w-4 h-4 flex-shrink-0" />
                  <code class="bg-gray-100 dark:bg-gray-800 px-3 py-1.5 rounded-md text-xs font-mono break-all">
                    {{ connection.connectString.length > 80 
                      ? connection.connectString.substring(0, 80) + '...' 
                      : connection.connectString }}
                  </code>
                </div>

                <div class="flex items-center gap-4 text-xs text-gray-500">
                  <span class="flex items-center gap-1">
                    <UIcon name="i-heroicons-calendar" class="w-3 h-3" />
                    Created: {{ formatDate(connection.createdAt) }}
                  </span>
                  <span v-if="connection.updatedAt" class="flex items-center gap-1">
                    <UIcon name="i-heroicons-clock" class="w-3 h-3" />
                    Updated: {{ formatDate(connection.updatedAt) }}
                  </span>
                </div>
              </div>
            </div>

            <!-- Right: Actions -->
            <div class="flex items-center gap-2 ml-4 flex-shrink-0">
              <UTooltip text="Test Connection">
                <UButton 
                  icon="i-heroicons-bolt" 
                  color="primary"
                  variant="ghost" 
                  size="sm" 
                  :loading="testingConnection"
                  @click="handleTestConnection(connection.connectString)" />
              </UTooltip>

              <UTooltip text="Edit">
                <UButton 
                  icon="i-heroicons-pencil-square" 
                  variant="ghost" 
                  size="sm"
                  @click="openEditModal(connection)" />
              </UTooltip>

              <UTooltip :text="connection.isActive ? 'Deactivate' : 'Activate'">
                <UButton 
                  :icon="connection.isActive ? 'i-heroicons-pause-circle' : 'i-heroicons-play-circle'"
                  variant="ghost" 
                  size="sm"
                  @click="handleToggleActive(connection)" />
              </UTooltip>

              <UTooltip text="Delete">
                <UButton 
                  icon="i-heroicons-trash" 
                  color="error"
                  variant="ghost" 
                  size="sm"
                  @click="handleDeleteConnection(connection.id!)" />
              </UTooltip>
            </div>
          </div>
        </UCard>
      </div>
    </div>

    <!-- ==================== MODAL: CREATE/EDIT ==================== -->
    <!-- ✅ Sử dụng Teleport để render modal ở body -->
    <Teleport to="body">
      <Transition
        enter-active-class="transition-opacity duration-300"
        leave-active-class="transition-opacity duration-300"
        enter-from-class="opacity-0"
        leave-to-class="opacity-0">
        <div 
          v-if="showModal"
          class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50"
          @click.self="closeModal">
          
          <div 
            class="relative w-full max-w-2xl bg-white dark:bg-gray-900 rounded-lg shadow-2xl max-h-[90vh] overflow-hidden"
            @click.stop>
            
            <!-- Header -->
            <div class="flex items-center justify-between p-6 border-b border-gray-200 dark:border-gray-800">
              <h3 class="text-xl font-semibold text-gray-900 dark:text-white">
                {{ isEditing ? 'Edit Connection' : 'Add New Connection' }}
              </h3>
              <UButton 
                icon="i-heroicons-x-mark" 
                variant="ghost"
                size="sm"
                :disabled="savingConnection"
                @click="closeModal" />
            </div>

            <!-- Content -->
            <div class="p-6 space-y-6 overflow-y-auto max-h-[calc(90vh-180px)]">
              <!-- Connection Name -->
              <div class="space-y-2">
                <label class="block text-sm font-medium text-gray-900 dark:text-white">
                  Connection Name 
                  <span class="text-red-500">*</span>
                </label>
                <UInput 
                  v-model="formData.name" 
                  placeholder="e.g. Production DB, Local Development" 
                  icon="i-heroicons-identification"
                  size="lg"
                  :maxlength="200" />
                <p class="text-xs text-gray-500 dark:text-gray-400">
                  Maximum 200 characters
                </p>
              </div>

              <!-- Connection String -->
              <div class="space-y-2">
                <label class="block text-sm font-medium text-gray-900 dark:text-white">
                  Connection String 
                  <span class="text-red-500">*</span>
                </label>
                <UTextarea 
                  v-model="formData.connectString"
                  placeholder="Server=localhost;Database=MyDB;User Id=sa;Password=****;TrustServerCertificate=True" 
                  :rows="6"
                  size="lg"
                  class="font-mono text-sm"
                  :maxlength="1000" />
                <div class="text-xs text-gray-500 dark:text-gray-400 space-y-1">
                  <p><strong>Example:</strong> Server=...;Database=...;Trusted_Connection=True;Trust...; </p>
                  <p>Maximum 1000 characters</p>
                </div>
              </div>

              <!-- Active Status -->
              <div class="space-y-2">
                <label class="block text-sm font-medium text-gray-900 dark:text-white">
                  Status
                </label>
                <div class="flex items-center gap-3 p-4 bg-gray-50 dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700">
                  <input 
                    type="checkbox"
                    v-model="formData.isActive"
                    class="w-5 h-5 text-primary border-gray-300 rounded focus:ring-primary cursor-pointer" />
                  <div>
                    <p class="text-sm font-medium text-gray-900 dark:text-white">
                      {{ formData.isActive ? 'Active' : 'Inactive' }}
                    </p>
                    <p class="text-xs text-gray-500 dark:text-gray-400">
                      {{ formData.isActive 
                        ? 'This connection is available for use' 
                        : 'This connection is disabled' }}
                    </p>
                  </div>
                </div>
              </div>

              <!-- Test Connection Button -->
              <UButton 
                icon="i-heroicons-bolt" 
                color="primary"
                variant="outline" 
                block
                size="lg"
                :loading="testingConnection"
                :disabled="!formData.connectString"
                @click="handleTestConnection()">
                {{ testingConnection ? 'Testing Connection...' : 'Test Connection' }}
              </UButton>
            </div>

            <!-- Footer -->
            <div class="flex items-center justify-end gap-3 p-6 border-t border-gray-200 dark:border-gray-800">
              <UButton 
                label="Cancel" 
                variant="ghost"
                size="lg"
                :disabled="savingConnection"
                @click="closeModal" />

              <UButton 
                :label="isEditing ? 'Update Connection' : 'Create Connection'" 
                icon="i-heroicons-check"
                size="lg"
                :loading="savingConnection"
                :disabled="!formData.name || !formData.connectString"
                @click="saveConnection" />
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<style scoped>
/* Smooth transitions */
.hover\:shadow-lg {
  transition: box-shadow 0.2s ease-in-out;
}

/* Custom scrollbar for textarea */
:deep(textarea) {
  scrollbar-width: thin;
  scrollbar-color: #cbd5e0 #f7fafc;
}

:deep(textarea::-webkit-scrollbar) {
  width: 8px;
}

:deep(textarea::-webkit-scrollbar-track) {
  background: #f7fafc;
  border-radius: 4px;
}

:deep(textarea::-webkit-scrollbar-thumb) {
  background-color: #cbd5e0;
  border-radius: 4px;
}

:deep(textarea::-webkit-scrollbar-thumb:hover) {
  background-color: #a0aec0;
}
</style>