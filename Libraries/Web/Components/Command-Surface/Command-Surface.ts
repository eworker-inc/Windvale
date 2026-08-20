export interface Commandˉitem {
  readonly Identifier: string;
  readonly Label: string;
  readonly Glyph: string;
  readonly Enabled: boolean;
}

export interface Commandˉgroup {
  readonly Label: string;
  readonly Commands: readonly Commandˉitem[];
}

export interface Commandˉtab {
  readonly Identifier: string;
  readonly Label: string;
  readonly Groups: readonly Commandˉgroup[];
}

export class Commandˉsurface {
  readonly #Host: HTMLElement;
  readonly #Onˉtab: (Identifier: string) => void;
  readonly #Onˉcommand: (Identifier: string) => void;
  readonly #Onˉcollapse: () => void;

  constructor(
    Host: HTMLElement,
    Onˉtab: (Identifier: string) => void,
    Onˉcommand: (Identifier: string) => void,
    Onˉcollapse: () => void
  ) {
    this.#Host = Host;
    this.#Onˉtab = Onˉtab;
    this.#Onˉcommand = Onˉcommand;
    this.#Onˉcollapse = Onˉcollapse;
  }

  Render(
    Tabs: readonly Commandˉtab[],
    Activeˉidentifier: string,
    Collapsed: boolean,
    Collapseˉlabel: string
  ): void {
    const Surface = document.createElement('div');
    Surface.className = 'wv-command-surface';
    Surface.dataset['collapsed'] = String(Collapsed);
    const Tablist = document.createElement('div');
    Tablist.className = 'wv-command-tabs';
    Tablist.setAttribute('role', 'tablist');
    for (const Tab of Tabs) {
      const Button = document.createElement('button');
      Button.type = 'button';
      Button.className = 'wv-command-tab';
      Button.textContent = Tab.Label;
      Button.dataset['active'] = String(Tab.Identifier === Activeˉidentifier);
      Button.setAttribute('role', 'tab');
      Button.setAttribute('aria-selected', String(Tab.Identifier === Activeˉidentifier));
      Button.addEventListener('click', () => this.#Onˉtab(Tab.Identifier));
      Tablist.append(Button);
    }
    const Collapse = document.createElement('button');
    Collapse.type = 'button';
    Collapse.className = 'wv-command-collapse';
    Collapse.textContent = Collapsed ? '⌄' : '⌃';
    Collapse.title = Collapseˉlabel;
    Collapse.setAttribute('aria-label', Collapseˉlabel);
    Collapse.setAttribute('aria-expanded', String(!Collapsed));
    Collapse.addEventListener('click', this.#Onˉcollapse);
    Tablist.append(Collapse);
    const Groups = document.createElement('div');
    Groups.className = 'wv-command-groups';
    const Activeˉtab = Tabs.find((Tab) => Tab.Identifier === Activeˉidentifier) ?? Tabs[0];
    for (const Group of Activeˉtab?.Groups ?? []) {
      const Groupˉelement = document.createElement('section');
      Groupˉelement.className = 'wv-command-group';
      const Items = document.createElement('div');
      Items.className = 'wv-command-items';
      for (const Command of Group.Commands) {
        const Button = document.createElement('button');
        Button.type = 'button';
        Button.className = 'wv-command-button';
        Button.disabled = !Command.Enabled;
        Button.dataset['command'] = Command.Identifier;
        const Glyph = document.createElement('span');
        Glyph.className = 'wv-command-glyph';
        Glyph.textContent = Command.Glyph;
        Glyph.setAttribute('aria-hidden', 'true');
        const Label = document.createElement('span');
        Label.textContent = Command.Label;
        Button.append(Glyph, Label);
        Button.addEventListener('click', () => this.#Onˉcommand(Command.Identifier));
        Items.append(Button);
      }
      const Label = document.createElement('span');
      Label.className = 'wv-command-group-label';
      Label.textContent = Group.Label;
      Groupˉelement.append(Items, Label);
      Groups.append(Groupˉelement);
    }
    Surface.append(Tablist, Groups);
    this.#Host.replaceChildren(Surface);
  }
}
