import axios from "axios";

const BASE = "/api";

export interface SourceChunk {
  pageNumber: number;
  lineNumber: number;
  excerpt: string;
  chunkIndex: number;
}

export interface QuestionResponse {
  answer: string;
  sources: SourceChunk[];
}

export interface ConversationEntry {
  id: string;
  question: string;
  answer: string;
  sources: SourceChunk[];
  askedAt: string;
}

export const uploadDocument = (file: File) => {
  const form = new FormData();
  form.append("file", file);
  return axios.post<{ documentId: string; status: string }>(
    `${BASE}/documents/upload`,
    form,
  );
};

export const askQuestion = (documentId: string, question: string) =>
  axios.post<QuestionResponse>(`${BASE}/questions`, { documentId, question });

export interface DocumentResponse {
  id: string;
  fileName: string;
  status: string;
  uploadedAt: string;
}

export const getDocument = (documentId: string) =>
  axios.get<DocumentResponse>(`${BASE}/documents/${documentId}`);

export const getConversation = (documentId: string) =>
  axios.get<ConversationEntry[]>(`${BASE}/questions/${documentId}`);
