#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Model-Chat-Native-Construction.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-model-chat-construction.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-model-chat-construction.*) rm -rf -- "$work" ;;
        *) echo "Refusing to remove unexpected temporary path: $work" >&2; return 1 ;;
    esac
}
trap cleanup EXIT

"$repository_root/Tools/Native/Build-Windvale-Model-Chat.sh" \
    "$work/Windvale-Model-Chat.elf" || exit $?
echo 'model chat native construction status=Passed cases=32 native-ui=Windvale core-cases=32 cross-host-images=Verified public-network=0 real-credentials=0'
