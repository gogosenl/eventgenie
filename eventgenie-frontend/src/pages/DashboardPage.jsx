import { useAuth } from '../context/AuthContext'
import { tripApi, eventApi } from '../services/api'
import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'

function StatCard({ label, value, icon, color = 'var(--accent)' }) {
  return (
    <div className="card fade-up" style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
      <div style={{
        width: 48, height: 48, borderRadius: 12,
        background: `${color}22`,
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        fontSize: 22, flexShrink: 0,
      }}>{icon}</div>
      <div>
        <div style={{ fontSize: '1.6rem', fontFamily: 'var(--font-head)', fontWeight: 700 }}>{value}</div>
        <div style={{ color: 'var(--text2)', fontSize: '0.85rem' }}>{label}</div>
      </div>
    </div>
  )
}

export default function DashboardPage() {
  const { user } = useAuth()
  const [activeTrips, setActiveTrips] = useState([])
  const [archivedTrips, setArchivedTrips] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!user) return
    Promise.all([
      tripApi.getActive(user.userId),
      tripApi.getArchived(user.userId),
    ]).then(([a, b]) => {
      setActiveTrips(a.data || [])
      setArchivedTrips(b.data || [])
    }).catch(() => {}).finally(() => setLoading(false))
  }, [user])

  const stats = [
    { label: 'Aktif Rota',       value: loading ? '—' : activeTrips.length,  icon: '◎', color: 'var(--accent)'  },
    { label: 'Geçmiş Rota',      value: loading ? '—' : archivedTrips.length, icon: '◷', color: 'var(--gold)'   },
    { label: 'Planlanan Durak',  value: loading ? '—' : activeTrips.length,   icon: '◈', color: 'var(--green)'  },
  ]

  return (
    <div className="page">
      {/* Header */}
      <div className="page-header fade-up">
        <h1 className="page-title">
          Merhaba, {user?.userName} ✦
        </h1>
        <p className="page-subtitle">Bugün hangi etkinliği keşfetmek istersin?</p>
      </div>

      {/* Stats */}
      <div className="grid-3" style={{ marginBottom: 32 }}>
        {stats.map((s, i) => (
          <div key={s.label} className={`fade-up fade-up-d${i + 1}`}>
            <StatCard {...s} />
          </div>
        ))}
      </div>

      {/* Quick actions */}
      <div className="card fade-up fade-up-d2" style={{ marginBottom: 24 }}>
        <h2 style={{ fontSize: '1rem', marginBottom: 16, color: 'var(--text2)' }}>Hızlı Erişim</h2>
        <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
          <Link to="/events" className="btn btn-primary">◈ Etkinlik Bul</Link>
          <Link to="/trips" className="btn btn-secondary">◎ Rotalarım</Link>
          <Link to="/nearby" className="btn btn-secondary">◉ Yakınımda</Link>
          <Link to="/chat" className="btn btn-secondary">◫ Chat</Link>
        </div>
      </div>

      {/* Active trips preview */}
      <div className="card fade-up fade-up-d3">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
          <h2 style={{ fontSize: '1rem' }}>Aktif Rota</h2>
          <Link to="/trips" style={{ color: 'var(--accent2)', fontSize: '0.85rem' }}>Tümünü gör →</Link>
        </div>
        {loading ? (
          <div className="loading-center"><div className="spinner" /></div>
        ) : activeTrips.length === 0 ? (
          <div className="empty-state" style={{ padding: '32px 0' }}>
            <span className="icon">◎</span>
            <h3>Aktif rota yok</h3>
            <p>Etkinlikler sayfasından yeni bir rota oluştur.</p>
            <Link to="/events" className="btn btn-primary btn-sm" style={{ marginTop: 8 }}>Etkinlik Bul</Link>
          </div>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
            {activeTrips.slice(0, 5).map(trip => (
              <div key={trip.tripId} style={{
                display: 'flex', alignItems: 'center', gap: 14,
                padding: '12px 14px', background: 'var(--bg3)', borderRadius: 'var(--radius)',
                border: '1px solid var(--border)',
              }}>
                <div style={{
                  width: 8, height: 8, borderRadius: '50%',
                  background: 'var(--accent)', flexShrink: 0,
                  boxShadow: '0 0 6px var(--accent-glow)',
                }} />
                <div style={{ flex: 1, minWidth: 0 }}>
                  <div style={{ fontWeight: 500, fontSize: '0.9rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                    {trip.tripName}
                  </div>
                  {trip.tripComment && (
                    <div style={{ fontSize: '0.78rem', color: 'var(--text3)', marginTop: 2, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                      {trip.tripComment}
                    </div>
                  )}
                </div>
                <div style={{ fontSize: '0.78rem', color: 'var(--text3)', flexShrink: 0 }}>
                  {trip.tripDate ? new Date(trip.tripDate).toLocaleDateString('tr-TR') : '—'}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
