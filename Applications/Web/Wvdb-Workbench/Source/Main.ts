import '../../../../Libraries/Web/Framework/Styles/Web-Framework.css';
import '../../../../Libraries/Web/Components/App-Shell/App-Shell.css';
import '../../../../Libraries/Web/Components/Assistant-Panel/Assistant-Panel.css';
import '../../../../Libraries/Web/Components/Command-Surface/Command-Surface.css';
import '../../../../Libraries/Web/Components/Command-Palette/Command-Palette.css';
import '../../../../Libraries/Web/Components/Editor-Toolbar/Editor-Toolbar.css';
import '../../../../Libraries/Web/Components/Pwa-Window-Frame/Pwa-Window-Frame.css';
import '../../../../Libraries/Web/Components/Status-Surface/Status-Surface.css';
import './Wvdb-Workbench.css';
import './Wvdb-Workbench-Dialogs.css';

import { Resolveˉfeatureˉorder } from '../../../../Libraries/Web/Framework/Composition/Feature-Manifest';
import { Lifecycleˉscope } from '../../../../Libraries/Web/Framework/Lifecycle/Lifecycle-Scope';
import { Localizationˉregistry } from '../../../../Libraries/Web/Framework/Localization/Localization';
import { Renderˉscheduler } from '../../../../Libraries/Web/Framework/UI/Render-Scheduler';
import { Themeˉregistry } from '../../../../Libraries/Web/Framework/Themes/Theme-Registry';
import {
  Appˉshell,
  Assistantˉpanel,
  Commandˉpalette,
  Commandˉsurface,
  Pwaˉwindowˉframe,
  Statusˉsurface,
  type Assistantˉmessage,
  type Commandˉpaletteˉitem
} from '../../../../Libraries/Web/Components/Index';
import { Wvdbˉworkbenchˉfeatures } from './App-Manifest';
import { Buildˉcommandˉtabs } from './Wvdb-Workbench-Commands';
import { Wvdbˉworkbenchˉlocalizationˉpacks } from './Wvdb-Workbench-Localization';
import {
  Wvdbˉworkbenchˉstateˉowner,
  type Wvdbˉworkbenchˉarea,
  type Wvdbˉworkbenchˉchange
} from './Wvdb-Workbench-State';
import { Consoleˉview, Explorerˉview, Workspaceˉview } from './Wvdb-Workbench-Views';
import { Wvdbˉworkbenchˉdialogs } from './Wvdb-Workbench-Dialogs';

interface Installˉpromptˉevent extends Event {
  prompt(): Promise<void>;
  readonly userChoice: Promise<{ readonly outcome: 'accepted' | 'dismissed' }>;
}

const Mount = document.getElementById('Wvdbˉworkbench');
if (Mount === null) {
  throw new Error('WVDB Workbench mount was not found.');
}

const Featureˉorder = Resolveˉfeatureˉorder(Wvdbˉworkbenchˉfeatures);
if (Featureˉorder.at(-1)?.Identifier !== 'wvdb.workbench') {
  throw new Error('WVDB Workbench feature composition did not resolve.');
}

const Scope = new Lifecycleˉscope();
const Owner = new Wvdbˉworkbenchˉstateˉowner();
const Localizer = new Localizationˉregistry('en', Owner.State.Read().Locale);
for (const Pack of Wvdbˉworkbenchˉlocalizationˉpacks) {
  Localizer.Register(Pack);
}
const Themes = new Themeˉregistry();
Themes.Register({ Identifier: 'dark', Colorˉscheme: 'dark' });
Themes.Register({ Identifier: 'light', Colorˉscheme: 'light' });

const Text = (Identifier: string): string => Localizer.Text('wvdb', Identifier);
const Initialˉlayout = Owner.State.Read().Layout;
const Shell = new Appˉshell(Mount, { ...Initialˉlayout, Onˉlayoutˉchange: (Layout) => Owner.Setˉlayout(Layout) });
Scope.Own(() => Shell.Dispose());

let Installˉprompt: Installˉpromptˉevent | undefined;

const Frame = new Pwaˉwindowˉframe(Shell.Frameˉhost, {
  Toggleˉexplorer: () => Owner.Toggleˉleft(),
  Toggleˉassistant: () => Owner.Toggleˉright(),
  Toggleˉtheme: () => Owner.Toggleˉtheme(),
  Toggleˉlocale: () => Owner.Toggleˉlocale(),
  Openˉpalette: () => Owner.Openˉpalette(),
  Openˉsettings: () => Dialogs.Openˉsettings(Text, Owner.State.Read()),
  Install: () => {
    const Prompt = Installˉprompt;
    if (Prompt === undefined) {
      return;
    }
    void Prompt.prompt().then(async () => {
      await Prompt.userChoice;
      Installˉprompt = undefined;
      Frame.Setˉinstallˉavailable(false);
    });
  }
});
Scope.Own(() => Frame.Dispose());

const Commands = new Commandˉsurface(
  Shell.Ribbonˉhost,
  (Identifier) => Owner.Setˉribbon(Identifier),
  (Identifier) => Runˉcommand(Identifier),
  () => Owner.Toggleˉribbon()
);
const Dialogs = new Wvdbˉworkbenchˉdialogs(Shell.Overlayˉhost, {
  Saveˉprofile: (Draft) => Owner.Saveˉconnectionˉprofile(Draft),
  Deleteˉprofile: (Identifier) => Owner.Deleteˉconnectionˉprofile(Identifier),
  Applyˉsettings: (Theme, Locale) => { Owner.Setˉtheme(Theme); Owner.Setˉlocale(Locale); },
  Resetˉlayout: () => Owner.Resetˉlayout()
});
const Runˉcommand = (Identifier: string): void => {
  if (Identifier === 'server.connect') Dialogs.Openˉconnection(Text);
  else if (Identifier === 'app.settings') Dialogs.Openˉsettings(Text, Owner.State.Read());
  else if (Identifier === 'query.copy') void navigator.clipboard?.writeText(Owner.State.Read().Queryˉtext);
  else Owner.Runˉcommand(Identifier);
};
const Explorer = new Explorerˉview(
  Shell.Explorerˉhost,
  (Identifier) => Owner.Selectˉnode(Identifier),
  () => Owner.Toggleˉleft(),
  (Identifier) => Owner.Toggleˉnode(Identifier),
  (Filter) => Owner.Setˉexplorerˉfilter(Filter),
  () => Dialogs.Openˉconnection(Text),
  (Identifier) => {
    const Profile = Owner.State.Read().Connectionˉprofiles.find((Entry) => Entry.Identifier === Identifier);
    if (Profile !== undefined) Dialogs.Openˉconnection(Text, Profile);
  },
  () => Owner.Expandˉall(),
  () => Owner.Collapseˉall()
);
const Workspace = new Workspaceˉview(
  Shell.Workspaceˉhost,
  (Identifier) => Owner.Setˉworkˉtab(Identifier),
  (Query) => Owner.Setˉqueryˉtext(Query),
  Runˉcommand
);
const Assistant = new Assistantˉpanel(
  Shell.Assistantˉhost,
  {
    Onˉsend: (Message) => Owner.Sendˉassistantˉmessage(Message),
    Onˉcollapse: () => Owner.Toggleˉright(),
    Onˉnewˉchat: () => Owner.Newˉassistantˉchat(),
    Onˉcontext: (Context) => Owner.Toggleˉassistantˉcontext(Context)
  }
);
const Console = new Consoleˉview(Shell.Consoleˉhost, (Identifier) => Owner.Setˉconsoleˉtab(Identifier), () => Owner.Clearˉconsole());
const Status = new Statusˉsurface(Shell.Statusˉhost, () => Owner.Toggleˉconsole());
const Palette = new Commandˉpalette(Shell.Overlayˉhost, Runˉcommand, () => Owner.Closeˉpalette());

const Renderˉframe = (): void => {
  const State = Owner.State.Read();
  Localizer.Setˉlocale(State.Locale);
  Themes.Apply(document.documentElement, State.Theme);
  document.documentElement.lang = State.Locale;
  const Themeˉcolor = document.querySelector<HTMLMetaElement>('meta[name="theme-color"]');
  Themeˉcolor?.setAttribute('content', State.Theme === 'dark' ? '#10141d' : '#f7f9fb');
  Frame.Render({
    Product: Text('frame.product'),
    Context: Text('frame.context'),
    Preview: Text('frame.preview'),
    Search: Text('frame.search'),
    Explorer: Text('frame.explorer'),
    Assistant: Text('frame.assistant'),
    Theme: Text('frame.theme'),
    Locale: Text('frame.locale'),
    Install: Text('frame.install'),
    Settings: Text('frame.settings')
  }, State.Theme, State.Locale);
};

const Renderˉlayout = (): void => {
  const State = Owner.State.Read();
  Shell.Setˉleftˉopen(State.Leftˉopen);
  Shell.Setˉrightˉopen(State.Rightˉopen);
  Shell.Setˉconsoleˉopen(State.Consoleˉopen);
  Shell.Applyˉlayout(State.Layout);
};

const Renderˉribbon = (): void => {
  const State = Owner.State.Read();
  Commands.Render(Buildˉcommandˉtabs(Text), State.Activeˉribbon, State.Ribbonˉcollapsed, Text('ribbon.collapse'));
};

const Renderˉexplorer = (): void => Explorer.Render(Owner.State.Read(), Text);
const Renderˉworkspace = (): void => Workspace.Render(Owner.State.Read(), Text);
const Renderˉconsole = (): void => Console.Render(Owner.State.Read(), Text);

const Renderˉassistant = (): void => {
  const Entries: readonly Assistantˉmessage[] = Owner.State.Read().Assistantˉentries.map((Entry) => ({
    Identifier: Entry.Identifier,
    Role: Entry.Role,
    Text: Entry.Rawˉtext ?? Text(Entry.Messageˉidentifier ?? 'assistant.deterministic_reply'),
    ...(Entry.Metaˉidentifier === undefined ? {} : { Meta: Text(Entry.Metaˉidentifier) })
  }));
  const State = Owner.State.Read();
  Assistant.Render({
    Title: Text('assistant.title'),
    Preview: Text('assistant.preview'),
    Emptyˉhint: Text('assistant.context'),
    Placeholder: Text('assistant.placeholder'),
    Send: Text('assistant.send'),
    Collapse: Text('assistant.collapse'),
    Newˉchat: Text('assistant.new_chat'),
    Queryˉcontext: Text('assistant.query_context'),
    Schemaˉcontext: Text('assistant.schema_context'),
    Sessionˉonly: Text('assistant.session_only'),
    Suggestions: [Text('assistant.suggestion_validate'), Text('assistant.suggestion_bounds'), Text('assistant.suggestion_index')]
  }, Entries, State.Assistantˉincludeˉquery, State.Assistantˉincludeˉschema);
};

const Buildˉpaletteˉcommands = (): readonly Commandˉpaletteˉitem[] => [
  { Identifier: 'query.new', Label: Text('command.new_query'), Detail: Text('palette.local_command'), Shortcut: '', Glyph: '+', Enabled: true },
  { Identifier: 'query.validate', Label: Text('command.validate'), Detail: Text('palette.local_command'), Shortcut: 'Ctrl+Enter', Glyph: '✓', Enabled: true },
  { Identifier: 'query.format', Label: Text('toolbar.format'), Detail: Text('palette.local_command'), Glyph: '≡', Enabled: true },
  { Identifier: 'server.connect', Label: Text('command.connect'), Detail: Text('palette.saved_profile'), Glyph: '↗', Enabled: true },
  { Identifier: 'view.explorer', Label: Text('command.explorer'), Detail: Text('palette.layout_command'), Shortcut: 'Ctrl+B', Glyph: '☷', Enabled: true },
  { Identifier: 'view.console', Label: Text('command.console'), Detail: Text('palette.layout_command'), Shortcut: 'Ctrl+J', Glyph: '▤', Enabled: true },
  { Identifier: 'view.assistant', Label: Text('command.assistant'), Detail: Text('palette.layout_command'), Shortcut: 'Ctrl+Shift+A', Glyph: 'AI', Enabled: true },
  { Identifier: 'view.ribbon', Label: Text('palette.ribbon'), Detail: Text('palette.layout_command'), Glyph: '⌃', Enabled: true },
  { Identifier: 'view.reset', Label: Text('settings.reset_layout'), Detail: Text('palette.layout_command'), Glyph: '↺', Enabled: true },
  { Identifier: 'assistant.new', Label: Text('assistant.new_chat'), Detail: Text('palette.local_command'), Glyph: 'AI', Enabled: true },
  { Identifier: 'theme.toggle', Label: Text('frame.theme'), Detail: Text('palette.preference'), Glyph: '◐', Enabled: true },
  { Identifier: 'locale.toggle', Label: Text('frame.locale'), Detail: Text('palette.preference'), Glyph: '文', Enabled: true },
  { Identifier: 'app.settings', Label: Text('settings.title'), Detail: Text('palette.preference'), Shortcut: 'Ctrl+,', Glyph: '⚙', Enabled: true }
];
const Renderˉpalette = (): void => {
  Palette.Render(Owner.State.Read().Paletteˉopen, {
    Title: Text('palette.title'), Placeholder: Text('palette.placeholder'), Empty: Text('palette.empty'), Close: Text('dialog.cancel')
  }, Buildˉpaletteˉcommands());
};

const Renderˉstatus = (): void => {
  const State = Owner.State.Read();
  Status.Render([
    { Identifier: 'server', Label: `● ${Text('status.server')}`, Tone: 'warning' },
    { Identifier: 'database', Label: `◇ ${Text('status.database')}` },
    { Identifier: 'mode', Label: Text('status.mode'), Tone: 'accent' },
    { Identifier: 'right.rows', Label: Text('status.rows') },
    { Identifier: 'right.connection', Label: Text('status.connection'), Tone: 'warning' }
  ], Text('status.console'), State.Consoleˉopen);
};

const Scheduler = new Renderˉscheduler<Wvdbˉworkbenchˉchange>((Identifier, Failure) => {
  console.error(`Render boundary ${Identifier} failed.`, Failure);
});
const Register = (
  Identifier: string,
  Areas: readonly Wvdbˉworkbenchˉarea[],
  Render: () => void
): void => {
  Scope.Own(Scheduler.Register({
    Identifier,
    Matches: (Change) => Areas.includes(Change.Area),
    Render
  }));
};
Register('frame', ['frame'], Renderˉframe);
Register('layout', ['layout'], Renderˉlayout);
Register('ribbon', ['ribbon'], Renderˉribbon);
Register('explorer', ['explorer'], Renderˉexplorer);
Register('workspace', ['workspace'], Renderˉworkspace);
Register('assistant', ['assistant'], Renderˉassistant);
Register('console', ['console'], Renderˉconsole);
Register('status', ['status'], Renderˉstatus);
Register('palette', ['palette', 'frame'], Renderˉpalette);

Scope.Own(Owner.State.Subscribe((_State, Changes) => {
  if (Changes.some((Change) => ['frame', 'layout', 'ribbon'].includes(Change.Area))) Owner.Persistˉpreferences();
  Scheduler.Notify(Changes);
}));

Renderˉframe();
Renderˉlayout();
Renderˉribbon();
Renderˉexplorer();
Renderˉworkspace();
Renderˉassistant();
Renderˉconsole();
Renderˉstatus();
Renderˉpalette();

Scope.Ownˉevent(window, 'keydown', (Rawˉevent) => {
  const Event = Rawˉevent as KeyboardEvent;
  const Modifier = Event.ctrlKey || Event.metaKey;
  const Target = Event.target as HTMLElement | null;
  const Editing = Target?.matches('input, textarea, select, [contenteditable="true"]') === true;
  if (Modifier && Event.key.toLocaleLowerCase() === 'k') { Event.preventDefault(); Owner.Openˉpalette(); }
  else if (Modifier && Event.key === ',') { Event.preventDefault(); Dialogs.Openˉsettings(Text, Owner.State.Read()); }
  else if (Modifier && Event.key === 'Enter' && Editing) { Event.preventDefault(); Runˉcommand('query.validate'); }
  else if (!Editing && Modifier && Event.key.toLocaleLowerCase() === 'b') { Event.preventDefault(); Owner.Toggleˉleft(); }
  else if (!Editing && Modifier && Event.key.toLocaleLowerCase() === 'j') { Event.preventDefault(); Owner.Toggleˉconsole(); }
  else if (!Editing && Modifier && Event.shiftKey && Event.key.toLocaleLowerCase() === 'a') { Event.preventDefault(); Owner.Toggleˉright(); }
  else if (Event.key === 'Escape' && Owner.State.Read().Paletteˉopen) Owner.Closeˉpalette();
});

Scope.Ownˉevent(window, 'beforeinstallprompt', (Event) => {
  Event.preventDefault();
  Installˉprompt = Event as Installˉpromptˉevent;
  Frame.Setˉinstallˉavailable(true);
});
Scope.Ownˉevent(window, 'appinstalled', () => {
  Installˉprompt = undefined;
  Frame.Setˉinstallˉavailable(false);
});
Scope.Ownˉevent(window, 'pagehide', (Event) => {
  if (!(Event as PageTransitionEvent).persisted) {
    Scope.Dispose();
  }
});

if ('serviceWorker' in navigator && (window.isSecureContext || location.hostname === '127.0.0.1')) {
  void navigator.serviceWorker.register('./Service-Worker.js', { scope: './' }).catch((Failure: unknown) => {
    console.warn('WVDB Workbench service worker registration failed.', Failure);
  });
}
