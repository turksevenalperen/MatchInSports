"use client"
import { useState, useEffect } from "react"
import { useAuth } from "@/contexts/AuthContext"
import { useRouter } from "next/navigation"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Users, Search, MessageCircle, Trophy, LogOut, Plus, Check, X, Star, MapPin, Calendar, Target, TrendingUp, Award, Activity, Zap, Shield, Clock, CheckCircle, Menu } from 'lucide-react'
import Link from "next/link"
import { useToastContext } from "@/components/toast-provider"
import { useNotifications } from "@/contexts/NotificationContext"

interface TeamRequest {
  id: string
  message: string
  createdAt: string
  sender: {
    id: string
    teamName: string
    city: string
    sport: string
    rating: number
  }
}

interface SentRequest {
  id: string
  message: string
  createdAt: string
  receiver: {
    id: string
    teamName: string
    city: string
    sport: string
    rating: number
    logo: string | null
    isPro: boolean
  }
}

interface Team {
  id: string
  teamName: string
  city: string
  sport: string
  rating: number
  isPro: boolean
  logo: string | null
}

interface Activity {
  id: string
  type: string
  title: string
  description: string
  metadata?: any
  createdAt: string
}

interface UserStats {
  totalMatches: number
  thisMonthMatches: number
  winRate: number
  recentMatches: Array<{
    id: string
    opponent: string
    score: string
    won: boolean
    date: string
  }>
}

export default function DashboardPage() {
  const { user, loading, logout, token } = useAuth()
  const router = useRouter()
  const { toast } = useToastContext()
  const { data: notificationData, refreshNotifications } = useNotifications()
  const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000'
  
  // Local states for UI interactions
  const [sentRequests, setSentRequests] = useState<SentRequest[]>([])
  const [isLoadingSentRequests, setIsLoadingSentRequests] = useState(false)
  const [loadingActions, setLoadingActions] = useState<Set<string>>(new Set())
  const [teams, setTeams] = useState<Team[]>([])
  const [isLoadingTeams, setIsLoadingTeams] = useState(false)
  const [searchFilters, setSearchFilters] = useState({
    city: "",
    sport: "",
    rating: ""
  })
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)

  // Notification context'inden gelen veriler
  const incomingRequests = notificationData.teamRequests
  const activities = notificationData.activities
  const isLoadingRequests = false // Context handles loading
  const isLoadingActivities = false // Context handles loading

  useEffect(() => {
    if (!loading && !user) {
      router.push("/login")
    }
  }, [loading, user, router])

  const fetchSentRequests = async () => {
    setIsLoadingSentRequests(true)
    try {
      const response = await fetch(`${API_URL}/api/teams/my-requests`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      })
      if (response.ok) {
        const data = await response.json()
        setSentRequests(data)
      }
    } catch (error) {
      console.error("Error fetching sent requests:", error)
    } finally {
      setIsLoadingSentRequests(false)
    }
  }

  useEffect(() => {
    if (user) {
      fetchTeams() // İlk yüklemede takımları getir
      fetchSentRequests() // Gönderilen istekleri getir
    }
  }, [user])

  const fetchTeams = async () => {
    setIsLoadingTeams(true)
    try {
      const params = new URLSearchParams()
      if (searchFilters.city) params.append("city", searchFilters.city)
      if (searchFilters.sport) params.append("sport", searchFilters.sport)
      if (searchFilters.rating) {
        const [min, max] = searchFilters.rating.split("-").map(Number)
        params.append("minRating", min.toString())
        params.append("maxRating", max.toString())
      }
      const response = await fetch(`${API_URL}/api/teams/search?${params.toString()}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      })
      if (response.ok) {
        const data = await response.json()
        // Backend direkt array döndürüyor
        setTeams(Array.isArray(data) ? data : [])
      } else {
        console.error("Error fetching teams:", response.statusText)
        setTeams([])
      }
    } catch (error) {
      console.error("Error fetching teams:", error)
      setTeams([])
    } finally {
      setIsLoadingTeams(false)
    }
  }

  const handleFilterChange = (filterType: string, value: string) => {
    setSearchFilters(prev => ({
      ...prev,
      [filterType]: value
    }))
  }

  // Filtreler değiştiğinde takımları yeniden getir
  useEffect(() => {
    if (user) {
      fetchTeams()
    }
  }, [searchFilters, user])

  const getActivityIcon = (type: string) => {
    switch (type) {
      case "PLATFORM_JOIN":
        return <Shield className="h-4 w-4 sm:h-5 sm:w-5 text-white" />
      case "REQUEST_SENT":
        return <MessageCircle className="h-4 w-4 sm:h-5 sm:w-5 text-white" />
      case "REQUEST_RECEIVED":
        return <MessageCircle className="h-4 w-4 sm:h-5 sm:w-5 text-white" />
      case "REQUEST_ACCEPTED":
        return <Check className="h-4 w-4 sm:h-5 sm:w-5 text-white" />
      case "REQUEST_REJECTED":
        return <X className="h-4 w-4 sm:h-5 sm:w-5 text-white" />
      case "REQUEST_CANCELLED":
        return <X className="h-4 w-4 sm:h-5 sm:w-5 text-white" />
      case "CHAT_STARTED":
        return <MessageCircle className="h-4 w-4 sm:h-5 sm:w-5 text-white" />
      default:
        return <Activity className="h-4 w-4 sm:h-5 sm:w-5 text-white" />
    }
  }

  const getActivityColor = (type: string) => {
    switch (type) {
      case "PLATFORM_JOIN":
        return "bg-orange-500"
      case "REQUEST_SENT":
        return "bg-blue-500"
      case "REQUEST_RECEIVED":
        return "bg-purple-500"
      case "REQUEST_ACCEPTED":
        return "bg-green-500"
      case "REQUEST_REJECTED":
        return "bg-red-500"
      case "REQUEST_CANCELLED":
        return "bg-gray-500"
      case "CHAT_STARTED":
        return "bg-orange-400"
      default:
        return "bg-gray-600"
    }
  }

  const handleRequestAction = async (requestId: string, action: "ACCEPTED" | "REJECTED") => {
    setLoadingActions((prev) => new Set([...prev, requestId]))
    try {
      const response = await fetch(`${API_URL}/api/teams/incoming-requests`, {
        method: "PATCH",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ requestId, action }),
      })
      if (response.ok) {
        const data = await response.json()
        if (action === "ACCEPTED") {
          toast.success("İstek Kabul Edildi! ✅ Artık takımla sohbet edebilirsiniz.")
        } else {
          toast.info("İstek Reddedildi 📝 İstek başarıyla reddedildi.")
        }
        
        // Başarılı işlem sonrası bildirimleri yenile
        await refreshNotifications()
      } else {
        const errorData = await response.json()
        toast.error("Hata Oluştu! ❌ " + (errorData.error || "İstek işlenirken bir hata oluştu."))
      }
    } catch (error) {
      toast.error("Bağlantı Hatası! 🌐 İstek işlenirken bir hata oluştu.")
    } finally {
      setLoadingActions((prev) => {
        const newSet = new Set(prev)
        newSet.delete(requestId)
        return newSet
      })
    }
  }

  const handleSendRequest = async (targetTeamId: string, targetTeamName: string) => {
    if (!user?.id) return
    
    setLoadingActions((prev) => new Set([...prev, targetTeamId]))
    try {
      const response = await fetch(`${API_URL}/api/teams/request`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({
          targetTeamId,
          message: `Merhaba ${targetTeamName}! Sizinle bir hazırlık maçı yapmak istiyoruz. İletişime geçelim!`
        }),
      })
      if (response.ok) {
        // Başarı bildirimi
        toast.success("İstek Gönderildi! ✅ " + `${targetTeamName} takımına maç isteğiniz başarıyla gönderildi.`)
        
        // Aktivite ekle
        const newActivity = {
          id: Date.now().toString(),
          type: "REQUEST_SENT",
          title: "Maç İsteği Gönderildi",
          description: `${targetTeamName} takımına maç isteği gönderdiniz`,
          createdAt: new Date().toISOString()
        }
        
        // Aktiviteleri yenile
        await refreshNotifications()
        
        // Gönderilen istekleri yenile
        fetchSentRequests()
      
      } else {
        const error = await response.json()
        toast.error("Hata! ❌ " + (error.error || "İstek gönderilirken bir hata oluştu."))
      }
    } catch (error) {
      console.error('Error sending request:', error)
      toast.error("Bağlantı Hatası! 🌐 İnternet bağlantınızı kontrol edin.")
    } finally {
      setLoadingActions((prev) => {
        const newSet = new Set(prev)
        newSet.delete(targetTeamId)
        return newSet
      })
    }
  }

  const handleCancelRequest = async (requestId: string, targetTeamName: string) => {
    setLoadingActions((prev) => new Set([...prev, requestId]))
    try {
      const response = await fetch(`${API_URL}/api/teams/my-requests`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ requestId }),
      })
      if (response.ok) {
        toast.info("İstek İptal Edildi ❌ " + `${targetTeamName} takımına gönderilen istek iptal edildi.`)
        
        // Gönderilen istekleri listeden kaldır
        setSentRequests(prev => prev.filter(req => req.id !== requestId))
        
        // Aktivite ekle
        const newActivity = {
          id: Date.now().toString(),
          type: "REQUEST_CANCELLED",
          title: "Maç İsteği İptal Edildi",
          description: `${targetTeamName} takımına gönderilen istek iptal edildi`,
          createdAt: new Date().toISOString()
        }
        
        // Aktiviteleri yenile
        await refreshNotifications()
      
      } else {
        const error = await response.json()
        toast.error("Hata! ❌ " + (error.error || "İstek iptal edilirken bir hata oluştu."))
      }
    } catch (error) {
      console.error('Error cancelling request:', error)
      toast.error("Bağlantı Hatası! 🌐 İnternet bağlantınızı kontrol edin.")
    } finally {
      setLoadingActions((prev) => {
        const newSet = new Set(prev)
        newSet.delete(requestId)
        return newSet
      })
    }
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("tr-TR", {
      year: "numeric",
      month: "long",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    })
  }

  const getSportIcon = (sport: string) => {
    switch (sport.toLowerCase()) {
      case "futbol":
        return "⚽"
      case "basketbol":
        return "🏀"
      case "voleybol":
        return "🏐"
      case "tenis":
        return "🎾"
      default:
        return "🏆"
    }
  }

  const getRatingColor = (rating: number) => {
    if (rating >= 80) return "text-emerald-400"
    if (rating >= 60) return "text-yellow-400"
    if (rating >= 40) return "text-orange-400"
    return "text-red-400"
  }

  const getRatingBadgeColor = (rating: number) => {
    if (rating >= 80) return "bg-emerald-500/20 text-emerald-300 border-emerald-500/30"
    if (rating >= 60) return "bg-yellow-500/20 text-yellow-300 border-yellow-500/30"
    if (rating >= 40) return "bg-orange-500/20 text-orange-300 border-orange-500/30"
    return "bg-red-500/20 text-red-300 border-red-500/30"
  }

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-gray-900 via-slate-900 to-gray-800">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto mb-4"></div>
          <div className="text-lg font-medium text-gray-200">Yükleniyor...</div>
        </div>
      </div>
    )
  }

  if (!user) {
    return null
  }

  const handleSignOut = async () => {
    logout()
    router.push("/")
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-900 via-black to-gray-900">
      {/* Header */}
      <header className="bg-black/95 backdrop-blur-lg shadow-2xl border-b border-orange-500/20 sticky top-0 z-50">
        <div className="max-w-7xl mx-auto px-3 sm:px-4 lg:px-8">
          <div className="flex justify-between items-center py-3 sm:py-4">
            <div className="flex items-center space-x-2 sm:space-x-4 flex-1 min-w-0">
              <button
                onClick={() => router.push(`/profile/`)}
                title="Takım Profilim"
                className="focus:outline-none flex-shrink-0"
              >
                <Avatar className="h-10 w-10 sm:h-14 sm:w-14 border-2 border-orange-500/30 ring-2 ring-orange-500/20 transition-transform hover:scale-105 cursor-pointer">
                  <AvatarImage
                    src={user.logo || ''}
                    alt={`${user.teamName} logo`}
                    className="object-cover w-full h-full rounded-full"
                    style={{ objectPosition: 'center' }}
                  />
                  <AvatarFallback className="bg-gradient-to-r from-orange-500 to-orange-600 text-white text-lg sm:text-2xl font-bold">
                    {getSportIcon(user.sport)}
                  </AvatarFallback>
                </Avatar>
              </button>
              <div className="min-w-0 flex-1">
                <div className="flex items-center gap-1 sm:gap-2 flex-wrap">
                  <h1 className="text-lg sm:text-2xl font-bold bg-gradient-to-r from-orange-400 to-orange-300 bg-clip-text text-transparent truncate">
                    {user.teamName}
                  </h1>
                  {user.isPro && (
                    <div className="flex items-center bg-blue-500/20 text-blue-400 px-1.5 sm:px-2 py-0.5 sm:py-1 rounded-full border border-blue-500/40 flex-shrink-0">
                      <CheckCircle className="h-3 w-3 sm:h-4 sm:w-4 mr-0.5 sm:mr-1" />
                      <span className="text-xs font-semibold">PRO</span>
                    </div>
                  )}
                </div>
                <div className="flex items-center space-x-2 sm:space-x-4 text-gray-300 text-xs sm:text-sm flex-wrap">
                  <div className="flex items-center">
                    <MapPin className="h-3 w-3 sm:h-4 sm:w-4 mr-1 text-orange-400" />
                    <span className="truncate">{user.city}</span>
                  </div>
                  <div className="flex items-center">
                    <Trophy className="h-3 w-3 sm:h-4 sm:w-4 mr-1 text-orange-400" />
                    <span className="truncate">{user.sport}</span>
                  </div>
                  <Badge className="bg-orange-500/20 text-orange-300 border border-orange-500/40 font-semibold text-xs">
                    <Star className="h-2 w-2 sm:h-3 sm:w-3 mr-0.5 sm:mr-1" />
                    {user.rating}/100
                  </Badge>
                </div>
              </div>
            </div>
            <Button
              onClick={handleSignOut}
              variant="outline"
              size="sm"
              className="hover:bg-orange-500/10 hover:text-orange-400 hover:border-orange-500/50 bg-black/50 border-gray-600 text-gray-300 ml-2 flex-shrink-0"
            >
              <LogOut className="mr-1 sm:mr-2 h-3 w-3 sm:h-4 sm:w-4" />
              <span className="hidden sm:inline">Çıkış Yap</span>
              <span className="sm:hidden">Çıkış</span>
            </Button>
          </div>
        </div>
      </header>

      {/* Top Navigation */}
      <nav className="bg-black/90 backdrop-blur-lg border-b border-orange-500/20 sticky top-16 sm:top-20 z-40">
        <div className="max-w-7xl mx-auto px-3 sm:px-4 lg:px-8">
          <div className="flex items-center justify-between py-2 sm:py-3">
            {/* Desktop Navigation */}
            <div className="hidden md:flex items-center space-x-6">
              <Link href="/matches" className="flex items-center space-x-2 px-4 py-2 rounded-lg hover:bg-orange-500/10 transition-colors">
                <Users className="h-5 w-5 text-orange-400" />
                <span className="text-orange-300 font-medium">İlanlar</span>
              </Link>
              <Link href="/matches/upcoming" className="flex items-center space-x-2 px-4 py-2 rounded-lg hover:bg-orange-500/10 transition-colors">
                <Calendar className="h-5 w-5 text-orange-400" />
                <span className="text-orange-300 font-medium">Yaklaşan Maçlar</span>
              </Link>
              <Link href="/chat" className="flex items-center space-x-2 px-4 py-2 rounded-lg hover:bg-orange-500/10 transition-colors">
                <MessageCircle className="h-5 w-5 text-orange-400" />
                <span className="text-orange-300 font-medium">Mesajlar</span>
              </Link>
              <Link href="/profile" className="flex items-center space-x-2 px-4 py-2 rounded-lg hover:bg-orange-500/10 transition-colors">
                <Award className="h-5 w-5 text-orange-400" />
                <span className="text-orange-300 font-medium">Profil</span>
              </Link>
            </div>

            {/* Mobile Navigation */}
            <div className="md:hidden flex items-center space-x-2 overflow-x-auto">
              <Link href="/matches" className="flex items-center space-x-1 px-2 py-1.5 rounded-lg hover:bg-orange-500/10 transition-colors whitespace-nowrap">
                <Users className="h-4 w-4 text-orange-400" />
                <span className="text-orange-300 font-medium text-sm">İlanlar</span>
              </Link>
              <Link href="/chat" className="flex items-center space-x-1 px-2 py-1.5 rounded-lg hover:bg-orange-500/10 transition-colors whitespace-nowrap">
                <MessageCircle className="h-4 w-4 text-orange-400" />
                <span className="text-orange-300 font-medium text-sm">Mesajlar</span>
              </Link>
              <Link href="/profile" className="flex items-center space-x-1 px-2 py-1.5 rounded-lg hover:bg-orange-500/10 transition-colors whitespace-nowrap">
                <Award className="h-4 w-4 text-orange-400" />
                <span className="text-orange-300 font-medium text-sm">Profil</span>
              </Link>
            </div>

            <div className="flex items-center space-x-2 sm:space-x-4">
              <Link href="/teams/incoming-requests" className="relative flex items-center space-x-1 sm:space-x-2 px-2 sm:px-4 py-1.5 sm:py-2 rounded-lg hover:bg-orange-500/10 transition-colors">
                <MessageCircle className="h-4 w-4 sm:h-5 sm:w-5 text-orange-400" />
                <span className="text-orange-300 font-medium text-sm sm:text-base hidden sm:inline">Gelen İstekler</span>
                <span className="text-orange-300 font-medium text-sm sm:hidden">İstekler</span>
                {incomingRequests.length > 0 && (
                  <Badge className="absolute -top-1 -right-1 sm:-top-2 sm:-right-2 bg-red-500 text-white text-xs px-1.5 sm:px-2 py-0.5 sm:py-1 rounded-full min-w-[16px] sm:min-w-[20px] h-4 sm:h-5 flex items-center justify-center">
                    {incomingRequests.length}
                  </Badge>
                )}
              </Link>
            </div>
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-3 sm:px-4 lg:px-8 py-4 sm:py-8">
        {/* Stats Overview */}
        <div className="mb-6 sm:mb-8">
          <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-6 mb-4 sm:mb-6">
            <Card className="border border-orange-500/30 shadow-lg bg-gradient-to-br from-gray-900 to-black backdrop-blur-sm">
              <CardContent className="p-3 sm:p-6">
                <div className="text-center">
                  <div className="text-xl sm:text-3xl font-bold text-orange-300 mb-1 sm:mb-2">0</div>
                  <div className="text-xs sm:text-sm text-gray-400">Toplam Maç</div>
                  <div className="w-6 sm:w-8 h-0.5 sm:h-1 bg-orange-500 mx-auto mt-1 sm:mt-2 rounded-full"></div>
                </div>
              </CardContent>
            </Card>
            <Card className="border border-orange-500/30 shadow-lg bg-gradient-to-br from-gray-900 to-black backdrop-blur-sm">
              <CardContent className="p-3 sm:p-6">
                <div className="text-center">
                  <div className="text-xl sm:text-3xl font-bold text-orange-200 mb-1 sm:mb-2">{user.rating}</div>
                  <div className="text-xs sm:text-sm text-gray-400">Rating</div>
                  <div className="w-6 sm:w-8 h-0.5 sm:h-1 bg-orange-400 mx-auto mt-1 sm:mt-2 rounded-full"></div>
                </div>
              </CardContent>
            </Card>
            <Card className="border border-orange-500/30 shadow-lg bg-gradient-to-br from-gray-900 to-black backdrop-blur-sm">
              <CardContent className="p-3 sm:p-6">
                <div className="text-center">
                  <div className="text-xl sm:text-3xl font-bold text-orange-300 mb-1 sm:mb-2">0</div>
                  <div className="text-xs sm:text-sm text-gray-400">Bu Ay</div>
                  <div className="w-6 sm:w-8 h-0.5 sm:h-1 bg-orange-500 mx-auto mt-1 sm:mt-2 rounded-full"></div>
                </div>
              </CardContent>
            </Card>
            <Link href="/matches/create" className="col-span-2 sm:col-span-1">
              <Card className="border border-orange-500/30 shadow-lg bg-gradient-to-br from-orange-500 to-orange-600 backdrop-blur-sm cursor-pointer hover:shadow-xl hover:shadow-orange-500/30 transition-all group h-full">
                <CardContent className="p-3 sm:p-6 h-full flex items-center justify-center">
                  <div className="text-center">
                    <Plus className="h-6 w-6 sm:h-8 sm:w-8 text-white mx-auto mb-1 sm:mb-2 group-hover:scale-110 transition-transform" />
                    <div className="text-white font-semibold text-sm sm:text-base">Maç İlanı Ver</div>
                  </div>
                </CardContent>
              </Card>
            </Link>
          </div>
        </div>

        {/* Team Search Section */}
        <div className="mb-6 sm:mb-8">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 sm:gap-6">
            {/* Team Search */}
            <div className="lg:col-span-2">
              <Card className="border border-orange-500/30 shadow-2xl bg-gradient-to-br from-gray-900 to-black backdrop-blur-sm h-full">
                <CardHeader className="pb-3 sm:pb-4 px-4 sm:px-6 pt-4 sm:pt-6">
                  <div className="flex items-center space-x-2 sm:space-x-3">
                    <div className="h-8 w-8 sm:h-12 sm:w-12 bg-orange-500/20 rounded-xl sm:rounded-2xl flex items-center justify-center border border-orange-500/40">
                      <Search className="h-4 w-4 sm:h-6 sm:w-6 text-orange-400" />
                    </div>
                    <div>
                      <CardTitle className="text-lg sm:text-2xl text-orange-300">Takım Ara</CardTitle>
                      <CardDescription className="text-gray-400 text-sm">Maç yapmak için takım bulun</CardDescription>
                    </div>
                  </div>
                </CardHeader>
                <CardContent className="px-4 sm:px-6 pb-4 sm:pb-6">
                  <div className="space-y-4 sm:space-y-6">
                    {/* Search Form */}
                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 sm:gap-4">
                      <div>
                        <label className="block text-sm font-medium text-orange-300 mb-2">Şehir</label>
                        <select
                          value={searchFilters.city}
                          onChange={(e) => handleFilterChange("city", e.target.value)}
                          className="w-full px-3 py-2 bg-gray-800 border border-orange-500/30 rounded-lg text-orange-200 focus:border-orange-500/50 focus:outline-none text-sm">
                          <option value="">Tümü</option>
                          <option value="İstanbul">İstanbul</option>
                          <option value="Ankara">Ankara</option>
                          <option value="İzmir">İzmir</option>
                          <option value="Bursa">Bursa</option>
                          <option value="Antalya">Antalya</option>
                        </select>
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-orange-300 mb-2">Spor</label>
                        <select
                          value={searchFilters.sport}
                          onChange={(e) => handleFilterChange("sport", e.target.value)}
                          className="w-full px-3 py-2 bg-gray-800 border border-orange-500/30 rounded-lg text-orange-200 focus:border-orange-500/50 focus:outline-none text-sm">
                          <option value="">Tümü</option>
                          <option value="FUTBOL">Futbol</option>
                          <option value="BASKETBOL">Basketbol</option>
                          <option value="VOLEYBOL">Voleybol</option>
                          <option value="TENIS">Tenis</option>
                        </select>
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-orange-300 mb-2">Rating</label>
                        <select
                          value={searchFilters.rating}
                          onChange={(e) => handleFilterChange("rating", e.target.value)}
                          className="w-full px-3 py-2 bg-gray-800 border border-orange-500/30 rounded-lg text-orange-200 focus:border-orange-500/50 focus:outline-none text-sm">
                          <option value="">Tümü</option>
                          <option value="80-100">80-100</option>
                          <option value="60-79">60-79</option>
                          <option value="40-59">40-59</option>
                          <option value="0-39">0-39</option>
                        </select>
                      </div>
                    </div>
                    
                    {/* Search Results */}
                    <div className="space-y-4 max-h-80 sm:max-h-96 overflow-y-auto">
                      {isLoadingTeams ? (
                        <div className="text-center py-6 sm:py-8">
                          <div className="animate-spin rounded-full h-6 w-6 sm:h-8 sm:w-8 border-b-2 border-orange-500 mx-auto mb-3 sm:mb-4"></div>
                          <div className="text-orange-300 text-sm sm:text-base">Takımlar yükleniyor...</div>
                        </div>
                      ) : teams.length === 0 ? (
                        <div className="text-center py-6 sm:py-8">
                          <Search className="h-8 w-8 sm:h-12 sm:w-12 mx-auto mb-2 sm:mb-3 text-orange-400/50" />
                          <p className="font-medium text-orange-300/80 text-sm">Takım bulunamadı</p>
                          <p className="text-xs text-gray-500">Farklı filtreler deneyin</p>
                        </div>
                      ) : (
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4">
                          {(Array.isArray(teams) ? teams : []).map((team) => {
                            // Bu takıma daha önce istek gönderilmiş mi kontrol et
                            const sentRequest = sentRequests.find(req => req.receiver.id === team.id)
                            return (
                              <div key={team.id} className="border border-orange-500/20 rounded-xl p-3 sm:p-4 bg-gradient-to-br from-gray-800/60 to-gray-900/60 hover:from-orange-900/40 hover:to-gray-900/80 transition-all shadow-md hover:shadow-orange-400/20 flex flex-col h-full min-h-[160px] sm:min-h-[180px]">
                                <div className="flex items-center gap-2 sm:gap-3 flex-1 cursor-pointer" onClick={() => router.push(`/teams/${team.id}`)}>
                                  <Avatar className="h-10 w-10 sm:h-14 sm:w-14 border-2 border-orange-500/40 ring-2 ring-orange-500/20 shadow-md flex-shrink-0">
                                    <AvatarImage
                                      src={team.logo || ''}
                                      alt={`${team.teamName} logo`}
                                      className="object-cover w-full h-full rounded-full"
                                      style={{ objectPosition: 'center' }}
                                    />
                                    <AvatarFallback className="bg-orange-500/20 text-orange-300 font-bold text-sm sm:text-base">
                                      {getSportIcon(team.sport)}
                                    </AvatarFallback>
                                  </Avatar>
                                  <div className="flex flex-col gap-1 min-w-0 flex-1">
                                    <div className="flex items-center gap-1 sm:gap-2 flex-wrap">
                                      <h4 className="font-bold text-orange-200 hover:text-orange-100 transition-colors text-sm sm:text-lg truncate">{team.teamName}</h4>
                                      {team.isPro && (
                                        <CheckCircle className="h-3 w-3 sm:h-4 sm:w-4 text-blue-400 flex-shrink-0" />
                                      )}
                                    </div>
                                    <div className="flex items-center flex-wrap gap-1 sm:gap-2 text-xs sm:text-sm text-gray-400">
                                      <span className="truncate">{getSportIcon(team.sport)} {team.sport}</span>
                                      <span className="truncate">📍 {team.city}</span>
                                      <Badge className={`${getRatingBadgeColor(team.rating)} text-xs`}>
                                        <Star className="h-2 w-2 sm:h-3 sm:w-3 mr-0.5 sm:mr-1" />
                                        {team.rating}
                                      </Badge>
                                    </div>
                                  </div>
                                </div>
                                <div className="mt-3 sm:mt-4 flex justify-end">
                                  {sentRequest ? (
                                    <Button
                                      variant="outline"
                                      size="sm"
                                      className="border-red-500/50 text-red-400 hover:bg-red-500/10 hover:border-red-500 w-full text-xs sm:text-sm"
                                      onClick={() => handleCancelRequest(sentRequest.id, team.teamName)}
                                      disabled={loadingActions.has(sentRequest.id)}
                                    >
                                      {loadingActions.has(sentRequest.id) ? "İptal Ediliyor..." : "İstek İptal Et"}
                                    </Button>
                                  ) : (
                                    <Button
                                      size="sm"
                                      className="bg-orange-500 hover:bg-orange-600 text-white w-full text-xs sm:text-sm"
                                      onClick={() => handleSendRequest(team.id, team.teamName)}
                                      disabled={loadingActions.has(team.id)}
                                    >
                                      {loadingActions.has(team.id) ? "Gönderiliyor..." : "İstek Gönder"}
                                    </Button>
                                  )}
                                </div>
                              </div>
                            )
                          })}
                        </div>
                      )}
                    </div>
                  </div>
                </CardContent>
              </Card>
            </div>
            
            {/* Activity Sidebar */}
            <div>
              <Card className="border border-orange-500/30 shadow-2xl bg-gradient-to-br from-gray-900 to-black backdrop-blur-sm h-full">
                <CardHeader className="pb-3 sm:pb-4 px-4 sm:px-6 pt-4 sm:pt-6">
                  <div className="flex items-center space-x-2">
                    <div className="h-8 w-8 sm:h-10 sm:w-10 bg-orange-500/20 rounded-xl flex items-center justify-center border border-orange-500/40">
                      <Activity className="h-4 w-4 sm:h-5 sm:w-5 text-orange-400" />
                    </div>
                    <div>
                      <CardTitle className="text-lg sm:text-xl text-orange-300">Son Aktiviteler</CardTitle>
                      <CardDescription className="text-gray-400 text-sm">Son hareketler</CardDescription>
                    </div>
                  </div>
                </CardHeader>
                <CardContent className="px-4 sm:px-6 pb-4 sm:pb-6">
                  <div className="space-y-3 sm:space-y-4">
                    {isLoadingActivities ? (
                      <div className="text-center py-4">
                        <div className="animate-spin rounded-full h-5 w-5 sm:h-6 sm:w-6 border-b-2 border-orange-500 mx-auto mb-2"></div>
                        <div className="text-xs text-gray-400">Yükleniyor...</div>
                      </div>
                    ) : activities.length === 0 ? (
                      <div className="text-center py-6 sm:py-8">
                        <Zap className="h-8 w-8 sm:h-10 sm:w-10 mx-auto mb-2 sm:mb-3 text-orange-400/50" />
                        <p className="font-medium text-orange-300/80 text-sm">Henüz aktivite yok</p>
                        <p className="text-xs text-gray-500">İlk ilanını ver!</p>
                      </div>
                    ) : (
                      <div className="space-y-2 sm:space-y-3 max-h-64 sm:max-h-80 overflow-y-auto">
                        {activities.map((activity) => (
                          <div key={activity.id} className="flex items-center space-x-2 sm:space-x-3 p-2 sm:p-3 bg-gray-800/30 rounded-lg border border-orange-500/20">
                            <div className={`h-6 w-6 sm:h-8 sm:w-8 ${getActivityColor(activity.type)} rounded-full flex items-center justify-center flex-shrink-0`}>
                              {getActivityIcon(activity.type)}
                            </div>
                            <div className="flex-1 min-w-0">
                              <p className="text-xs sm:text-sm font-medium text-orange-200 truncate">{activity.title}</p>
                              <p className="text-xs text-gray-400 truncate">{activity.description}</p>
                              <p className="text-xs text-gray-500">{formatDate(activity.createdAt)}</p>
                            </div>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                </CardContent>
              </Card>
            </div>
          </div>
        </div>
      </main>
    </div>
  )
}

