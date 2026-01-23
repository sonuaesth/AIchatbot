export type Guid = string;

export interface ChatDto {
  id: Guid;
  userId: Guid;
  title: string | null;
  createdAt: string;
}

export interface MessageDto {
  id: Guid;
  chatId: Guid;
  role: string | null;
  text: string | null;
  createdAt: string;
}

export interface CreateChatRequest {
  userId: Guid;
  title?: string | null;
}

export interface SendMessageRequest {
  userId: Guid;
  text: string;
}

export type StreamEvent = {
  Type: "data" | "done" | "error";
  Text?: string | null;
  Error?: string | null;
};
