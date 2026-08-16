export interface Themeˉdefinition {
  readonly Identifier: string;
  readonly Colorˉscheme: 'light' | 'dark';
}

export class Themeˉregistry {
  readonly #Themes = new Map<string, Themeˉdefinition>();

  Register(Theme: Themeˉdefinition): void {
    if (this.#Themes.has(Theme.Identifier)) {
      throw new Error(`Duplicate theme: ${Theme.Identifier}`);
    }
    this.#Themes.set(Theme.Identifier, Object.freeze({ ...Theme }));
  }

  Apply(Root: HTMLElement, Identifier: string): void {
    const Theme = this.#Themes.get(Identifier);
    if (Theme === undefined) {
      throw new Error(`Unknown theme: ${Identifier}`);
    }
    Root.dataset['theme'] = Theme.Identifier;
    Root.style.colorScheme = Theme.Colorˉscheme;
  }
}
