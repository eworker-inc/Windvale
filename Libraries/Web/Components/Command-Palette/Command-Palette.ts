export interface Commandˉpaletteˉitem {
  readonly Identifier: string;
  readonly Label: string;
  readonly Detail: string;
  readonly Shortcut?: string;
  readonly Glyph: string;
  readonly Enabled: boolean;
}

export interface Commandˉpaletteˉcopy {
  readonly Title: string;
  readonly Placeholder: string;
  readonly Empty: string;
  readonly Close: string;
}

export class Commandˉpalette {
  readonly #Host: HTMLElement;
  readonly #Run: (Identifier: string) => void;
  readonly #Close: () => void;

  constructor(Host: HTMLElement, Run: (Identifier: string) => void, Close: () => void) {
    this.#Host = Host;
    this.#Run = Run;
    this.#Close = Close;
  }

  Render(Open: boolean, Copy: Commandˉpaletteˉcopy, Commands: readonly Commandˉpaletteˉitem[]): void {
    if (!Open) {
      this.#Host.replaceChildren();
      return;
    }
    const Backdrop = document.createElement('div');
    Backdrop.className = 'wv-command-palette-backdrop';
    const Palette = document.createElement('section');
    Palette.className = 'wv-command-palette';
    Palette.setAttribute('role', 'dialog');
    Palette.setAttribute('aria-modal', 'true');
    Palette.setAttribute('aria-label', Copy.Title);
    const Search = document.createElement('input');
    Search.type = 'search';
    Search.maxLength = 128;
    Search.placeholder = Copy.Placeholder;
    Search.setAttribute('aria-label', Copy.Placeholder);
    const Results = document.createElement('div');
    Results.className = 'wv-command-palette-results';
    const Renderˉresults = (): void => {
      const Query = Search.value.trim().toLocaleLowerCase();
      const Matches = Commands.filter((Command) =>
        `${Command.Label} ${Command.Detail}`.toLocaleLowerCase().includes(Query)
      ).slice(0, 24);
      Results.replaceChildren();
      if (Matches.length === 0) {
        const Empty = document.createElement('p');
        Empty.className = 'wv-command-palette-empty';
        Empty.textContent = Copy.Empty;
        Results.append(Empty);
        return;
      }
      for (const Command of Matches) {
        const Button = document.createElement('button');
        Button.type = 'button';
        Button.disabled = !Command.Enabled;
        Button.dataset['command'] = Command.Identifier;
        const Glyph = document.createElement('span');
        Glyph.className = 'wv-command-palette-glyph';
        Glyph.textContent = Command.Glyph;
        const Copyˉhost = document.createElement('span');
        Copyˉhost.innerHTML = `<strong></strong><small></small>`;
        Copyˉhost.querySelector('strong')!.textContent = Command.Label;
        Copyˉhost.querySelector('small')!.textContent = Command.Detail;
        const Shortcut = document.createElement('kbd');
        Shortcut.textContent = Command.Shortcut ?? '';
        Button.append(Glyph, Copyˉhost, Shortcut);
        Button.addEventListener('click', () => {
          this.#Run(Command.Identifier);
          this.#Close();
        });
        Results.append(Button);
      }
    };
    Search.addEventListener('input', Renderˉresults);
    Search.addEventListener('keydown', (Event) => {
      if (Event.key === 'Escape') {
        Event.preventDefault();
        this.#Close();
      } else if (Event.key === 'Enter') {
        const First = Results.querySelector<HTMLButtonElement>('button:not(:disabled)');
        First?.click();
      }
    });
    Backdrop.addEventListener('pointerdown', (Event) => {
      if (Event.target === Backdrop) this.#Close();
    });
    Palette.append(Search, Results);
    Backdrop.append(Palette);
    this.#Host.replaceChildren(Backdrop);
    Renderˉresults();
    requestAnimationFrame(() => Search.focus());
  }
}
