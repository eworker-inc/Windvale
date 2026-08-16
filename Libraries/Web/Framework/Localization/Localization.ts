export interface Localizationˉpack {
  readonly Locale: string;
  readonly Scope: string;
  readonly Messages: Readonly<Record<string, string>>;
}

export class Localizationˉregistry {
  readonly #Fallbackˉlocale: string;
  readonly #Packs = new Map<string, Readonly<Record<string, string>>>();
  #Locale: string;

  constructor(Fallbackˉlocale: string, Initialˉlocale: string) {
    this.#Fallbackˉlocale = Fallbackˉlocale;
    this.#Locale = Initialˉlocale;
  }

  Register(Pack: Localizationˉpack): void {
    const Key = `${Pack.Locale}:${Pack.Scope}`;
    if (this.#Packs.has(Key)) {
      throw new Error(`Duplicate localization pack: ${Key}`);
    }
    this.#Packs.set(Key, Object.freeze({ ...Pack.Messages }));
  }

  Setˉlocale(Locale: string): void {
    this.#Locale = Locale;
  }

  Text(
    Scope: string,
    Identifier: string,
    Values: Readonly<Record<string, string | number>> = {}
  ): string {
    const Active = this.#Packs.get(`${this.#Locale}:${Scope}`)?.[Identifier];
    const Fallback = this.#Packs.get(`${this.#Fallbackˉlocale}:${Scope}`)?.[Identifier];
    const Template = Active ?? Fallback ?? `[${Scope}.${Identifier}]`;
    return Template.replace(/\{([A-Za-z0-9_]+)\}/g, (Match, Name: string) => {
      const Value = Values[Name];
      return Value === undefined ? Match : String(Value);
    });
  }
}
