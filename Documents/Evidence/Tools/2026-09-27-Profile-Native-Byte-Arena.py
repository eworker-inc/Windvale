"""Sample x86-64 Linux byte-arena writes; always terminate the sampled guest.

The optional object must be staged from the exact WVB used to build the runner.
Results describe one bounded time window, not the total allocation or live set.
"""

import argparse
import collections
import ctypes
import json
import os
import platform
import signal
import struct
import subprocess
import time
from pathlib import Path


parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("runner")
parser.add_argument("guest")
parser.add_argument("--attach-ms", type=int, default=3000)
parser.add_argument("--object-manifest", type=Path)
arguments = parser.parse_args()
if platform.system() != "Linux" or platform.machine() != "x86_64":
    parser.error("requires x86-64 Linux")
if not 0 <= arguments.attach_ms <= 30000:
    parser.error("--attach-ms must be between 0 and 30000")

RUNNER, GUEST = arguments.runner, arguments.guest
ATTACH_DELAY = arguments.attach_ms / 1000
MAX_EVENTS = 5000
MAX_SECONDS = 8
PTRACE_PEEKDATA = 2
PTRACE_POKEUSER = 6
PTRACE_CONT = 7
PTRACE_GETREGS = 12
PTRACE_ATTACH = 16
PTRACE_DETACH = 17
# x86-64 Linux struct user.u_debugreg[0] offset; DR7 is its seventh entry.
DEBUG_REGISTER_OFFSET = 848


class Registers(ctypes.Structure):
    _fields_ = [(name, ctypes.c_ulonglong) for name in
                "r15 r14 r13 r12 rbp rbx r11 r10 r9 r8 rax rcx rdx rsi rdi "
                "orig_rax rip cs eflags rsp ss fs_base gs_base ds es fs gs".split()]


libc = ctypes.CDLL(None, use_errno=True)
libc.ptrace.restype = ctypes.c_long
libc.ptrace.argtypes = [ctypes.c_uint, ctypes.c_int, ctypes.c_void_p, ctypes.c_void_p]


def ptrace(request, pid, address=0, data=0):
    ctypes.set_errno(0)
    result = libc.ptrace(request, pid, ctypes.c_void_p(address), ctypes.c_void_p(data))
    if result == -1 and ctypes.get_errno():
        raise OSError(ctypes.get_errno(), os.strerror(ctypes.get_errno()))
    return result


def read_symbol_map(runner, manifest_path):
    manifest = manifest_path.read_bytes()
    if len(manifest) > 1024 or len(manifest) < 24 or manifest[:4] != b"WVOP":
        raise ValueError("invalid bounded WVOP manifest")
    chunk_count = struct.unpack_from("<I", manifest, 16)[0]
    if not 1 <= chunk_count <= 32:
        raise ValueError("invalid WVOP chunk count")
    prefix = manifest_path.with_suffix("")
    chunks = []
    total = 0
    for index in range(chunk_count):
        chunk_path = Path(f"{prefix}.chunk-{index}")
        size = chunk_path.stat().st_size
        total += size
        if total > 32 * 1024 * 1024:
            raise ValueError("WVO exceeds the diagnostic limit")
        chunks.append(chunk_path.read_bytes())
    obj = b"".join(chunks)
    if len(obj) < 24 or obj[:4] != b"WVO1":
        raise ValueError("invalid WVO header")
    section_count, symbol_count = struct.unpack_from("<II", obj, 12)
    if not 1 <= section_count <= 512 or symbol_count > 10000:
        raise ValueError("WVO symbol geometry exceeds the diagnostic limit")
    cursor = 24
    text_data = None
    for index in range(section_count):
        if cursor + 20 > len(obj):
            raise ValueError("truncated WVO section")
        file_bytes, name_length = struct.unpack_from("<II", obj, cursor + 12)
        data_offset = cursor + 20 + name_length
        if data_offset + file_bytes > len(obj):
            raise ValueError("truncated WVO section data")
        if index == 0:
            if obj[cursor + 20:data_offset] != b".text" or file_bytes < 64:
                raise ValueError("missing WVO text section")
            text_data = obj[data_offset:data_offset + file_bytes]
        cursor = data_offset + file_bytes
    symbols = []
    for _ in range(symbol_count):
        if cursor + 20 > len(obj):
            raise ValueError("truncated WVO symbol")
        section, offset, size, name_length = struct.unpack_from("<IIII", obj, cursor + 4)
        cursor += 20
        if cursor + name_length > len(obj):
            raise ValueError("truncated WVO symbol name")
        if name_length > 128:
            raise ValueError("WVO symbol name exceeds the diagnostic limit")
        name = obj[cursor:cursor + name_length].decode("ascii")
        cursor += name_length
        if section == 0 and offset + size <= len(text_data):
            symbols.append((offset, offset + size, name))
    if Path(runner).stat().st_size > 64 * 1024 * 1024:
        raise ValueError("runner exceeds the diagnostic limit")
    elf = Path(runner).read_bytes()
    # The linker patches relocation sites, so complete WVO/ELF text equality is invalid.
    # Anchor the same-build object by a unique leading code sequence and text bounds.
    image_base = elf.find(text_data[:64])
    if (image_base < 0 or elf.find(text_data[:64], image_base + 1) >= 0 or
            image_base + len(text_data) > len(elf)):
        raise ValueError("WVO text anchor is absent, ambiguous, or out of runner bounds")
    return image_base, symbols


image_base = None
symbols = []
if arguments.object_manifest is not None:
    image_base, symbols = read_symbol_map(RUNNER, arguments.object_manifest)


started = time.monotonic()
process = subprocess.Popen([RUNNER, GUEST, "--report-steps"],
                           stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
attached = False
try:
    time.sleep(ATTACH_DELAY)
    if process.poll() is not None:
        raise RuntimeError("guest completed before attach")
    ptrace(PTRACE_ATTACH, process.pid)
    attached = True
    _, status = os.waitpid(process.pid, 0)
    if not os.WIFSTOPPED(status):
        raise RuntimeError(f"attach status={status}")
    registers = Registers()
    ptrace(PTRACE_GETREGS, process.pid, 0, ctypes.addressof(registers))
    context = registers.r15
    mappings = open(f"/proc/{process.pid}/maps", encoding="ascii").read().splitlines()
    target = os.path.abspath(RUNNER)
    base = min(int(line.split("-")[0], 16) - int(line.split()[2], 16)
               for line in mappings if target in line)
    cursor_address = context + 60
    geometry = ptrace(PTRACE_PEEKDATA, process.pid, context + 56)
    initial = geometry >> 32
    capacity = geometry & 0xffffffff
    if capacity == 0 or capacity > 536870912 or initial > capacity:
        raise RuntimeError("r15 does not identify an admitted byte arena context")
    ptrace(PTRACE_POKEUSER, process.pid, DEBUG_REGISTER_OFFSET, cursor_address)
    # Local DR0, watch 4-byte writes.
    ptrace(PTRACE_POKEUSER, process.pid, DEBUG_REGISTER_OFFSET + 7 * 8, 0xD0001)
    ptrace(PTRACE_CONT, process.pid)
    sites = collections.defaultdict(lambda: [0, 0])
    sizes = collections.defaultdict(collections.Counter)
    examples = collections.defaultdict(list)
    decreases = 0
    previous = initial
    events = 0
    while events < MAX_EVENTS and time.monotonic() - started < MAX_SECONDS:
        stopped, status = os.waitpid(process.pid, os.WNOHANG)
        if stopped == 0:
            time.sleep(0.001)
            continue
        if os.WIFEXITED(status) or os.WIFSIGNALED(status):
            attached = False
            break
        if not os.WIFSTOPPED(status) or os.WSTOPSIG(status) != signal.SIGTRAP:
            raise RuntimeError(f"unexpected stop status={status}")
        ptrace(PTRACE_GETREGS, process.pid, 0, ctypes.addressof(registers))
        cursor = ptrace(PTRACE_PEEKDATA, process.pid, context + 56) >> 32
        delta = cursor - previous
        if delta < 0:
            decreases += 1
        elif delta > 0:
            row = sites[registers.rip - base]
            row[0] += 1
            row[1] += delta
            sizes[registers.rip - base][delta] += 1
            if len(examples[registers.rip - base]) < 3:
                examples[registers.rip - base].append({
                    "delta": delta, "r8": registers.r8 & 0xffffffff,
                    "r9": registers.r9 & 0xffffffff,
                    "rcx": registers.rcx & 0xffffffff})
        previous = cursor
        events += 1
        ptrace(PTRACE_CONT, process.pid)
    mapped = []
    for offset, row in sorted(sites.items(), key=lambda item: -item[1][1])[:25]:
        site = {"offset": hex(offset), "events": row[0], "bytes": row[1],
                "sizes": sizes[offset].most_common(6), "examples": examples[offset]}
        if image_base is not None:
            image_offset = offset - image_base
            for begin, end, name in symbols:
                if begin <= image_offset < end:
                    site["symbol"] = name
                    site["functionOffset"] = image_offset - begin
                    break
        mapped.append(site)
    print(json.dumps({"events": events, "seconds": round(time.monotonic() - started, 3),
                      "initial": initial, "final": previous, "capacity": capacity,
                      "decreases": decreases, "sampledOnly": True,
                      "imageBase": image_base, "sites": mapped}), flush=True)
finally:
    if attached:
        try:
            os.kill(process.pid, signal.SIGSTOP)
            os.waitpid(process.pid, 0)
            ptrace(PTRACE_DETACH, process.pid, 0, signal.SIGKILL)
        except (ProcessLookupError, ChildProcessError, OSError):
            pass
    if process.poll() is None:
        process.kill()
    try:
        process.communicate(timeout=5)
    except subprocess.TimeoutExpired:
        process.kill()
        process.communicate(timeout=5)
