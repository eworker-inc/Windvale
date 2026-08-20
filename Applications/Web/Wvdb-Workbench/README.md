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

The frontend currently supports collapsible and resizable panels, a filterable
server tree, local query validation and formatting, an editor toolbar, console
tabs, a command palette, light/dark themes, English/French copy, native settings
and connection-profile dialogs, and a deterministic session-only assistant.
Connection profiles are validated, limited to eight, and stored in the browser
without credentials; saving a profile never attempts a server connection.

Useful shortcuts include `Ctrl+K` for the command palette, `Ctrl+Enter` for
local query validation, `Ctrl+B` for the explorer, `Ctrl+J` for the console,
`Ctrl+Shift+A` for the assistant, and `Ctrl+,` for settings. On macOS, Command
may be used in place of Control.
