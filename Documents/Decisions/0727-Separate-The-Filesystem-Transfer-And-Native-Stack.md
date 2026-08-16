# Decision 0727: Separate the filesystem transfer and native stack

- Status: Accepted; boot integration pending
- Date: 2026-08-16
- Corrects: [Decision 0716](0716-Reserve-The-Complete-Filesystem-Envelope.md)
- Advances: [x86-64 filesystem-machine emission](../../Specifications/Windvale-Os-X64-Process-Filesystem-Machine-Emission.md)

## Context

Decision 0716 enlarged the filesystem private region enough to contain the
65,600-byte transfer window, but it did not account for the generated native
service's separately measured 16-page stack. The first generation-three
constructor draft therefore placed the stack over the transfer pages. That
alias is unsafe once the provider resumes from receive and processes a maximum
request: normal stack growth could overwrite the live request, and a reply
could expose stale stack bytes.

The first draft also treated launch-profile rights and process generation as
if they occupied the primary `WVPROC17` capability fields. The retained record
instead requires endpoint slot 0, endpoint generation 2, provider rights 46,
capacity one, and process generation 3 in their established fields.

## Decision

Increase filesystem profile 2 from 65 to 81 charged user pages and preserve
four additional kernel-only paging pages. Partition the 85-page physical
extent as:

- pages 0–3: kernel-only paging structures;
- pages 4–51: 48 RX service-image pages;
- pages 52–68: 17 RW/NX context and transfer pages;
- pages 69–84: 16 RW/NX native-stack pages; and
- pages 85–86: absent guard entries.

Keep the receive window at extent offset 214,016 and the 16-page stack top at
extent offset 348,160. Reject the earlier 65-page request. Encode the primary
capability as slot 0, endpoint generation 2, rights 46, and capacity one, and
encode process generation 3 separately.

## Consequences

The complete transfer and measured native stack are simultaneously mapped but
disjoint. The 85-page allocation still fits the released 122-page client slot.
Resource-domain reservation, commit, readiness, drain, release, and terminal
zero-charge evidence now carry 81 user pages.

This correction does not boot-link or enter the provider. The focused machine
owner and the existing service/provider launch owners regenerate exact
artifacts before the constructor can be published.

The resulting focused owners pass 3 filesystem-machine, 69 application-launch,
and 18 provider-transaction cases. The service policy is 10,150 bytes at
`d7eabbaaab65ce642f7eb3fd1362429d9994d979974c525408bd7e46a470fa73`;
the composed provider policy is 30,268 bytes at
`9c69b5f8ae752367d6ad1052ada500a864a77d89ef1bede72daff5c48b0eaa6d`.

## Reconsideration triggers

Recompute the complete allocation when image size, generated stack proof,
context layout, maximum transfer, guard policy, or page-table shape changes.
Never reuse stack pages as an IPC window without a separate phase, scrubbing,
and non-alias proof accepted by a new decision.
