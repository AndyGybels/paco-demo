import { useState, useRef, useCallback, useEffect } from 'react';
import { Document, Page, pdfjs } from 'react-pdf';
import * as signalR from '@microsoft/signalr';
import 'react-pdf/dist/Page/TextLayer.css';
import 'react-pdf/dist/Page/AnnotationLayer.css';
import { uploadDocument, askQuestion, getConversation, getDocument, type SourceChunk } from './api/client';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { Badge } from '@/components/ui/badge';
import { ScrollArea } from '@/components/ui/scroll-area';

pdfjs.GlobalWorkerOptions.workerSrc = new URL(
  'pdfjs-dist/build/pdf.worker.min.mjs',
  import.meta.url
).toString();

interface ChatMessage {
  id: string;
  question: string;
  answer: string | null;
  sources: SourceChunk[];
}

const statusVariant: Record<string, 'default' | 'secondary' | 'destructive' | 'outline'> = {
  Pending:    'secondary',
  Processing: 'secondary',
  Ready:      'default',
  Failed:     'destructive',
};

export default function App() {
  const [documentId, setDocumentId]   = useState<string | null>(null);
  const [pdfUrl, setPdfUrl]           = useState<string | null>(null);
  const [status, setStatus]           = useState<string>('');
  const [numPages, setNumPages]       = useState<number>(0);
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [question, setQuestion]       = useState<string>('');
  const [messages, setMessages]       = useState<ChatMessage[]>([]);

  const fileInputRef   = useRef<HTMLInputElement>(null);
  const connectionRef  = useRef<signalR.HubConnection | null>(null);
  const documentIdRef  = useRef<string | null>(null);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const isLoading = messages.some(m => m.answer === null);

  useEffect(() => { documentIdRef.current = documentId; }, [documentId]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  useEffect(() => {
    return () => { connectionRef.current?.stop(); };
  }, []);

  // Restore session from URL on mount
  useEffect(() => {
    const sessionId = new URLSearchParams(window.location.search).get('session');
    if (!sessionId) return;

    (async () => {
      try {
        const { data: doc } = await getDocument(sessionId);
        documentIdRef.current = sessionId;
        setDocumentId(sessionId);
        setStatus(doc.status);

        setPdfUrl(`/api/documents/${sessionId}/file`);

        if (doc.status === 'Ready') {
          const { data: history } = await getConversation(sessionId);
          setMessages(history.map(e => ({
            id: e.id,
            question: e.question,
            answer: e.answer,
            sources: e.sources,
          })));
        }
      } catch {
        window.history.replaceState({}, '', window.location.pathname);
      }
    })();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const loadHistory = useCallback(async (docId: string) => {
    try {
      const { data } = await getConversation(docId);
      setMessages(data.map(e => ({
        id: e.id,
        question: e.question,
        answer: e.answer,
        sources: e.sources,
      })));
    } catch {
      // history unavailable — start fresh
    }
  }, []);

  const startSignalR = useCallback(async () => {
    await connectionRef.current?.stop();

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/documents')
      .withAutomaticReconnect()
      .build();

    connection.on('DocumentStatusChanged', async (id: string, newStatus: string) => {
      if (id !== documentIdRef.current) return;
      setStatus(newStatus);
      if (newStatus === 'Ready') {
        await loadHistory(id);
        connection.stop();
        connectionRef.current = null;
      } else if (newStatus === 'Failed') {
        connection.stop();
        connectionRef.current = null;
      }
    });

    await connection.start();
    connectionRef.current = connection;
  }, [loadHistory]);

  const handleUpload = useCallback(async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setMessages([]);
    setCurrentPage(1);
    setNumPages(0);
    setPdfUrl(URL.createObjectURL(file));
    setStatus('Uploading');

    try {
      const { data } = await uploadDocument(file);
      setDocumentId(data.documentId);
      documentIdRef.current = data.documentId;
      setStatus(data.status);
      window.history.pushState({}, '', `?session=${data.documentId}`);
      await startSignalR();
    } catch {
      setStatus('Failed');
    }
  }, [startSignalR]);

  const handleAsk = useCallback(async () => {
    if (!documentId || !question.trim() || isLoading) return;

    const pendingId = crypto.randomUUID();
    const q = question.trim();
    setQuestion('');
    setMessages(prev => [...prev, { id: pendingId, question: q, answer: null, sources: [] }]);

    try {
      const { data } = await askQuestion(documentId, q);
      setMessages(prev => prev.map(m =>
        m.id === pendingId ? { ...m, answer: data.answer, sources: data.sources } : m
      ));
      if (data.sources.length > 0) setCurrentPage(data.sources[0].pageNumber);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { title?: string } } })?.response?.data?.title;
      setMessages(prev => prev.map(m =>
        m.id === pendingId ? { ...m, answer: msg ?? 'Something went wrong. Please try again.' } : m
      ));
    }
  }, [documentId, question, isLoading]);

  return (
    <div className="flex h-screen bg-background">

      {/* Left panel — chat */}
      <div className="w-[440px] min-w-[340px] border-r flex flex-col">

        {/* Header */}
        <div className="px-4 py-3 border-b flex items-center justify-between gap-3">
          <div className="flex items-center gap-3 min-w-0">
            <div className="min-w-0">
              <h1 className="text-base font-semibold tracking-tight truncate">PacoDemo</h1>
              <p className="text-xs text-muted-foreground">PDF Question &amp; Answer</p>
            </div>
            {status && (
              <Badge variant={statusVariant[status] ?? 'secondary'} className="shrink-0">
                {status}
              </Badge>
            )}
          </div>
          <div>
            <input
              ref={fileInputRef}
              type="file"
              accept=".pdf"
              className="hidden"
              onChange={handleUpload}
            />
            <Button
              variant="outline"
              size="sm"
              onClick={() => fileInputRef.current?.click()}
            >
              Upload PDF
            </Button>
          </div>
        </div>

        {/* Messages */}
        <div className="flex-1 overflow-y-auto">
          {messages.length === 0 ? (
            <div className="h-full flex items-center justify-center">
              <p className="text-sm text-muted-foreground">
                {status === 'Ready'
                  ? 'Ask a question to get started.'
                  : status
                    ? 'Processing document…'
                    : 'Upload a PDF to get started.'}
              </p>
            </div>
          ) : (
            <div className="p-4 space-y-6">
              {messages.map(msg => (
                <div key={msg.id} className="space-y-2">

                  {/* Question — right */}
                  <div className="flex justify-end">
                    <div className="bg-primary text-primary-foreground rounded-2xl rounded-tr-sm px-4 py-2.5 max-w-[85%]">
                      <p className="text-sm leading-relaxed">{msg.question}</p>
                    </div>
                  </div>

                  {/* Answer — left */}
                  <div className="flex justify-start">
                    <div className="bg-muted rounded-2xl rounded-tl-sm px-4 py-2.5 max-w-[85%] space-y-2">
                      {msg.answer === null ? (
                        <p className="text-sm text-muted-foreground animate-pulse">Thinking…</p>
                      ) : (
                        <>
                          <p className="text-sm leading-relaxed whitespace-pre-wrap">{msg.answer}</p>
                          {msg.sources.length > 0 && (
                            <div className="flex flex-wrap gap-1.5 pt-1">
                              {msg.sources.map((s, i) => (
                                <button
                                  key={i}
                                  onClick={() => setCurrentPage(s.pageNumber)}
                                  className="text-left"
                                >
                                  <Badge
                                    variant={currentPage === s.pageNumber ? 'default' : 'secondary'}
                                    className="text-xs cursor-pointer hover:opacity-80 transition-opacity"
                                  >
                                    Page {s.pageNumber}, Line {s.lineNumber}
                                  </Badge>
                                </button>
                              ))}
                            </div>
                          )}
                        </>
                      )}
                    </div>
                  </div>

                </div>
              ))}
              <div ref={messagesEndRef} />
            </div>
          )}
        </div>

        {/* Input */}
        {status === 'Ready' && (
          <div className="p-3 border-t">
            <div className="flex items-end gap-2">
              <Textarea
                placeholder="Ask a question about this document…"
                value={question}
                onChange={e => setQuestion(e.target.value)}
                onKeyDown={e => {
                  if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    handleAsk();
                  }
                }}
                rows={2}
                className="resize-none flex-1"
              />
              <Button
                onClick={handleAsk}
                disabled={isLoading || !question.trim()}
                className="shrink-0"
              >
                {isLoading ? 'Thinking…' : 'Send'}
              </Button>
            </div>
          </div>
        )}
      </div>

      {/* Right panel — PDF viewer */}
      <div className="flex-1 flex flex-col bg-muted/30">
        {pdfUrl ? (
          <>
            <div className="flex items-center gap-3 px-4 py-2.5 border-b bg-background">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                disabled={currentPage <= 1}
              >
                ‹ Prev
              </Button>
              <span className="text-sm text-muted-foreground">
                Page <span className="font-medium text-foreground">{currentPage}</span>
                {numPages > 0 && <> / {numPages}</>}
              </span>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setCurrentPage(p => Math.min(numPages || p, p + 1))}
                disabled={numPages > 0 && currentPage >= numPages}
              >
                Next ›
              </Button>
            </div>
            <ScrollArea className="flex-1">
              <div className="flex justify-center p-6">
                <Document
                  file={pdfUrl}
                  onLoadSuccess={({ numPages }) => setNumPages(numPages)}
                >
                  <Page
                    pageNumber={currentPage}
                    width={600}
                    className="shadow-md rounded-sm overflow-hidden"
                  />
                </Document>
              </div>
            </ScrollArea>
          </>
        ) : (
          <div className="flex-1 flex items-center justify-center">
            <p className="text-muted-foreground text-sm">Upload a PDF to get started</p>
          </div>
        )}
      </div>

    </div>
  );
}
