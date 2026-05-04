import { useState, useEffect, useRef, useCallback } from 'react'
import { useAuth } from '../context/AuthContext'
import { useToast } from '../context/ToastContext'
import { chatApi, tripApi } from '../services/api'

function Message({ msg, isOwn, onDelete }) {
  return (
    <div style={{
      display: 'flex',
      flexDirection: isOwn ? 'row-reverse' : 'row',
      gap: 10, alignItems: 'flex-end', marginBottom: 12,
    }}>
      {!isOwn && (
        <div style={{
          width: 32, height: 32, borderRadius: '50%', flexShrink: 0,
          background: 'var(--accent)', display: 'flex', alignItems: 'center', justifyContent: 'center',
          fontSize: 13, fontWeight: 600, color: '#fff',
        }}>
          {(msg.userName || '?').charAt(0).toUpperCase()}
        </div>
      )}
      <div style={{ maxWidth: '70%' }}>
        {!isOwn && (
          <div style={{ fontSize: '0.72rem', color: 'var(--text3)', marginBottom: 4, paddingLeft: 4 }}>
            {msg.userName}
          </div>
        )}
        <div style={{
          padding: '10px 14px',
          borderRadius: isOwn ? '16px 16px 4px 16px' : '16px 16px 16px 4px',
          background: isOwn ? 'var(--accent)' : 'var(--surface)',
          border: isOwn ? 'none' : '1px solid var(--border)',
          fontSize: '0.88rem', lineHeight: 1.5, color: isOwn ? '#fff' : 'var(--text)',
          position: 'relative',
        }}>
          {msg.message}
          <div style={{
            fontSize: '0.68rem', marginTop: 4, opacity: 0.7, textAlign: isOwn ? 'right' : 'left',
          }}>
            {msg.createdAt ? new Date(msg.createdAt).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' }) : ''}
          </div>
        </div>
      </div>
      {isOwn && (
        <button onClick={() => onDelete(msg.chatMessageId)}
          style={{ color: 'var(--text3)', fontSize: 12, padding: 4, opacity: 0, transition: 'opacity 0.2s' }}
          className="btn-ghost"
          onMouseEnter={e => e.currentTarget.style.opacity = 1}
          onMouseLeave={e => e.currentTarget.style.opacity = 0}
          title="Sil"
        >×</button>
      )}
    </div>
  )
}

export default function ChatPage() {
  const { user } = useAuth()
  const { addToast } = useToast()
  const [rooms, setRooms] = useState([])
  const [selectedRoom, setSelectedRoom] = useState(null)
  const [messages, setMessages] = useState([])
  const [newMessage, setNewMessage] = useState('')
  const [loading, setLoading] = useState(false)
  const [sending, setSending] = useState(false)
  const [polling, setPolling] = useState(null)
  const messagesEndRef = useRef(null)

  // Load rooms from active trips
  useEffect(() => {
    if (!user) return
    tripApi.getActive(user.userId).then(res => {
      const trips = res.data || []
      const uniqueRooms = [...new Map(trips.map(t => [t.tripName, t])).values()]
      setRooms(uniqueRooms)
      if (uniqueRooms.length > 0 && !selectedRoom) setSelectedRoom(uniqueRooms[0].tripName)
    }).catch(() => {})
  }, [user])

  const loadMessages = useCallback(async (roomKey) => {
    if (!roomKey) return
    try {
      const res = await chatApi.getMessages(roomKey)
const data = res.data?.messages || res.data?.Messages || res.data || []
setMessages(Array.isArray(data) ? data : [])
    } catch {}
  }, [])

  useEffect(() => {
    if (!selectedRoom) return
    setLoading(true)
    loadMessages(selectedRoom).finally(() => setLoading(false))

    // Polling her 5 saniyede bir
    const id = setInterval(() => loadMessages(selectedRoom), 5000)
    setPolling(id)
    return () => clearInterval(id)
  }, [selectedRoom, loadMessages])

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  async function handleSend(e) {
    e.preventDefault()
    if (!newMessage.trim() || !selectedRoom) return
    setSending(true)
    try {
      await chatApi.sendMessage({
        roomKey: selectedRoom,
        userId: user.userId,
        userName: user.userName,
        message: newMessage.trim(),
      })
      setNewMessage('')
      await loadMessages(selectedRoom)
    } catch {
      addToast('Mesaj gönderilemedi.', 'error')
    } finally {
      setSending(false)
    }
  }

  async function handleDelete(id) {
    try {
      await chatApi.deleteMessage(id)
      setMessages(prev => prev.filter(m => m.chatMessageId !== id))
    } catch { addToast('Silinemedi.', 'error') }
  }

  return (
    <div className="page" style={{ padding: 0, height: '100vh', display: 'flex', flexDirection: 'column' }}>
      {/* Header */}
      <div style={{ padding: '24px 32px 16px', borderBottom: '1px solid var(--border)' }}>
        <h1 style={{ fontFamily: 'var(--font-head)', fontSize: '1.6rem', marginBottom: 4 }}>Chat</h1>
        <p style={{ color: 'var(--text2)', fontSize: '0.88rem' }}>Etkinlik katılımcılarıyla konuş</p>
      </div>

      <div style={{ display: 'flex', flex: 1, overflow: 'hidden' }}>
        {/* Rooms sidebar */}
        <div style={{
          width: 240, borderRight: '1px solid var(--border)',
          background: 'var(--bg2)', display: 'flex', flexDirection: 'column',
          overflow: 'hidden', flexShrink: 0,
        }}>
          <div style={{ padding: '16px 12px 8px', fontSize: '0.75rem', fontWeight: 600, color: 'var(--text3)', textTransform: 'uppercase', letterSpacing: '0.08em' }}>
            Aktif Etkinlikler
          </div>
          <div style={{ flex: 1, overflow: 'auto', padding: '0 8px 12px' }}>
            {rooms.length === 0 ? (
              <div style={{ padding: '20px 12px', color: 'var(--text3)', fontSize: '0.82rem', textAlign: 'center', lineHeight: 1.5 }}>
                Aktif rota yok. Etkinlik sayfasından rota oluştur.
              </div>
            ) : rooms.map(room => (
              <button
                key={room.tripName}
                onClick={() => setSelectedRoom(room.tripName)}
                style={{
                  width: '100%', textAlign: 'left', padding: '10px 12px',
                  borderRadius: 8, marginBottom: 2, cursor: 'pointer',
                  background: selectedRoom === room.tripName ? 'rgba(124,106,247,0.12)' : 'transparent',
                  color: selectedRoom === room.tripName ? 'var(--accent2)' : 'var(--text2)',
                  border: `1px solid ${selectedRoom === room.tripName ? 'rgba(124,106,247,0.2)' : 'transparent'}`,
                  fontSize: '0.85rem', fontWeight: 500, transition: 'all var(--transition)',
                  overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap',
                }}
              >
                ◫ {room.tripName}
              </button>
            ))}
          </div>
        </div>

        {/* Chat area */}
        <div style={{ flex: 1, display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
          {!selectedRoom ? (
            <div className="empty-state" style={{ flex: 1 }}>
              <span className="icon">◫</span>
              <h3>Bir sohbet seçin</h3>
              <p>Soldaki listeden etkinlik seçin.</p>
            </div>
          ) : (
            <>
              {/* Room header */}
              <div style={{
                padding: '14px 24px', borderBottom: '1px solid var(--border)',
                display: 'flex', alignItems: 'center', gap: 12,
              }}>
                <div style={{
                  width: 10, height: 10, borderRadius: '50%', background: 'var(--green)',
                  boxShadow: '0 0 8px rgba(52,211,153,0.5)',
                }} />
                <span style={{ fontWeight: 500, fontSize: '0.95rem', fontFamily: 'var(--font-head)' }}>
                  {selectedRoom}
                </span>
                <span className="badge badge-green" style={{ marginLeft: 'auto', fontSize: '0.72rem' }}>
                  Canlı
                </span>
              </div>

              {/* Messages */}
              <div style={{ flex: 1, overflow: 'auto', padding: '20px 24px' }}>
                {loading ? (
                  <div className="loading-center"><div className="spinner" /></div>
                ) : messages.length === 0 ? (
                  <div className="empty-state">
                    <span className="icon">◫</span>
                    <h3>Henüz mesaj yok</h3>
                    <p>İlk mesajı sen gönder!</p>
                  </div>
                ) : (
                  messages.map(msg => (
                    <Message
                      key={msg.chatMessageId}
                      msg={msg}
                      isOwn={msg.userId === user.userId}
                      onDelete={handleDelete}
                    />
                  ))
                )}
                <div ref={messagesEndRef} />
              </div>

              {/* Input */}
              <form onSubmit={handleSend} style={{
                padding: '16px 24px',
                borderTop: '1px solid var(--border)',
                display: 'flex', gap: 10,
              }}>
                <input
                  value={newMessage}
                  onChange={e => setNewMessage(e.target.value)}
                  placeholder="Mesaj yaz..."
                  style={{ flex: 1 }}
                  disabled={sending}
                />
                <button type="submit" className="btn btn-primary" disabled={sending || !newMessage.trim()}>
                  {sending ? <span className="spinner" style={{ width: 14, height: 14 }} /> : '↑'}
                </button>
              </form>
            </>
          )}
        </div>
      </div>
    </div>
  )
}
