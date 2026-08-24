# Verified AI workloads and agent-aware inference proposal

> Status: Exploratory product and systems proposal, first recorded 2026-08-22
> and updated with agent-aware inference evidence on 2026-08-24. This document
> records positioning, dated external evidence, candidate responsibility
> boundaries, and experiments. It is not an accepted architecture, normative
> capability or serialized-format contract, implementation plan, provider
> support claim, performance claim, or statement that the proposed behavior
> exists. A dated decision is required before any candidate contract becomes
> durable direction.

## Purpose

Windvale should investigate a role as the verified execution and residency
layer for AI services and agents. The working product proposition is:

> **Package an AI service once. Run it through a remote model, a local CPU, or
> an admitted GPU provider. Windvale verifies the code, binds only approved data
> and tools, enforces resource limits, and records exactly what ran and what
> effects occurred.**

This document calls such a package and admitted execution a **verified AI
workload**. Verification in this proposal means that Windvale validates the
parts it owns: package identity, code and metadata, capability closure, input
and output bounds, selected provider evidence, resource admission, and effects.
It does not mean that Windvale can independently prove every computation inside
an opaque remote provider or unmeasured accelerator.

The investigation asks whether Windvale can add value above established model
engines by combining four properties:

1. one portable workload identity across local and remote cognitive providers;
2. explicit authority over data, tools, persistence, processors, and effects;
3. agent-aware placement and inference-state residency hints; and
4. durable evidence that remains independent of one model session or vendor.

The proposal complements the existing
[external model provider library](Windvale-External-Model-Provider-Library-Proposal.md),
[accelerator compute design](Windvale-Accelerator-Compute-And-AI-Design.md), and
[agent runtime architecture](../Architecture/Agent-Runtime-And-Digital-Subconscious.md).
It does not replace or silently extend any of them.

## Product outcome under investigation

A future Windvale host could admit one AI service package, then select among
rights-limited processors for each cognitive operation:

- ordinary server CPU for coordination, deterministic tools, preprocessing,
  retrieval, validation, small-model work, and durable state;
- an admitted local GPU provider for latency-sensitive or private inference,
  embedding, reranking, vision, or other supported acceleration;
- a local CPU model provider when no suitable GPU is present or the workload is
  small enough for a CPU latency budget; and
- an admitted remote model provider for frontier quality, specialized
  modalities, very large context, or visible fallback.

The same agent need not run wholly in any one location. Its durable identity,
goals, authority, evidence, and effects remain Windvale-owned while replaceable
model providers perform bounded cognitive operations. A local provider can use
llama.cpp, Ollama, vLLM, or another engine without making that engine's API,
model format, cache layout, or device policy Windvale semantics.

The product is not merely a router. The proposed differentiator is a verified
workload envelope whose placement decisions are informed by the agent's
authorized purpose and lifecycle, whose tool effects remain separate from model
output, and whose evidence states what Windvale actually knows.

## Problem with current agent execution

Many current agents combine a coordinator, a model API, a tool loop, retrieval,
and ad hoc persistence. These pieces often have different owners and failure
models:

- the model provider sees prompts but may not know why or when the agent will
  resume;
- the inference engine sees requests and cache pressure but not the agent's
  pending tool, wake condition, deadline, or durable intention;
- the tool runner may receive broader host authority than the model operation
  needs;
- conversation messages, provider continuation handles, hidden reasoning, and
  physical inference caches may be treated as one kind of state even though
  their portability and authority differ; and
- logs may record outputs without proving the admitted code, provider
  generation, bound data, resource limits, or external effects.

An inference server can observe that a request is idle. An agent-aware Windvale
coordinator may additionally know that the agent is waiting for a compiler,
database query, human approval, deadline, or provider recovery and can estimate
when the same cognitive context is likely to be useful again. The central
hypothesis is that this semantic information can improve placement and
residency without putting model policy in the kernel or exposing provider cache
formats to portable code.

## Current landscape and strategic boundary

The following snapshot is evidence reviewed through 2026-08-24. It is deliberately
qualitative. Repository direction must not depend on mutable star counts,
subjective maturity scores, or undocumented implementation details.

| Category | Representative systems | Current strength relevant to Windvale | Strategic consequence |
| --- | --- | --- | --- |
| Local inference engines | [llama.cpp](https://github.com/ggml-org/llama.cpp) | Model loading, quantized CPU/GPU inference, broad hardware support, prompt caching, and an embeddable/server execution base. | Use as an admitted provider before considering replacement of measured components. Do not redefine GGUF or its cache layout as Windvale semantics. |
| Local model service | [Ollama](https://docs.ollama.com/faq) | Operational model distribution and service, CPU/GPU placement, model retention controls, concurrency, and configurable KV-cache quantization. | Treat it as a convenient provider option, not as the durable agent or capability boundary. |
| High-throughput serving | [vLLM](https://docs.vllm.ai/en/latest/) | Continuous serving, batching, prefix caching, and provider-managed KV offload to CPU and secondary tiers. | Reuse its scheduling and cache machinery; test whether agent lifecycle hints improve results above its native policies. |
| Agent-aware inference orchestration | [NVIDIA Dynamo agentic inference](https://docs.nvidia.com/dynamo/dev/digest/agentic-inference) and the experimental [ThunderAgent program scheduler](https://docs.dynamo.nvidia.com/dynamo/dev/agents/thunder-agent-program-scheduler) | KV-aware placement, agent hints, priority scheduling, speculative prefill, cache-lifecycle controls, and program-level tool-boundary pause/resume over engines including vLLM, SGLang, and TensorRT-LLM. | Treat agent-aware scheduling as an active upstream field rather than a Windvale novelty claim. Prefer a rights-limited integration and evidence mapping before creating a competing scheduler. |
| Remote model APIs | OpenAI, Anthropic, Google, and others | Frontier models, specialized modalities, hosted scaling, and vendor-specific reasoning or continuation controls. | Keep exact adapters behind the existing provider-neutral model boundary; expose uncertainty and placement changes rather than claiming identical semantics. |
| Agent frameworks | Model- and application-specific coordinators | Rapid tool-loop composition, retrieval, memory integrations, and application-facing SDKs. | Compete on verified execution, authority, portability, and evidence rather than on the number of convenience integrations. |
| Generic workload isolation | Processes, containers, VMs, and restricted runtimes | Mature CPU, memory, network, filesystem, and lifecycle isolation. | Reuse host mechanisms while giving AI operations more exact semantic capabilities and evidence. |

The conclusion is not that Windvale should become another inference engine.
Established engines already solve difficult model loading, tensor-kernel,
batching, hardware, and cache problems. Windvale should first make them
replaceable, rights-limited providers. Selective native replacement becomes
rational only when measurement identifies a missing semantic guarantee,
portability boundary, safety property, or material performance opportunity.

NVIDIA Dynamo demonstrates that an agent harness can already communicate
lifecycle and priority information to an inference orchestrator, and that an
orchestrator can combine it with KV-aware routing and provider-owned cache
policy. This strengthens the case for an agent/inference seam while narrowing
Windvale's novelty claim. Windvale should differentiate through durable agent
identity, exact authority, provider-neutral placement, reproducible workload
identity, and execution/effect evidence—not by claiming to have originated
agent-aware cache scheduling.

### Provider interoperability

The Windvale agent should work with existing inference infrastructure rather
than require a Windvale-native inference engine. NVIDIA Dynamo, vLLM, SGLang,
TensorRT-LLM, llama.cpp, Ollama, and future systems are eligible provider or
orchestration mechanisms when an adapter can preserve the required bounds,
identity, isolation, cancellation, teardown, and evidence semantics.

For NVIDIA infrastructure, a candidate integration would map a Windvale
cognitive-operation envelope into the supported Dynamo request extensions and
provider profiles, then translate observable route, model, cache, resource,
completion, and failure results back into Windvale evidence. Provider-specific
session, KV, routing, and device state remains outside portable agent identity.
An application may deliberately select an NVIDIA-scoped optimized profile
without making that profile a portable requirement.

## Proposed differentiation

### Verified workload package

One package identifies the application code, immutable resources, declared
processor requirements, capability requirements, resource maxima, and expected
evidence. Launch approval selects the exact transitive authority and binds
rights-limited providers. A library requirement is not a grant.

The package may permit several placement classes without requiring all of them.
For example, it may approve a private local provider and one remote provider for
non-sensitive tasks while denying prompt persistence and filesystem tools. The
launcher rejects an execution when no admitted placement can preserve the
requested requirements.

### Hybrid but continuous agent

The CPU-hosted coordinator owns durable run state and invokes model processors
as replaceable services. A tool wait does not make the provider session the
agent's memory, and replacing a local model with a remote one does not replace
the agent's identity or authority.

Placement changes remain visible. A fallback from local GPU to remote inference
can change privacy, cost, latency, quality, retention, and evidence. Windvale
must record and, where policy requires, approve that change rather than hide it
inside an adapter.

### Agent-aware inference residency

The coordinator may send a bounded, advisory **resumption forecast** with a
cognitive operation. Candidate information includes:

- expected next-use window and confidence class;
- maximum acceptable resume latency;
- operation priority and deadline;
- maximum retained bytes and retention duration;
- whether recomputation is permitted;
- whether secondary-storage persistence is permitted;
- sensitivity and isolation domain; and
- the provider and model identities to which reuse must remain bound.

The forecast is not an allocation request, truth claim, or promise that the
agent will resume. The provider decides whether to keep state on GPU, move it to
host memory, retain an admitted provider-specific artifact, discard it, or
recompute it later. The provider reports the actual disposition at the evidence
level it supports.

Existing systems now demonstrate that an agent harness can expose lifecycle
information unavailable to a request-local cache policy. Windvale's narrower
technical hypothesis is that its durable intentions, wake conditions, tool
boundaries, authority, sensitivity, and evidence model can map into one or more
provider hint mechanisms and produce measurable value without becoming vendor
semantics. The Windvale-specific claim remains unproven until a representative
workload beats or materially improves an upstream baseline under bounded memory
without weakening correctness, isolation, portability, or teardown.

### Exact evidence rather than an oversized claim

Windvale should distinguish evidence classes rather than state that every AI
operation was independently verified:

| Evidence class | Meaning |
| --- | --- |
| Windvale-validated | Windvale independently validated its package, request, bounds, authority, provider generation, response envelope, and effect record. |
| Provider-reported | The provider reported a model identity, placement, usage, cache result, or completion that Windvale could validate structurally but not reproduce independently. |
| Attested | A separately accepted attestation contract binds measured provider or device evidence to the execution. This proposal accepts no such contract. |
| Reproduced | An admitted independent execution produced results under a specified comparison contract. Reproduction is evidence, not automatic proof of identical hidden computation. |

Remote-provider model identity, usage, reasoning-token counts, cache behavior,
and hidden computation normally remain provider-reported unless a stronger
future contract proves otherwise.

## Responsibility boundaries

### Agent coordinator and workload runtime

The agent coordinator owns:

- durable agent, episode, wake, and cognitive-operation identity;
- goals, policy, context compilation, and model/processor placement policy;
- capability selection, budgets, deadlines, and visible fallback;
- expected next wake and resumption forecasts;
- validation of provider results before they become agent evidence; and
- tool proposals, approvals, execution requests, and effect records.

It must not own provider credentials, vendor payloads, tokenizer internals,
physical KV layouts, device handles, ambient tool authority, or hidden
provider reasoning.

### Local model provider service

An isolated local model provider owns:

- exact engine, model, tokenizer, chat template, adapter, and generation;
- batching, tokenization, model loading, and processor selection within its
  admitted binding;
- provider-specific weight, activation, prefix, and KV-cache representations;
- GPU, host-memory, and optional secondary-tier placement;
- cache promotion, eviction, quantization, compression, and recomputation;
- bounded cancellation, teardown, and provider-loss behavior; and
- provider evidence about actual resource use and cache outcomes.

The provider cannot silently broaden data access, gain agent tools, choose an
unapproved remote service, persist state where retention is denied, or publish
model output as an authorized action.

### Remote model adapter

The existing provider-neutral model boundary continues to own canonical
requests and results. A remote adapter owns vendor endpoint mapping,
authentication, bounded payload translation, provider-specific controls, and
response validation. It does not own durable agent state, routing policy, tool
authority, fallback, or an unrestricted model catalog.

### Windvale runtime and OS mechanisms

The runtime and future OS provide bounded mechanisms:

- process and service isolation;
- CPU, memory, queue, time, and retained-state accounting;
- timers, cancellation, IPC, shared memory, and immutable package data;
- admitted accelerator attachment and rights-limited device access;
- revocation, stale-generation detection, reset, and teardown; and
- DMA/IOMMU, interrupt, ownership, and recovery enforcement where applicable.

Model selection, batching, tokenization, KV-cache policy, tensor scheduling,
and AI-specific placement remain outside the kernel. An AI-aware service may
use OS mechanisms without turning model policy into privileged mechanism.

## Candidate cognitive-operation envelope

The following fields are research candidates, not accepted record names or wire
members. A later corpus and decision must prove which belong in a portable
contract, provider profile, agent-only policy, or retained evidence.

| Candidate field | Purpose |
| --- | --- |
| Operation identity and role | Bind the request to one admitted agent cycle and cognitive role. |
| Required capability profile | Ask for semantic abilities without naming one vendor model when policy permits selection. |
| Exact model requirement or approved set | Prevent silent model substitution where identity matters. |
| Quality objective and optional effort class | Express desired depth without claiming that vendor effort levels are numerically equivalent. |
| Deadline and resume-latency objective | Bound admission and make degradation visible. |
| Input, output, compute, memory, and cost ceilings | Reject or stop work that cannot remain inside authorized resources. |
| Placement and disclosure class | Define whether CPU, local GPU, named remote providers, or fallback are permitted. |
| Retention and recomputation policy | Bound provider-managed acceleration state without exposing its physical format. |
| Resumption forecast | Give the provider advisory expected-reuse evidence. |
| Required result and usage evidence | State what the caller needs before accepting a result. |

`Low`, `Medium`, and `High` reasoning effort are useful adapter controls but are
not portable units of thought, quality, time, tokens, or energy. A provider may
map an admitted effort class to an exact supported control and record the
mapping. It must reject or visibly degrade when it cannot preserve a required
quality or deadline contract.

## Four different kinds of agent-related state

The design must not collapse these categories:

| State | Owner and portability | Retention rule |
| --- | --- | --- |
| Durable agent state | Windvale-owned tasks, observations, plans, commitments, evidence, checkpoints, and effects. Portable only under its accepted schema and authority. | Retained according to agent and workspace policy. |
| Compiled conversation context | Windvale-owned bounded messages, retrieved evidence, tool results, and instructions supplied for one cognitive operation. | Reconstructible from admitted durable sources where policy permits. |
| Provider continuation state | Opaque remote response identifiers, encrypted reasoning items, or local provider session handles. Provider- and generation-specific. | Optional acceleration or continuity evidence; never the sole durable agent memory. |
| Physical inference state | Tokenized prefixes, KV blocks, activations, batching state, and engine-specific cache artifacts. Not a portable Windvale value. | Owned and bounded by the provider; reusable only under exact identity and isolation checks. |

A private chain of thought is not required durable agent state. Windvale should
retain the observable problem frame, admitted context manifest, requested
controls, result, usage evidence, verification, and effects needed for
continuity. A local provider that can expose reasoning traces requires an
explicit sensitive-data and retention policy; availability does not justify
default persistence or end-user disclosure.

## Cache identity, sharing, and isolation

Shared immutable model weights are a useful provider optimization but are not a
new Windvale semantic value. The provider may share one admitted resident model
across agents only when package identity, tenant policy, accounting, mutation
rules, and teardown permit it.

Provider-specific reusable inference state requires a stronger match. Relevant
identity can include:

- exact model content and configuration;
- tokenizer and chat-template identity;
- fine-tune, adapter, LoRA, or other model modification;
- provider implementation, generation, cache-format version, and numeric mode;
- tensor or KV layout and parallel placement;
- exact admitted token prefix or provider cache key;
- tenant, workspace, account, and data-sensitivity domain; and
- retention generation, expiry, and revocation state.

The provider rejects reuse when it cannot prove the required match. Hash
collision handling, stale generations, partial writes, corrupt secondary
state, provider restart, GPU reset, cancellation, and diagnostic exhaustion
require bounded failure paths. A cache hit is performance evidence, not semantic
permission to disclose or act on another run's data.

Current engine mechanisms reinforce the need for this boundary. vLLM documents
provider-managed KV offload through CPU memory and optional secondary tiers;
llama.cpp exposes prompt-cache and slot persistence controls; and Ollama exposes
model keep-alive and KV-cache settings. Those mechanisms are valuable provider
inputs, but their formats and correctness properties remain engine- and
version-specific.

## Memory placement model

Terms such as very hot, hot, warm, cold, and discarded can help an internal
provider describe expected access cost. They should not define portable
physical tiers. One host may have discrete GPU memory over PCIe, another may
have physically shared memory, and a coherent CPU/GPU system may expose a
shared address space over physically distinct memory pools with different
latency and bandwidth.

A portable policy should describe semantic constraints:

- which admitted processors may access the state;
- maximum retained bytes and duration;
- resume-latency objective and expected reuse;
- recomputation and persistence permission;
- sensitivity and isolation requirements; and
- required accounting and teardown evidence.

The provider maps those constraints to its memory architecture. A coherent or
unified address space does not prove equal access cost, zero movement, unlimited
capacity, or safe cross-tenant sharing.

## Authority and safety properties

A future verified AI workload retains the following properties:

1. Model output is untrusted cognitive evidence, not a capability grant or tool
   invocation.
2. Model catalog, inference, accelerator admission, retained inference state,
   secondary persistence, network, filesystem, tools, and external mutations
   remain separately authorized responsibilities.
3. The provider receives only the minimum admitted context and cannot fetch
   additional workspace data through ambient paths or handles.
4. A local-to-remote placement change is denied when the data-disclosure policy
   does not admit it.
5. Every queue, context, output, cache, diagnostic, retry, wake, and teardown
   path is bounded.
6. Cancellation request, provider acceptance, clean completion, provider loss,
   and indeterminate submission remain distinct outcomes.
7. An uncertain paid remote request or external mutation is not retried without
   an accepted idempotency contract.
8. Provider restart, device reset, revocation, and stale handles fail visibly;
   no cached state silently crosses a generation.

Tool execution stays on the verified workload path even when a model requests
it. The coordinator validates the proposed operation, checks policy and
authority, obtains approval where required, binds a short-lived rights-reduced
capability, executes the tool, records its exact outcome, and then compiles only
the permitted result into later model context.

## Performance and memory hypothesis

Agent-aware residency is useful only if it produces measured value over native
engine policy while keeping memory and correctness bounded. The proposal does
not assume that semantic forecasts will beat LRU, prefix caching, or existing
offload schedulers.

Potential advantages include:

- preserving a coding context during a predictable short compiler or test run;
- evicting a context known to be waiting indefinitely for human approval;
- retaining a reusable immutable prefix while discarding private conversation
  suffixes;
- choosing CPU recomputation when GPU transfer and retention costs exceed a
  deadline; and
- reserving local GPU capacity for a latency-critical wake while routing an
  admitted batch operation elsewhere.

Potential costs include forecast errors, extra coordination, retained-memory
pressure, transfer amplification, secondary-storage wear, privacy exposure,
cache fragmentation, and provider-specific complexity. Measurements must
include memory, transfers, recomputation, latency, throughput, energy when
practical, correctness, isolation, and teardown rather than reporting cache hit
rate alone.

## First experiment

### Question

Does bounded Windvale agent-lifecycle evidence improve a representative hybrid
agent workload beyond or alongside the selected infrastructure's native and
agent-aware cache, routing, and offload policies?

### Workload

Use one coding agent that alternates among:

1. context compilation and a model operation;
2. local CPU work such as source inspection, compilation, or focused tests;
3. waits ranging from a few seconds to a human-approval-shaped long pause;
4. resumption of the same context, a shared prefix, or an unrelated context;
5. concurrent pressure from several isolated agent episodes; and
6. clean cancellation, provider restart, resource pressure, and teardown.

The first implementation should use an existing local engine or orchestration
stack through an isolated provider. A remote provider may supply a separately
admitted frontier operation, but live spending and network variability must not
be part of the deterministic performance oracle.

### Baselines

Compare at least:

- stateless reconstruction or recomputation;
- the selected engine's native cache, LRU, and offload policy without Windvale
  forecasts;
- an available upstream agent-aware policy, such as Dynamo agent hints or the
  experimental ThunderAgent program scheduler, when the selected provider
  supports it; and
- the same infrastructure with bounded Windvale lifecycle and resumption
  evidence translated by the provider.

### Measurements

Record the host, processor, engine and version, model and quantization, context
sizes, concurrency, wait distribution, resource limits, and exact test corpus.
Measure:

- time to first token after each resumption and its tail distribution;
- end-to-end operation latency and throughput;
- prompt or prefix tokens recomputed;
- cache hits, rejected reuse, and stale-state detections;
- GPU, host-memory, pinned-memory, and secondary-storage working sets;
- bytes transferred among processor and storage tiers;
- admission failures, cancellations, reset recovery, and teardown time;
- output comparison under the selected numeric or behavioral oracle; and
- cross-tenant, cross-model, and cross-generation isolation failures.

The experiment succeeds only if it demonstrates a material, reproducible
benefit selected after measuring the baseline, with no correctness or isolation
regression and with bounded retained resources. A negative result is useful: it
would keep semantic residency out of the portable surface and leave placement
to existing providers.

## Candidate staged route

### Stage 0: preserve and review the hypothesis

Keep this document exploratory, review external evidence by date, and identify
overlap with accepted model, accelerator, agent, package, and capability
contracts. Do not add source directories, APIs, or manifests.

### Stage 1: provider and workload measurement

Select one existing engine or orchestration stack and one small representative
model. An NVIDIA/Dynamo path is eligible when accessible, but the experiment
must retain a provider-neutral workload and at least one simpler comparison
route. Establish CPU, GPU, memory, tokenization, model-load, prefix-cache, and
tool-wait baselines on Windows and Linux without claiming cross-host equivalence
where hardware differs.

### Stage 2: first local model provider

Place the engine behind an isolated, rights-limited provider. Reuse the existing
provider-neutral model records where their semantics remain exact; introduce no
local extension until a corpus proves that the version-1 contract is
insufficient. Record exact model, engine, placement, resource, completion, and
teardown evidence.

### Stage 3: hybrid verified-agent demonstration

Run the agent coordinator and deterministic tools on CPU, one admitted
cognitive operation on the local provider, and one separately admitted remote
operation when policy permits. Demonstrate that durable agent state and effect
evidence survive provider replacement without treating continuation or KV state
as portable memory.

### Stage 4: semantic-residency experiment

Implement lifecycle mappings or resumption forecasts only inside the
experimental coordinator/provider boundary. Compare them with native and
available upstream agent-aware policies under the first experiment. Do not
expose a public capability or serialized format merely to complete the
experiment.

### Stage 5: decision

If measurement proves value, write a dated decision freezing the smallest
semantic contract, ownership boundary, evidence requirements, limits, failure
behavior, and reconsideration triggers. If it does not, record the negative
result and keep cache policy provider-owned.

Selective Windvale-native tokenizers, model readers, tensor operations,
kernels, schedulers, or cache components remain later options. Each replacement
requires an independently useful contract and evidence that it improves
portability, safety, reproducibility, or measured performance.

## Non-goals

This proposal does not accept or promise:

- a Windvale-native tensor framework, model format, tokenizer, or inference
  engine;
- portable KV-cache bytes or migration of live KV state among unrelated
  providers;
- AI-specific scheduling policy in the kernel;
- one universal measure of reasoning effort or model quality;
- access to, retention of, or verification of a remote model's hidden reasoning;
- silent local/remote fallback or silent model substitution;
- automatic spending, external mutation, or retry authority;
- training, fine-tuning, distributed inference, collectives, or multi-node
  accelerator orchestration;
- attested GPU execution without a separately accepted measurement contract;
- equal performance on unified, coherent, and discrete memory systems; or
- supported-provider, throughput, latency, cost, energy, or model-size claims.

## Open questions

- Is **verified AI workload** the durable product term, or should it name only
  the admitted execution while another term names the package?
- Which local provider offers the smallest honest first integration on both
  Windows and Linux?
- Can the current non-streaming text model protocol represent the first local
  provider exactly, or does local execution require separate resource evidence?
- Which quality, latency, cost, privacy, and effort controls are portable enough
  to standardize without hiding provider differences?
- Which parts of the candidate cognitive-operation envelope map exactly to
  Dynamo agent hints, priority, speculative prefill, and program identity, and
  which Windvale authority or evidence fields must remain outside the provider?
- Would a direct Dynamo integration, a lower-level vLLM/SGLang/TensorRT-LLM
  adapter, or a simpler local engine create the smallest honest first proof?
- Can useful reuse be expressed with an advisory forecast, or would any public
  hint overfit one family of autoregressive transformers?
- Is prefix identity sufficient for the first experiment, or is provider-owned
  full-context state necessary to measure the hypothesis?
- What tenant and workspace isolation evidence is required before model weights
  or prefixes may be shared?
- When may provider-specific state reach secondary storage, and what encryption,
  deletion, durability, and wear evidence would that require?
- What evidence can a local provider independently prove about processor
  placement, model bytes, cache reuse, numeric mode, and completion?
- When does local inference save cost or improve privacy enough to justify its
  operational complexity and energy use?
- Which failures should permit visible quality degradation, and which must
  reject the operation?

## Decision and implementation triggers

A dated decision is required before accepting:

- the term **verified AI workload** as a normative package or runtime contract;
- a local model provider capability, public facade, or serialized extension;
- a portable model-placement, reasoning-effort, or resumption-hint field;
- retained or persistent provider inference state;
- model-weight, prefix, or cache sharing across agents, tenants, or accounts;
- an automatic local/remote placement or fallback policy;
- any attestation or independent-execution claim;
- an AI-specific OS service or accelerator attachment profile; or
- replacement of an upstream inference component with a Windvale-native one.

Implementation creates no empty source area before the first accepted owner,
contract, consumer, hostile-input corpus, resource limits, and teardown path
exist.

## Evidence snapshot

The review through 2026-08-24 used the following primary project sources:

- [llama.cpp server documentation](https://github.com/ggml-org/llama.cpp/blob/master/tools/server/README.md)
  for prompt-cache, slot, and server controls;
- [Ollama FAQ](https://docs.ollama.com/faq) for model residency, CPU/GPU
  placement, concurrency, and KV-cache controls;
- [vLLM KV-offloading documentation](https://docs.vllm.ai/en/latest/features/kv_offloading_usage/)
  and [cache configuration](https://docs.vllm.ai/en/latest/api/vllm/config/cache/)
  for CPU and secondary-tier provider-managed cache behavior;
- [NVIDIA Dynamo agentic-inference documentation](https://docs.nvidia.com/dynamo/dev/digest/agentic-inference)
  for the harness/orchestrator/runtime split, agent hints, KV-aware routing,
  priority, speculative prefill, and provider-owned cache policy;
- the experimental
  [ThunderAgent program scheduler](https://docs.dynamo.nvidia.com/dynamo/dev/agents/thunder-agent-program-scheduler)
  for program-level accounting and tool-boundary pause/resume over Dynamo;
- [NVIDIA CUDA Unified Memory documentation](https://docs.nvidia.com/cuda/cuda-programming-guide/04-special-topics/unified-memory.html)
  and [Grace memory-placement guidance](https://docs.nvidia.com/dccpu/grace-perf-tuning-guide/os-settings.html)
  for distinctions among coherent address spaces, memory locality, and
  physical performance; and
- [OpenAI gpt-oss documentation](https://openai.com/index/introducing-gpt-oss/)
  for one current example of provider/model-specific low, medium, and high
  reasoning-effort controls.

These links support a dated landscape observation, not a dependency or semantic
endorsement. Exact behavior must be rechecked against the selected version when
an experiment or decision depends on it. Reported upstream issues may motivate
hostile-input and concurrency tests but are not accepted as universal defects
without reproduction against a pinned version.

## Immediate recommendation

Preserve **verified AI workload** as the product thesis and treat **agent-aware
inference integration** as an important research lane rather than an exclusive
novelty claim. Build the first practical path above existing providers:
CPU-hosted durable agent and tool coordination, one admitted local inference
stack, an optional separately authorized remote model, and exact execution and
effect evidence. Prefer mapping onto an established agent-aware platform such as
NVIDIA Dynamo when it can preserve the contract. Measure the upstream policy
before adding Windvale-specific forecasts. Keep physical model and KV state
provider-owned, keep AI policy out of the kernel, and require a dated decision
before the experiment becomes public architecture.
