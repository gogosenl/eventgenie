import { useState } from "react";
import { useAuth } from "../context/AuthContext";
import { useToast } from "../context/ToastContext";
import { tripApi } from "../services/api";
import axios from "axios";

const TURKEY_CITIES = [
  "Adana",
  "Adıyaman",
  "Afyonkarahisar",
  "Ağrı",
  "Amasya",
  "Ankara",
  "Antalya",
  "Artvin",
  "Aydın",
  "Balıkesir",
  "Bilecik",
  "Bingöl",
  "Bitlis",
  "Bolu",
  "Burdur",
  "Bursa",
  "Çanakkale",
  "Çankırı",
  "Çorum",
  "Denizli",
  "Diyarbakır",
  "Edirne",
  "Elazığ",
  "Erzincan",
  "Erzurum",
  "Eskişehir",
  "Gaziantep",
  "Giresun",
  "Gümüşhane",
  "Hakkari",
  "Hatay",
  "Isparta",
  "Mersin",
  "İstanbul",
  "İzmir",
  "Kars",
  "Kastamonu",
  "Kayseri",
  "Kırklareli",
  "Kırşehir",
  "Kocaeli",
  "Konya",
  "Kütahya",
  "Malatya",
  "Manisa",
  "Kahramanmaraş",
  "Mardin",
  "Muğla",
  "Muş",
  "Nevşehir",
  "Niğde",
  "Ordu",
  "Rize",
  "Sakarya",
  "Samsun",
  "Siirt",
  "Sinop",
  "Sivas",
  "Tekirdağ",
  "Tokat",
  "Trabzon",
  "Tunceli",
  "Şanlıurfa",
  "Uşak",
  "Van",
  "Yozgat",
  "Zonguldak",
  "Aksaray",
  "Bayburt",
  "Karaman",
  "Kırıkkale",
  "Batman",
  "Şırnak",
  "Bartın",
  "Ardahan",
  "Iğdır",
  "Yalova",
  "Karabük",
  "Kilis",
  "Osmaniye",
  "Düzce",
];

// İllerin koordinatları
const CITY_COORDINATES = {
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

// Güzergahtaki illeri koordinat bazlı bul
function getRouteCities(selectedCities) {
  const result = new Set()

  // Seçilen şehirleri ekle
  selectedCities.forEach(c => result.add(c))

  // Her ardışık şehir çifti arasındaki illeri bul
  for (let i = 0; i < selectedCities.length - 1; i++) {
    const from = selectedCities[i]
    const to = selectedCities[i + 1]

    const fromCoord = CITY_COORDINATES[from]
    const toCoord = CITY_COORDINATES[to]
    if (!fromCoord || !toCoord) continue

    // İki şehir arasındaki mesafe
    const totalDist = haversine(fromCoord.lat, fromCoord.lng, toCoord.lat, toCoord.lng)

    // Güzergah genişliği: mesafenin %30'u (max 150km)
    const corridorWidth = Math.min(totalDist * 0.1, 60)

    // Tüm illeri kontrol et
    for (const [city, coord] of Object.entries(CITY_COORDINATES)) {
      if (result.has(city)) continue

      // Şehrin güzergah hattına olan uzaklığını hesapla
      const distToLine = pointToLineDistance(
        coord.lat, coord.lng,
        fromCoord.lat, fromCoord.lng,
        toCoord.lat, toCoord.lng
      )

      // Güzergah hattına yakın mı?
if (distToLine <= corridorWidth) {
  // Ayrıca ilin başlangıç ve bitiş noktaları arasında mı kontrol et
  const distFromStart = haversine(coord.lat, coord.lng, fromCoord.lat, fromCoord.lng)
  const distFromEnd = haversine(coord.lat, coord.lng, toCoord.lat, toCoord.lng)
  
  // Başlangıç veya bitişten daha uzakta değilse ekle
  if (distFromStart <= totalDist + 30 && distFromEnd <= totalDist + 30) {
    result.add(city)
  }
}
    }
  }

  return Array.from(result)
}

function haversine(lat1, lng1, lat2, lng2) {
  const R = 6371
  const dLat = (lat2 - lat1) * Math.PI / 180
  const dLng = (lng2 - lng1) * Math.PI / 180
  const a = Math.sin(dLat/2)**2 +
    Math.cos(lat1*Math.PI/180) * Math.cos(lat2*Math.PI/180) * Math.sin(dLng/2)**2
  return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a))
}

function pointToLineDistance(px, py, ax, ay, bx, by) {
  // Nokta P'nin A-B doğru segmentine uzaklığı (km)
  const dx = bx - ax
  const dy = by - ay
  const lenSq = dx*dx + dy*dy
  if (lenSq === 0) return haversine(px, py, ax, ay)

  let t = ((px - ax)*dx + (py - ay)*dy) / lenSq
  t = Math.max(0, Math.min(1, t))

  const closestX = ax + t * dx
  const closestY = ay + t * dy
  return haversine(px, py, closestX, closestY)
}

function CityInput({ value, onChange, placeholder }) {
  const [open, setOpen] = useState(false)
  const [query, setQuery] = useState(value || '')

  const normalize = str => str.toLocaleLowerCase('tr-TR')
const filtered = query.length === 0 ? [] : TURKEY_CITIES.filter(c =>
  normalize(c).includes(normalize(query))
).slice(0, 8)

  function select(city) {
    onChange(city)
    setQuery(city)
    setOpen(false)
  }

  return (
    <div style={{ position: 'relative' }}>
      <input
        placeholder={placeholder}
        value={query}
        onChange={e => { setQuery(e.target.value); onChange(e.target.value); setOpen(true) }}
        onFocus={() => setOpen(true)}
        onBlur={() => setTimeout(() => setOpen(false), 150)}
        style={{ fontSize: '0.88rem', padding: '9px 12px' }}
      />
      {open && filtered.length > 0 && (
        <div style={{
          position: 'absolute', top: '105%', left: 0, right: 0, zIndex: 200,
          background: 'var(--surface)', border: '1px solid var(--border2)',
          borderRadius: 'var(--radius)', boxShadow: 'var(--shadow)',
          overflow: 'hidden',
        }}>
          {filtered.map(c => (
            <div
              key={c}
              onMouseDown={() => select(c)}
              style={{
                padding: '9px 14px', fontSize: '0.88rem', cursor: 'pointer',
                color: 'var(--text)', transition: 'background 0.15s',
                borderBottom: '1px solid var(--border)',
              }}
              onMouseEnter={e => e.currentTarget.style.background = 'var(--bg3)'}
              onMouseLeave={e => e.currentTarget.style.background = 'transparent'}
            >
              {c}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

function DatePicker({ value, onChange, onClose }) {
  const today = new Date()
  const [viewYear, setViewYear] = useState(today.getFullYear())
  const [viewMonth, setViewMonth] = useState(today.getMonth())

  const MONTHS = ['Ocak','Şubat','Mart','Nisan','Mayıs','Haziran','Temmuz','Ağustos','Eylül','Ekim','Kasım','Aralık']
  const DAYS = ['Pt','Sa','Ça','Pe','Cu','Ct','Pz']

  const firstDay = new Date(viewYear, viewMonth, 1).getDay()
  const offset = firstDay === 0 ? 6 : firstDay - 1
  const daysInMonth = new Date(viewYear, viewMonth + 1, 0).getDate()

  const selected = value ? new Date(value) : null

  function prevMonth() {
    if (viewMonth === 0) { setViewMonth(11); setViewYear(y => y - 1) }
    else setViewMonth(m => m - 1)
  }
  function nextMonth() {
    if (viewMonth === 11) { setViewMonth(0); setViewYear(y => y + 1) }
    else setViewMonth(m => m + 1)
  }
  function selectDay(day) {
  const month = String(viewMonth + 1).padStart(2, '0')
  const dayStr = String(day).padStart(2, '0')
  onChange(`${viewYear}-${month}-${dayStr}`)
}
  function isSelected(day) {
    if (!selected) return false
    return selected.getFullYear() === viewYear && selected.getMonth() === viewMonth && selected.getDate() === day
  }
  function isToday(day) {
    return today.getFullYear() === viewYear && today.getMonth() === viewMonth && today.getDate() === day
  }

  return (
    <>
      <div onClick={onClose} style={{ position: 'fixed', inset: 0, zIndex: 99 }} />
      <div style={{
        position: 'absolute', top: '110%', left: 0, zIndex: 100,
        background: 'var(--surface)', border: '1px solid var(--border2)',
        borderRadius: 'var(--radius-lg)', padding: 16, width: 280,
        boxShadow: 'var(--shadow-lg)',
      }}>
        {/* Header */}
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 12 }}>
          <button onClick={prevMonth} className="btn-ghost" style={{ padding: '4px 8px', fontSize: 16 }}>‹</button>
          <span style={{ fontFamily: 'var(--font-head)', fontWeight: 600, fontSize: '0.95rem' }}>
            {MONTHS[viewMonth]} {viewYear}
          </span>
          <button onClick={nextMonth} className="btn-ghost" style={{ padding: '4px 8px', fontSize: 16 }}>›</button>
        </div>
        {/* Day headers */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: 2, marginBottom: 6 }}>
          {DAYS.map(d => (
            <div key={d} style={{ textAlign: 'center', fontSize: '0.72rem', color: 'var(--text3)', padding: '4px 0' }}>{d}</div>
          ))}
        </div>
        {/* Days */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: 2 }}>
          {Array.from({ length: offset }).map((_, i) => <div key={`e${i}`} />)}
          {Array.from({ length: daysInMonth }).map((_, i) => {
            const day = i + 1
            const sel = isSelected(day)
            const tod = isToday(day)
            return (
              <button
                key={day}
                onClick={() => selectDay(day)}
                style={{
                  padding: '7px 0', borderRadius: 8, fontSize: '0.85rem',
                  textAlign: 'center', cursor: 'pointer',
                  background: sel ? 'var(--accent)' : tod ? 'rgba(124,106,247,0.15)' : 'transparent',
                  color: sel ? '#fff' : tod ? 'var(--accent2)' : 'var(--text)',
                  fontWeight: sel || tod ? 600 : 400,
                  border: 'none',
                  transition: 'background 0.15s',
                }}
                onMouseEnter={e => { if (!sel) e.currentTarget.style.background = 'var(--bg3)' }}
                onMouseLeave={e => { if (!sel) e.currentTarget.style.background = tod ? 'rgba(124,106,247,0.15)' : 'transparent' }}
              >
                {day}
              </button>
            )
          })}
        </div>
      </div>
    </>
  )
}
function EventCard({ event, onSave, saved }) {
  return (
    <div
      className="card"
      style={{
        border: "1px solid var(--border)",
        transition:
          "transform var(--transition), border-color var(--transition)",
      }}
      onMouseEnter={(e) =>
        (e.currentTarget.style.transform = "translateY(-2px)")
      }
      onMouseLeave={(e) => (e.currentTarget.style.transform = "translateY(0)")}
    >
      {event.imageUrl && (
        <div
          style={{
            width: "100%",
            height: 130,
            borderRadius: 10,
            overflow: "hidden",
            marginBottom: 12,
            background: "var(--bg3)",
          }}
        >
          <img
            src={event.imageUrl}
            alt={event.eventName}
            style={{ width: "100%", height: "100%", objectFit: "cover" }}
            onError={(e) =>
              (e.currentTarget.parentElement.style.display = "none")
            }
          />
        </div>
      )}
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "flex-start",
          marginBottom: 8,
          gap: 8,
        }}
      >
        <h3
          style={{
            fontSize: "0.95rem",
            fontFamily: "var(--font-head)",
            lineHeight: 1.3,
            flex: 1,
            overflow: "hidden",
            textOverflow: "ellipsis",
            display: "-webkit-box",
            WebkitLineClamp: 2,
            WebkitBoxOrient: "vertical",
          }}
        >
          {event.eventName}
        </h3>
        {event.isPreferenceMatch && (
          <span className="badge badge-accent" style={{ flexShrink: 0 }}>
            ✦ Senin İçin
          </span>
        )}
      </div>
      <div
        style={{
          display: "flex",
          flexDirection: "column",
          gap: 5,
          marginBottom: 12,
        }}
      >
        {event.venue && (
          <div
            style={{
              fontSize: "0.8rem",
              color: "var(--text2)",
              display: "flex",
              gap: 6,
            }}
          >
            <span>◉</span>
            <span>
              {event.venue}
              {event.city ? `, ${event.city}` : ""}
            </span>
          </div>
        )}
        {event.eventDate && (
          <div
            style={{
              fontSize: "0.8rem",
              color: "var(--text2)",
              display: "flex",
              gap: 6,
            }}
          >
            <span>◷</span>
            <span>
              {new Date(event.eventDate).toLocaleDateString("tr-TR", {
                day: "numeric",
                month: "long",
                year: "numeric",
                hour: "2-digit",
                minute: "2-digit",
              })}
            </span>
          </div>
        )}
        {event.genre && (
          <span className="tag" style={{ width: "fit-content" }}>
            {event.genre}
          </span>
        )}
        {event.priceRange && (
  <div style={{ fontSize: '0.8rem', color: 'var(--gold)', display: 'flex', gap: 6, fontWeight: 500 }}>
    <span>₺</span>
    <span>{event.priceRange}</span>
  </div>
)}
      </div>
      <div style={{ display: "flex", gap: 8 }}>
        {event.eventUrl && (
          <a
            href={event.eventUrl}
            target="_blank"
            rel="noreferrer"
            className="btn btn-sm btn-secondary"
            style={{ flex: 1, justifyContent: "center" }}
          >
            Bilet Al ↗
          </a>
        )}
        <button
          onClick={() => onSave(event)}
          className={`btn btn-sm ${saved ? "btn-secondary" : "btn-primary"}`}
          style={{ flex: 1, justifyContent: "center" }}
          disabled={saved}
        >
          {saved ? "✓ Kaydedildi" : "+ Rotaya Ekle"}
        </button>
      </div>
    </div>
  );
}

export default function EventsPage() {
  const { user } = useAuth();
  const { addToast } = useToast();
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [cities, setCities] = useState(["", ""]);
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(false);
  const [searched, setSearched] = useState(false);
  const [savedEvents, setSavedEvents] = useState({});
  const [saving, setSaving] = useState(false);
  const [showStartPicker, setShowStartPicker] = useState(false);
  const [showEndPicker, setShowEndPicker] = useState(false);

  function addCity() {
    if (cities.length >= 6) {
      addToast("En fazla 6 şehir ekleyebilirsiniz.", "info");
      return;
    }
    setCities((p) => [...p, ""]);
  }

  function removeCity(i) {
    if (cities.length <= 2) {
      addToast("En az 2 şehir gerekli.", "info");
      return;
    }
    setCities((p) => p.filter((_, idx) => idx !== i));
  }

  function updateCity(i, val) {
    setCities((p) => p.map((c, idx) => (idx === i ? val : c)));
  }

async function handleSearch() {
  if (!startDate || !endDate) { addToast('Tarih aralığı seçin.', 'error'); return }
  if (new Date(startDate) >= new Date(endDate)) { addToast('Başlangıç tarihi bitiş tarihinden önce olmalı.', 'error'); return }
  const filledCities = cities.filter(c => c.trim())
  if (filledCities.length < 2) { addToast('En az 2 şehir girin.', 'error'); return }

  setLoading(true)
  setEvents([])
  setSearched(false)
  setSavedEvents({})

  const start = new Date(startDate + 'T00:00:00').toISOString().slice(0, 19) + 'Z'
  const end = new Date(endDate + 'T23:59:59').toISOString().slice(0, 19) + 'Z'

  try {
    // Güzergahtaki tüm illeri bul
    const routeCities = getRouteCities(filledCities)
    addToast(`${routeCities.length} il taranıyor...`, 'info')

    const allEvents = []
    const seen = new Set()

    for (const city of routeCities) {
      try {
        const url = `/api/Event/nearby/${user.userId}?cityOverride=${encodeURIComponent(city)}&startDate=${encodeURIComponent(start)}&endDate=${encodeURIComponent(end)}`
        const res = await axios.get(url)
        const data = res.data?.data || []
        for (const ev of data) {
          const key = `${ev.eventName}_${ev.eventDate}`
          if (!seen.has(key)) {
            seen.add(key)
            allEvents.push({ ...ev, _sourceCity: city })
          }
        }
      } catch {}
    }

    allEvents.sort((a, b) => new Date(a.eventDate) - new Date(b.eventDate))
    setEvents(allEvents)
    setSearched(true)
    if (allEvents.length === 0) addToast('Etkinlik bulunamadı.', 'info')
    else addToast(`${allEvents.length} etkinlik bulundu!`, 'success')
  } catch (err) {
    addToast('Etkinlikler yüklenemedi.', 'error')
  } finally {
    setLoading(false)
  }
}

  function handleSaveEvent(event) {
    const key = `${event.eventName}_${event.eventDate}`;
    setSavedEvents((p) => ({ ...p, [key]: event }));
  }

  async function handleSaveRoute() {
    const toSave = Object.values(savedEvents);
    if (toSave.length === 0) {
      addToast("Rotaya eklenecek etkinlik seçin.", "info");
      return;
    }
    setSaving(true);
    try {
      const cityOrder = cities.filter((c) => c.trim());
      for (const ev of toSave) {
        const cityIdx = cityOrder.findIndex(
          (c) =>
            ev.city?.toLowerCase().includes(c.toLowerCase()) ||
            ev._sourceCity?.toLowerCase().includes(c.toLowerCase()),
        );
        await tripApi.create({
          tripName: ev.eventName,
          tripDescription: ev.venue || "",
          tripUrl: ev.eventUrl || "",
          latitude: ev.latitude || 0,
          longitude: ev.longitude || 0,
          tripDate: ev.eventDate,
          userId: user.userId,
          requestGroupId: cityIdx >= 0 ? cityIdx + 1 : 99,
          tripComment: "",
          isActive: true,
          createdAt: new Date().toISOString(),
        });
      }
      addToast(`${toSave.length} etkinlik rotaya kaydedildi!`, "success");
      setSavedEvents({});
    } catch {
      addToast("Kaydetme başarısız.", "error");
    } finally {
      setSaving(false);
    }
  }

  const savedCount = Object.keys(savedEvents).length;

  return (
    <div className="page">
      <div className="page-header fade-up">
        <h1 className="page-title">Etkinlik Keşfet</h1>
        <p className="page-subtitle">
          Seyahat rotanı gir, güzergahındaki etkinlikleri bul
        </p>
      </div>

      {/* Form */}
      <div className="card fade-up fade-up-d1" style={{ marginBottom: 24 }}>
        {/* Tarih */}
        <h2
          style={{ fontSize: "1rem", marginBottom: 16, color: "var(--text2)" }}
        >
          Tarih Aralığı
        </h2>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16, marginBottom: 24 }}>
  <div className="form-group">
    <label>Başlangıç</label>
    <div style={{ position: 'relative' }}>
      <button
        type="button"
        onClick={() => { setShowStartPicker(p => !p); setShowEndPicker(false) }}
        style={{
          width: '100%', padding: '12px 16px', borderRadius: 'var(--radius)',
          background: 'var(--bg3)', border: '1px solid var(--border2)',
          color: startDate ? 'var(--text)' : 'var(--text3)',
          textAlign: 'left', cursor: 'pointer', fontSize: '0.95rem',
        }}
      >
        {startDate ? new Date(startDate).toLocaleDateString('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' }) : 'Tarih seçin'}
      </button>
      {showStartPicker && (
        <DatePicker
          value={startDate}
          onChange={v => { setStartDate(v); setShowStartPicker(false) }}
          onClose={() => setShowStartPicker(false)}
        />
      )}
    </div>
  </div>
  <div className="form-group">
    <label>Bitiş</label>
    <div style={{ position: 'relative' }}>
      <button
        type="button"
        onClick={() => { setShowEndPicker(p => !p); setShowStartPicker(false) }}
        style={{
          width: '100%', padding: '12px 16px', borderRadius: 'var(--radius)',
          background: 'var(--bg3)', border: '1px solid var(--border2)',
          color: endDate ? 'var(--text)' : 'var(--text3)',
          textAlign: 'left', cursor: 'pointer', fontSize: '0.95rem',
        }}
      >
        {endDate ? new Date(endDate).toLocaleDateString('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' }) : 'Tarih seçin'}
      </button>
      {showEndPicker && (
        <DatePicker
          value={endDate}
          onChange={v => { setEndDate(v); setShowEndPicker(false) }}
          onClose={() => setShowEndPicker(false)}
        />
      )}
    </div>
  </div>
</div>

        {/* Şehirler */}
        <h2
          style={{ fontSize: "1rem", marginBottom: 12, color: "var(--text2)" }}
        >
          Güzergah
        </h2>
        <div
          style={{
            display: "flex",
            flexDirection: "column",
            gap: 10,
            marginBottom: 16,
          }}
        >
          {cities.map((city, i) => (
  <div key={i} style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
    <div style={{
      width: 24, height: 24, borderRadius: '50%', flexShrink: 0,
      background: i === 0 ? 'var(--green)' : i === cities.length - 1 ? 'var(--red)' : 'var(--accent)',
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      fontSize: '0.68rem', fontWeight: 700, color: '#fff',
    }}>
      {i === 0 ? 'A' : i === cities.length - 1 ? 'B' : i}
    </div>
    <div style={{ position: 'relative', flex: 1 }}>
      <CityInput
        placeholder={i === 0 ? 'Başlangıç şehri' : i === cities.length - 1 ? 'Bitiş şehri' : `Ara durak ${i}`}
        value={city}
        onChange={v => updateCity(i, v)}
      />
    </div>
    {cities.length > 2 && (
      <button onClick={() => removeCity(i)} className="btn btn-danger btn-sm" style={{ flexShrink: 0, padding: '6px 10px' }}>×</button>
    )}
  </div>
))}
        </div>
        <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
          <button className="btn btn-secondary btn-sm" onClick={addCity}>
            + Ara Durak Ekle
          </button>
          <button
            className="btn btn-primary"
            onClick={handleSearch}
            disabled={loading}
          >
            {loading ? (
              <>
                <span className="spinner" style={{ width: 14, height: 14 }} />{" "}
                Aranıyor...
              </>
            ) : (
              "◈ Etkinlikleri Bul"
            )}
          </button>
        </div>
      </div>

      {/* Kaydet butonu */}
      {savedCount > 0 && (
        <div
          className="card fade-up"
          style={{
            marginBottom: 24,
            borderColor: "rgba(124,106,247,0.4)",
            background: "rgba(124,106,247,0.06)",
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
            gap: 12,
          }}
        >
          <div>
            <div style={{ fontWeight: 600, fontFamily: "var(--font-head)" }}>
              {savedCount} etkinlik seçildi
            </div>
            <div
              style={{
                fontSize: "0.82rem",
                color: "var(--text2)",
                marginTop: 2,
              }}
            >
              Rotayı kaydetmek için butona tıklayın.
            </div>
          </div>
          <button
            className="btn btn-primary"
            onClick={handleSaveRoute}
            disabled={saving}
          >
            {saving ? (
              <>
                <span className="spinner" style={{ width: 14, height: 14 }} />{" "}
                Kaydediliyor...
              </>
            ) : (
              "✦ Rotayı Kaydet"
            )}
          </button>
        </div>
      )}

      {/* Sonuçlar */}
      {events.length > 0 && (
        <div>
          <div
            style={{
              display: "flex",
              alignItems: "center",
              gap: 12,
              marginBottom: 20,
            }}
          >
            <h2 style={{ fontSize: "1.1rem" }}>Bulunan Etkinlikler</h2>
            <span className="badge badge-gold">{events.length} etkinlik</span>
          </div>
          <div className="grid-3">
            {events.map((ev, i) => {
              const key = `${ev.eventName}_${ev.eventDate}`;
              return (
                <EventCard
                  key={i}
                  event={ev}
                  onSave={handleSaveEvent}
                  saved={!!savedEvents[key]}
                />
              );
            })}
          </div>
        </div>
      )}

      {searched && events.length === 0 && (
        <div className="empty-state card">
          <span className="icon">◈</span>
          <h3>Etkinlik bulunamadı</h3>
          <p>Farklı şehirler veya tarih aralığı deneyin.</p>
        </div>
      )}
    </div>
  );
}
