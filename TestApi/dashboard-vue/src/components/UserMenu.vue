<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuth } from '../composables/useAuth'
import type { DropdownMenuItem } from '@nuxt/ui'

defineProps<{
  collapsed?: boolean
}>()

const router = useRouter()
const { isAuthenticated, user: authUser, logout, fetchProfile } = useAuth()


onMounted(async () => {
  if (isAuthenticated.value && !authUser.value) {
    try {
      await fetchProfile()
    } catch (error) {
      console.error('Failed to fetch profile on mount:', error)
    }
  }
})

const user = computed(() => {
  if (authUser.value) {
    return {
      name: authUser.value.fullName || authUser.value.username,
      avatar: {
        src: authUser.value.avatarUrl || `https://ui-avatars.com/api/?name=${authUser.value.username}`,
        alt: authUser.value.username
      }
    }
  }
  return {
    name: 'Guest',
    avatar: {
      src: 'https://ui-avatars.com/api/?name=Guest',
      alt: 'Guest'
    }
  }
})

const items = computed<DropdownMenuItem[][]>(() => ([
  [
    {
      type: 'label',
      label: user.value.name,
      avatar: user.value.avatar
    }
  ],
  [
    {
      label: 'Log out',
      icon: 'i-lucide-log-out',
      async onSelect() {
        await logout()
      }
    }
  ]
]))
</script>

<template>
  <div>
    <!-- ✅ Chỉ hiển thị dropdown user (luôn có isAuthenticated = true ở đây) -->
    <UDropdownMenu
      :items="items"
      :content="{ align: 'center', collisionPadding: 12 }"
      :ui="{ content: collapsed ? 'w-48' : 'w-(--reka-dropdown-menu-trigger-width)' }"
    >
      <UButton
        v-bind="{
          ...user,
          label: collapsed ? undefined : user?.name,
          trailingIcon: collapsed ? undefined : 'i-lucide-chevrons-up-down'
        }"
        color="neutral"
        variant="ghost"
        block
        :square="collapsed"
        class="data-[state=open]:bg-elevated"
        :ui="{
          trailingIcon: 'text-dimmed'
        }"
      />
    </UDropdownMenu>

    <!-- ✅ Xóa LoginModal khỏi đây -->
  </div>
</template>