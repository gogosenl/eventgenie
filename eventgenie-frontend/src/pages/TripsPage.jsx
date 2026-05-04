import { useState, useEffect, useCallback } from "react";
import { useAuth } from "../context/AuthContext";
import { useToast } from "../context/ToastContext";
import { tripApi } from "../services/api";

function openGoogleMapsRoute(trips) {
  if (!trips || trips.length === 0) return;

  // Tarihe göre sırala
  
const sorted = [...trips].sort((a, b) => {
  if (a.requestGroupId !== b.requestGroupId) return a.requestGroupId - b.requestGroupId
  return new Date(a.tripDate) - new Date(b.tripDate)
})

  if (sorted.length === 1) {
    // Tek nokta — sadece o konumu aç
    const t = sorted[0];
    window.open(
      `https://www.google.com/maps?q=${t.latitude},${t.longitude}`,
      "_blank",
    );
    return;
  }

  // Başlangıç, waypoints, bitiş
  const origin = `${sorted[0].latitude},${sorted[0].longitude}`;
  const destination = `${sorted[sorted.length - 1].latitude},${sorted[sorted.length - 1].longitude}`;
  const waypoints = sorted
    .slice(1, -1)
    .map((t) => `${t.latitude},${t.longitude}`)
    .join("|");

  let url = `https://www.google.com/maps/dir/?api=1&origin=${origin}&destination=${destination}&travelmode=driving`;
  if (waypoints) url += `&waypoints=${waypoints}`;

  window.open(url, "_blank");
}
function TripItem({ trip, onDelete }) {
  const [deleting, setDeleting] = useState(false);

  async function handleDelete() {
    if (!confirm(`"${trip.tripName}" silinsin mi?`)) return;
    setDeleting(true);
    try {
      await onDelete(trip.tripId);
    } finally {
      setDeleting(false);
    }
  }

  return (
    <div
      className="card"
      style={{
        display: "flex",
        alignItems: "flex-start",
        gap: 14,
        border: "1px solid var(--border)",
        transition: "border-color var(--transition)",
      }}
      onMouseEnter={(e) =>
        (e.currentTarget.style.borderColor = "var(--border2)")
      }
      onMouseLeave={(e) =>
        (e.currentTarget.style.borderColor = "var(--border)")
      }
    >
      <div
        style={{
          width: 10,
          height: 10,
          borderRadius: "50%",
          marginTop: 5,
          flexShrink: 0,
          background: trip.isActive ? "var(--accent)" : "var(--text3)",
          boxShadow: trip.isActive ? "0 0 8px var(--accent-glow)" : "none",
        }}
      />
      <div style={{ flex: 1, minWidth: 0 }}>
        <div
          style={{
            fontWeight: 500,
            fontFamily: "var(--font-head)",
            marginBottom: 4,
            overflow: "hidden",
            textOverflow: "ellipsis",
            whiteSpace: "nowrap",
          }}
        >
          {trip.tripName}
        </div>
        {trip.tripComment && (
          <p
            style={{
              fontSize: "0.8rem",
              color: "var(--accent2)",
              marginBottom: 6,
              fontStyle: "italic",
            }}
          >
            ✦ {trip.tripComment}
          </p>
        )}
        {trip.tripDescription && (
          <p
            style={{
              fontSize: "0.8rem",
              color: "var(--text3)",
              marginBottom: 8,
              lineHeight: 1.5,
            }}
          >
            {trip.tripDescription.slice(0, 120)}
            {trip.tripDescription.length > 120 ? "…" : ""}
          </p>
        )}
        <div
          style={{
            display: "flex",
            gap: 8,
            flexWrap: "wrap",
            alignItems: "center",
          }}
        >
          {trip.tripDate && (
            <span className="tag">
              {new Date(trip.tripDate).toLocaleDateString("tr-TR")}
            </span>
          )}
          {trip.tripUrl && (
            <a
              href={trip.tripUrl}
              target="_blank"
              rel="noreferrer"
              className="btn btn-sm btn-ghost"
              style={{ padding: "3px 8px", fontSize: "0.78rem" }}
            >
              ↗ Link
            </a>
          )}
        </div>
      </div>
      <button
        className="btn btn-danger btn-sm"
        onClick={handleDelete}
        disabled={deleting}
        style={{ flexShrink: 0 }}
      >
        {deleting ? (
          <span className="spinner" style={{ width: 12, height: 12 }} />
        ) : (
          "×"
        )}
      </button>
    </div>
  );
}

export default function TripsPage() {
  const { user } = useAuth();
  const { addToast } = useToast();
  const [tab, setTab] = useState("active");
  const [active, setActive] = useState([]);
  const [archived, setArchived] = useState([]);
  const [loading, setLoading] = useState(true);
  const [clearingActive, setClearingActive] = useState(false);

  const load = useCallback(async () => {
    if (!user) return;
    setLoading(true);
    try {
      const [a, b] = await Promise.all([
        tripApi.getActive(user.userId),
        tripApi.getArchived(user.userId),
      ]);
      setActive(a.data || []);
      setArchived(b.data || []);
    } catch {
      addToast("Rotalar yüklenemedi.", "error");
    } finally {
      setLoading(false);
    }
  }, [user]);

  useEffect(() => {
    load();
  }, [load]);

  async function handleDeleteTrip(id) {
    try {
      await tripApi.delete(id);
      addToast("Etkinlik silindi.", "success");
      load();
    } catch {
      addToast("Silinemedi.", "error");
    }
  }

  async function handleClearActive() {
    if (!confirm("Tüm aktif rota silinsin mi?")) return;
    setClearingActive(true);
    try {
      await tripApi.deleteActive(user.userId);
      addToast("Aktif rota silindi.", "success");
      load();
    } catch {
      addToast("İşlem başarısız.", "error");
    } finally {
      setClearingActive(false);
    }
  }

  const list = tab === "active" ? active : archived;

  // Group archived by createdAt date
  const grouped =
    tab === "archived"
      ? archived.reduce((acc, t) => {
          const key = t.createdAt
            ? new Date(t.createdAt).toLocaleDateString("tr-TR")
            : "Bilinmiyor";
          if (!acc[key]) acc[key] = [];
          acc[key].push(t);
          return acc;
        }, {})
      : null;

  return (
    <div className="page">
      <div className="page-header fade-up">
        <h1 className="page-title">Rotalarım</h1>
        <p className="page-subtitle">Aktif ve geçmiş etkinlik rotalarınız</p>
      </div>

      {/* Tabs */}
      <div
        style={{
          display: "flex",
          gap: 4,
          marginBottom: 24,
          background: "var(--bg2)",
          padding: 4,
          borderRadius: "var(--radius)",
          width: "fit-content",
        }}
      >
        {[
          ["active", `Aktif (${active.length})`],
          ["archived", `Geçmiş (${archived.length})`],
        ].map(([key, label]) => (
          <button
            key={key}
            onClick={() => setTab(key)}
            className="btn"
            style={{
              padding: "8px 20px",
              fontSize: "0.88rem",
              background: tab === key ? "var(--surface)" : "transparent",
              color: tab === key ? "var(--text)" : "var(--text2)",
              border:
                tab === key
                  ? "1px solid var(--border2)"
                  : "1px solid transparent",
              borderRadius: 8,
            }}
          >
            {label}
          </button>
        ))}
      </div>

      {loading ? (
        <div className="loading-center">
          <div className="spinner" />
          <span>Yükleniyor...</span>
        </div>
      ) : tab === "active" ? (
        <>
          {active.length > 0 && (
            <div
              style={{
                display: "flex",
                justifyContent: "flex-end",
                marginBottom: 16,
              }}
            >
              <button
                className="btn btn-primary btn-sm"
                onClick={() => openGoogleMapsRoute(active)}
              >
                🗺 Güzergah Çiz
              </button>
              <button
                className="btn btn-danger btn-sm"
                onClick={handleClearActive}
                disabled={clearingActive}
              >
                {clearingActive ? (
                  <>
                    <span
                      className="spinner"
                      style={{ width: 12, height: 12 }}
                    />{" "}
                    Siliniyor...
                  </>
                ) : (
                  "⊗ Aktif Rotayı Temizle"
                )}
              </button>
            </div>
          )}
          {active.length === 0 ? (
            <div className="empty-state card">
              <span className="icon">◎</span>
              <h3>Aktif rota yok</h3>
              <p>Etkinlik sayfasından yeni rota oluşturun.</p>
            </div>
          ) : (
            <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
              {active.map((t) => (
                <div key={t.tripId} className="fade-up">
                  <TripItem trip={t} onDelete={handleDeleteTrip} />
                </div>
              ))}
            </div>
          )}
        </>
      ) : archived.length === 0 ? (
        <div className="empty-state card">
          <span className="icon">◷</span>
          <h3>Geçmiş rota yok</h3>
          <p>Tamamlanan rotalar burada görünür.</p>
        </div>
      ) : (
        <div style={{ display: "flex", flexDirection: "column", gap: 24 }}>
          {Object.entries(grouped).map(([date, trips]) => (
            <div key={date}>
              <div
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: 12,
                  marginBottom: 12,
                }}
              >
                <span className="label">{date}</span>
                <div className="divider" style={{ flex: 1 }} />
                <span className="badge badge-gold">{trips.length}</span>
              </div>
              <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
                {trips.map((t) => (
                  <div key={t.tripId} className="fade-up">
                    <TripItem trip={t} onDelete={handleDeleteTrip} />
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
