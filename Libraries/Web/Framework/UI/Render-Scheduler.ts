export interface Renderˉboundary<Change> {
  readonly Identifier: string;
  Matches(Change: Change): boolean;
  Render(): void;
}

export class Renderˉscheduler<Change> {
  readonly #Boundaries = new Map<string, Renderˉboundary<Change>>();
  readonly #Pending: Change[] = [];
  readonly #Onˉfailure: (Identifier: string, Failure: unknown) => void;
  #Scheduled = false;

  constructor(Onˉfailure: (Identifier: string, Failure: unknown) => void) {
    this.#Onˉfailure = Onˉfailure;
  }

  Register(Boundary: Renderˉboundary<Change>): () => void {
    if (this.#Boundaries.has(Boundary.Identifier)) {
      throw new Error(`Duplicate render boundary: ${Boundary.Identifier}`);
    }
    this.#Boundaries.set(Boundary.Identifier, Boundary);
    return () => this.#Boundaries.delete(Boundary.Identifier);
  }

  Notify(Changes: readonly Change[]): void {
    this.#Pending.push(...Changes);
    if (this.#Scheduled) {
      return;
    }
    this.#Scheduled = true;
    queueMicrotask(() => this.#Flush());
  }

  #Flush(): void {
    this.#Scheduled = false;
    const Changes = this.#Pending.splice(0);
    for (const Boundary of this.#Boundaries.values()) {
      if (!Changes.some((Change) => Boundary.Matches(Change))) {
        continue;
      }
      try {
        Boundary.Render();
      } catch (Failure: unknown) {
        this.#Onˉfailure(Boundary.Identifier, Failure);
      }
    }
  }
}
