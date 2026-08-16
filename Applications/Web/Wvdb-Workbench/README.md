# WVDB Workbench

WVDB Workbench is Windvale's first independently deployable browser application.
This initial PWA is an explicitly synthetic, read-only product preview: it does
not connect to WVDB, call a model provider, or perform database mutations.

From this directory:

```powershell
npm ci
npm run check
npm run dev
```

The Vite preview listens on `http://127.0.0.1:5182/`. Shared browser framework
and workbench components live under `Libraries/Web/`.
