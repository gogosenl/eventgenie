import { createContext, useContext, useState, useCallback } from 'react'
import { userApi } from '../services/api'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    try {
      const stored = localStorage.getItem('eg_user')
      return stored ? JSON.parse(stored) : null
    } catch { return null }
  })
  const [loading, setLoading] = useState(false)

  const login = useCallback(async (email, password) => {
    setLoading(true)
    try {
      const res = await userApi.login({ email, password })
      const data = res.data
      if (data.success) {
        const userData = { userId: data.data.userId, userName: data.data.userName, email }
        setUser(userData)
        localStorage.setItem('eg_user', JSON.stringify(userData))
        return { ok: true }
      }
      return { ok: false, message: data.message }
    } catch (err) {
      const msg = err.response?.data?.message || 'Giriş yapılamadı.'
      return { ok: false, message: msg }
    } finally {
      setLoading(false)
    }
  }, [])

  const logout = useCallback(() => {
    setUser(null)
    localStorage.removeItem('eg_user')
  }, [])

  const register = useCallback(async (formData) => {
    setLoading(true)
    try {
      const res = await userApi.create(formData)
      if (res.data.success) return { ok: true }
      return { ok: false, message: res.data.message }
    } catch (err) {
      const msg = err.response?.data?.message || 'Kayıt yapılamadı.'
      return { ok: false, message: msg }
    } finally {
      setLoading(false)
    }
  }, [])

  return (
    <AuthContext.Provider value={{ user, loading, login, logout, register }}>
      {children}
    </AuthContext.Provider>
  )
}

export const useAuth = () => {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
