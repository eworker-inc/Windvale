export type Disposeˉaction = () => void;

export class Lifecycleˉscope {
  readonly #Disposers: Disposeˉaction[] = [];
  #Disposed = false;

  Own(Disposer: Disposeˉaction): Disposeˉaction {
    if (this.#Disposed) {
      Disposer();
      return Disposer;
    }
    this.#Disposers.push(Disposer);
    return Disposer;
  }

  Ownˉevent(
    Target: EventTarget,
    Type: string,
    Listener: EventListenerOrEventListenerObject,
    Options?: AddEventListenerOptions | boolean
  ): void {
    Target.addEventListener(Type, Listener, Options);
    this.Own(() => Target.removeEventListener(Type, Listener, Options));
  }

  Dispose(): void {
    if (this.#Disposed) {
      return;
    }
    this.#Disposed = true;
    const Failures: unknown[] = [];
    for (let Index = this.#Disposers.length - 1; Index >= 0; Index -= 1) {
      try {
        this.#Disposers[Index]?.();
      } catch (Failure: unknown) {
        Failures.push(Failure);
      }
    }
    this.#Disposers.length = 0;
    if (Failures.length > 0) {
      throw new AggregateError(Failures, 'One or more lifecycle disposers failed.');
    }
  }
}
