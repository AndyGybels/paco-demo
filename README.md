# PacoDemo

PDF Q&A application. Upload a PDF, ask questions about its content, and see which page the answer came from.

## Architecture

- **PacoDemo.Api** — ASP.NET Core web API + React frontend (served as static files)
- **PacoDemo.Processor** — .NET Worker Service handling PDF extraction, embeddings, and LLM inference
- **RabbitMQ** — message broker between API and Processor (managed by Aspire)
- **PostgreSQL** — document metadata storage (managed by Aspire)
- **Ollama** — local LLM and embedding model (must be running separately)

## Prerequisites

### .NET & Aspire
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- .NET Aspire workload:
  ```bash
  dotnet workload install aspire
  ```

### Node.js
- Node.js 18+ (for building the frontend)

### Docker
- Docker Desktop (Aspire uses it to run RabbitMQ and PostgreSQL)

### Ollama
- [Ollama](https://ollama.com) installed and running
- Pull the required models:
  ```bash
  ollama pull nomic-embed-text
  ollama pull llama3.2
  ```

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
- **PostgreSQL** (with automatic schema creation)
- **PacoDemo.Api** on `http://localhost:5000`
- **PacoDemo.Processor** connected to RabbitMQ

The Aspire dashboard opens automatically and shows logs, traces, and resource health for all services.

### 3. Open the app

Navigate to `http://localhost:5000`.

## Configuration

### Ollama (Processor)

`PacoDemo.Processor/appsettings.Development.json`:

```json
{
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "LlmModel": "llama3.2"
  }
}
```

Change `BaseUrl` if Ollama is running on a different machine.
Change `LlmModel` to any chat model you have pulled in Ollama.

## Usage

1. Click **Upload** and select a PDF (max 50 MB)
2. Wait for the status to change to **Ready** (the Processor is extracting text and generating embeddings — time depends on PDF size)
3. Type a question and click **Ask**
4. The answer appears alongside source references showing which page and excerpt it came from

## Notes

- The Processor keeps an **in-memory vector store** — restarting it clears all embeddings and PDFs must be re-uploaded
- Only machine-readable PDFs are supported (no scanned image PDFs)
- Question response time depends on `llama3.2` inference speed on your hardware
