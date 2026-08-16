import '../../../../Libraries/Web/Framework/Styles/Web-Framework.css';
import '../../../../Libraries/Web/Components/App-Shell/App-Shell.css';
import '../../../../Libraries/Web/Components/Assistant-Panel/Assistant-Panel.css';
import '../../../../Libraries/Web/Components/Command-Surface/Command-Surface.css';
import '../../../../Libraries/Web/Components/Pwa-Window-Frame/Pwa-Window-Frame.css';
import '../../../../Libraries/Web/Components/Status-Surface/Status-Surface.css';
import './Wvdb-Workbench.css';

import { Resolveˉfeatureˉorder } from '../../../../Libraries/Web/Framework/Composition/Feature-Manifest';
import { Lifecycleˉscope } from '../../../../Libraries/Web/Framework/Lifecycle/Lifecycle-Scope';
import { Localizationˉregistry } from '../../../../Libraries/Web/Framework/Localization/Localization';
import { Renderˉscheduler } from '../../../../Libraries/Web/Framework/UI/Render-Scheduler';
import { Themeˉregistry } from '../../../../Libraries/Web/Framework/Themes/Theme-Registry';
import {
  Appˉshell,
  Assistantˉpanel,
  Commandˉsurface,
  Pwaˉwindowˉframe,
  Statusˉsurface,
  type Assistantˉmessage
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
const Shell = new Appˉshell(Mount, { Leftˉwidth: 274, Rightˉwidth: 352, Consoleˉheight: 168 });
Scope.Own(() => Shell.Dispose());

let Installˉprompt: Installˉpromptˉevent | undefined;

const Frame = new Pwaˉwindowˉframe(Shell.Frameˉhost, {
  Toggleˉexplorer: () => Owner.Toggleˉleft(),
  Toggleˉassistant: () => Owner.Toggleˉright(),
  Toggleˉtheme: () => Owner.Toggleˉtheme(),
  Toggleˉlocale: () => Owner.Toggleˉlocale(),
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
  (Identifier) => Owner.Runˉcommand(Identifier)
);
const Explorer = new Explorerˉview(
  Shell.Explorerˉhost,
  (Identifier) => Owner.Selectˉnode(Identifier),
  () => Owner.Toggleˉleft()
);
const Workspace = new Workspaceˉview(
  Shell.Workspaceˉhost,
  (Identifier) => Owner.Setˉworkˉtab(Identifier),
  (Query) => Owner.Setˉqueryˉtext(Query)
);
const Assistant = new Assistantˉpanel(
  Shell.Assistantˉhost,
  (Message) => Owner.Sendˉassistantˉmessage(Message),
  () => Owner.Toggleˉright()
);
const Console = new Consoleˉview(Shell.Consoleˉhost);
const Status = new Statusˉsurface(Shell.Statusˉhost, () => Owner.Toggleˉconsole());

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
    Install: Text('frame.install')
  }, State.Theme, State.Locale);
};

const Renderˉlayout = (): void => {
  const State = Owner.State.Read();
  Shell.Setˉleftˉopen(State.Leftˉopen);
  Shell.Setˉrightˉopen(State.Rightˉopen);
  Shell.Setˉconsoleˉopen(State.Consoleˉopen);
};

const Renderˉribbon = (): void => {
  const State = Owner.State.Read();
  Commands.Render(Buildˉcommandˉtabs(Text), State.Activeˉribbon);
};

const Renderˉexplorer = (): void => Explorer.Render(Owner.State.Read(), Text);
const Renderˉworkspace = (): void => Workspace.Render(Owner.State.Read(), Text);
const Renderˉconsole = (): void => Console.Render(Owner.State.Read().Logs, Text);

const Renderˉassistant = (): void => {
  const Entries: readonly Assistantˉmessage[] = Owner.State.Read().Assistantˉentries.map((Entry) => ({
    Identifier: Entry.Identifier,
    Role: Entry.Role,
    Text: Entry.Rawˉtext ?? Text(Entry.Messageˉidentifier ?? 'assistant.deterministic_reply'),
    ...(Entry.Metaˉidentifier === undefined ? {} : { Meta: Text(Entry.Metaˉidentifier) })
  }));
  Assistant.Render({
    Title: Text('assistant.title'),
    Preview: Text('assistant.preview'),
    Emptyˉhint: Text('assistant.context'),
    Placeholder: Text('assistant.placeholder'),
    Send: Text('assistant.send'),
    Collapse: Text('assistant.collapse')
  }, Entries);
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

Scope.Own(Owner.State.Subscribe((State, Changes) => {
  if (Changes.some((Change) => Change.Area === 'frame')) {
    try {
      localStorage.setItem('wvdb-workbench.theme', State.Theme);
      localStorage.setItem('wvdb-workbench.locale', State.Locale);
    } catch {
      // Browser preferences are optional and have no effect on application authority.
    }
  }
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
