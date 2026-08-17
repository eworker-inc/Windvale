# Workload 6 package and resource plan

## Mapping

Package identity: `windvale.paper.concurrent_hosted_service.v1`.

| File | Module | Profile | Authority |
| --- | --- | --- | --- |
| `Source/Concurrent-Service-Types.wv` | `Concurrentˉserviceˉtypes` | Hosted | library |
| `Source/Concurrent-Service-Policy.wv` | `Concurrentˉserviceˉpolicy` | Hosted | library |
| `Source/Concurrent-Service-Application.wv` | `Concurrentˉserviceˉapplication` | Hosted | application |

Platforms are Windows, Linux, and Windvale. The sole capability requirement is
`network.service.accept` version 1. The application statically imports the
reviewed workload-5 HTTP modules; it does not duplicate their parser or
response code.

## Reference limits

| Limit | Reference value | Hard paper ceiling |
| --- | ---: | ---: |
| accepted children | 5 | 64 |
| runnable children | 4 | 64 |
| retained completions | 5 | 64 |
| scope retained bytes | 262,144 | 262,144 |
| scope work units | 4,096 | 65,536 |
| task call depth | 32 | 64 |
| timers | 1 | 8 |
| retained diagnostics | 16 | 64 |
| memory per HTTP child | 131,072 | 131,072 |

The launcher supplies a root memory budget of at least 917,504 bytes with six
child splits: 262,144 scope bytes and five 131,072-byte handler budgets. A
failed split cannot consume a later child budget. Each accepted spawn moves one
complete handler budget into that child. Rejection returns the exact closure and
moved budget before the application maps and releases it.

At most four handlers run concurrently. The fifth budget remains local until a
single refresh is justified; otherwise it is released unused. Maximum live
application-accounted memory is bounded by the root, not multiplied by host
threads.

## Effect closure

The application entry closes over:

```text
memory.allocate
network.service.accept
resource.acquire
resource.release
task.cancel
task.spawn
task.suspend
```

It has no filesystem, DNS, raw socket, TLS, clock construction, entropy,
process, terminal, thread, unsafe, reflection, detach, or dynamic capability
effect. The operation context conveys deadline/cancellation observations without
granting ambient time authority.

## Artifact plan

All source lowers through ordinary records, closed variants, explicit closures,
task handles, results, borrows, and capability calls. Async/task lowering needs
a verified continuation representation and bounded scheduler state, but no HTTP
or restart-specific opcode. Current Seed cannot parse the edition-1 surface, so
source/WIR/WVB/native sizes, compile time, execution time, and peak working set
remain unmeasured until implementation.

There is no package data, schema, generated route table, certificate, key,
installer payload, or shipped content object in this workload.

The reviewed bundle contains 3 source files, 849 LF-terminated source lines,
17 top-level declarations, and 28,303 UTF-8 bytes. These source measurements are
reproducible repository facts, not compiler or runtime measurements.
