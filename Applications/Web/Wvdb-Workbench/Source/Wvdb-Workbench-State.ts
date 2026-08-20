import { Stateˉowner } from '../../../../Libraries/Web/Framework/State/State-Owner';
import {
  CONNECTION_PROFILE_LIMIT, Readˉconnectionˉprofiles, Validateˉconnectionˉprofile,
  Writeˉconnectionˉprofiles, type Connectionˉprofile, type Connectionˉprofileˉdraft,
  type Connectionˉprofileˉresult
} from './Connection-Profile';

export type Wvdbˉworkbenchˉarea = 'frame' | 'layout' | 'ribbon' | 'explorer' |
  'workspace' | 'assistant' | 'console' | 'status' | 'draft' | 'palette';
export interface Wvdbˉworkbenchˉchange { readonly Area: Wvdbˉworkbenchˉarea; }
export interface Wvdbˉassistantˉentry {
  readonly Identifier: string; readonly Role: 'assistant' | 'user';
  readonly Messageˉidentifier?: string; readonly Rawˉtext?: string; readonly Metaˉidentifier?: string;
}
export interface Wvdbˉlogˉentry {
  readonly Identifier: string; readonly Tone: 'normal' | 'accent' | 'warning' | 'success';
  readonly Time: string; readonly Messageˉidentifier: string;
}
export interface Wvdbˉworkbenchˉlayout {
  readonly Leftˉwidth: number; readonly Rightˉwidth: number; readonly Consoleˉheight: number;
}
export interface Wvdbˉworkbenchˉstate {
  readonly Theme: 'light' | 'dark'; readonly Locale: 'en' | 'fr';
  readonly Activeˉribbon: string; readonly Ribbonˉcollapsed: boolean;
  readonly Activeˉworkˉtab: 'overview' | 'query' | 'customers';
  readonly Activeˉconsoleˉtab: 'console' | 'activity' | 'problems';
  readonly Selectedˉnode: string; readonly Expandedˉnodes: readonly string[];
  readonly Explorerˉfilter: string; readonly Leftˉopen: boolean; readonly Rightˉopen: boolean;
  readonly Consoleˉopen: boolean; readonly Paletteˉopen: boolean; readonly Layout: Wvdbˉworkbenchˉlayout;
  readonly Queryˉtext: string; readonly Queryˉstatus: 'draft' | 'valid';
  readonly Assistantˉincludeˉquery: boolean; readonly Assistantˉincludeˉschema: boolean;
  readonly Assistantˉentries: readonly Wvdbˉassistantˉentry[];
  readonly Connectionˉprofiles: readonly Connectionˉprofile[]; readonly Logs: readonly Wvdbˉlogˉentry[];
}

const UI_STORAGE_KEY = 'wvdb-workbench.ui.v1';
const DEFAULT_LAYOUT: Wvdbˉworkbenchˉlayout = Object.freeze({ Leftˉwidth: 274, Rightˉwidth: 352, Consoleˉheight: 168 });
const Initialˉquery = `from Customers\nselect CustomerId, DisplayName, Region, Status\nwhere Status = $status\norder by DisplayName ascending\nlimit 50`;

function Clamp(Value: unknown, Minimum: number, Maximum: number, Fallback: number): number {
  return typeof Value === 'number' && Number.isFinite(Value) ? Math.min(Maximum, Math.max(Minimum, Value)) : Fallback;
}
function Readˉpreferences(): Partial<Wvdbˉworkbenchˉstate> {
  try {
    const Raw = localStorage.getItem(UI_STORAGE_KEY);
    if (Raw === null || Raw.length > 8_192) return {};
    const Value = JSON.parse(Raw) as Record<string, unknown>;
    const Layout = typeof Value['Layout'] === 'object' && Value['Layout'] !== null ? Value['Layout'] as Record<string, unknown> : {};
    return {
      ...(Value['Theme'] === 'light' || Value['Theme'] === 'dark' ? { Theme: Value['Theme'] } : {}),
      ...(Value['Locale'] === 'en' || Value['Locale'] === 'fr' ? { Locale: Value['Locale'] } : {}),
      ...(typeof Value['Leftˉopen'] === 'boolean' ? { Leftˉopen: Value['Leftˉopen'] } : {}),
      ...(typeof Value['Rightˉopen'] === 'boolean' ? { Rightˉopen: Value['Rightˉopen'] } : {}),
      ...(typeof Value['Consoleˉopen'] === 'boolean' ? { Consoleˉopen: Value['Consoleˉopen'] } : {}),
      ...(typeof Value['Ribbonˉcollapsed'] === 'boolean' ? { Ribbonˉcollapsed: Value['Ribbonˉcollapsed'] } : {}),
      Layout: Object.freeze({
        Leftˉwidth: Clamp(Layout['Leftˉwidth'], 208, 440, DEFAULT_LAYOUT.Leftˉwidth),
        Rightˉwidth: Clamp(Layout['Rightˉwidth'], 286, 520, DEFAULT_LAYOUT.Rightˉwidth),
        Consoleˉheight: Clamp(Layout['Consoleˉheight'], 92, 420, DEFAULT_LAYOUT.Consoleˉheight)
      })
    };
  } catch { return {}; }
}

export class Wvdbˉworkbenchˉstateˉowner {
  readonly State: Stateˉowner<Wvdbˉworkbenchˉstate, Wvdbˉworkbenchˉchange>;
  #Sequence = 4;
  constructor() {
    const Preferredˉtheme = matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark';
    const Preferences = Readˉpreferences();
    this.State = new Stateˉowner<Wvdbˉworkbenchˉstate, Wvdbˉworkbenchˉchange>({
      Theme: Preferences.Theme ?? Preferredˉtheme, Locale: Preferences.Locale ?? 'en', Activeˉribbon: 'home',
      Ribbonˉcollapsed: Preferences.Ribbonˉcollapsed ?? false, Activeˉworkˉtab: 'query', Activeˉconsoleˉtab: 'console',
      Selectedˉnode: 'collection.customers', Expandedˉnodes: Object.freeze(['server.local', 'database.development', 'group.collections']),
      Explorerˉfilter: '', Leftˉopen: Preferences.Leftˉopen ?? true, Rightˉopen: Preferences.Rightˉopen ?? true,
      Consoleˉopen: Preferences.Consoleˉopen ?? true, Paletteˉopen: false, Layout: Preferences.Layout ?? DEFAULT_LAYOUT,
      Queryˉtext: Initialˉquery, Queryˉstatus: 'draft', Assistantˉincludeˉquery: true, Assistantˉincludeˉschema: true,
      Assistantˉentries: Object.freeze([{ Identifier: 'assistant-1', Role: 'assistant', Messageˉidentifier: 'assistant.welcome', Metaˉidentifier: 'assistant.local_meta' }]),
      Connectionˉprofiles: Readˉconnectionˉprofiles(),
      Logs: Object.freeze([
        { Identifier: 'log-1', Tone: 'accent', Time: '09:41:08', Messageˉidentifier: 'log.shell_ready' },
        { Identifier: 'log-2', Tone: 'success', Time: '09:41:08', Messageˉidentifier: 'log.preview_loaded' },
        { Identifier: 'log-3', Tone: 'warning', Time: '09:41:09', Messageˉidentifier: 'log.no_service' }
      ])
    });
  }

  Persistˉpreferences(): void {
    const State = this.State.Read();
    try {
      localStorage.setItem(UI_STORAGE_KEY, JSON.stringify({ Theme: State.Theme, Locale: State.Locale,
        Leftˉopen: State.Leftˉopen, Rightˉopen: State.Rightˉopen, Consoleˉopen: State.Consoleˉopen,
        Ribbonˉcollapsed: State.Ribbonˉcollapsed, Layout: State.Layout }));
    } catch { /* Preferences are optional and grant no authority. */ }
  }
  Setˉribbon(Identifier: string): void { this.#Patch({ Activeˉribbon: Identifier }, ['ribbon']); }
  Toggleˉribbon(): void { this.#Patch({ Ribbonˉcollapsed: !this.State.Read().Ribbonˉcollapsed }, ['ribbon']); }
  Setˉworkˉtab(Identifier: Wvdbˉworkbenchˉstate['Activeˉworkˉtab']): void { this.#Patch({ Activeˉworkˉtab: Identifier }, ['workspace', 'status']); }
  Setˉconsoleˉtab(Identifier: Wvdbˉworkbenchˉstate['Activeˉconsoleˉtab']): void { this.#Patch({ Activeˉconsoleˉtab: Identifier }, ['console']); }
  Selectˉnode(Identifier: string): void {
    const Tab = Identifier === 'collection.customers' ? 'customers' : this.State.Read().Activeˉworkˉtab;
    this.#Patch({ Selectedˉnode: Identifier, Activeˉworkˉtab: Tab }, ['explorer', 'workspace', 'status']);
  }
  Toggleˉnode(Identifier: string): void {
    const Current = this.State.Read().Expandedˉnodes;
    const Expandedˉnodes = Current.includes(Identifier) ? Current.filter((Entry) => Entry !== Identifier) : [...Current, Identifier];
    this.#Patch({ Expandedˉnodes: Object.freeze(Expandedˉnodes) }, ['explorer']);
  }
  Expandˉall(): void { this.#Patch({ Expandedˉnodes: Object.freeze(['server.local', 'database.development', 'group.collections', 'group.indexes', 'group.operations', 'group.profiles']) }, ['explorer']); }
  Collapseˉall(): void { this.#Patch({ Expandedˉnodes: Object.freeze([]) }, ['explorer']); }
  Setˉexplorerˉfilter(Text: string): void { this.#Patch({ Explorerˉfilter: Text.slice(0, 128) }, ['explorer']); }
  Setˉqueryˉtext(Text: string): void { this.#Patch({ Queryˉtext: Text.slice(0, 65_536), Queryˉstatus: 'draft' }, ['draft']); }
  Formatˉquery(): void {
    const Queryˉtext = this.State.Read().Queryˉtext.split('\n').map((Line) => Line.trimEnd()).join('\n').replace(/\n{3,}/g, '\n\n').trim();
    this.#Patch({ Queryˉtext, Queryˉstatus: 'draft' }, ['workspace', 'console'], 'log.query_formatted', 'accent');
  }
  Setˉtheme(Theme: Wvdbˉworkbenchˉstate['Theme']): void { this.#Patch({ Theme }, ['frame']); }
  Toggleˉtheme(): void { this.Setˉtheme(this.State.Read().Theme === 'dark' ? 'light' : 'dark'); }
  Setˉlocale(Locale: Wvdbˉworkbenchˉstate['Locale']): void { this.#Patch({ Locale }, ['frame', 'ribbon', 'explorer', 'workspace', 'assistant', 'console', 'status', 'palette']); }
  Toggleˉlocale(): void { this.Setˉlocale(this.State.Read().Locale === 'en' ? 'fr' : 'en'); }
  Toggleˉleft(): void { this.#Patch({ Leftˉopen: !this.State.Read().Leftˉopen }, ['layout', 'frame']); }
  Toggleˉright(): void { this.#Patch({ Rightˉopen: !this.State.Read().Rightˉopen }, ['layout', 'frame']); }
  Toggleˉconsole(): void { this.#Patch({ Consoleˉopen: !this.State.Read().Consoleˉopen }, ['layout', 'status']); }
  Openˉpalette(): void { this.#Patch({ Paletteˉopen: true }, ['palette']); }
  Closeˉpalette(): void { this.#Patch({ Paletteˉopen: false }, ['palette']); }
  Setˉlayout(Layout: Wvdbˉworkbenchˉlayout): void { this.#Patch({ Layout: Object.freeze(Layout) }, ['layout']); }
  Resetˉlayout(): void { this.#Patch({ Layout: DEFAULT_LAYOUT, Leftˉopen: true, Rightˉopen: true, Consoleˉopen: true, Ribbonˉcollapsed: false }, ['layout', 'frame', 'ribbon']); }
  Toggleˉassistantˉcontext(Context: 'query' | 'schema'): void {
    const State = this.State.Read();
    this.#Patch(Context === 'query' ? { Assistantˉincludeˉquery: !State.Assistantˉincludeˉquery } : { Assistantˉincludeˉschema: !State.Assistantˉincludeˉschema }, ['assistant']);
  }
  Newˉassistantˉchat(): void {
    this.#Patch({ Assistantˉentries: Object.freeze([{ Identifier: `assistant-${this.#Sequence++}`, Role: 'assistant', Messageˉidentifier: 'assistant.welcome', Metaˉidentifier: 'assistant.local_meta' }]) }, ['assistant', 'console'], 'log.assistant_reset', 'accent');
  }
  Clearˉconsole(): void { this.#Patch({ Logs: Object.freeze([]) }, ['console']); }
  Saveˉconnectionˉprofile(Draft: Connectionˉprofileˉdraft): Connectionˉprofileˉresult {
    const Result = Validateˉconnectionˉprofile(Draft);
    if (!Result.Ok) return Result;
    const Current = this.State.Read().Connectionˉprofiles;
    const Existing = Current.findIndex((Profile) => Profile.Identifier === Result.Profile.Identifier);
    if (Existing < 0 && Current.length >= CONNECTION_PROFILE_LIMIT) return { Ok: false, Error: 'storage' };
    const Profiles = Existing < 0 ? [...Current, Result.Profile] : Current.map((Profile, Index) => Index === Existing ? Result.Profile : Profile);
    if (!Writeˉconnectionˉprofiles(Profiles)) return { Ok: false, Error: 'storage' };
    const Expanded = [...new Set([...this.State.Read().Expandedˉnodes, 'group.profiles'])];
    this.#Patch({ Connectionˉprofiles: Object.freeze(Profiles), Expandedˉnodes: Object.freeze(Expanded) }, ['explorer', 'console'], 'log.profile_saved', 'success');
    return Result;
  }
  Deleteˉconnectionˉprofile(Identifier: string): boolean {
    const Profiles = this.State.Read().Connectionˉprofiles.filter((Profile) => Profile.Identifier !== Identifier);
    if (!Writeˉconnectionˉprofiles(Profiles)) return false;
    this.#Patch({ Connectionˉprofiles: Object.freeze(Profiles) }, ['explorer', 'console'], 'log.profile_deleted', 'warning');
    return true;
  }
  Runˉcommand(Identifier: string): void {
    switch (Identifier) {
      case 'query.new': this.#Patch({ Activeˉworkˉtab: 'query', Queryˉstatus: 'draft' }, ['workspace', 'status', 'console'], 'log.query_opened', 'accent'); break;
      case 'query.validate': this.#Patch({ Queryˉstatus: 'valid', Consoleˉopen: true }, ['workspace', 'layout', 'status', 'console'], 'log.query_validated', 'success'); break;
      case 'query.format': this.Formatˉquery(); break;
      case 'data.browse': this.Setˉworkˉtab('customers'); break;
      case 'view.console': this.Toggleˉconsole(); break;
      case 'view.explorer': this.Toggleˉleft(); break;
      case 'view.assistant': case 'ai.focus': this.#Patch({ Rightˉopen: true }, ['layout', 'frame', 'assistant']); break;
      case 'view.ribbon': this.Toggleˉribbon(); break;
      case 'view.reset': this.Resetˉlayout(); break;
      case 'workspace.overview': this.Setˉworkˉtab('overview'); break;
      case 'theme.toggle': this.Toggleˉtheme(); break;
      case 'locale.toggle': this.Toggleˉlocale(); break;
      case 'assistant.new': this.Newˉassistantˉchat(); break;
      case 'console.clear': this.Clearˉconsole(); break;
      default: break;
    }
  }
  Sendˉassistantˉmessage(Text: string): void {
    const Current = this.State.Read();
    const Entries = [...Current.Assistantˉentries,
      { Identifier: `assistant-${this.#Sequence++}`, Role: 'user' as const, Rawˉtext: Text.slice(0, 2_000) },
      { Identifier: `assistant-${this.#Sequence++}`, Role: 'assistant' as const, Messageˉidentifier: 'assistant.deterministic_reply', Metaˉidentifier: 'assistant.local_meta' }
    ].slice(-32);
    this.#Patch({ Assistantˉentries: Object.freeze(Entries) }, ['assistant', 'console'], 'log.assistant_local', 'accent');
  }
  #Patch(Patch: Partial<Wvdbˉworkbenchˉstate>, Areas: readonly Wvdbˉworkbenchˉarea[], Logˉmessage?: string, Logˉtone: Wvdbˉlogˉentry['Tone'] = 'normal'): void {
    this.State.Update((Current) => {
      const Logs = Logˉmessage === undefined ? Current.Logs : Object.freeze([...Current.Logs, {
        Identifier: `log-${this.#Sequence++}`, Tone: Logˉtone,
        Time: new Date().toLocaleTimeString('en-CA', { hour12: false }), Messageˉidentifier: Logˉmessage
      }].slice(-200));
      return { Nextˉstate: { ...Current, ...Patch, Logs }, Changes: Areas.map((Area) => ({ Area })) };
    });
  }
}
