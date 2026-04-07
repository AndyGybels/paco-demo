# PacoDemo

PDF Q&A application. Upload a PDF, ask questions about its content, and see which page and line the answer came from. Each document gets a session URL so conversations persist across page reloads.

## Architecture

- **PacoDemo.Api** — ASP.NET Core web API + React frontend (served as static files)
- **PacoDemo.Processor** — .NET Worker Service handling PDF extraction, embeddings, and LLM inference
- **RabbitMQ** — message broker between API and Processor (managed by Aspire)
- **PostgreSQL** — stores document metadata and full conversation history (managed by Aspire)
- **Ollama** — local LLM and embedding model (must be running separately)

The API and Processor communicate over RabbitMQ, so the Processor can run on a separate machine. PDF bytes are sent in the message — no shared filesystem required.

## Prerequisites

### .NET & Aspire
- .NET 10 SDK
- .NET Aspire workload:
  ```bash
  dotnet workload install aspire
  ```

### Node.js
- Node.js 18+ (for building the frontend)

### Docker
- Docker Desktop (Aspire uses it to run RabbitMQ and PostgreSQL)

### Ollama
- Ollama installed and running
- Pull the required models:
  ```bash
  ollama pull nomic-embed-text
  ollama pull llama3.2
  ```

`nomic-embed-text` generates embeddings for semantic search. `mistral` (or any chat model) answers questions. See the Configuration section to change the LLM.

## Running the app

### 1. Build the frontend

```bash
cd frontend
npm install
npm run build
cd ..
```

This outputs the built frontend into `PacoDemo.Api/wwwroot`, where the API serves it as static files.

### 2. Start the Aspire AppHost

```bash
dotnet run --project PacoDemo.AppHost
```

This starts:
- **RabbitMQ** (with management UI)
- **PostgreSQL** (schema is created automatically on first run)
- **PacoDemo.Api** on `http://localhost:5000`
- **PacoDemo.Processor** connected to RabbitMQ and Ollama

The Aspire dashboard opens automatically and shows logs, traces, and resource health for all services.

### 3. Open the app

Navigate to `http://localhost:5000`.

## Configuration

### Ollama (`PacoDemo.Processor/appsettings.Development.json`)

```json
{
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "LlmModel": "llama3.2"
  }
}
```

- Change `BaseUrl` if Ollama is running on a different machine.
- Change `LlmModel` to any chat model you have pulled. Models with strong instruction-following (e.g. `mistral`, `qwen2.5:7b`, `llama3.1:8b`) give the best source attribution accuracy.

### PDF storage (`PacoDemo.Api/appsettings.Development.json`)

By default PDFs are stored in the system temp directory. Override with:

```json
{
  "Storage": {
    "BasePath": "/your/preferred/path"
  }
}
```

## Usage

1. Click **Upload PDF** and select a file (max 50 MB, machine-readable PDFs only)
2. Wait for the status badge to change to **Ready** — the Processor is extracting text, chunking, and generating embeddings
3. Type a question and press **Enter** or click **Send**
4. The answer appears in the chat; source chips below it show the exact page and line number — click a chip to jump the PDF viewer to that location
5. The session URL (`?session=<id>`) is updated automatically — reload the page to restore the conversation and PDF viewer

## Notes

- **Conversation history** is persisted in PostgreSQL — questions and answers survive API restarts
- **Vector store is in-memory** — restarting the Processor clears embeddings; re-upload the document to re-process it
- **Scanned PDFs are not supported** — only PDFs with embedded machine-readable text work with PdfPig
- Source attribution accuracy depends on how well the chosen LLM follows citation instructions
