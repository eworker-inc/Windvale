import { Lifecycleˉscope } from '../../Framework/Lifecycle/Lifecycle-Scope';

export interface Appˉshellˉoptions {
  readonly Leftˉwidth?: number;
  readonly Rightˉwidth?: number;
  readonly Consoleˉheight?: number;
}

export class Appˉshell {
  readonly Element: HTMLElement;
  readonly Frameˉhost: HTMLElement;
  readonly Ribbonˉhost: HTMLElement;
  readonly Explorerˉhost: HTMLElement;
  readonly Workspaceˉhost: HTMLElement;
  readonly Assistantˉhost: HTMLElement;
  readonly Consoleˉhost: HTMLElement;
  readonly Statusˉhost: HTMLElement;

  readonly #Scope = new Lifecycleˉscope();
  readonly #Leftˉresizer: HTMLElement;
  readonly #Rightˉresizer: HTMLElement;
  readonly #Consoleˉresizer: HTMLElement;

  constructor(Mount: HTMLElement, Options: Appˉshellˉoptions = {}) {
    this.Element = document.createElement('section');
    this.Element.className = 'wv-app-shell';
    this.Element.dataset['leftOpen'] = 'true';
    this.Element.dataset['rightOpen'] = 'true';
    this.Element.dataset['consoleOpen'] = 'true';
    this.Element.style.setProperty('--wv-shell-left-width', `${Options.Leftˉwidth ?? 272}px`);
    this.Element.style.setProperty('--wv-shell-right-width', `${Options.Rightˉwidth ?? 348}px`);
    this.Element.style.setProperty('--wv-shell-console-height', `${Options.Consoleˉheight ?? 176}px`);

    this.Frameˉhost = Appˉshell.#Create('header', 'wv-shell-frame');
    this.Ribbonˉhost = Appˉshell.#Create('nav', 'wv-shell-ribbon');
    this.Ribbonˉhost.setAttribute('aria-label', 'Command ribbon');

    const Workbench = Appˉshell.#Create('div', 'wv-shell-workbench');
    this.Explorerˉhost = Appˉshell.#Create('aside', 'wv-shell-explorer');
    this.Explorerˉhost.setAttribute('aria-label', 'Server explorer');
    this.#Leftˉresizer = Appˉshell.#Create('div', 'wv-shell-resizer wv-shell-resizer--vertical');
    this.#Leftˉresizer.setAttribute('role', 'separator');
    this.#Leftˉresizer.setAttribute('aria-orientation', 'vertical');
    this.#Leftˉresizer.tabIndex = 0;
    this.Workspaceˉhost = Appˉshell.#Create('main', 'wv-shell-workspace');
    this.#Rightˉresizer = Appˉshell.#Create('div', 'wv-shell-resizer wv-shell-resizer--vertical');
    this.#Rightˉresizer.setAttribute('role', 'separator');
    this.#Rightˉresizer.setAttribute('aria-orientation', 'vertical');
    this.#Rightˉresizer.tabIndex = 0;
    this.Assistantˉhost = Appˉshell.#Create('aside', 'wv-shell-assistant');
    this.Assistantˉhost.setAttribute('aria-label', 'AI assistant');
    Workbench.append(
      this.Explorerˉhost,
      this.#Leftˉresizer,
      this.Workspaceˉhost,
      this.#Rightˉresizer,
      this.Assistantˉhost
    );

    this.#Consoleˉresizer = Appˉshell.#Create('div', 'wv-shell-resizer wv-shell-resizer--horizontal');
    this.#Consoleˉresizer.setAttribute('role', 'separator');
    this.#Consoleˉresizer.setAttribute('aria-orientation', 'horizontal');
    this.#Consoleˉresizer.tabIndex = 0;
    this.Consoleˉhost = Appˉshell.#Create('section', 'wv-shell-console');
    this.Consoleˉhost.setAttribute('aria-label', 'Console and logs');
    this.Statusˉhost = Appˉshell.#Create('footer', 'wv-shell-status');

    this.Element.append(
      this.Frameˉhost,
      this.Ribbonˉhost,
      Workbench,
      this.#Consoleˉresizer,
      this.Consoleˉhost,
      this.Statusˉhost
    );
    Mount.replaceChildren(this.Element);

    this.#Bindˉhorizontalˉresize(this.#Leftˉresizer, '--wv-shell-left-width', false, 208, 440);
    this.#Bindˉhorizontalˉresize(this.#Rightˉresizer, '--wv-shell-right-width', true, 286, 520);
    this.#Bindˉconsoleˉresize();
  }

  Setˉleftˉopen(Open: boolean): void {
    this.Element.dataset['leftOpen'] = String(Open);
  }

  Setˉrightˉopen(Open: boolean): void {
    this.Element.dataset['rightOpen'] = String(Open);
  }

  Setˉconsoleˉopen(Open: boolean): void {
    this.Element.dataset['consoleOpen'] = String(Open);
  }

  Dispose(): void {
    this.#Scope.Dispose();
    this.Element.remove();
  }

  #Bindˉhorizontalˉresize(
    Handle: HTMLElement,
    Property: string,
    Reverse: boolean,
    Minimum: number,
    Maximum: number
  ): void {
    const Begin = (Event: Event): void => {
      const Pointer = Event as PointerEvent;
      Pointer.preventDefault();
      const Startˉx = Pointer.clientX;
      const Current = Number.parseFloat(getComputedStyle(this.Element).getPropertyValue(Property));
      Handle.setPointerCapture(Pointer.pointerId);
      this.Element.dataset['resizing'] = 'true';

      const Move = (Moveˉevent: PointerEvent): void => {
        const Delta = (Moveˉevent.clientX - Startˉx) * (Reverse ? -1 : 1);
        const Width = Math.min(Maximum, Math.max(Minimum, Current + Delta));
        this.Element.style.setProperty(Property, `${Math.round(Width)}px`);
      };
      const End = (): void => {
        Handle.removeEventListener('pointermove', Move);
        Handle.removeEventListener('pointerup', End);
        Handle.removeEventListener('pointercancel', End);
        delete this.Element.dataset['resizing'];
      };
      Handle.addEventListener('pointermove', Move);
      Handle.addEventListener('pointerup', End);
      Handle.addEventListener('pointercancel', End);
    };
    this.#Scope.Ownˉevent(Handle, 'pointerdown', Begin);
  }

  #Bindˉconsoleˉresize(): void {
    const Begin = (Event: Event): void => {
      const Pointer = Event as PointerEvent;
      Pointer.preventDefault();
      this.#Consoleˉresizer.setPointerCapture(Pointer.pointerId);
      this.Element.dataset['resizing'] = 'true';
      const Move = (Moveˉevent: PointerEvent): void => {
        const Height = Math.min(420, Math.max(92, window.innerHeight - Moveˉevent.clientY - 25));
        this.Element.style.setProperty('--wv-shell-console-height', `${Math.round(Height)}px`);
      };
      const End = (): void => {
        this.#Consoleˉresizer.removeEventListener('pointermove', Move);
        this.#Consoleˉresizer.removeEventListener('pointerup', End);
        this.#Consoleˉresizer.removeEventListener('pointercancel', End);
        delete this.Element.dataset['resizing'];
      };
      this.#Consoleˉresizer.addEventListener('pointermove', Move);
      this.#Consoleˉresizer.addEventListener('pointerup', End);
      this.#Consoleˉresizer.addEventListener('pointercancel', End);
    };
    this.#Scope.Ownˉevent(this.#Consoleˉresizer, 'pointerdown', Begin);
  }

  static #Create(Tag: string, Classˉname: string): HTMLElement {
    const Element = document.createElement(Tag);
    Element.className = Classˉname;
    return Element;
  }
}
