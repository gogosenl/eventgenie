import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { useState } from 'react'

const NAV = [
  { to: '/dashboard',  icon: '⬡', label: 'Dashboard'   },
  { to: '/events',     icon: '◈', label: 'Etkinlikler' },
  { to: '/trips',      icon: '◎', label: 'Rotalarım'   },
  { to: '/nearby',     icon: '◉', label: 'Yakınımda'   },
  { to: '/chat',       icon: '◫', label: 'Chat'        },
  { to: '/profile',    icon: '◯', label: 'Profil'      },
]

export default function Sidebar() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [collapsed, setCollapsed] = useState(false)

  function handleLogout() {
    logout()
    navigate('/login')
  }

  return (
    <aside style={{
      width: collapsed ? 68 : 220,
      minHeight: '100vh',
      background: 'var(--bg2)',
      borderRight: '1px solid var(--border)',
      display: 'flex',
      flexDirection: 'column',
      transition: 'width 0.3s cubic-bezier(0.4,0,0.2,1)',
      overflow: 'hidden',
      flexShrink: 0,
      position: 'sticky',
      top: 0,
    }}>
      {/* Logo */}
      <div style={{
        padding: '24px 16px 20px',
        display: 'flex',
        alignItems: 'center',
        gap: 10,
        borderBottom: '1px solid var(--border)',
      }}>
        <div style={{
          width: 36, height: 36, borderRadius: 10,
          background: 'var(--accent)',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          fontSize: 18, flexShrink: 0,
          boxShadow: '0 0 16px var(--accent-glow)',
        }}>✦</div>
        {!collapsed && (
          <span style={{ fontFamily: 'var(--font-head)', fontSize: '1.1rem', fontWeight: 700, color: 'var(--text)', whiteSpace: 'nowrap' }}>
            EventGenie
          </span>
        )}
        <button
          onClick={() => setCollapsed(p => !p)}
          style={{ marginLeft: 'auto', color: 'var(--text3)', fontSize: 14, flexShrink: 0 }}
          className="btn-ghost"
          title={collapsed ? 'Genişlet' : 'Daralt'}
        >
          {collapsed ? '›' : '‹'}
        </button>
      </div>

      {/* Nav */}
      <nav style={{ flex: 1, padding: '12px 8px', display: 'flex', flexDirection: 'column', gap: 2 }}>
        {NAV.map(item => (
          <NavLink
            key={item.to}
            to={item.to}
            title={collapsed ? item.label : undefined}
            style={({ isActive }) => ({
              display: 'flex',
              alignItems: 'center',
              gap: 12,
              padding: '10px 12px',
              borderRadius: 'var(--radius)',
              color: isActive ? 'var(--accent2)' : 'var(--text2)',
              background: isActive ? 'rgba(124,106,247,0.12)' : 'transparent',
              fontWeight: isActive ? 500 : 400,
              fontSize: '0.9rem',
              transition: 'all var(--transition)',
              whiteSpace: 'nowrap',
              textDecoration: 'none',
            })}
          >
            <span style={{ fontSize: 18, flexShrink: 0 }}>{item.icon}</span>
            {!collapsed && item.label}
          </NavLink>
        ))}
      </nav>

      {/* User / logout */}
      <div style={{ padding: '12px 8px', borderTop: '1px solid var(--border)' }}>
        {!collapsed && user && (
          <div style={{
            padding: '10px 12px',
            borderRadius: 'var(--radius)',
            background: 'var(--bg3)',
            marginBottom: 8,
          }}>
            <div style={{ fontSize: '0.82rem', fontWeight: 500, color: 'var(--text)' }}>
              {user.userName}
            </div>
            <div style={{ fontSize: '0.75rem', color: 'var(--text3)', marginTop: 2, overflow: 'hidden', textOverflow: 'ellipsis' }}>
              {user.email}
            </div>
          </div>
        )}
        <button
          onClick={handleLogout}
          className="btn btn-ghost"
          title="Çıkış"
          style={{
            width: '100%',
            justifyContent: collapsed ? 'center' : 'flex-start',
            gap: 10,
            color: 'var(--red)',
            fontSize: '0.88rem',
          }}
        >
          <span style={{ fontSize: 16 }}>⊗</span>
          {!collapsed && 'Çıkış'}
        </button>
      </div>
    </aside>
  )
}
