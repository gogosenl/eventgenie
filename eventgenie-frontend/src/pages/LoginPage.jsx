import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { useToast } from '../context/ToastContext'

export default function LoginPage() {
  const { login, loading } = useAuth()
  const { addToast } = useToast()
  const navigate = useNavigate()
  const [form, setForm] = useState({ email: '', password: '' })
  const [error, setError] = useState('')

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    if (!form.email || !form.password) { setError('Tüm alanları doldurun.'); return }
    const res = await login(form.email, form.password)
    if (res.ok) {
      addToast('Hoş geldiniz!', 'success')
      navigate('/dashboard')
    } else {
      setError(res.message)
    }
  }

  return (
    <div style={{
      minHeight: '100vh',
      background: 'var(--bg)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      padding: 24,
      position: 'relative',
      overflow: 'hidden',
    }}>
      {/* Background decoration */}
      <div style={{
        position: 'absolute', top: -200, right: -200,
        width: 600, height: 600, borderRadius: '50%',
        background: 'radial-gradient(circle, rgba(124,106,247,0.12) 0%, transparent 70%)',
        pointerEvents: 'none',
      }} />
      <div style={{
        position: 'absolute', bottom: -200, left: -200,
        width: 500, height: 500, borderRadius: '50%',
        background: 'radial-gradient(circle, rgba(167,139,250,0.08) 0%, transparent 70%)',
        pointerEvents: 'none',
      }} />

      <div className="fade-up" style={{ width: '100%', maxWidth: 440, position: 'relative' }}>
        {/* Logo */}
        <div style={{ textAlign: 'center', marginBottom: 48 }}>
          <div style={{
            width: 56, height: 56, borderRadius: 16,
            background: 'var(--accent)',
            display: 'inline-flex', alignItems: 'center', justifyContent: 'center',
            fontSize: 28, marginBottom: 16,
            boxShadow: '0 0 32px var(--accent-glow)',
          }}>✦</div>
          <h1 style={{ fontFamily: 'var(--font-head)', fontSize: '2rem', color: 'var(--text)', marginBottom: 6 }}>
            EventGenie
          </h1>
          <p style={{ color: 'var(--text2)', fontSize: '0.95rem' }}>
            Etkinlik keşfet, rotanı planla
          </p>
        </div>

        {/* Form card */}
        <div className="card" style={{ padding: 32 }}>
          <h2 style={{ fontSize: '1.3rem', marginBottom: 24, color: 'var(--text)' }}>Giriş Yap</h2>
          <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 18 }}>
            <div className="form-group">
              <label>E-posta</label>
              <input
                type="email"
                placeholder="ornek@email.com"
                value={form.email}
                onChange={e => setForm(p => ({ ...p, email: e.target.value }))}
                autoComplete="email"
              />
            </div>
            <div className="form-group">
              <label>Şifre</label>
              <input
                type="password"
                placeholder="••••••••"
                value={form.password}
                onChange={e => setForm(p => ({ ...p, password: e.target.value }))}
                autoComplete="current-password"
              />
            </div>
            {error && <p className="error-msg">⚠ {error}</p>}
            <button type="submit" className="btn btn-primary" disabled={loading} style={{ marginTop: 4 }}>
              {loading ? <><span className="spinner" style={{ width: 16, height: 16 }} /> Giriş yapılıyor...</> : 'Giriş Yap'}
            </button>
          </form>
        </div>

        <p style={{ textAlign: 'center', marginTop: 20, color: 'var(--text2)', fontSize: '0.9rem' }}>
          Hesabın yok mu?{' '}
          <Link to="/register" style={{ color: 'var(--accent2)', fontWeight: 500 }}>
            Kayıt ol
          </Link>
        </p>
      </div>
    </div>
  )
}
