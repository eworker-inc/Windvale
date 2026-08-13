# Decision 0527: Native-only forward development boundary

- Date: 2026-08-12
- Status: Accepted
- Clarifies: [Decision 0526](0526-Dotnet-Retirement-Qualification-And-Stage0-Archive.md)

## Context

Decision 0526 retired .NET from Windvale's normal Windows and Linux workflow and
published the immutable Stage 0 recovery release. The repository still contains
the frozen C# implementation, managed projects, solution metadata, and recovery
scripts so that the accepted bootstrap lineage remains inspectable.

Forward repository changes now need a sharper maintenance boundary. Updating the
frozen managed tree for new project formats, repository paths, source semantics,
libraries, or native tools would restore an unqualified second implementation and
make ordinary work depend on a retired stack. The immutable Stage 0 release already
preserves the exact managed recovery state independently of later repository layout.

## Decision

Windvale forward development is native-only.

- New source, project, workspace, package, library, bytecode, runtime, tool, and OS
  behavior is implemented in Windvale-owned or native host tooling.
- Files under the retained C# implementation, `*.csproj`, `Windvale.slnx`,
  `global.json`, and `Directory.Build.props` are frozen historical and recovery
  material. Ordinary changes do not update them for parity or path movement.
- Normal build, verification, test, packaging, execution, WebAssembly, OS-image,
  and bootstrap entry points must not invoke `dotnet` or a managed artifact.
- The immutable `stage0-recovery-e5a1a7473c57` release is the authoritative managed
  recovery state. Managed tests in a later checkout are not maintained as an oracle
  for post-retirement repository organization or contracts.
- An exceptional managed security or recovery correction requires a separate
  decision naming the defect, the exact affected recovery contract, and why the
  immutable release is insufficient. It does not reopen forward feature parity.

The retained files are not deleted by this decision. Their presence documents the
bootstrap history and permits source inspection without making them active product
dependencies.

## Consequences

- Workspace 1, Project 2, package contracts, and library growth have exactly one
  forward implementation.
- Repository paths may change without modifying frozen managed tests or projects.
- Current native verification must own every changed active boundary and refuse
  uncovered gaps instead of falling back to managed verification.
- Recovery from the managed bootstrap uses the exact archived release rather than
  assuming the current checkout still matches the retired repository layout.

## Reconsideration triggers

Reconsider only if the native toolchain and immutable recovery release both fail to
reconstruct an accepted source state, or if a security defect makes the archived
recovery procedure unsafe. Convenience, differential breadth, or a missing native
test is not sufficient reason to restore .NET to the normal path.
