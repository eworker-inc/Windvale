# Windvale browser libraries

This tree owns browser-native TypeScript shared by independently deployable
Windvale web applications. It follows the state, component, lifecycle, theme,
localization, and CSS boundaries in the repository handbook.

The library deliberately has no automatic registration, global singleton, WVDB
authority, database protocol, model provider, credential store, router, or
deployment behavior. An application host explicitly composes every feature.
