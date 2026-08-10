#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 || $2 != *.wvo ]]; then
    echo 'Usage: ./Tools/Native/Produce-Os-Probe-Object.sh <exceptions|wvb-admission-bridge|native-bridge-and-support|paging|memory|memory-invalid-opcode|memory-general-protection|loader> <output.wvo>' >&2
    exit 64
fi
case $1 in
    exceptions)
        expected_bytes=483
        expected_digest=9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c
        ;;
    wvb-admission-bridge)
        expected_bytes=484
        expected_digest=271c378b1f12bb4affa33474d865611cbf14e5b1b8996c703cb3d3cbe22eee7d
        ;;
    native-bridge-and-support)
        expected_bytes=461
        expected_digest=472a0fbe6497525e634a4785e92aa9ee62c3c7d70fff7510e45acbea644eea0b
        ;;
    paging)
        expected_bytes=1292
        expected_digest=a6bcad24e4752acc1fbab75d6667e965f2ab4d5613edd2c8e6cda244616fba2d
        ;;
    memory)
        expected_bytes=1529
        expected_digest=2668e17c3181e168415fb7bdee530873e2ddc8fa2d100af94bcc7b74909df3ed
        ;;
    memory-invalid-opcode)
        expected_bytes=1545
        expected_digest=09aa0fcfe12c561b79367cb26569dbc6f1f47ca3b98dc892426ca57b4328f868
        ;;
    memory-general-protection)
        expected_bytes=1545
        expected_digest=23a052f9d47a9416618c9b7a50a382c68c46d3bf7834410cc79f8fef2aa461e0
        ;;
    loader)
        expected_bytes=6336
        expected_digest=b310bc0e9aebc7b14c0892bb3dd4b833d42539c2194427a8f333b511d6af3804
        ;;
    *)
        echo 'Usage: ./Tools/Native/Produce-Os-Probe-Object.sh <exceptions|wvb-admission-bridge|native-bridge-and-support|paging|memory|memory-invalid-opcode|memory-general-protection|loader> <output.wvo>' >&2
        exit 64
        ;;
esac

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
code_fixture=
code_fixture_bytes=
code_fixture_digest=
if [[ $1 == loader ]]; then
    producer="$repository_root/Artifacts/Native-Os-Probe-Loader-Object-Producer-Candidate/linux-x64-os-probe-loader-object.elf"
    producer_bytes=389120
    producer_digest=616cc30cdd6c46dba15ead2dc7881f4ce53df187e485939337cfd0c5a540dc42
    code_fixture="$repository_root/Artifacts/Native-Os-Probe-Loader-Object-Producer-Candidate/normal-x64-loader.bin"
    code_fixture_bytes=6115
    code_fixture_digest=19008f698db52c206dae920cf57ca4461eb009d47d8ecba258d6b021b05a2eed
elif [[ $1 == memory || $1 == memory-invalid-opcode || $1 == memory-general-protection ]]; then
    producer="$repository_root/Artifacts/Native-Os-Probe-Memory-Object-Producer-Candidate/linux-x64-os-probe-memory-object.elf"
    producer_bytes=405504
    producer_digest=1ea358f8cc77b36201b22ff820ef6fd000b4bbd48342dfe6eed994e487a15c7b
    case $1 in
        memory)
            code_fixture="$repository_root/Artifacts/Native-Os-Probe-Memory-Object-Producer-Candidate/normal-x64-memory.bin"
            code_fixture_bytes=1089
            code_fixture_digest=07d2508132456706d8718a0bc9a54cf9b0228afbb61aec8e66ce92d34cf5e803
            ;;
        memory-invalid-opcode)
            code_fixture="$repository_root/Artifacts/Native-Os-Probe-Memory-Object-Producer-Candidate/invalid-opcode-x64-memory.bin"
            code_fixture_bytes=1105
            code_fixture_digest=f350059d181b4a640ab03734807243348bcaca723484b1fe093767e4d042ea18
            ;;
        memory-general-protection)
            code_fixture="$repository_root/Artifacts/Native-Os-Probe-Memory-Object-Producer-Candidate/general-protection-x64-memory.bin"
            code_fixture_bytes=1105
            code_fixture_digest=69f31f4fc8a08bea9202e4accc6101101103ea83ee213f4b4f8f51202655e049
            ;;
    esac
else
    producer="$repository_root/Artifacts/Native-Os-Probe-Object-Producer-Candidate/linux-x64-os-probe-object.elf"
    producer_bytes=462848
    producer_digest=c4e22a9f67d5bdb4f186ddfbb63aa93032712ea7bdc260ed28076b12f0217e80
fi
output_directory=$(dirname -- "$2")
if [[ ! -d $output_directory ]]; then
    echo 'The native OS Probe object output directory does not exist.' >&2
    exit 1
fi
output_directory=$(CDPATH= cd -- "$output_directory" && pwd -P)
output="$output_directory/$(basename -- "$2")"
if [[ -e $output ]]; then
    echo 'The native OS Probe object output already exists.' >&2
    exit 1
fi
if [[ ! -f $producer || $(wc -c < "$producer") -ne $producer_bytes ]] ||
    ! printf '%s  %s\n' \
        "$producer_digest" \
        "$producer" | sha256sum --check --strict --quiet; then
    echo 'The Linux native OS Probe object producer identity is invalid.' >&2
    exit 1
fi
if [[ -n $code_fixture ]] &&
    { [[ ! -f $code_fixture || $(wc -c < "$code_fixture") -ne $code_fixture_bytes ]] ||
        ! printf '%s  %s\n' \
            "$code_fixture_digest" \
            "$code_fixture" | sha256sum --check --strict --quiet; }; then
    echo 'The native OS Probe code fixture identity is invalid.' >&2
    exit 1
fi

if [[ -n $code_fixture ]]; then
    "$producer" "$1" "$code_fixture" "$output"
else
    "$producer" "$1" "$output"
fi
status=$?
if [[ $status -ne 0 || ! -f $output || $(wc -c < "$output") -ne $expected_bytes ]] ||
    ! printf '%s  %s\n' "$expected_digest" "$output" |
        sha256sum --check --strict --quiet; then
    rm -f -- "$output"
    echo 'The native OS Probe object producer failed.' >&2
    exit 1
fi
