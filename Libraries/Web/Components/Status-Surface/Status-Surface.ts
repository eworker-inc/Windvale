export interface Statusˉsegment {
  readonly Identifier: string;
  readonly Label: string;
  readonly Tone?: 'normal' | 'accent' | 'warning' | 'success';
}

export class Statusˉsurface {
  readonly #Host: HTMLElement;
  readonly #Onˉconsole: () => void;

  constructor(Host: HTMLElement, Onˉconsole: () => void) {
    this.#Host = Host;
    this.#Onˉconsole = Onˉconsole;
  }

  Render(Segments: readonly Statusˉsegment[], Consoleˉlabel: string, Consoleˉopen: boolean): void {
    const Bar = document.createElement('div');
    Bar.className = 'wv-status-surface';
    const Left = document.createElement('div');
    const Right = document.createElement('div');
    for (const Segment of Segments) {
      const Item = document.createElement('span');
      Item.className = 'wv-status-segment';
      if (Segment.Tone !== undefined) {
        Item.dataset['tone'] = Segment.Tone;
      }
      Item.textContent = Segment.Label;
      (Segment.Identifier.startsWith('right.') ? Right : Left).append(Item);
    }
    const Console = document.createElement('button');
    Console.type = 'button';
    Console.className = 'wv-status-console';
    Console.dataset['active'] = String(Consoleˉopen);
    Console.textContent = `▤ ${Consoleˉlabel}`;
    Console.addEventListener('click', this.#Onˉconsole, { once: true });
    Right.prepend(Console);
    Bar.append(Left, Right);
    this.#Host.replaceChildren(Bar);
  }
}
