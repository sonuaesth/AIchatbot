import { useState, useEffect, useMemo } from "react";
import { createChat, getChats, getMessages, sendMessage } from "./api";
import type { ChatDto, MessageDto, Guid } from "./types";
import "./App.css";

function getOrCreateUserId(): Guid {
  const key = "aichat_user_id";
  let userId = localStorage.getItem(key);
  if (!userId) {
    userId = crypto.randomUUID();
    localStorage.setItem(key, userId);
  }
  return userId;
}

export default function App() {
  const userId = useMemo(() => getOrCreateUserId(), []);
  const [chats, setChats] = useState<ChatDto[]>([]);
  const [activeChatId, setActiveChatId] = useState<Guid | null>(null);
  const [messages, setMessages] = useState<MessageDto[]>([]);
  const [text, setText] = useState("");
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState("");

  async function refreshChats() {
    const chats = await getChats(userId);
    setChats(chats);
    if (!activeChatId && chats.length > 0) {
      setActiveChatId(chats[0].id);
    }
  }

  async function openChat(chatId: Guid) {
    setActiveChatId(chatId);
    const messages = await getMessages(chatId, userId);
    setMessages(messages);
  }

  async function onNewChat() {
    setErr("");
    const chat = await createChat({ userId, title: null });
    await refreshChats();
    await openChat(chat.id);
  }

  async function onSend() {
    const t = text.trim();
    if (!t || !activeChatId) return;

    setErr("");
    setLoading(true);

    const tempUserMsg: MessageDto = {
      id: `temp-${Date.now()}`,
      chatId: activeChatId,
      role: "User",
      text: t,
      createdAt: new Date().toISOString(),
    };

    setMessages((prev) => [...prev, tempUserMsg]);
    setText("");

    try {
      const assistantMsg = await sendMessage(activeChatId, { userId, text: t });

      setMessages((prev) => [
        ...prev.filter((x) => x.id !== tempUserMsg.id),
        tempUserMsg,
        assistantMsg,
      ]);

      await refreshChats();
    } catch (e) {
      setErr(String((e as Error).message || "Something went wrong"));
      setMessages((prev) => prev.filter((x) => x.id !== tempUserMsg.id));
      setText(t);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    refreshChats().catch((e) =>
      setErr(String((e as Error).message || "Something went wrong"))
    );
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (activeChatId) {
      openChat(activeChatId).catch((e) =>
        setErr(String((e as Error).message || "Something went wrong"))
      );
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeChatId]);

  return (
    <>
      <div
        style={{
          display: "grid",
          gridTemplateColumns: "300px 1fr",
          height: "80vh",
          width: "80vw",
        }}
      >
        <aside
          style={{
            borderRight: "3px solid #747474ff",
            padding: 12,
            overflow: "auto",
          }}
        >
          <div style={{ display: "flex", gap: 8, marginBottom: 12 }}>
            <button onClick={onNewChat}>+ New chat</button>
          </div>

          <div style={{ fontSize: 12, opacity: 0.7, marginBottom: 8 }}>
            userId: {userId}
          </div>

          {chats.map((c) => (
            <button
              key={c.id}
              onClick={() => setActiveChatId(c.id)}
              style={{
                width: "100%",
                textAlign: "left",
                padding: 10,
                marginBottom: 6,
                border: "1px solid #7f7f7fff",
                background: c.id === activeChatId ? "#f0f0f0ff" : "grey",
                color: c.id === activeChatId ? "black" : "white",
                borderRadius: 8,
                cursor: "pointer",
              }}
            >
              <div style={{ fontWeight: 600 }}>{c.title || "(no title)"}</div>
              <div style={{ fontSize: 12, opacity: 0.7 }}>
                {new Date(c.createdAt).toLocaleString()}
              </div>
            </button>
          ))}
        </aside>

        <main
          style={{
            display: "grid",
            gridTemplateRows: "1fr auto",
            height: "100%",
          }}
        >
          <div style={{ padding: 16, overflow: "auto" }}>
            {err && (
              <div
                style={{
                  marginBottom: 12,
                  padding: 10,
                  border: "1px solid #f99",
                  background: "#fee",
                }}
              >
                {err}
              </div>
            )}

            {!activeChatId ? (
              <div>Create a chat on the left</div>
            ) : (
              messages.map((m) => (
                <div key={m.id} style={{ marginBottom: 12 }}>
                  <div style={{ fontSize: 12, opacity: 0.7 }}>{m.role}</div>
                  <div style={{ whiteSpace: "pre-wrap" }}>{m.text}</div>
                </div>
              ))
            )}
          </div>

          <div
            style={{
              // borderTop: "1px solid #ddd",
              padding: 12,
              display: "flex",
              gap: 8,
            }}
          >
            <input
              value={text}
              onChange={(e) => setText(e.target.value)}
              placeholder="Type message..."
              style={{
                flex: 1,
                padding: 20,
                borderRadius: 8,
                backgroundColor: "#e6e6e6ff",
                fontSize: 18,
                border: "1px solid #ddd",
              }}
              onKeyDown={(e) => {
                if (e.key === "Enter" && !e.shiftKey) {
                  e.preventDefault();
                  onSend();
                }
              }}
              disabled={!activeChatId || loading}
            />
            <button
              onClick={onSend}
              style={{
                padding: "0 12px",
                borderRadius: 8,
                border: "1px solid #3c3c3cff",
                backgroundColor: "#232323ff",
                color: "white",
                fontSize: 18,
                cursor: "pointer",
              }}
              disabled={!activeChatId || loading || !text.trim()}
            >
              {loading ? "..." : "Send"}
            </button>
          </div>
        </main>
      </div>
    </>
  );
}
