import { useState, useEffect } from 'react'
import { useAuth } from '../context/AuthContext'
import { useToast } from '../context/ToastContext'
import { userApi, locationApi } from '../services/api'

export default function ProfilePage() {
  const { user, logout } = useAuth()
  const { addToast } = useToast()
  const [profile, setProfile] = useState(null)
  const [location, setLocation] = useState(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState({})
  const TURKEY_CITIES_COORDS = [
  'Adana','Adıyaman','Afyonkarahisar','Ağrı','Aksaray','Amasya','Ankara','Antalya',
  'Artvin','Ardahan','Aydın','Balıkesir','Bartın','Batman','Bayburt','Bilecik',
  'Bingöl','Bitlis','Bolu','Burdur','Bursa','Çanakkale','Çankırı','Çorum',
  'Denizli','Diyarbakır','Düzce','Edirne','Elazığ','Erzincan','Erzurum','Eskişehir',
  'Gaziantep','Giresun','Gümüşhane','Hakkari','Hatay','Iğdır','Isparta','İstanbul',
  'İzmir','Kahramanmaraş','Karabük','Karaman','Kars','Kastamonu','Kayseri','Kilis',
  'Kırıkkale','Kırklareli','Kırşehir','Kocaeli','Konya','Kütahya','Malatya','Manisa',
  'Mardin','Mersin','Muğla','Muş','Nevşehir','Niğde','Ordu','Osmaniye','Rize',
  'Sakarya','Samsun','Şanlıurfa','Siirt','Sinop','Sivas','Şırnak','Tekirdağ',
  'Tokat','Trabzon','Tunceli','Uşak','Van','Yalova','Yozgat','Zonguldak'
]

const CITY_COORDS = {
  'Adana':{lat:37.0,lng:35.32},'Adıyaman':{lat:37.76,lng:38.28},
  'Afyonkarahisar':{lat:38.74,lng:30.54},'Ağrı':{lat:39.72,lng:43.05},
  'Aksaray':{lat:38.37,lng:34.03},'Amasya':{lat:40.65,lng:35.83},
  'Ankara':{lat:39.93,lng:32.85},'Antalya':{lat:36.89,lng:30.71},
  'Artvin':{lat:41.18,lng:41.82},'Ardahan':{lat:41.11,lng:42.70},
  'Aydın':{lat:37.85,lng:27.85},'Balıkesir':{lat:39.65,lng:27.89},
  'Bartın':{lat:41.64,lng:32.34},'Batman':{lat:37.88,lng:41.13},
  'Bayburt':{lat:40.26,lng:40.23},'Bilecik':{lat:40.05,lng:30.00},
  'Bingöl':{lat:38.88,lng:40.50},'Bitlis':{lat:38.40,lng:42.11},
  'Bolu':{lat:40.73,lng:31.61},'Burdur':{lat:37.72,lng:30.29},
  'Bursa':{lat:40.19,lng:29.06},'Çanakkale':{lat:40.15,lng:26.41},
  'Çankırı':{lat:40.60,lng:33.62},'Çorum':{lat:40.55,lng:34.96},
  'Denizli':{lat:37.77,lng:29.09},'Diyarbakır':{lat:37.91,lng:40.24},
  'Düzce':{lat:40.84,lng:31.16},'Edirne':{lat:41.68,lng:26.56},
  'Elazığ':{lat:38.67,lng:39.22},'Erzincan':{lat:39.75,lng:39.49},
  'Erzurum':{lat:39.90,lng:41.27},'Eskişehir':{lat:39.78,lng:30.52},
  'Gaziantep':{lat:37.07,lng:37.38},'Giresun':{lat:40.91,lng:38.39},
  'Gümüşhane':{lat:40.46,lng:39.48},'Hakkari':{lat:37.57,lng:43.74},
  'Hatay':{lat:36.40,lng:36.35},'Iğdır':{lat:39.92,lng:44.05},
  'Isparta':{lat:37.76,lng:30.55},'İstanbul':{lat:41.01,lng:28.98},
  'İzmir':{lat:38.42,lng:27.14},'Kahramanmaraş':{lat:37.59,lng:36.93},
  'Karabük':{lat:41.20,lng:32.63},'Karaman':{lat:37.18,lng:33.23},
  'Kars':{lat:40.60,lng:43.10},'Kastamonu':{lat:41.39,lng:33.78},
  'Kayseri':{lat:38.73,lng:35.49},'Kilis':{lat:36.72,lng:37.12},
  'Kırıkkale':{lat:39.85,lng:33.51},'Kırklareli':{lat:41.74,lng:27.23},
  'Kırşehir':{lat:39.15,lng:34.17},'Kocaeli':{lat:40.77,lng:29.92},
  'Konya':{lat:37.87,lng:32.48},'Kütahya':{lat:39.42,lng:29.98},
  'Malatya':{lat:38.35,lng:38.31},'Manisa':{lat:38.61,lng:27.43},
  'Mardin':{lat:37.31,lng:40.73},'Mersin':{lat:36.80,lng:34.63},
  'Muğla':{lat:37.22,lng:28.36},'Muş':{lat:38.95,lng:41.75},
  'Nevşehir':{lat:38.63,lng:34.71},'Niğde':{lat:37.97,lng:34.68},
  'Ordu':{lat:40.98,lng:37.88},'Osmaniye':{lat:37.07,lng:36.25},
  'Rize':{lat:41.02,lng:40.52},'Sakarya':{lat:40.69,lng:30.40},
  'Samsun':{lat:41.29,lng:36.33},'Şanlıurfa':{lat:37.16,lng:38.79},
  'Siirt':{lat:37.93,lng:41.94},'Sinop':{lat:42.03,lng:35.15},
  'Sivas':{lat:39.75,lng:37.02},'Şırnak':{lat:37.52,lng:42.46},
  'Tekirdağ':{lat:41.00,lng:27.52},'Tokat':{lat:40.31,lng:36.55},
  'Trabzon':{lat:41.00,lng:39.72},'Tunceli':{lat:39.11,lng:39.55},
  'Uşak':{lat:38.67,lng:29.41},'Van':{lat:38.49,lng:43.38},
  'Yalova':{lat:40.66,lng:29.27},'Yozgat':{lat:39.82,lng:34.81},
  'Zonguldak':{lat:41.46,lng:31.79}
}

const PREFERENCES = [
  'Pop', 'Rock', 'Hip-Hop/Rap', 'Alternative',
  'Theatre', 'Dance', 'Comedy',
]

function Section({ title, children }) {
  return (
    <div className="card fade-up" style={{ marginBottom: 20 }}>
      <h2 style={{ fontSize: '1rem', fontFamily: 'var(--font-head)', marginBottom: 20, paddingBottom: 12, borderBottom: '1px solid var(--border)' }}>
        {title}
      </h2>
      {children}
    </div>
  )
}

function CitySelector({ value, onChange }) {
  const [query, setQuery] = useState(value || '')
  const [open, setOpen] = useState(false)
  const normalize = s => s.toLocaleLowerCase('tr-TR')
  const filtered = query.length === 0 ? [] : TURKEY_CITIES_COORDS.filter(c =>
    normalize(c).includes(normalize(query))
  ).slice(0, 8)

  return (
    <div style={{ position: 'relative' }}>
      <input
        value={query}
        placeholder="Şehir ara..."
        onChange={e => { setQuery(e.target.value); setOpen(true) }}
        onFocus={() => setOpen(true)}
        onBlur={() => setTimeout(() => setOpen(false), 150)}
      />
      {open && filtered.length > 0 && (
        <div style={{
          position: 'absolute', top: '105%', left: 0, right: 0, zIndex: 200,
          background: 'var(--surface)', border: '1px solid var(--border2)',
          borderRadius: 'var(--radius)', boxShadow: 'var(--shadow)', overflow: 'hidden',
        }}>
          {filtered.map(c => (
            <div key={c} onMouseDown={() => { onChange(c); setQuery(c); setOpen(false) }}
              style={{
                padding: '9px 14px', fontSize: '0.88rem', cursor: 'pointer',
                borderBottom: '1px solid var(--border)',
              }}
              onMouseEnter={e => e.currentTarget.style.background = 'var(--bg3)'}
              onMouseLeave={e => e.currentTarget.style.background = 'transparent'}
            >{c}</div>
          ))}
        </div>
      )}
    </div>
  )
}



  // Form states
  const [pwForm, setPwForm] = useState({ currentPassword: '', newPassword: '' })
  const [selectedPrefs, setSelectedPrefs] = useState([])
  const [eventRange, setEventRange] = useState('50')
  const [locForm, setLocForm] = useState({ country: '', city: '', latitude: '', longitude: '' })

  useEffect(() => {
    if (!user) return
    Promise.all([
      userApi.getById(user.userId),
      locationApi.getByUser(user.userId).catch(() => ({ data: { data: null } })),
    ]).then(([u, l]) => {
      const p = u.data.data
      setProfile(p)
      setSelectedPrefs(p.preferences ? p.preferences.split(',').map(s => s.trim()).filter(Boolean) : [])
      setEventRange(p.eventRange || '50')
      const loc = l.data.data
      if (loc) {
        setLocation(loc)
        setLocForm({
          country: loc.country || '',
          city: loc.city || '',
          latitude: String(loc.latitude || ''),
          longitude: String(loc.longitude || ''),
        })
      }
    }).catch(() => addToast('Profil yüklenemedi.', 'error'))
      .finally(() => setLoading(false))
  }, [user])

  function togglePref(p) {
    setSelectedPrefs(prev => prev.includes(p) ? prev.filter(x => x !== p) : [...prev, p])
  }

  async function savePreferences() {
    setSaving(s => ({ ...s, prefs: true }))
    try {
      await userApi.updatePreferences(user.userId, selectedPrefs.join(','))
      addToast('Tercihler kaydedildi.', 'success')
    } catch { addToast('Kaydedilemedi.', 'error') }
    finally { setSaving(s => ({ ...s, prefs: false })) }
  }

  async function saveEventRange() {
    setSaving(s => ({ ...s, range: true }))
    try {
      await userApi.update(user.userId, { ...profile, eventRange })
      addToast('Mesafe güncellendi.', 'success')
    } catch { addToast('Kaydedilemedi.', 'error') }
    finally { setSaving(s => ({ ...s, range: false })) }
  }

  async function changePassword(e) {
    e.preventDefault()
    if (!pwForm.currentPassword || !pwForm.newPassword) {
      addToast('Tüm alanları doldurun.', 'error'); return
    }
    setSaving(s => ({ ...s, pw: true }))
    try {
      await userApi.changePassword(user.userId, pwForm)
      addToast('Şifre değiştirildi.', 'success')
      setPwForm({ currentPassword: '', newPassword: '' })
    } catch (err) {
      addToast(err.response?.data?.message || 'Şifre değiştirilemedi.', 'error')
    } finally { setSaving(s => ({ ...s, pw: false })) }
  }

  async function saveLocation(e) {
    e.preventDefault()
    setSaving(s => ({ ...s, loc: true }))
    const locData = {
      ...locForm,
      latitude: parseFloat(locForm.latitude) || 0,
      longitude: parseFloat(locForm.longitude) || 0,
      isAutoDetected: false,
      userId: user.userId,
    }
    try {
      if (location) {
        await locationApi.update(user.userId, locData)
      } else {
        await locationApi.create({ ...locData })
      }
      addToast('Konum kaydedildi.', 'success')
    } catch (err) {
      addToast(err.response?.data?.message || 'Konum kaydedilemedi.', 'error')
    } finally { setSaving(s => ({ ...s, loc: false })) }
  }

  function detectLocation() {
  if (!navigator.geolocation) { addToast('GPS desteklenmiyor.', 'error'); return }
  navigator.geolocation.getCurrentPosition(pos => {
    const lat = pos.coords.latitude
    const lng = pos.coords.longitude
    // En yakın şehri bul
    const nearest = findNearestCity(lat, lng)
    const coords = nearest ? CITY_COORDS[nearest] : null
    setLocForm(p => ({
      ...p,
      latitude: lat.toFixed(6),
      longitude: lng.toFixed(6),
      city: nearest || p.city,
      country: 'Turkey',
    }))
    addToast(`Konum tespit edildi: ${nearest || 'Bilinmiyor'}`, 'success')
  }, () => addToast('Konum alınamadı.', 'error'))
}

function findNearestCity(lat, lng) {
  let nearest = null
  let minDist = Infinity
  for (const [city, coords] of Object.entries(CITY_COORDS)) {
    const d = Math.sqrt(Math.pow(lat - coords.lat, 2) + Math.pow(lng - coords.lng, 2))
    if (d < minDist) { minDist = d; nearest = city }
  }
  return nearest
}

  if (loading) return <div className="loading-center" style={{ minHeight: '100vh' }}><div className="spinner" /><span>Yükleniyor...</span></div>

  return (
    <div className="page" style={{ maxWidth: 720 }}>
      <div className="page-header fade-up">
        <h1 className="page-title">Profilim</h1>
        <p className="page-subtitle">Hesap bilgilerinizi yönetin</p>
      </div>

      {/* User info */}
      <Section title="Hesap Bilgileri">
        <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
          <div style={{
            width: 56, height: 56, borderRadius: 16,
            background: 'var(--accent)', display: 'flex', alignItems: 'center', justifyContent: 'center',
            fontSize: 24, fontWeight: 700, color: '#fff', flexShrink: 0,
            boxShadow: '0 0 20px var(--accent-glow)',
          }}>
            {(user?.userName || '?').charAt(0).toUpperCase()}
          </div>
          <div>
            <div style={{ fontFamily: 'var(--font-head)', fontSize: '1.1rem', fontWeight: 600 }}>
              {profile?.userName} {profile?.userSurname}
            </div>
            <div style={{ color: 'var(--text2)', fontSize: '0.85rem', marginTop: 2 }}>
              {profile?.userEmail}
            </div>
            {profile?.userDateOfBirth && (
              <div style={{ color: 'var(--text3)', fontSize: '0.78rem', marginTop: 4 }}>
                Doğum: {new Date(profile.userDateOfBirth).toLocaleDateString('tr-TR')}
              </div>
            )}
          </div>
        </div>
      </Section>

      {/* Preferences */}
      <Section title="✦ İlgi Alanlarım">
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8, marginBottom: 16 }}>
          {PREFERENCES.map(p => (
            <button key={p} type="button" onClick={() => togglePref(p)}
              style={{
                padding: '7px 16px', borderRadius: 100, fontSize: '0.85rem', fontWeight: 500, cursor: 'pointer',
                background: selectedPrefs.includes(p) ? 'var(--accent)' : 'var(--bg3)',
                color: selectedPrefs.includes(p) ? '#fff' : 'var(--text2)',
                border: `1px solid ${selectedPrefs.includes(p) ? 'var(--accent)' : 'var(--border2)'}`,
                transition: 'all var(--transition)',
              }}
            >{p}</button>
          ))}
        </div>
        <button className="btn btn-primary btn-sm" onClick={savePreferences} disabled={saving.prefs}>
          {saving.prefs ? <><span className="spinner" style={{ width: 12, height: 12 }} /> Kaydediliyor...</> : 'Tercihleri Kaydet'}
        </button>
      </Section>

      {/* Event range */}
      <Section title="Etkinlik Arama Mesafesi">
        <div style={{ marginBottom: 12 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
            <label style={{ fontSize: '0.88rem', color: 'var(--text2)' }}>Mesafe</label>
            <span style={{ fontFamily: 'var(--font-head)', fontWeight: 700, color: 'var(--accent2)' }}>
              {eventRange} km
            </span>
          </div>
          <input type="range" min="10" max="500" value={eventRange}
            onChange={e => setEventRange(e.target.value)}
            style={{ padding: 0, border: 'none', background: 'transparent', cursor: 'pointer' }} />
        </div>
        <button className="btn btn-primary btn-sm" onClick={saveEventRange} disabled={saving.range}>
          {saving.range ? <><span className="spinner" style={{ width: 12, height: 12 }} /> Kaydediliyor...</> : 'Mesafeyi Kaydet'}
        </button>
      </Section>

      <Section title="◉ Konumum">
  <form onSubmit={saveLocation} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
      <div className="form-group">
        <label>Şehir Seç</label>
        <CitySelector
          value={locForm.city}
          onChange={city => {
            const coords = CITY_COORDS[city]
            setLocForm(p => ({
              ...p,
              city,
              country: 'Turkey',
              latitude: coords ? String(coords.lat) : p.latitude,
              longitude: coords ? String(coords.lng) : p.longitude,
            }))
          }}
        />
      </div>
      <div className="form-group">
        <label>Ülke</label>
        <input value={locForm.country} onChange={e => setLocForm(p => ({ ...p, country: e.target.value }))} placeholder="Turkey" />
      </div>
      <div className="form-group">
        <label>Enlem (otomatik)</label>
        <input value={locForm.latitude} readOnly style={{ opacity: 0.6 }} />
      </div>
      <div className="form-group">
        <label>Boylam (otomatik)</label>
        <input value={locForm.longitude} readOnly style={{ opacity: 0.6 }} />
      </div>
    </div>
    <div style={{ display: 'flex', gap: 10 }}>
      <button type="button" className="btn btn-secondary btn-sm" onClick={detectLocation}>
        ◉ GPS ile Otomatik Tespit
      </button>
      <button type="submit" className="btn btn-primary btn-sm" disabled={saving.loc}>
        {saving.loc ? <><span className="spinner" style={{ width: 12, height: 12 }} /> Kaydediliyor...</> : 'Konumu Kaydet'}
      </button>
    </div>
  </form>
</Section>

      {/* Password */}
      <Section title="Şifre Değiştir">
        <form onSubmit={changePassword} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          <div className="form-group">
            <label>Mevcut Şifre</label>
            <input type="password" placeholder="••••••••" value={pwForm.currentPassword}
              onChange={e => setPwForm(p => ({ ...p, currentPassword: e.target.value }))} />
          </div>
          <div className="form-group">
            <label>Yeni Şifre</label>
            <input type="password" placeholder="En az 6 karakter" value={pwForm.newPassword}
              onChange={e => setPwForm(p => ({ ...p, newPassword: e.target.value }))} />
          </div>
          <button type="submit" className="btn btn-primary btn-sm" disabled={saving.pw}>
            {saving.pw ? <><span className="spinner" style={{ width: 12, height: 12 }} /> Değiştiriliyor...</> : 'Şifreyi Değiştir'}
          </button>
        </form>
      </Section>

      {/* Danger zone */}
      <div className="card fade-up" style={{ borderColor: 'rgba(248,113,113,0.2)', background: 'rgba(248,113,113,0.03)' }}>
        <h2 style={{ fontSize: '0.95rem', color: 'var(--red)', marginBottom: 12 }}>Çıkış</h2>
        <p style={{ fontSize: '0.85rem', color: 'var(--text2)', marginBottom: 14 }}>
          Oturumunuzu güvenli bir şekilde kapatın.
        </p>
        <button className="btn btn-danger btn-sm" onClick={logout}>
          ⊗ Çıkış Yap
        </button>
      </div>
    </div>
  )
}
