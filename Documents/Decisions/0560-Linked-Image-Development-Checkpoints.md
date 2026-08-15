# Decision 0560: Linked-image development checkpoints

- Status: Implemented
- Date: 2026-08-14
- Extends: Decisions 0553 through 0555, 0557, and 0559
- Scope: repeated affected-owner feedback and ordinary dual-host development

## Context

Decision 0559 reduced the repeated eight-case database owner from 402,638 ms to
135,670 ms but left it 15,670 ms above the roadmap's two-minute target. An
isolated depth-three measurement attributed 12,556 ms to deterministic flat
linking, while execution itself took 884 ms. Repeating the same link for six
unchanged portable targets was construction overhead rather than new behavioral
evidence.

GitHub development runners are ephemeral. Repository-external checkpoints
therefore began cold on every ordinary push even though each immutable entry
already had a complete identity and hit-validation contract. Reusing those
entries across development runs requires an external transport boundary that
cannot leak into qualification.

The three measured expensive owners also described their dependencies in
separate scripts, manifests, and prose. Milestone 1 requires one machine-checked
source, producer, and artifact declaration for each owner without replacing the
changed-file planner's fail-closed mapping.

## Decision

- Add host-scoped `linked-image-v1` checkpoints for the six portable database
  development targets. The key binds the format, namespace, host, base address,
  entry symbol, exact WVO, exact link front door, and exact native linker.
- Store the flat image, exact link map, parsed entry offset, sizes, and SHA-256
  identities in one immutable entry. Every hit rejects links, reconstructs the
  complete record, materializes fresh copies, and compares both products byte
  for byte. Every database behavior still executes.
- Accept Windows 8.3 aliases only for generated temporary WVB/WVO inputs. Reject
  linked final files, resolve the alias, and hash the canonical target.
  Repository-owned producers retain their canonical-path requirement.
- Restore and save the external checkpoint root only in the two ordinary
  development jobs. Pin `actions/cache` to exact commit
  `27d5ce7f107fe9357f9df03efb73ab90386fccae` (`v5.0.5`), isolate the root under
  `runner.temp`, use a versioned host-specific prefix, and give every run
  attempt a new immutable key. Qualification jobs must contain neither the
  cache action nor `WINDVALE_NATIVE_CACHE_ROOT`.
- Add `Tests/Native/Development-Owner-Dependencies.txt` as the canonical
  dependency declaration for `seed-native-front-door`, `webassembly-engine`,
  and `database-storage`. Verify LF/UTF-8 form, ordinal order, uniqueness,
  ordinary repository paths, all three closure kinds, all five database
  checkpoint families, zero planner gaps, and selection of every declared
  owner.
- Preserve live phase and target reports. A cache hit changes construction cost,
  not the selected behavioral or admission result.

GitHub documents that caches are immutable after creation and that restore-key
prefixes can select a prior matching cache. The exact action commit and the
development-only trust boundary are therefore reviewed workflow inputs, not an
ambient convenience dependency. See the
[dependency-caching reference](https://docs.github.com/en/actions/reference/workflows-and-actions/dependency-caching)
and the [pinned cache action](https://github.com/actions/cache/tree/27d5ce7f107fe9357f9df03efb73ab90386fccae).

## Evidence

The six-target cache-population Windows run passed all eight behaviors in
130,240 ms. The immediate all-hit run passed in 87,800 ms:

| Phase | Elapsed |
| --- | ---: |
| Tool preparation | 9,190 ms |
| Six portable targets | 24,290 ms |
| Host storage | 28,570 ms |
| Host tree reader | 25,660 ms |
| Total | 87,800 ms |

All six project, link, and current-host application reports were `Hit`; the two
host projects and applications were also `Hit`. All eight behaviors executed.
Compared with 402,638 ms, the direct owner is 78.19% faster and has 32,200 ms of
margin below two minutes.

A coherent changed-file gate on commit `6a4e2504` passed 79 native planner
contracts, the GitHub workflow policy, and the eight-case database owner. The
owner reported 89,530 ms. Focused Windows tests also proved that hosted and
linked cache keys accept an ordinary generated input through an 8.3 alias while
continuing to hash its canonical target.

GitHub Verify run
[31852544894](https://github.com/eworker-inc/Windvale/actions/runs/31852544894)
first populated both cache directories. Its cold development jobs passed in
9m33s on Windows and 8m00s on Linux. Exact
[attempt 2](https://github.com/eworker-inc/Windvale/actions/runs/31852544894/attempts/2)
restored the attempt-1 keys and passed in 1m42s on Windows and 1m15s on Linux
end to end. The affected-owner steps took 66 and 49 seconds. The database owner
reported 57,870 ms on Windows and 43,000 ms on Linux; every tool, project, link,
and current-host application checkpoint was `Hit`, and all eight behaviors
passed. The aggregate verification gate passed. The complete qualification,
WebAssembly, bootstrap, website, and lightweight jobs were correctly skipped by
the selected development scope.

## Consequences

- Repeated affected-owner work now fits inside the two-minute local target with
  the same eight behaviors and validation boundaries.
- Ordinary GitHub development runs can reuse validated immutable construction
  products across ephemeral runners. Cache corruption fails the owner; it does
  not trigger implicit repair inside an existing entry.
- The three measured expensive owners and five database checkpoint families now
  have one canonical machine-checked dependency declaration.
- Milestone 1's five completion gates are satisfied. Milestone 2 becomes the
  active product milestone without spending a full qualification run.
- Complete qualification remains cold and explicit. No release, promotion,
  bootstrap, security, ABI, or conformance evidence is inferred from a cached
  development run.

## Reconsideration triggers

- Repeated dual-host development feedback exceeds the five-minute working
  target under representative affected-owner changes.
- Cache transport approaches its service quota or restore behavior becomes
  nondeterministic enough to obscure useful feedback.
- A checkpoint key ceases to bind a complete producer identity or a hit ceases
  to revalidate and rerun selected behavior.
- Another expensive owner cannot express an honest source, producer, and
  artifact closure through the same declaration contract.
