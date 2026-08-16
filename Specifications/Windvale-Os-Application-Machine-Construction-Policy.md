# Windvale OS application-machine-construction policy

## Status and scope

Application-machine-construction policy 1 is the portable admission gate for
the private machine shape of both fixed Probe 40 client generations. It is an
implemented current-Windows-host native candidate; cross-host qualification is
pending. It validates a bounded requested layout before application-launch
policy can advance from `Reserved` to `Constructed`.

This policy is not an allocator, serialized kernel record, syscall ABI, image
loader, page-table builder, capability-transfer mechanism, or general process
creator. The Probe machine still performs the admitted construction through
its existing fixed native seam.

## Admission contract

[`Application-Machine-Construction-Policy.wv`](../Operating-System/Kernel/Application-Machine-Construction-Policy.wv)
exports one pure operation:

```text
Application_machine_construction_status(
    plan_reference,
    object_mask,
    code_pages,
    data_pages,
    mapping_profile,
    capability_slots
) -> u32
```

The accepted plan references are generation-safe LaunchPlan1 references
`65537` and `131073`. Object mask `63` requires exactly six private objects:
an address space, code object, data object, stack object, observer, and initial
thread. Stack size is fixed at four pages; non-empty code and data partition
the other 118 pages. The two live profiles deliberately use different
partitions:

| Client generation | Code pages | Data pages | Stack pages |
| --- | ---: | ---: | ---: |
| 1 | 110 | 8 | 4 |
| 2 | 100 | 18 | 4 |

Mapping profile `1` means code is read+execute, data and stack are read+write
without execute, and input, output, diagnostics, and observer are all bound.
The initial capability table accepts 4 through 64 slots. Checked subtraction
validates the page partition without an overflowing addition.

Status is `0` on success. Rejections are stable and ordered: `1` plan,
`2` object set, `3` page layout, `4` mapping/binding profile, and `5`
capability-table shape. The launch transaction treats only status
`0` as successful private construction; any other result remains unpublished
and its reservation is discarded.

The interface uses explicit scalar inputs because the frozen profile-5 native
runner used by this OS construction does not admit the newer packed arithmetic
operations that a serialized request decoder would need. The runner is not
widened merely to compress this policy. A typed serialized record belongs with
the later user-callable kernel admission boundary.

## Evidence and limits

The standalone policy/self-test WVB is 2,513 bytes with SHA-256
`f3537c3c2686cc852a83ca065ff14e6f449e08f7e638efe6a0f54d3857e071f0`.
The native runner returns 43 at exactly 997 instructions after proving each
rejection class plus both accepted layouts. The combined
`os-application-launch` owner covers two projects, two behavior groups, and
eleven cases. The composed process policy proves both generations complete
reserve, private construction, charge-backed publication, and teardown while
the failed-construction transcript reaches rights status `4`, publishes
nothing, and restores the exact baseline.

Policy 1 still fixes the total charge at 122 pages and accepts only the two
known versioned requests and executable publication. It does not admit an arbitrary executable, choose virtual
addresses, allocate or map objects, transfer capabilities, accept arbitrary
total charges, expose a construction reference, or decode a user buffer.
