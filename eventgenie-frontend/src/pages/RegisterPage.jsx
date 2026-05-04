import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { useToast } from '../context/ToastContext'

const PREFERENCES = [
  'Pop', 'Rock', 'Hip-Hop/Rap', 'Alternative',
  'Theatre', 'Dance', 'Comedy',
]

export default function RegisterPage() {
  const { register, loading } = useAuth()
  const { addToast } = useToast()
  const navigate = useNavigate()
  const [step, setStep] = useState(1)
  const [form, setForm] = useState({
    userName: '', userSurname: '', userEmail: '',
    userPassword: '', userDateOfBirth: '',
    preferences: '', eventRange: '50',
  })
  const [selectedPrefs, setSelectedPrefs] = useState([])
  const [error, setError] = useState('')

  function togglePref(p) {
    setSelectedPrefs(prev =>
      prev.includes(p) ? prev.filter(x => x !== p) : [...prev, p]
    )
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    const finalForm = {
      ...form,
      preferences: selectedPrefs.join(','),
      eventRange: form.eventRange,
    }
    const res = await register(finalForm)
    if (res.ok) {
      addToast('Hesabınız oluşturuldu! Giriş yapabilirsiniz.', 'success')
      navigate('/login')
    } else {
      setError(res.message)
    }
  }

  return (
    <div style={{
      minHeight: '100vh', background: 'var(--bg)',
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      padding: 24, position: 'relative', overflow: 'hidden',
    }}>
      <div style={{
        position: 'absolute', top: -200, left: -200, width: 600, height: 600, borderRadius: '50%',
        background: 'radial-gradient(circle, rgba(124,106,247,0.1) 0%, transparent 70%)',
        pointerEvents: 'none',
      }} />

      <div className="fade-up" style={{ width: '100%', maxWidth: 480 }}>
        {/* Logo */}
        <div style={{ textAlign: 'center', marginBottom: 40 }}>
          <div style={{
            width: 48, height: 48, borderRadius: 14, background: 'var(--accent)',
            display: 'inline-flex', alignItems: 'center', justifyContent: 'center',
            fontSize: 24, marginBottom: 12, boxShadow: '0 0 24px var(--accent-glow)',
          }}>✦</div>
          <h1 style={{ fontFamily: 'var(--font-head)', fontSize: '1.8rem' }}>EventGenie</h1>
        </div>

        <div className="card" style={{ padding: 32 }}>
          {/* Step indicator */}
          <div style={{ display: 'flex', gap: 8, marginBottom: 28 }}>
            {[1, 2].map(s => (
              <div key={s} style={{
                flex: 1, height: 3, borderRadius: 2,
                background: s <= step ? 'var(--accent)' : 'var(--border2)',
                transition: 'background 0.3s',
              }} />
            ))}
          </div>

          <h2 style={{ fontSize: '1.2rem', marginBottom: 6 }}>
            {step === 1 ? 'Hesap Oluştur' : 'Tercihleriniz'}
          </h2>
          <p style={{ color: 'var(--text2)', fontSize: '0.85rem', marginBottom: 24 }}>
            {step === 1 ? 'Kişisel bilgilerinizi girin' : 'Sizi daha iyi tanıyalım'}
          </p>

          {step === 1 ? (
            <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                <div className="form-group">
                  <label>Ad</label>
                  <input placeholder="Adınız" value={form.userName}
                    onChange={e => setForm(p => ({ ...p, userName: e.target.value }))} />
                </div>
                <div className="form-group">
                  <label>Soyad</label>
                  <input placeholder="Soyadınız" value={form.userSurname}
                    onChange={e => setForm(p => ({ ...p, userSurname: e.target.value }))} />
                </div>
              </div>
              <div className="form-group">
                <label>E-posta</label>
                <input type="email" placeholder="ornek@email.com" value={form.userEmail}
                  onChange={e => setForm(p => ({ ...p, userEmail: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Şifre</label>
                <input type="password" placeholder="En az 6 karakter" value={form.userPassword}
                  onChange={e => setForm(p => ({ ...p, userPassword: e.target.value }))} />
              </div>
              <div className="form-group">
                <label>Doğum Tarihi</label>
                <input type="date" value={form.userDateOfBirth}
                  onChange={e => setForm(p => ({ ...p, userDateOfBirth: e.target.value }))} />
              </div>
              {error && <p className="error-msg">⚠ {error}</p>}
              <button className="btn btn-primary" onClick={() => {
                if (!form.userName || !form.userEmail || !form.userPassword) {
                  setError('Lütfen tüm alanları doldurun.'); return
                }
                setError(''); setStep(2)
              }}>
                Devam →
              </button>
            </div>
          ) : (
            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
              <div>
                <label className="label" style={{ display: 'block', marginBottom: 12 }}>
                  İlgi Alanlarınız
                </label>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8 }}>
                  {PREFERENCES.map(p => (
                    <button
                      key={p} type="button"
                      onClick={() => togglePref(p)}
                      style={{
                        padding: '6px 14px', borderRadius: 100,
                        fontSize: '0.85rem', fontWeight: 500, cursor: 'pointer',
                        background: selectedPrefs.includes(p) ? 'var(--accent)' : 'var(--bg3)',
                        color: selectedPrefs.includes(p) ? '#fff' : 'var(--text2)',
                        border: `1px solid ${selectedPrefs.includes(p) ? 'var(--accent)' : 'var(--border2)'}`,
                        transition: 'all var(--transition)',
                      }}
                    >{p}</button>
                  ))}
                </div>
              </div>
              <div className="form-group">
                <label>Etkinlik Arama Mesafesi: {form.eventRange} km</label>
                <input type="range" min="10" max="500" value={form.eventRange}
                  onChange={e => setForm(p => ({ ...p, eventRange: e.target.value }))}
                  style={{ padding: 0, border: 'none', background: 'transparent' }} />
              </div>
              {error && <p className="error-msg">⚠ {error}</p>}
              <div style={{ display: 'flex', gap: 10 }}>
                <button type="button" className="btn btn-secondary" onClick={() => setStep(1)} style={{ flex: 1 }}>
                  ← Geri
                </button>
                <button type="submit" className="btn btn-primary" disabled={loading} style={{ flex: 2 }}>
                  {loading ? <><span className="spinner" style={{ width: 14, height: 14 }} /> Kayıt olunuyor...</> : 'Kayıt Ol'}
                </button>
              </div>
            </form>
          )}
        </div>

        <p style={{ textAlign: 'center', marginTop: 20, color: 'var(--text2)', fontSize: '0.9rem' }}>
          Zaten hesabın var mı?{' '}
          <Link to="/login" style={{ color: 'var(--accent2)', fontWeight: 500 }}>Giriş yap</Link>
        </p>
      </div>
    </div>
  )
}
