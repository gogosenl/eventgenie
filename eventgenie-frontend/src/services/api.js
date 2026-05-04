import axios from 'axios'

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

// Request interceptor — token ekle (ileride JWT kullanılırsa)
api.interceptors.request.use(cfg => {
  const token = localStorage.getItem('eg_token')
  if (token) cfg.headers.Authorization = `Bearer ${token}`
  return cfg
})

// ── USER ────────────────────────────────────────────────
export const userApi = {
  getAll:    ()         => api.get('/User'),
  getById:   (id)       => api.get(`/User/${id}`),
  create:    (data)     => api.post('/User', data),
  update:    (id, data) => api.put(`/User/${id}`, data),
  delete:    (id)       => api.delete(`/User/${id}`),
  login:     (data)     => api.post('/User/Login', data),
  updatePreferences: (id, prefs) => api.patch(`/User/${id}/preferences`, JSON.stringify(prefs)),
  changePassword:    (id, data)  => api.patch(`/User/${id}/change-password`, data),
}

// ── EVENT ───────────────────────────────────────────────
export const eventApi = {
  fetchAndRecommend: (userId, startDateTime, endDateTime) =>
    api.get(`/Event/fetch-and-recommend/${userId}`, { params: { startDateTime, endDateTime } }),
  getNearby: (userId, lat, lng) =>
    api.get(`/Event/nearby/${userId}`, { params: { lat, lng } }),
}

// ── TRIP ────────────────────────────────────────────────
export const tripApi = {
  getAll:      ()       => api.get('/Trip'),
  getByUser:   (userId) => api.get(`/Trip/by-user/${userId}`),
  getActive:   (userId) => api.get(`/Trip/active/${userId}`),
  getArchived: (userId) => api.get(`/Trip/archived/${userId}`),
  getById:     (id)     => api.get(`/Trip/${id}`),
  create:      (data)   => api.post('/Trip', data),
  update:      (id, data) => api.put(`/Trip/${id}`, data),
  delete:      (id)     => api.delete(`/Trip/${id}`),
  deleteActive:   (userId)            => api.delete(`/Trip/active/${userId}`),
  deleteRoute:    (userId, tripNames) => api.delete(`/Trip/delete-route/${userId}`, { data: tripNames }),
}

// ── LOCATION ────────────────────────────────────────────
export const locationApi = {
  getAll:     ()         => api.get('/Location'),
  getById:    (id)       => api.get(`/Location/${id}`),
  getByUser:  (userId)   => api.get(`/Location/by-user/${userId}`),
  create:     (data)     => api.post('/Location', data),
  update:     (userId, data) => api.put(`/Location/${userId}`, data),
  delete:     (id)       => api.delete(`/Location/${id}`),
}

// ── TICKETMASTER ────────────────────────────────────────
export const ticketmasterApi = {
  getEvents: (userId, startDateTime, endDateTime) =>
    api.get(`/Ticketmaster/${userId}`, { params: { startDateTime, endDateTime } }),
}

// ── CHATGPT ─────────────────────────────────────────────
export const chatgptApi = {
  threeEventsWithComment: (userId) =>
    api.get(`/Chatgpt/three-events-with-comment/${userId}`),
}

// ── CHAT ────────────────────────────────────────────────
export const chatApi = {
  getMessages:       (roomKey)  => api.get(`/Chat/${encodeURIComponent(roomKey)}`),
  sendMessage:       (data)     => api.post('/Chat', data),
  deleteMessage:     (id)       => api.delete(`/Chat/${id}`),
  getParticipants:   (roomKey)  => api.get(`/Chat/participants/${encodeURIComponent(roomKey)}`),
}

export default api
