"use client"

import type React from "react"

import { ToastProvider } from "@/components/toast-provider"
import { NotificationProvider } from "@/contexts/NotificationContext"
import { AuthProvider } from "@/contexts/AuthContext"

export default function Providers({ children }: { children: React.ReactNode }) {
  return (
    <AuthProvider>
      <NotificationProvider>
        <ToastProvider>{children}</ToastProvider>
      </NotificationProvider>
    </AuthProvider>
  )
}
