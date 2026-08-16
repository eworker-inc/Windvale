export interface Stateˉtransition<State, Change> {
  readonly Nextˉstate: State;
  readonly Changes: readonly Change[];
}

export type Stateˉsubscriber<State, Change> = (
  Snapshot: State,
  Changes: readonly Change[]
) => void;

export class Stateˉowner<State extends object, Change> {
  #Snapshot: State;
  readonly #Subscribers = new Set<Stateˉsubscriber<State, Change>>();

  constructor(Initialˉstate: State) {
    this.#Snapshot = Object.freeze(Initialˉstate);
  }

  Read(): State {
    return this.#Snapshot;
  }

  Subscribe(Subscriber: Stateˉsubscriber<State, Change>): () => void {
    this.#Subscribers.add(Subscriber);
    return () => this.#Subscribers.delete(Subscriber);
  }

  Update(Transition: (Current: State) => Stateˉtransition<State, Change>): void {
    const Result = Transition(this.#Snapshot);
    if (Result.Changes.length === 0 || Result.Nextˉstate === this.#Snapshot) {
      return;
    }

    this.#Snapshot = Object.freeze(Result.Nextˉstate);
    const Changes = Object.freeze([...Result.Changes]);
    for (const Subscriber of [...this.#Subscribers]) {
      Subscriber(this.#Snapshot, Changes);
    }
  }
}
