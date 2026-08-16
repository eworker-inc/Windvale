import { Stateˉowner } from '../../../../Libraries/Web/Framework/State/State-Owner';

export type Wvdbˉworkbenchˉarea =
  | 'frame'
  | 'layout'
  | 'ribbon'
  | 'explorer'
  | 'workspace'
  | 'assistant'
  | 'console'
  | 'status'
  | 'draft';

export interface Wvdbˉworkbenchˉchange {
  readonly Area: Wvdbˉworkbenchˉarea;
}

export interface Wvdbˉassistantˉentry {
  readonly Identifier: string;
  readonly Role: 'assistant' | 'user';
  readonly Messageˉidentifier?: string;
  readonly Rawˉtext?: string;
  readonly Metaˉidentifier?: string;
}

export interface Wvdbˉlogˉentry {
  readonly Identifier: string;
  readonly Tone: 'normal' | 'accent' | 'warning' | 'success';
  readonly Time: string;
  readonly Messageˉidentifier: string;
}

export interface Wvdbˉworkbenchˉstate {
  readonly Theme: 'light' | 'dark';
  readonly Locale: 'en' | 'fr';
  readonly Activeˉribbon: string;
  readonly Activeˉworkˉtab: 'overview' | 'query' | 'customers';
  readonly Selectedˉnode: string;
  readonly Leftˉopen: boolean;
  readonly Rightˉopen: boolean;
  readonly Consoleˉopen: boolean;
  readonly Queryˉtext: string;
  readonly Queryˉstatus: 'draft' | 'valid';
  readonly Assistantˉentries: readonly Wvdbˉassistantˉentry[];
  readonly Logs: readonly Wvdbˉlogˉentry[];
}

const Initialˉquery = `from Customers\nselect CustomerId, DisplayName, Region, Status\nwhere Status = $status\norder by DisplayName ascending\nlimit 50`;

function Readˉpreference<Name extends 'Theme' | 'Locale'>(
  Name: Name,
  Allowed: readonly Wvdbˉworkbenchˉstate[Name][],
  Fallback: Wvdbˉworkbenchˉstate[Name]
): Wvdbˉworkbenchˉstate[Name] {
  try {
    const Value = localStorage.getItem(`wvdb-workbench.${Name.toLowerCase()}`);
    return Allowed.includes(Value as Wvdbˉworkbenchˉstate[Name])
      ? Value as Wvdbˉworkbenchˉstate[Name]
      : Fallback;
  } catch {
    return Fallback;
  }
}

export class Wvdbˉworkbenchˉstateˉowner {
  readonly State: Stateˉowner<Wvdbˉworkbenchˉstate, Wvdbˉworkbenchˉchange>;
  #Sequence = 4;

  constructor() {
    const Preferredˉtheme = matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark';
    this.State = new Stateˉowner<Wvdbˉworkbenchˉstate, Wvdbˉworkbenchˉchange>({
      Theme: Readˉpreference('Theme', ['light', 'dark'], Preferredˉtheme),
      Locale: Readˉpreference('Locale', ['en', 'fr'], 'en'),
      Activeˉribbon: 'home',
      Activeˉworkˉtab: 'query',
      Selectedˉnode: 'collection.customers',
      Leftˉopen: true,
      Rightˉopen: true,
      Consoleˉopen: true,
      Queryˉtext: Initialˉquery,
      Queryˉstatus: 'draft',
      Assistantˉentries: Object.freeze([
        {
          Identifier: 'assistant-1',
          Role: 'assistant',
          Messageˉidentifier: 'assistant.welcome',
          Metaˉidentifier: 'assistant.local_meta'
        }
      ]),
      Logs: Object.freeze([
        { Identifier: 'log-1', Tone: 'accent', Time: '09:41:08', Messageˉidentifier: 'log.shell_ready' },
        { Identifier: 'log-2', Tone: 'success', Time: '09:41:08', Messageˉidentifier: 'log.preview_loaded' },
        { Identifier: 'log-3', Tone: 'warning', Time: '09:41:09', Messageˉidentifier: 'log.no_service' }
      ])
    });
  }

  Setˉribbon(Identifier: string): void {
    this.#Patch({ Activeˉribbon: Identifier }, ['ribbon']);
  }

  Setˉworkˉtab(Identifier: Wvdbˉworkbenchˉstate['Activeˉworkˉtab']): void {
    this.#Patch({ Activeˉworkˉtab: Identifier }, ['workspace', 'status']);
  }

  Selectˉnode(Identifier: string): void {
    const Tab = Identifier === 'collection.customers' ? 'customers' : undefined;
    this.#Patch(
      { Selectedˉnode: Identifier, ...(Tab === undefined ? {} : { Activeˉworkˉtab: Tab }) },
      ['explorer', 'workspace', 'status']
    );
  }

  Setˉqueryˉtext(Text: string): void {
    this.#Patch({ Queryˉtext: Text, Queryˉstatus: 'draft' }, ['draft']);
  }

  Toggleˉtheme(): void {
    const Current = this.State.Read();
    this.#Patch({ Theme: Current.Theme === 'dark' ? 'light' : 'dark' }, ['frame']);
  }

  Toggleˉlocale(): void {
    const Current = this.State.Read();
    this.#Patch(
      { Locale: Current.Locale === 'en' ? 'fr' : 'en' },
      ['frame', 'ribbon', 'explorer', 'workspace', 'assistant', 'console', 'status']
    );
  }

  Toggleˉleft(): void {
    this.#Patch({ Leftˉopen: !this.State.Read().Leftˉopen }, ['layout', 'frame']);
  }

  Toggleˉright(): void {
    this.#Patch({ Rightˉopen: !this.State.Read().Rightˉopen }, ['layout', 'frame']);
  }

  Toggleˉconsole(): void {
    this.#Patch({ Consoleˉopen: !this.State.Read().Consoleˉopen }, ['layout', 'status']);
  }

  Runˉcommand(Identifier: string): void {
    switch (Identifier) {
      case 'query.new':
        this.#Patch(
          { Activeˉworkˉtab: 'query', Queryˉstatus: 'draft' },
          ['workspace', 'status', 'console'],
          'log.query_opened',
          'accent'
        );
        break;
      case 'query.validate':
        this.#Patch(
          { Queryˉstatus: 'valid', Consoleˉopen: true },
          ['workspace', 'layout', 'status', 'console'],
          'log.query_validated',
          'success'
        );
        break;
      case 'data.browse':
        this.Setˉworkˉtab('customers');
        break;
      case 'view.console':
        this.Toggleˉconsole();
        break;
      case 'view.explorer':
        this.Toggleˉleft();
        break;
      case 'view.assistant':
      case 'ai.focus':
        this.#Patch({ Rightˉopen: true }, ['layout', 'frame', 'assistant']);
        break;
      case 'workspace.overview':
        this.Setˉworkˉtab('overview');
        break;
      default:
        break;
    }
  }

  Sendˉassistantˉmessage(Text: string): void {
    const Current = this.State.Read();
    const Userˉidentifier = `assistant-${this.#Sequence++}`;
    const Replyˉidentifier = `assistant-${this.#Sequence++}`;
    this.#Patch(
      {
        Assistantˉentries: Object.freeze([
          ...Current.Assistantˉentries,
          { Identifier: Userˉidentifier, Role: 'user', Rawˉtext: Text },
          {
            Identifier: Replyˉidentifier,
            Role: 'assistant',
            Messageˉidentifier: 'assistant.deterministic_reply',
            Metaˉidentifier: 'assistant.local_meta'
          }
        ])
      },
      ['assistant', 'console'],
      'log.assistant_local',
      'accent'
    );
  }

  #Patch(
    Patch: Partial<Wvdbˉworkbenchˉstate>,
    Areas: readonly Wvdbˉworkbenchˉarea[],
    Logˉmessage?: string,
    Logˉtone: Wvdbˉlogˉentry['Tone'] = 'normal'
  ): void {
    this.State.Update((Current) => {
      const Nextˉlogs = Logˉmessage === undefined
        ? Current.Logs
        : Object.freeze([
            ...Current.Logs,
            {
              Identifier: `log-${this.#Sequence++}`,
              Tone: Logˉtone,
              Time: new Date().toLocaleTimeString('en-CA', { hour12: false }),
              Messageˉidentifier: Logˉmessage
            }
          ]);
      return {
        Nextˉstate: { ...Current, ...Patch, Logs: Nextˉlogs },
        Changes: Areas.map((Area) => ({ Area }))
      };
    });
  }
}
