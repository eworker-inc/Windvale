import type {
  Wvdbˉlogˉentry,
  Wvdbˉworkbenchˉstate
} from './Wvdb-Workbench-State';

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

  constructor(Host: HTMLElement, Select: (Identifier: string) => void, Collapse: () => void) {
    this.#Host = Host;
    this.#Select = Select;
    this.#Collapse = Collapse;
  }

  Render(State: Wvdbˉworkbenchˉstate, Text: Textˉreader): void {
    const Panel = Createˉelement('section', 'wv-explorer-panel');
    const Header = Createˉelement('header');
    Header.append(Createˉelement('strong', undefined, Text('explorer.title')));
    const Headerˉactions = Createˉelement('div');
    const Add = document.createElement('button');
    Add.type = 'button'; Add.disabled = true; Add.textContent = '+'; Add.title = Text('explorer.add');
    const Collapse = document.createElement('button');
    Collapse.type = 'button'; Collapse.textContent = '‹'; Collapse.title = Text('frame.explorer');
    Collapse.addEventListener('click', this.#Collapse);
    Headerˉactions.append(Add, Collapse); Header.append(Headerˉactions);

    const Search = document.createElement('input');
    Search.type = 'search'; Search.className = 'wv-explorer-filter'; Search.placeholder = Text('explorer.filter');
    const Tree = Createˉelement('div', 'wv-server-tree');
    Tree.setAttribute('role', 'tree');
    Tree.append(
      this.#Row('server.local', 'server', Text('explorer.local'), '●', State, 0, true),
      this.#Meta(Text('explorer.synthetic'), 1),
      this.#Row('database.development', 'database', Text('explorer.database'), '◇', State, 1, true),
      this.#Row('group.collections', 'group', Text('explorer.collections'), '⌄', State, 2, true),
      this.#Row('collection.customers', 'collection', Text('explorer.customers'), '▦', State, 3),
      this.#Row('collection.orders', 'collection', Text('explorer.orders'), '▦', State, 3),
      this.#Row('collection.agent-runs', 'collection', Text('explorer.agent_runs'), '▦', State, 3),
      this.#Row('group.indexes', 'group', Text('explorer.indexes'), '›', State, 2),
      this.#Row('group.operations', 'group', Text('explorer.operations'), '›', State, 2)
    );
    const Foot = Createˉelement('div', 'wv-explorer-foot');
    Foot.innerHTML = '<span aria-hidden="true"></span>';
    Foot.append(document.createTextNode(Text('status.connection')));
    Panel.append(Header, Search, Tree, Foot);
    this.#Host.replaceChildren(Panel);
  }

  #Row(
    Identifier: string,
    Kind: string,
    Label: string,
    Glyph: string,
    State: Wvdbˉworkbenchˉstate,
    Depth: number,
    Expanded = false
  ): HTMLElement {
    const Button = document.createElement('button');
    Button.type = 'button';
    Button.className = 'wv-tree-row';
    Button.style.setProperty('--wv-tree-depth', String(Depth));
    Button.dataset['selected'] = String(State.Selectedˉnode === Identifier);
    Button.dataset['kind'] = Kind;
    Button.setAttribute('role', 'treeitem');
    if (Expanded) { Button.setAttribute('aria-expanded', 'true'); }
    const Icon = Createˉelement('span', 'wv-tree-icon', Glyph);
    const Name = Createˉelement('span', 'wv-tree-name', Label);
    Button.append(Icon, Name);
    if (Identifier === 'server.local') {
      Button.append(Createˉelement('span', 'wv-tree-state', 'preview'));
    }
    Button.addEventListener('click', () => this.#Select(Identifier));
    return Button;
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

  constructor(
    Host: HTMLElement,
    Openˉtab: (Identifier: Wvdbˉworkbenchˉstate['Activeˉworkˉtab']) => void,
    Setˉquery: (Text: string) => void
  ) {
    this.#Host = Host;
    this.#Openˉtab = Openˉtab;
    this.#Setˉquery = Setˉquery;
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
    switch (State.Activeˉworkˉtab) {
      case 'overview': Content.append(this.#Overview(Text)); break;
      case 'customers': Content.append(this.#Customers(Text)); break;
      case 'query': Content.append(this.#Query(State, Text)); break;
    }
    Workspace.append(Tabs, Content);
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

  constructor(Host: HTMLElement) {
    this.#Host = Host;
  }

  Render(Logs: readonly Wvdbˉlogˉentry[], Text: Textˉreader): void {
    const Console = Createˉelement('section', 'wv-console-view');
    const Header = Createˉelement('header');
    const Tabs = Createˉelement('div', 'wv-console-tabs');
    const Tabˉdefinitions: readonly (readonly [string, string])[] = [
      ['console.title', ''], ['console.activity', '3'], ['console.problems', '0']
    ];
    for (const [Identifier, Count] of Tabˉdefinitions) {
      const Tab = document.createElement('button'); Tab.type = 'button';
      Tab.dataset['active'] = String(Identifier === 'console.title');
      Tab.textContent = `${Text(Identifier)}${Count.length > 0 ? `  ${Count}` : ''}`; Tabs.append(Tab);
    }
    const Actions = Createˉelement('div');
    const Copy = document.createElement('button'); Copy.type = 'button'; Copy.textContent = Text('console.copy'); Copy.disabled = true;
    const Clear = document.createElement('button'); Clear.type = 'button'; Clear.textContent = Text('console.clear'); Clear.disabled = true;
    Actions.append(Copy, Clear); Header.append(Tabs, Actions);
    const Feed = Createˉelement('div', 'wv-console-feed'); Feed.setAttribute('role', 'log');
    for (const Entry of Logs) {
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
