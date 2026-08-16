# Windvale OS application-launch policy

## Status and scope

Application-launch policy 1 is the immutable kernel-admission gate for both
sequential generations of the known Probe 40 child. Its version-1 start
function composes with resource-domain policy 1 and
application-machine-construction policy 1 before the process machine publishes
either client. Syscall operation 8 is now the first fixed application-start
entry: it derives the current init context, accounts the call, snapshots one
64-byte request from the current user page, and erases the kernel snapshot
before returning. It is not a package resolver, general process creator,
service manager, or claim that arbitrary applications can now be started. The separate
[`WVSR 1`](Windvale-Os-Application-Start-Request.md) decoder now validates one
fixed serialized application profile before calling this typed ABI.
The architecture-neutral
[`application-start user-copy policy 1`](Windvale-Os-Application-Start-User-Copy.md)
now copies that exact value, bounds it to an admitted window, and checks the
encoded caller against a separately supplied current-caller identity before
the decoder runs.

LaunchPlan1 deliberately freezes the measured first consumer. An accepted
request binds version `1`, init caller reference `65537`, domain reference
`65537` (identity/generation `1/1`), executable-publication reference `65576`
(identity/generation `40/1`), and admission profile `1`. That profile means one
process and 122 pages, provider/client right profiles `46 → 17`, three standard
streams, and one observer. The caller supplies request reference `65537` or
`131073`; policy derives child reference `65538` or `131074` and does not accept
a caller-selected child identity or raw image address.

## Immutable transition

[`Application-Launch-Policy.wv`](../Operating-System/Kernel/Application-Launch-Policy.wv)
publishes a compact transition containing status, state, and plan reference.
The states are `Planned`, `Reserved`, `Constructed`, `Published`,
`Rolled_back`, and `Rejected`. A child is visible only in `Published`; its
process reference is the admitted plan reference plus one only in that state.
No other state yields a usable process identity.

`Application_start_admit` rejects an unsupported request version, unauthorized
caller, stale or mismatched executable publication, request reference, domain,
or admission profile before a transaction can reserve resources.
`Application_launch_advance` owns all later state changes through one immutable
constructor. Its resource input is the caller's boolean result from the exact
ResourceDomain1 preflight or committed state check; it does not copy the full
domain record into this transition:

1. `Planned → Reserved` requires ResourceDomain1's exact one-process,
   122-page, zero-endpoint reserve preflight to succeed.
2. `Reserved → Constructed` retains an unpublished plan only after private
   construction succeeds.
3. `Constructed → Published` requires the matching live domain, no outstanding
   reservation, and committed process/page evidence.
4. construction failure or explicit rollback reaches `Rolled_back`; the caller
   discards the private reserved domain replacement and publishes neither
   record.

The domain commit and launch publication are constructed as private immutable
replacements. The process-composition owner retains the typed domain evidence
and supplies only the checked result to the launch transition. It exposes the
pair only after both checks succeed, so no observer can see a committed child
transition without its charge or an unpublished process identity.
Move-transfer ownership is not yet implemented; the fixed `46 → 17` grant is
copy-reduced evidence.

## Live Probe 40 composition

The process-policy path first commits the retained init and directory base of
two processes, 22 pages, and two endpoints. For generation 1 it validates
LaunchPlan1, preflights and reserves one process plus 122 pages, admits the
private `110/8/4` code/data/stack layout, commits the domain charge, and
publishes plan `65537`. After teardown it repeats the complete transaction for
generation 2 with plan `131073` and a distinct admitted `100/18/4` layout. A
separate failure transcript reserves the same charge, rejects writable code,
retains no published child state, discards the reservation, and reproduces the
exact two-process/22-page/two-endpoint baseline.

The composed WVB returns token 97 only after the boot-service envelope and both
launch transactions admit. Its 699,394-byte link-facing object remains inside
the fixed 772 KiB supervisor RX window. The normal 1,692,160-byte
current-Windows-host Probe 40 image at SHA-256
`6da0c529425e3d301657501411573b268e64d4c13347d8ae74c9fcb7a45cb354`
passes the pinned QEMU/OVMF gate through application execution and
guest-controlled shutdown.

## Focused evidence and limits

The `os-application-launch` native owner builds the admission policies,
serialized request decoder, user-copy boundary, and derived syscall context and
executes focused behavior programs covering 61 cases: every `WVSR 1`
structural status and typed handoff,
nine copy/window/caller cases, plus
unsupported-version, unauthorized-caller, stale-publication, malformed-plan,
wrong-domain, and rights-profile rejection before exposure; successful
reserve/construct/publish with exact domain charge; failed construction with
complete unpublished rollback; every machine-policy rejection class; and both
accepted layouts. `os-process-policy` proves
the same module composes into token 97; `os-process-object` pins the measured
context; and `os-probe` pins all three EFI constructions.

Policy 1 does not resolve packages, load an arbitrary image, allocate or map
machine objects dynamically, transfer/move
capabilities, accept variable total resource charges, expose
cancellation/completion, or accept callers other than fixed init generation 1.
Operation 8 currently ends after checked context loading and snapshot
admission; connecting the admitted request to private machine construction and
charge-backed publication is the next slice.
