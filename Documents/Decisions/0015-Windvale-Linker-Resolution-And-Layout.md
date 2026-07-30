# Decision 0015: Windvale linker resolution and layout passes

- Date: 2026-07-30
- Status: Accepted and implemented; cross-host qualification pending

## Context

After WVO validation, the first link-wide work must combine as many as 64 objects without moving symbol or layout policy into C#. Seed still has no general collection or module-import facility. The accepted Windvale Linking 1 contract nevertheless requires deterministic aggregate limits, duplicate-export detection, resolution of every import, a unique exported-function entry, section ordering, actual-address alignment, image limits, and defined-symbol addresses before any relocation or output can occur.

The hosted resource boundary now guarantees that each exact input name returns one immutable first-read snapshot. Canonical WVO ordering gives each object sorted local, export, and import ranges and sorted sections.

## Decision

- Use the accepted full shell argument shape during development: `<base-address> <entry> <output.bin> <input.wvo>...`, with one through 64 ordered inputs. The current analysis slice does not write the output argument.
- Parse the base address as exact invariant decimal `u32` and validate the entry with the WVO machine-name grammar before reading any object.
- Re-read input names through `file.read_bytes` in bounded passes, relying on the resource context's immutable first-read snapshot rather than retaining native handles or delegating decoding.
- Enforce aggregate section, symbol, and relocation limits while validating inputs in semantic input order.
- Detect exports duplicated across objects by merge-walking their canonical sorted export ranges. Resolve each import against the canonical export ranges and require the same symbol kind.
- Select exactly one exported-function entry before layout.
- Place section contributions by kind, input index, and source section index. Compute alignment from `base address + current image offset`, materialize the resulting image length conceptually, and reject image or `u32` address-space overflow.
- Recompute placements while validating every defined-symbol address and derive the entry address from the selected definition.
- Return immutable analysis evidence only. Image construction, relocation, independent reconstruction, canonical map construction, and the one final hosted write remain later passes.

## Consequences

- Resolution and layout semantics now execute in verified Windvale bytecode and can be compared directly with the Stage 0 oracle before byte publication complicates failures.
- Semantic input order is explicit and observable; native directory enumeration, path order, locale, and timestamps remain irrelevant.
- Repeated bounded passes avoid a premature mutable/global collection, but worst-case import lookup and definition placement may exceed practical instruction budgets. Full-linker measurements must either justify a narrow bounded collection/index or demonstrate that the accepted limits remain usable with these passes.
- The alignment implementation uses the low byte or low 16 bits according to the accepted maximum power-of-two alignment, avoiding a new modulo or bitwise primitive solely for this linker slice.
- Aggregate failures retain the contract's no-input sentinel; object, duplicate-export, import, and layout failures retain their deterministic input indices.

## Reconsider when

- Relocation or map construction needs the same resolved records often enough that rescanning dominates execution.
- A shared immutable bounded collection can serve both the assembler and linker with a smaller verified surface than repeated passes.
- A future output target changes ordering or alignment policy; add an explicit target adapter rather than making the flat-image rules ambient.
