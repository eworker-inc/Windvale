import type { Wvdbˉworkbenchˉstate } from './Wvdb-Workbench-State';
import { Editorˉtoolbar } from '../../../../Libraries/Web/Components/Editor-Toolbar/Editor-Toolbar';

type Textˉreader = (Identifier: string) => string;

function Createˉelement(Tag: string, Classˉname?: string, Text?: string): HTMLElement {
  const Element = document.createElement(Tag);
  if (Classˉname !== undefined) {
    Element.className = Classˉname;
  }
  if (Text !== undefined) {
    Element.textContent = Text;
  }
  return Element;
}

export class Explorerˉview {
  readonly #Host: HTMLElement;
  readonly #Select: (Identifier: string) => void;
  readonly #Collapse: () => void;
  readonly #Toggle: (Identifier: string) => void;
  readonly #Filter: (Text: string) => void;
  readonly #Add: () => void;
  readonly #Edit: (Identifier: string) => void;
  readonly #Expandˉall: () => void;
  readonly #Collapseˉall: () => void;

  constructor(
    Host: HTMLElement, Select: (Identifier: string) => void, Collapse: () => void,
    Toggle: (Identifier: string) => void, Filter: (Text: string) => void,
    Add: () => void, Edit: (Identifier: string) => void,
    Expandˉall: () => void, Collapseˉall: () => void
  ) {
    this.#Host = Host;
    this.#Select = Select;
    this.#Collapse = Collapse;
    this.#Toggle = Toggle; this.#Filter = Filter; this.#Add = Add; this.#Edit = Edit;
    this.#Expandˉall = Expandˉall; this.#Collapseˉall = Collapseˉall;
  }

  Render(State: Wvdbˉworkbenchˉstate, Text: Textˉreader): void {
    const Panel = Createˉelement('section', 'wv-explorer-panel');
    const Header = Createˉelement('header');
    Header.append(Createˉelement('strong', undefined, Text('explorer.title')));
    const Headerˉactions = Createˉelement('div');
    const Add = document.createElement('button');
    Add.type = 'button'; Add.textContent = '+'; Add.title = Text('explorer.add'); Add.addEventListener('click', this.#Add);
    const Expand = document.createElement('button'); Expand.type = 'button'; Expand.textContent = '↕'; Expand.title = Text('explorer.expand_all');
    Expand.addEventListener('click', () => State.Expandedˉnodes.length === 0 ? this.#Expandˉall() : this.#Collapseˉall());
    const Collapse = document.createElement('button');
    Collapse.type = 'button'; Collapse.textContent = '‹'; Collapse.title = Text('frame.explorer');
    Collapse.addEventListener('click', this.#Collapse);
    Headerˉactions.append(Add, Expand, Collapse); Header.append(Headerˉactions);

    const Search = document.createElement('input');
    Search.type = 'search'; Search.className = 'wv-explorer-filter'; Search.placeholder = Text('explorer.filter'); Search.maxLength = 128; Search.value = State.Explorerˉfilter;
    Search.addEventListener('input', () => this.#Filter(Search.value));
    const Tree = Createˉelement('div', 'wv-server-tree');
    Tree.setAttribute('role', 'tree');
    const Match = (Label: string): boolean => State.Explorerˉfilter.length === 0 || Label.toLocaleLowerCase().includes(State.Explorerˉfilter.toLocaleLowerCase());
    const Append = (Element: HTMLElement): void => { Tree.append(Element); };
    Append(this.#Row('server.local', 'server', Text('explorer.local'), State, 0, true));
    if (State.Expandedˉnodes.includes('server.local') || State.Explorerˉfilter.length > 0) {
      Append(this.#Meta(Text('explorer.synthetic'), 1));
      Append(this.#Row('database.development', 'database', Text('explorer.database'), State, 1, true));
      if (State.Expandedˉnodes.includes('database.development') || State.Explorerˉfilter.length > 0) {
        for (const [Identifier, Label, Children] of [
          ['group.collections', 'explorer.collections', [['collection.customers', 'explorer.customers'], ['collection.orders', 'explorer.orders'], ['collection.agent-runs', 'explorer.agent_runs']]],
          ['group.indexes', 'explorer.indexes', []], ['group.operations', 'explorer.operations', []]
        ] as const) {
          Append(this.#Row(Identifier, 'group', Text(Label), State, 2, true));
          if (State.Expandedˉnodes.includes(Identifier) || State.Explorerˉfilter.length > 0) {
            for (const [Child, Childˉlabel] of Children) if (Match(Text(Childˉlabel))) Append(this.#Row(Child, 'collection', Text(Childˉlabel), State, 3));
          }
        }
      }
    }
    Append(this.#Row('group.profiles', 'group', Text('explorer.saved_profiles'), State, 0, true));
    if (State.Expandedˉnodes.includes('group.profiles') || State.Explorerˉfilter.length > 0) {
      if (State.Connectionˉprofiles.length === 0) Append(this.#Meta(Text('explorer.no_profiles'), 1));
      for (const Profile of State.Connectionˉprofiles) if (Match(`${Profile.Displayˉname} ${Profile.Endpoint}`)) {
        Append(this.#Profileˉrow(Profile.Identifier, Profile.Displayˉname, Profile.Endpoint, State));
      }
    }
    const Foot = Createˉelement('div', 'wv-explorer-foot');
    Foot.innerHTML = '<span aria-hidden="true"></span>';
    Foot.append(document.createTextNode(Text('status.connection')));
    Panel.append(Header, Search, Tree, Foot);
    this.#Host.replaceChildren(Panel);
    if (State.Explorerˉfilter.length > 0) requestAnimationFrame(() => { Search.focus(); Search.setSelectionRange(Search.value.length, Search.value.length); });
  }

  #Row(
    Identifier: string,
    Kind: string,
    Label: string,
    State: Wvdbˉworkbenchˉstate,
    Depth: number,
    Expandable = false
  ): HTMLElement {
    const Button = document.createElement('button');
    Button.type = 'button';
    Button.className = 'wv-tree-row';
    Button.style.setProperty('--wv-tree-depth', String(Depth));
    Button.dataset['selected'] = String(State.Selectedˉnode === Identifier);
    Button.dataset['kind'] = Kind;
    Button.setAttribute('role', 'treeitem');
    if (Expandable) Button.setAttribute('aria-expanded', String(State.Expandedˉnodes.includes(Identifier)));
    const Glyph = Expandable ? (State.Expandedˉnodes.includes(Identifier) ? '⌄' : '›') : (Kind === 'collection' ? '▦' : '◇');
    const Icon = Createˉelement('span', 'wv-tree-icon', Glyph);
    const Name = Createˉelement('span', 'wv-tree-name', Label);
    Button.append(Icon, Name);
    if (Identifier === 'server.local') {
      Button.append(Createˉelement('span', 'wv-tree-state', 'preview'));
    }
    Button.addEventListener('click', () => Expandable ? this.#Toggle(Identifier) : this.#Select(Identifier));
    return Button;
  }

  #Profileˉrow(Identifier: string, Label: string, Endpoint: string, State: Wvdbˉworkbenchˉstate): HTMLElement {
    const Row = document.createElement('div'); Row.className = 'wv-tree-profile'; Row.dataset['selected'] = String(State.Selectedˉnode === `profile.${Identifier}`);
    const Select = document.createElement('button'); Select.type = 'button'; Select.innerHTML = '<span>◇</span><span></span><small></small>';
    Select.children[1]!.textContent = Label; Select.children[2]!.textContent = Endpoint; Select.addEventListener('click', () => this.#Select(`profile.${Identifier}`));
    const Edit = document.createElement('button'); Edit.type = 'button'; Edit.textContent = '⋯'; Edit.addEventListener('click', () => this.#Edit(Identifier));
    Row.append(Select, Edit); return Row;
  }

  #Meta(Label: string, Depth: number): HTMLElement {
    const Meta = Createˉelement('div', 'wv-tree-meta', Label);
    Meta.style.setProperty('--wv-tree-depth', String(Depth));
    return Meta;
  }
}

export class Workspaceˉview {
  readonly #Host: HTMLElement;
  readonly #Openˉtab: (Identifier: Wvdbˉworkbenchˉstate['Activeˉworkˉtab']) => void;
  readonly #Setˉquery: (Text: string) => void;
  readonly #Run: (Identifier: string) => void;

  constructor(
    Host: HTMLElement,
    Openˉtab: (Identifier: Wvdbˉworkbenchˉstate['Activeˉworkˉtab']) => void,
    Setˉquery: (Text: string) => void,
    Run: (Identifier: string) => void
  ) {
    this.#Host = Host;
    this.#Openˉtab = Openˉtab;
    this.#Setˉquery = Setˉquery;
    this.#Run = Run;
  }

  Render(State: Wvdbˉworkbenchˉstate, Text: Textˉreader): void {
    const Workspace = Createˉelement('section', 'wv-workspace');
    const Tabs = Createˉelement('div', 'wv-work-tabs');
    Tabs.setAttribute('role', 'tablist');
    const Definitions: readonly [Wvdbˉworkbenchˉstate['Activeˉworkˉtab'], string, string][] = [
      ['overview', 'work.overview', '◇'], ['query', 'work.query', '⌁'], ['customers', 'work.customers', '▦']
    ];
    for (const [Identifier, Label, Glyph] of Definitions) {
      const Tab = document.createElement('button');
      Tab.type = 'button'; Tab.className = 'wv-work-tab';
      Tab.dataset['active'] = String(State.Activeˉworkˉtab === Identifier);
      Tab.setAttribute('role', 'tab');
      Tab.setAttribute('aria-selected', String(State.Activeˉworkˉtab === Identifier));
      Tab.textContent = `${Glyph}  ${Text(Label)}`;
      Tab.addEventListener('click', () => this.#Openˉtab(Identifier), { once: true });
      Tabs.append(Tab);
    }
    const Content = Createˉelement('div', 'wv-work-content');
    const Toolbarˉhost = Createˉelement('div');
    const Toolbar = new Editorˉtoolbar(Toolbarˉhost, this.#Run);
    Toolbar.Render([
      { Identifier: 'query.new', Label: Text('toolbar.new'), Glyph: '+', Enabled: true },
      { Identifier: 'query.validate', Label: Text('toolbar.validate'), Glyph: '✓', Enabled: State.Activeˉworkˉtab === 'query', Primary: true, Separatorˉbefore: true },
      { Identifier: 'query.format', Label: Text('toolbar.format'), Glyph: '≡', Enabled: State.Activeˉworkˉtab === 'query' },
      { Identifier: 'query.copy', Label: Text('toolbar.copy'), Glyph: '▣', Enabled: State.Activeˉworkˉtab === 'query' },
      { Identifier: 'query.execute', Label: Text('toolbar.execute'), Glyph: '▶', Enabled: false, Separatorˉbefore: true }
    ], Text('toolbar.local_context'));
    switch (State.Activeˉworkˉtab) {
      case 'overview': Content.append(this.#Overview(Text)); break;
      case 'customers': Content.append(this.#Customers(Text)); break;
      case 'query': Content.append(this.#Query(State, Text)); break;
    }
    Workspace.append(Tabs, Toolbarˉhost, Content);
    this.#Host.replaceChildren(Workspace);
  }

  #Query(State: Wvdbˉworkbenchˉstate, Text: Textˉreader): HTMLElement {
    const Surface = Createˉelement('div', 'wv-query-surface');
    const Editorˉpanel = Createˉelement('section', 'wv-query-editor-panel');
    const Header = Createˉelement('header', 'wv-work-header');
    const Titles = Createˉelement('div');
    Titles.append(
      Createˉelement('strong', undefined, Text('work.query_title')),
      Createˉelement('span', undefined, Text('work.query_subtitle'))
    );
    const Stateˉpill = Createˉelement(
      'span',
      'wv-preview-pill',
      State.Queryˉstatus === 'valid' ? Text('work.valid') : Text('work.preview_only')
    );
    Stateˉpill.dataset['valid'] = String(State.Queryˉstatus === 'valid');
    Header.append(Titles, Stateˉpill);
    const Editorˉbody = Createˉelement('div', 'wv-query-editor-body');
    const Gutter = Createˉelement('pre', 'wv-query-gutter', '1\n2\n3\n4\n5\n6');
    const Editor = document.createElement('textarea');
    Editor.className = 'wv-query-editor'; Editor.spellcheck = false; Editor.value = State.Queryˉtext;
    Editor.setAttribute('aria-label', Text('work.query_title'));
    Editor.addEventListener('input', () => this.#Setˉquery(Editor.value));
    Editorˉbody.append(Gutter, Editor);
    const Parameters = Createˉelement('div', 'wv-query-parameters');
    Parameters.append(
      Createˉelement('strong', undefined, Text('work.parameters')),
      Createˉelement('code', undefined, Text('work.status_parameter')),
      Createˉelement('span', undefined, '='),
      Createˉelement('code', undefined, `"${Text('work.status_value')}"`)
    );
    Editorˉpanel.append(Header, Editorˉbody, Parameters);

    const Bounds = Createˉelement('aside', 'wv-query-bounds');
    Bounds.append(Createˉelement('strong', undefined, Text('work.query_bounds')));
    const Boundˉlist = document.createElement('ul');
    for (const Identifier of ['work.bound_collection', 'work.bound_predicates', 'work.bound_orders', 'work.bound_limit']) {
      const Item = document.createElement('li');
      Item.append(Createˉelement('span', undefined, '✓'), document.createTextNode(Text(Identifier)));
      Boundˉlist.append(Item);
    }
    Bounds.append(Boundˉlist);
    const Result = this.#Results(Text);
    Surface.append(Editorˉpanel, Bounds, Result);
    return Surface;
  }

  #Results(Text: Textˉreader): HTMLElement {
    const Result = Createˉelement('section', 'wv-result-panel');
    const Header = Createˉelement('header');
    const Title = Createˉelement('div');
    Title.append(Createˉelement('strong', undefined, Text('work.result_preview')), Createˉelement('span', undefined, Text('work.synthetic_rows')));
    Header.append(Title, Createˉelement('span', 'wv-result-count', '3 / 50'));
    const Tableˉwrap = Createˉelement('div', 'wv-data-grid-wrap');
    const Table = document.createElement('table');
    Table.className = 'wv-data-grid';
    const Head = document.createElement('thead');
    const Headˉrow = document.createElement('tr');
    for (const Identifier of ['work.column_id', 'work.column_name', 'work.column_region', 'work.column_status']) {
      Headˉrow.append(Createˉelement('th', undefined, Text(Identifier)));
    }
    Head.append(Headˉrow);
    const Body = document.createElement('tbody');
    const Rows = [
      ['CUS-00142', 'Northstar Labs', 'Atlantic', Text('work.active')],
      ['CUS-00208', 'Lumen Atelier', 'Québec', Text('work.active')],
      ['CUS-00317', 'Harbour Systems', 'Pacific', Text('work.active')]
    ];
    for (const Row of Rows) {
      const Element = document.createElement('tr');
      for (const [Index, Value] of Row.entries()) {
        const Cell = Createˉelement('td', Index === 3 ? 'wv-state-cell' : undefined, Value);
        Element.append(Cell);
      }
      Body.append(Element);
    }
    Table.append(Head, Body); Tableˉwrap.append(Table); Result.append(Header, Tableˉwrap);
    return Result;
  }

  #Overview(Text: Textˉreader): HTMLElement {
    const Overview = Createˉelement('section', 'wv-overview');
    const Heading = Createˉelement('div', 'wv-overview-heading');
    Heading.append(Createˉelement('span', 'wv-overview-icon', '◇'));
    const Copy = Createˉelement('div');
    Copy.append(Createˉelement('h1', undefined, Text('work.overview_title')), Createˉelement('p', undefined, Text('work.overview_copy')));
    Heading.append(Copy);
    const Cards = Createˉelement('div', 'wv-overview-cards');
    const Cardˉdefinitions: readonly (readonly [string, string, string])[] = [
      ['work.card_collections', '3', 'true'], ['work.card_indexes', '2', 'false'], ['work.card_generation', '17', 'false']
    ];
    for (const [Label, Value, Accent] of Cardˉdefinitions) {
      const Card = Createˉelement('article'); Card.dataset['accent'] = Accent;
      Card.append(Createˉelement('span', undefined, Text(Label)), Createˉelement('strong', undefined, Value)); Cards.append(Card);
    }
    Overview.append(Heading, Cards, this.#Results(Text)); return Overview;
  }

  #Customers(Text: Textˉreader): HTMLElement {
    const Customers = Createˉelement('section', 'wv-customers-view');
    const Header = Createˉelement('div', 'wv-customers-heading');
    const Textˉhost = Createˉelement('div');
    Textˉhost.append(Createˉelement('h1', undefined, Text('work.customer_title')), Createˉelement('p', undefined, Text('work.customer_copy')));
    Header.append(Textˉhost, Createˉelement('span', 'wv-preview-pill', Text('work.preview_only')));
    Customers.append(Header, this.#Results(Text)); return Customers;
  }
}

export class Consoleˉview {
  readonly #Host: HTMLElement;
  readonly #Selectˉtab: (Identifier: Wvdbˉworkbenchˉstate['Activeˉconsoleˉtab']) => void;
  readonly #Clear: () => void;

  constructor(Host: HTMLElement, Selectˉtab: (Identifier: Wvdbˉworkbenchˉstate['Activeˉconsoleˉtab']) => void, Clear: () => void) {
    this.#Host = Host; this.#Selectˉtab = Selectˉtab; this.#Clear = Clear;
  }

  Render(State: Wvdbˉworkbenchˉstate, Text: Textˉreader): void {
    const Console = Createˉelement('section', 'wv-console-view');
    const Header = Createˉelement('header');
    const Tabs = Createˉelement('div', 'wv-console-tabs');
    const Tabˉdefinitions: readonly (readonly [Wvdbˉworkbenchˉstate['Activeˉconsoleˉtab'], string, string])[] = [
      ['console', 'console.title', ''], ['activity', 'console.activity', String(State.Logs.length)], ['problems', 'console.problems', '0']
    ];
    for (const [Tabˉidentifier, Identifier, Count] of Tabˉdefinitions) {
      const Tab = document.createElement('button'); Tab.type = 'button';
      Tab.dataset['active'] = String(Tabˉidentifier === State.Activeˉconsoleˉtab);
      Tab.textContent = `${Text(Identifier)}${Count.length > 0 ? `  ${Count}` : ''}`;
      Tab.addEventListener('click', () => this.#Selectˉtab(Tabˉidentifier)); Tabs.append(Tab);
    }
    const Actions = Createˉelement('div');
    const Copy = document.createElement('button'); Copy.type = 'button'; Copy.textContent = Text('console.copy'); Copy.disabled = true;
    const Clear = document.createElement('button'); Clear.type = 'button'; Clear.textContent = Text('console.clear'); Clear.disabled = State.Logs.length === 0 || State.Activeˉconsoleˉtab === 'problems'; Clear.addEventListener('click', this.#Clear);
    Actions.append(Copy, Clear); Header.append(Tabs, Actions);
    const Feed = Createˉelement('div', 'wv-console-feed'); Feed.setAttribute('role', 'log');
    if (State.Activeˉconsoleˉtab === 'problems') Feed.append(Createˉelement('p', 'wv-console-empty', Text('console.no_problems')));
    for (const Entry of State.Activeˉconsoleˉtab === 'problems' ? [] : State.Logs) {
      const Line = Createˉelement('div', 'wv-console-line'); Line.dataset['tone'] = Entry.Tone;
      Line.append(
        Createˉelement('time', undefined, Entry.Time),
        Createˉelement('span', 'wv-console-bullet', '●'),
        Createˉelement('span', undefined, Text(Entry.Messageˉidentifier))
      );
      Feed.append(Line);
    }
    Console.append(Header, Feed); this.#Host.replaceChildren(Console); Feed.scrollTop = Feed.scrollHeight;
  }
}
