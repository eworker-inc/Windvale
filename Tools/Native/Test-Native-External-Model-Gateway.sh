#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Native-External-Model-Gateway.sh' >&2
    exit 64
fi
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-native-model-gateway.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-native-model-gateway.*)
            rm -f -- "$work"/*
            rmdir -- "$work"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $work" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

echo 'START native external model gateway phase=supervisor item=1/4'
node "$repository_root/Tools/Models/Test-Native-External-Model-Gateway-Supervisor.mjs" || exit $?
echo 'PASS  native external model gateway phase=supervisor item=1/4'

echo 'START native external model gateway phase=objects item=2/4'
for name in Probe Host Windows Linux; do
    case "$name" in
        Probe) source_path="$repository_root/Tests/Native/X64-External-Model-Gateway-Probe.wva" ;;
        Host) source_path="$repository_root/Runtime/Native/X64-External-Model-Gateway-Host.wva" ;;
        Windows) source_path="$repository_root/Runtime/Native/Windows-X64-External-Model-Gateway.wva" ;;
        Linux) source_path="$repository_root/Runtime/Native/Linux-X64-External-Model-Gateway.wva" ;;
    esac
    "$script_directory/Assemble-Wva.sh" "$source_path" "$work/$name-A.wvo" >/dev/null || exit $?
    "$script_directory/Assemble-Wva.sh" "$source_path" "$work/$name-B.wvo" >/dev/null || exit $?
    cmp -s -- "$work/$name-A.wvo" "$work/$name-B.wvo" || exit 1
    "$script_directory/Check-Wvo.sh" "$work/$name-A.wvo" >/dev/null || exit $?
done
echo 'PASS  native external model gateway phase=objects item=2/4'

echo 'START native external model gateway phase=images item=3/4'
"$script_directory/Link-Wvo.sh" 0 Model_gateway_host_entry "$work/Windows-Image.chunk-0" \
    "$work/Probe-A.wvo" "$work/Host-A.wvo" "$work/Windows-A.wvo" >"$work/Windows-Link.txt" || exit $?
"$script_directory/Link-Wvo.sh" 0 Model_gateway_host_entry "$work/Linux-Image.chunk-0" \
    "$work/Probe-A.wvo" "$work/Host-A.wvo" "$work/Linux-A.wvo" >"$work/Linux-Link.txt" || exit $?
windows_entry=$(sed -n 's/^entry name=Model_gateway_host_entry address=//p' "$work/Windows-Link.txt")
linux_entry=$(sed -n 's/^entry name=Model_gateway_host_entry address=//p' "$work/Linux-Link.txt")
[[ $windows_entry =~ ^[0-9]+$ && $linux_entry =~ ^[0-9]+$ ]] || exit 1
"$script_directory/Build-Cached-Hosted-Application.sh" 6 \
    "$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo.wvb" "$work/Windows-Image" 1 \
    "$windows_entry" "$work/Model-Worker.exe" windows >/dev/null || exit $?
"$script_directory/Build-Cached-Hosted-Application.sh" 6 \
    "$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo.wvb" "$work/Linux-Image" 1 \
    "$linux_entry" "$work/Model-Worker.elf" linux >/dev/null || exit $?
echo 'PASS  native external model gateway phase=images item=3/4'

echo 'START native external model gateway phase=execute item=4/4'
node "$repository_root/Tools/Models/Test-Native-External-Model-Gateway-Execution.mjs" \
    "$work/Model-Worker.elf" || exit $?
echo 'PASS  native external model gateway phase=execute item=4/4'
echo 'native external model gateway status=Passed cases=14 local-result=0 cross-host-images=Verified public-network=0 real-credentials=0'
