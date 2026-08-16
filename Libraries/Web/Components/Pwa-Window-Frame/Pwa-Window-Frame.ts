import { Lifecycleˉscope } from '../../Framework/Lifecycle/Lifecycle-Scope';

export interface Pwaˉwindowˉframeˉcopy {
  readonly Product: string;
  readonly Context: string;
  readonly Preview: string;
  readonly Search: string;
  readonly Explorer: string;
  readonly Assistant: string;
  readonly Theme: string;
  readonly Locale: string;
  readonly Install: string;
}

export interface Pwaˉwindowˉframeˉactions {
  Toggleˉexplorer(): void;
  Toggleˉassistant(): void;
  Toggleˉtheme(): void;
  Toggleˉlocale(): void;
  Install(): void;
}

export class Pwaˉwindowˉframe {
  readonly #Host: HTMLElement;
  readonly #Actions: Pwaˉwindowˉframeˉactions;
  readonly #Scope = new Lifecycleˉscope();
  readonly #Product: HTMLElement;
  readonly #Context: HTMLElement;
  readonly #Preview: HTMLElement;
  readonly #Search: HTMLButtonElement;
  readonly #Explorer: HTMLButtonElement;
  readonly #Assistant: HTMLButtonElement;
  readonly #Theme: HTMLButtonElement;
  readonly #Locale: HTMLButtonElement;
  readonly #Install: HTMLButtonElement;

  constructor(Host: HTMLElement, Actions: Pwaˉwindowˉframeˉactions) {
    this.#Host = Host;
    this.#Actions = Actions;
    const Bar = document.createElement('div');
    Bar.className = 'wv-pwa-frame';
    const Brand = document.createElement('div');
    Brand.className = 'wv-pwa-brand';
    Brand.innerHTML = '<span class="wv-pwa-mark" aria-hidden="true"><i></i><b>WV</b></span>';
    const Titles = document.createElement('div');
    Titles.className = 'wv-pwa-titles';
    this.#Product = document.createElement('strong');
    this.#Context = document.createElement('span');
    Titles.append(this.#Product, this.#Context);
    Brand.append(Titles);

    this.#Preview = document.createElement('span');
    this.#Preview.className = 'wv-pwa-preview';
    this.#Search = Pwaˉwindowˉframe.#Button('wv-pwa-search');
    this.#Explorer = Pwaˉwindowˉframe.#Button('wv-icon-button');
    this.#Assistant = Pwaˉwindowˉframe.#Button('wv-icon-button');
    this.#Theme = Pwaˉwindowˉframe.#Button('wv-icon-button');
    this.#Locale = Pwaˉwindowˉframe.#Button('wv-icon-button wv-locale-button');
    this.#Install = Pwaˉwindowˉframe.#Button('wv-install-button');
    this.#Install.hidden = true;
    const Actionsˉhost = document.createElement('div');
    Actionsˉhost.className = 'wv-pwa-actions';
    Actionsˉhost.append(this.#Explorer, this.#Assistant, this.#Theme, this.#Locale, this.#Install);
    Bar.append(Brand, this.#Preview, this.#Search, Actionsˉhost);
    Host.replaceChildren(Bar);

    this.#Scope.Ownˉevent(this.#Explorer, 'click', () => this.#Actions.Toggleˉexplorer());
    this.#Scope.Ownˉevent(this.#Assistant, 'click', () => this.#Actions.Toggleˉassistant());
    this.#Scope.Ownˉevent(this.#Theme, 'click', () => this.#Actions.Toggleˉtheme());
    this.#Scope.Ownˉevent(this.#Locale, 'click', () => this.#Actions.Toggleˉlocale());
    this.#Scope.Ownˉevent(this.#Install, 'click', () => this.#Actions.Install());
  }

  Render(Copy: Pwaˉwindowˉframeˉcopy, Theme: string, Locale: string): void {
    this.#Product.textContent = Copy.Product;
    this.#Context.textContent = Copy.Context;
    this.#Preview.textContent = Copy.Preview;
    this.#Search.textContent = `⌕  ${Copy.Search}`;
    this.#Explorer.textContent = '☷';
    this.#Explorer.title = Copy.Explorer;
    this.#Explorer.setAttribute('aria-label', Copy.Explorer);
    this.#Assistant.textContent = 'AI';
    this.#Assistant.title = Copy.Assistant;
    this.#Assistant.setAttribute('aria-label', Copy.Assistant);
    this.#Theme.textContent = Theme === 'dark' ? '☀' : '◐';
    this.#Theme.title = Copy.Theme;
    this.#Theme.setAttribute('aria-label', Copy.Theme);
    this.#Locale.textContent = Locale.toUpperCase();
    this.#Locale.title = Copy.Locale;
    this.#Locale.setAttribute('aria-label', Copy.Locale);
    this.#Install.textContent = Copy.Install;
  }

  Setˉinstallˉavailable(Available: boolean): void {
    this.#Install.hidden = !Available;
  }

  Dispose(): void {
    this.#Scope.Dispose();
    this.#Host.replaceChildren();
  }

  static #Button(Classˉname: string): HTMLButtonElement {
    const Button = document.createElement('button');
    Button.type = 'button';
    Button.className = Classˉname;
    return Button;
  }
}
