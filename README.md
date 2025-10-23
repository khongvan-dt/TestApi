# AutoApiTester (sample)

This archive contains a minimal runnable sample for **AutoApiTester**:
- backend: .NET 8 Web API (uses Microsoft.Playwright and Newtonsoft.Json)
- frontend: Vue 3 (Vite) minimal UI

## Prerequisites
- .NET 8 SDK installed (https://dotnet.microsoft.com)
- Node.js & npm (for frontend)
- (Optional) Playwright dependencies if you want to run real Playwright requests:
  - `dotnet tool install --global Microsoft.Playwright.CLI`
  - `playwright install` (may be required)

## Run backend
1. Open a terminal in `backend/`
2. Run:
   ```bash
   dotnet restore
   dotnet run
   ```
   The backend will start at `http://localhost:5000`.

## Run frontend
1. Open a terminal in `frontend/`
2. Run:
   ```bash
   npm install
   npm run dev
   ```
   The frontend dev server will start at `http://localhost:5173` and proxy `/api` to the backend.

## Notes
- Test data examples are in `backend/TestData/`
- Logs are saved to `backend/Logs/YYYY-MM-DD/`
- This is a minimal starter. Consider adding:
  - File listing endpoints
  - Monaco editor integration
  - SignalR for live logs
  - Better error handling and authentication
