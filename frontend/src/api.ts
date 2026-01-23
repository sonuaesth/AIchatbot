import type {
  ChatDto,
  MessageDto,
  SendMessageRequest,
  CreateChatRequest,
  Guid,
  StreamEvent,
} from "./types";

// const API = import.meta.env.VITE_API_BASE_URL || "";
const API = import.meta.env.VITE_API_BASE_URL;
// const API = "";

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const response = await fetch(API + path, {
    headers: {
      "Content-Type": "application/json",
      ...(options.headers || {}),
    },
    ...options,
  });
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `HTTP ${response.status}`);
  }
  return response.json() as Promise<T>;
}

export function createChat(body: CreateChatRequest): Promise<ChatDto> {
  return request<ChatDto>("/api/chats", {
    method: "POST",
    body: JSON.stringify(body),
  });
}

export function getChats(userId: Guid): Promise<ChatDto[]> {
  return request<ChatDto[]>("/api/chats?userId=" + userId);
}

export function getMessages(chatId: Guid, userId: Guid): Promise<MessageDto[]> {
  return request<MessageDto[]>(
    "/api/chats/" + chatId + "/messages?userId=" + userId
  );
}

export function sendMessage(
  chatId: Guid,
  body: SendMessageRequest
): Promise<MessageDto> {
  return request<MessageDto>("/api/chats/" + chatId + "/message", {
    method: "POST",
    body: JSON.stringify(body),
  });
}

export async function sendMessagesStream(
  chatId: Guid,
  body: SendMessageRequest,
  onToken: (token: string) => void,
  signal?: AbortSignal
): Promise<void> {
  const res = await fetch(`${API}/api/chats/${chatId}/message/stream`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
    signal,
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || `HTTP ${res.status}`);
  }

  if (!res.body) {
    throw new Error("Streaming response body is missing");
  }

  const reader = res.body.getReader();
  const decoder = new TextDecoder("utf-8");

  let buffer = "";

  while (true) {
    const { value, done } = await reader.read();
    if (done) {
      break;
    }
    buffer += decoder.decode(value, { stream: true });
    const lines = buffer.split("\n");
    buffer = lines.pop() ?? "";
    for (const line of lines) {
      if (!line.trim()) continue;

      let evt: StreamEvent;
      try {
        evt = JSON.parse(line);
      } catch (e) {
        continue;
      }

      const type =
        (evt as StreamEvent & { type?: StreamEvent["Type"] }).Type ??
        (evt as StreamEvent & { type?: StreamEvent["Type"] }).type;
      const text =
        (evt as StreamEvent & { text?: string | null }).Text ??
        (evt as StreamEvent & { text?: string | null }).text;
      const error =
        (evt as StreamEvent & { error?: string | null }).Error ??
        (evt as StreamEvent & { error?: string | null }).error;

      if (type === "data") {
        if (text) {
          onToken(text);
        }
      } else if (type === "error") {
        throw new Error(error || "Unknown error");
      } else if (type === "done") {
        return;
      }
    }
  }
}
