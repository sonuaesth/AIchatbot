import type {
  ChatDto,
  MessageDto,
  SendMessageRequest,
  CreateChatRequest,
  Guid,
} from "./types";

// const API = import.meta.env.VITE_API_BASE_URL || "";
const API = import.meta.env.VITE_API_BASE_URL || "/api";

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
