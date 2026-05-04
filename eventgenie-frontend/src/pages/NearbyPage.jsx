import { useState, useEffect } from 'react'
import { useAuth } from '../context/AuthContext'
import { useToast } from '../context/ToastContext'
import { eventApi } from '../services/api'

function NearbyCard({ event }) {
  return (
    <div className="card fade-up" style={{
      border: `1px solid ${event.isPreferenceMatch ? 'rgba(124,106,247,0.3)' : 'var(--border)'}`,
      background: event.isPreferenceMatch ? 'rgba(124,106,247,0.04)' : 'var(--surface)',
      transition: 'transform var(--transition)',
    }}
      onMouseEnter={e => e.currentTarget.style.transform = 'translateY(-2px)'}
      onMouseLeave={e => e.currentTarget.style.transform = 'translateY(0)'}
    >
      {event.imageUrl && (
        <div style={{
          width: '100%', height: 140, borderRadius: 10, overflow: 'hidden',
          marginBottom: 14, background: 'var(--bg3)',
        }}>
          <img src={event.imageUrl} alt={event.eventName}
            style={{ width: '100%', height: '100%', objectFit: 'cover' }}
            onError={e => e.currentTarget.style.display = 'none'} />
        </div>
      )}

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 8, gap: 8 }}>
        <h3 style={{
          fontSize: '0.95rem', fontFamily: 'var(--font-head)', lineHeight: 1.3,
          overflow: 'hidden', textOverflow: 'ellipsis',
          display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical',
          flex: 1,
        }}>
          {event.eventName}
        </h3>
        {event.isPreferenceMatch
          ? <span className="badge badge-accent">✦ Senin İçin</span>
          : <span className="badge" style={{ background: 'var(--bg3)', color: 'var(--text3)' }}>Yakında</span>
        }
      </div>

      <div style={{ display: 'flex', flexDirection: 'column', gap: 5, marginBottom: 12 }}>
        {event.venue && (
          <div style={{ fontSize: '0.8rem', color: 'var(--text2)', display: 'flex', gap: 6 }}>
            <span>◉</span>
            <span>{event.venue}{event.city ? `, ${event.city}` : ''}</span>
          </div>
        )}
        {event.eventDate && (
          <div style={{ fontSize: '0.8rem', color: 'var(--text2)', display: 'flex', gap: 6 }}>
            <span>◷</span>
            <span>{new Date(event.eventDate).toLocaleDateString('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' })}</span>
          </div>
        )}
        {event.genre && (
          <span className="tag" style={{ width: 'fit-content', marginTop: 2 }}>{event.genre}</span>
        )}
        {event.priceRange && (
  <div style={{ fontSize: '0.8rem', color: 'var(--gold)', display: 'flex', gap: 6, fontWeight: 500 }}>
    <span>₺</span>
    <span>{event.priceRange}</span>
  </div>
)}
      </div>

      {event.eventUrl && (
        <a href={event.eventUrl} target="_blank" rel="noreferrer" className="btn btn-sm btn-secondary" style={{ width: '100%', justifyContent: 'center' }}>
          Bilet Al ↗
        </a>
      )}
    </div>
  )
}

export default function NearbyPage() {
  const { user } = useAuth()
  const { addToast } = useToast()
  const [events, setEvents] = useState([])
  const [loading, setLoading] = useState(false)
  const [useGps, setUseGps] = useState(false)
  const [coords, setCoords] = useState(null)
  const [filter, setFilter] = useState('all') // 'all' | 'preference' | 'nearby'

  async function fetchEvents(lat, lng) {
    setLoading(true)
    try {
      const res = await eventApi.getNearby(user.userId, lat, lng)
      setEvents(res.data?.data || [])
      if ((res.data?.data || []).length === 0) addToast('Yakında etkinlik bulunamadı.', 'info')
      else addToast(`${res.data.count} etkinlik bulundu!`, 'success')
    } catch (err) {
      addToast(err.response?.data?.message || 'Etkinlikler yüklenemedi.', 'error')
    } finally {
      setLoading(false)
    }
  }

  function handleSearch() {
    if (useGps) {
      if (!navigator.geolocation) { addToast('Tarayıcınız konum desteklemiyor.', 'error'); return }
      navigator.geolocation.getCurrentPosition(
        pos => {
          const { latitude: lat, longitude: lng } = pos.coords
          setCoords({ lat, lng })
          fetchEvents(lat, lng)
        },
        () => { addToast('Konum alınamadı.', 'error') }
      )
    } else {
      fetchEvents(null, null)
    }
  }

  const filtered = filter === 'preference'
    ? events.filter(e => e.isPreferenceMatch)
    : filter === 'nearby'
    ? events.filter(e => !e.isPreferenceMatch)
    : events

  const preferenceCount = events.filter(e => e.isPreferenceMatch).length

  return (
    <div className="page">
      <div className="page-header fade-up">
        <h1 className="page-title">Yakınımdaki Etkinlikler</h1>
        <p className="page-subtitle">Konumuna göre etkinlik keşfet, tercihlerine uyanları öğren</p>
      </div>

      {/* Search panel */}
      <div className="card fade-up fade-up-d1" style={{ marginBottom: 24 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 16, flexWrap: 'wrap' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: 8, cursor: 'pointer' }}>
            <div style={{
              width: 44, height: 24, borderRadius: 12,
              background: useGps ? 'var(--accent)' : 'var(--bg3)',
              position: 'relative', transition: 'background 0.2s', flexShrink: 0,
              border: '1px solid var(--border2)',
            }} onClick={() => setUseGps(p => !p)}>
              <div style={{
                width: 18, height: 18, borderRadius: '50%', background: '#fff',
                position: 'absolute', top: 2,
                left: useGps ? 22 : 2,
                transition: 'left 0.2s',
              }} />
            </div>
            <span style={{ fontSize: '0.9rem', color: 'var(--text2)' }}>
              {useGps ? 'GPS Konumumu Kullan' : 'Profilimdeki Konumu Kullan'}
            </span>
          </label>
        </div>
        <button className="btn btn-primary" onClick={handleSearch} disabled={loading}>
          {loading
            ? <><span className="spinner" style={{ width: 14, height: 14 }} /> Aranıyor...</>
            : '◉ Yakındaki Etkinlikleri Getir'}
        </button>
      </div>

      {/* Filter tabs */}
      {events.length > 0 && (
        <div style={{ display: 'flex', gap: 8, marginBottom: 20, flexWrap: 'wrap', alignItems: 'center' }}>
          {[['all', `Tümü (${events.length})`], ['preference', `✦ Senin İçin (${preferenceCount})`], ['nearby', `Yakında (${events.length - preferenceCount})`]].map(([key, label]) => (
            <button key={key} onClick={() => setFilter(key)}
              className="btn btn-sm"
              style={{
                background: filter === key ? 'var(--accent)' : 'var(--surface)',
                color: filter === key ? '#fff' : 'var(--text2)',
                border: `1px solid ${filter === key ? 'var(--accent)' : 'var(--border2)'}`,
              }}>{label}</button>
          ))}
        </div>
      )}

      {/* Results */}
{filtered.length > 0 ? (
  <div className="grid-3">
    {filtered.map((ev, i) => (
      <NearbyCard key={i} event={ev} />
    ))}
  </div>
      ) : events.length > 0 ? (
        <div className="empty-state card">
          <span className="icon">◉</span>
          <h3>Bu filtreda etkinlik yok</h3>
        </div>
      ) : null}
    </div>
  )
}
