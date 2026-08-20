export interface Editorˉtoolbarˉitem {
  readonly Identifier: string;
  readonly Label: string;
  readonly Glyph: string;
  readonly Enabled: boolean;
  readonly Primary?: boolean;
  readonly Separatorˉbefore?: boolean;
}

export class Editorˉtoolbar {
  readonly #Host: HTMLElement;
  readonly #Run: (Identifier: string) => void;

  constructor(Host: HTMLElement, Run: (Identifier: string) => void) {
    this.#Host = Host;
    this.#Run = Run;
  }

  Render(Items: readonly Editorˉtoolbarˉitem[], Context: string): void {
    const Toolbar = document.createElement('div');
    Toolbar.className = 'wv-editor-toolbar';
    Toolbar.setAttribute('role', 'toolbar');
    for (const Item of Items) {
      if (Item.Separatorˉbefore) {
        const Separator = document.createElement('span');
        Separator.className = 'wv-editor-toolbar-separator';
        Separator.setAttribute('role', 'separator');
        Toolbar.append(Separator);
      }
      const Button = document.createElement('button');
      Button.type = 'button';
      Button.disabled = !Item.Enabled;
      Button.dataset['primary'] = String(Item.Primary === true);
      Button.title = Item.Label;
      Button.innerHTML = '<span aria-hidden="true"></span><span></span>';
      Button.children[0]!.textContent = Item.Glyph;
      Button.children[1]!.textContent = Item.Label;
      Button.addEventListener('click', () => this.#Run(Item.Identifier));
      Toolbar.append(Button);
    }
    const Contextˉhost = document.createElement('span');
    Contextˉhost.className = 'wv-editor-toolbar-context';
    Contextˉhost.textContent = Context;
    Toolbar.append(Contextˉhost);
    this.#Host.replaceChildren(Toolbar);
  }
}
