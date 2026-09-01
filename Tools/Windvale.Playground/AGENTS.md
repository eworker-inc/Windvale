# Playground instructions

Follow the repository handbook and the shared
[browser application architecture](../../Documents/Architecture/Browser-Application-Development.md).

- Keep compilation and execution in bounded, disposable workers with explicit
  cancellation and cleanup.
- Validate every worker message and generated artifact before use.
- Preserve the pinned WebAssembly engine and execution ABI where the playground
  specification requires them; browser convenience must not redefine Windvale
  semantics.
