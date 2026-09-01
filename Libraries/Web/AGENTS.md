# Browser library instructions

Follow the repository handbook and the shared
[browser application architecture](../../Documents/Architecture/Browser-Application-Development.md).

- Reusable modules are inert on import and do not register application-global
  state or behavior.
- Components own and dispose their listeners, observers, timers, workers, and
  other resources through an explicit lifecycle scope.
- Use semantic `--wv-*` tokens and `wv.*` cascade layers without depending on
  application selectors.
