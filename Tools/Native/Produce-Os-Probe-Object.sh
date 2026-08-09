#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 || $2 != *.wvo ]]; then
    echo 'Usage: ./Tools/Native/Produce-Os-Probe-Object.sh <exceptions|wvb-admission-bridge|native-bridge-and-support|paging|memory|loader> <output.wvo>' >&2
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
    loader)
        expected_bytes=6336
        expected_digest=b310bc0e9aebc7b14c0892bb3dd4b833d42539c2194427a8f333b511d6af3804
        ;;
    *)
        echo 'Usage: ./Tools/Native/Produce-Os-Probe-Object.sh <exceptions|wvb-admission-bridge|native-bridge-and-support|paging|memory|loader> <output.wvo>' >&2
        exit 64
        ;;
esac

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
code_fixture=
if [[ $1 == loader ]]; then
    producer="$repository_root/Artifacts/Native-Os-Probe-Loader-Object-Producer-Candidate/linux-x64-os-probe-loader-object.elf"
    producer_bytes=389120
    producer_digest=616cc30cdd6c46dba15ead2dc7881f4ce53df187e485939337cfd0c5a540dc42
    code_fixture="$repository_root/Artifacts/Native-Os-Probe-Loader-Object-Producer-Candidate/normal-x64-loader.bin"
elif [[ $1 == memory ]]; then
    producer="$repository_root/Artifacts/Native-Os-Probe-Memory-Object-Producer-Candidate/linux-x64-os-probe-memory-object.elf"
    producer_bytes=401408
    producer_digest=02280b115ead806f8b6e2f1dd066d7d06a85ae571d790c66d05daecf2acc6554
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
    { [[ ! -f $code_fixture || $(wc -c < "$code_fixture") -ne 6115 ]] ||
        ! printf '%s  %s\n' \
            '19008f698db52c206dae920cf57ca4461eb009d47d8ecba258d6b021b05a2eed' \
            "$code_fixture" | sha256sum --check --strict --quiet; }; then
    echo 'The native OS Probe loader code fixture identity is invalid.' >&2
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
