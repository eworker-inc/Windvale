# Windvale OS instructions

These instructions apply throughout `Operating-System/` in addition to the
repository handbook.

- Treat running Windvale OS as a guest, accelerating that guest through a host
  hypervisor, and making Windvale OS a VM host as separate contracts.
- Preserve the pinned emulation oracle and report the selected engine, provider,
  and complete nested topology. Prefer physical or root providers for baseline
  qualification; nesting remains optional unless a named decision qualifies it.
- Keep privileged guest-memory, vCPU, interrupt, DMA, accounting, and teardown
  enforcement in the kernel/WVA boundary. Keep machine, firmware, device, GPU,
  compute, and lifecycle policy in isolated services.
- Keep interrupt, timer, memory, DMA/IOMMU, accounting, and teardown mechanisms
  in the kernel. Isolated drivers own link-device mechanics, and one bounded
  user-space service initially owns standards-based packet, route, UDP, and TCP
  processing.
- Use semantic rights-limited application capabilities rather than ambient
  sockets or raw service protocols.
- Build remote terminals over an authenticated secure ordered stream with a
  small bounded session protocol. Keep identity and authorization separate; do
  not permit production plaintext, replayable early data, implicit resume,
  ambient remote-root authority, custom cryptography, or terminal parsing in the
  kernel or shell.
- Treat guest images, firmware, VM state, page tables, shared queues, shaders,
  compute kernels, and device commands as untrusted input. Bound exits,
  interrupts, queues, pinned pages, work, diagnostics, and teardown time while
  reserving host recovery resources.
- VM-management authority never implies storage, network, display, GPU,
  accelerator, firmware, host-file, or passthrough authority. Bind attachments
  separately and describe whether each is software, paravirtual shared,
  hardware-partitioned, or exclusive passthrough.
- Never permit device passthrough or guest/accelerator DMA without measured
  IOMMU, interrupt-remapping, topology, ownership, reset, range, generation,
  revocation, and teardown evidence.
