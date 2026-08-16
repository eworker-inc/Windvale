#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Wvb-Inspector-Reconstruction.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
hosted="$repository_root/Artifacts/Native-Hosted-Container-Toolset-Candidate/linux-x64"
service_root="$repository_root/Runtime/Windvale.Native/Consumers"
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-wvb-inspector-reconstruction-test.XXXXXXXX") || exit 1

cleanup() {
    case "$work" in
        "$temporary_root"/windvale-wvb-inspector-reconstruction-test.*)
            rm -rf -- "$work"
            ;;
        *)
            echo "Refusing to remove unexpected test path: $work" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

fail() {
    echo "FAIL  WVB inspector reconstruction: $1" >&2
    exit 1
}

prepare_tool() {
    local project=$1
    local name=$2
    "$script_directory/Build-Cached-Project-Wvb.sh" "$project" "$work/$name.wvb" >/dev/null || return $?
    "$script_directory/Lower-Wvb-To-Wvo.sh" "$work/$name.wvb" "$work/$name.wvo" >/dev/null || return $?
    "$script_directory/Link-Wvo.sh" 0 Main "$work/$name-Image.chunk-0" "$work/$name.wvo" >"$work/$name.map" || return $?
    local entry
    entry=$(sed -n 's/^entry name=Main address=//p' "$work/$name.map")
    case "$entry" in ''|*[!0-9]*) return 1 ;; esac
    "$script_directory/Build-Cached-Hosted-Application.sh" 1 \
        "$work/$name.wvb" "$work/$name-Image" 1 "$entry" \
        "$work/$name.elf" linux >/dev/null || return $?
}

echo 'native WVB inspector reconstruction step=construction-tools item=1/4'
prepare_tool "$repository_root/Projects/Linker/Windvale-Native-Hosted-Verifier-Container-Tool.wvproj" wvhostverifiercompose || fail 'container tool'
prepare_tool "$repository_root/Projects/Linker/Windvale-Native-Hosted-Verifier-Platform-Tool.wvproj" wvhostverifierbytes || fail 'platform tool'
prepare_tool "$repository_root/Projects/Linker/Windvale-Native-Hosted-Verifier-Startup-Tool.wvproj" wvhostverifierstartup || fail 'startup tool'
prepare_tool "$repository_root/Projects/Runtime/Windvale-Native-Hosted-Verifier-Service-Bundle-Request-Tool.wvproj" wvhostverifierbundle || fail 'bundle-request tool'
prepare_tool "$repository_root/Projects/Runtime/Windvale-Native-Hosted-Verifier-Publisher-Base-Metadata-Tool.wvproj" wvhostverifierpublisherbasemetadata || fail 'metadata tool'
prepare_tool "$repository_root/Projects/Runtime/Windvale-Native-Hosted-Verifier-Publisher-Base-Runtime-Tool.wvproj" wvhostverifierpublisherbaseruntime || fail 'runtime tool'

echo 'native WVB inspector reconstruction step=application item=2/4'
"$script_directory/Build-Cached-Project-Wvb.sh" \
    "$repository_root/Projects/Examples/Windvale-Wvb-Inspector.wvproj" \
    "$work/Wvb-Inspector.wvb" >"$work/Inspector-Build.txt" || fail 'inspector WVB'
"$hosted/wvhostenumrequest.elf" "$work/Wvb-Inspector.wvb" "$work/Inspector.wveq" >/dev/null || fail 'enum request'
"$hosted/wvhostenumservice.elf" "$work/Inspector.wveq" "$work/Enum-Service.bin" >/dev/null || fail 'enum service'
"$script_directory/Lower-Wvb-To-Wvo.sh" "$work/Wvb-Inspector.wvb" "$work/Wvb-Inspector.wvo" >/dev/null || fail 'inspector lowering'
"$script_directory/Link-Wvo.sh" 0 Main "$work/Wvb-Inspector.bin" "$work/Wvb-Inspector.wvo" >"$work/Inspector.map" || fail 'inspector link'
inspector_entry=$(sed -n 's/^entry name=Main address=//p' "$work/Inspector.map")
case "$inspector_entry" in ''|*[!0-9]*) fail 'inspector entry' ;; esac
"$script_directory/Assemble-Wva.sh" \
    "$repository_root/Linker/Startup/Linux-X64-Hosted-Inspector.wva" \
    "$work/Inspector-Startup.wvo" >/dev/null || fail 'inspector startup'

"$work/wvhostverifierbundle.elf" wvb-inspector \
    "$work/Wvb-Inspector.bin" \
    "$service_root/Native-X64-Linux-Console-Output-Service.bin" \
    "$service_root/Native-X64-Argument-Count-Service.bin" \
    "$service_root/Native-X64-Argument-Service.bin" \
    "$service_root/Native-X64-Linux-File-Input-Service.bin" \
    "$service_root/Native-X64-Utf8-Service.bin" \
    "$service_root/Native-X64-Linux-Diagnostic-Output-Service.bin" \
    "$work/Enum-Service.bin" \
    "$service_root/Native-X64-Text-Concat-Service.bin" \
    "$service_root/Native-X64-Text-Quote-Service.bin" \
    "$service_root/Native-X64-I32-Format-Service.bin" \
    "$service_root/Native-X64-U32-Format-Service.bin" \
    "$work/Bundle.wvsq" >/dev/null || fail 'bundle request'
"$work/wvhostverifierpublisherbasemetadata.elf" wvb-inspector 2 "$inspector_entry" \
    "$work/Bundle.wvsq" "$work/Metadata.wvhv" || fail 'metadata'
"$work/wvhostverifierpublisherbaseruntime.elf" \
    "$work/Metadata.wvhv" "$work/Runtime.wvhr" || fail 'runtime'
"$hosted/wvhostbundle.elf" "$work/Bundle.wvsq" "$work/Bundle.wvsi" >/dev/null || fail 'bundle'
"$work/wvhostverifierbytes.elf" wvb-inspector \
    "$work/Runtime.wvhr" "$work/Platform.wvhb" >/dev/null || fail 'platform'
"$work/wvhostverifierstartup.elf" wvb-inspector \
    "$work/Runtime.wvhr" "$work/Inspector-Startup.wvo" \
    "$work/Startup.wvsd" >/dev/null || fail 'startup'
"$work/wvhostverifiercompose.elf" wvb-inspector \
    "$work/Runtime.wvhr" "$work/Platform.wvhb" "$work/Startup.wvsd" \
    "$work/Bundle.wvsi" "$work/Wvb-Inspector.elf" >/dev/null || fail 'container'
"$work/wvhostverifiercompose.elf" wvb-inspector \
    "$work/Runtime.wvhr" "$work/Platform.wvhb" "$work/Startup.wvsd" \
    "$work/Bundle.wvsi" "$work/Wvb-Inspector-Second.elf" >/dev/null || fail 'second container'
cmp --silent -- "$work/Wvb-Inspector.elf" "$work/Wvb-Inspector-Second.elf" || fail 'nondeterministic container'
echo 'PASS  WVB inspector reconstruction deterministic Linux application'

echo 'native WVB inspector reconstruction step=execute item=3/4'
"$work/Wvb-Inspector.elf" >"$work/Self.txt" 2>&1 || fail 'self-tests'
echo 'PASS  WVB inspector reconstruction self-tests'
"$work/Wvb-Inspector.elf" \
    "$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Return-42.wvb" \
    >"$work/Absent.txt" 2>&1 || fail 'metadata-absent inspection'
grep -F 'module version=1.11 profile=portable name=' "$work/Absent.txt" >/dev/null || fail 'metadata-absent report'
echo 'PASS  WVB inspector reconstruction metadata-absent module'
"$work/Wvb-Inspector.elf" \
    "$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Metadata.wvb" \
    >"$work/Present.txt" 2>&1 || fail 'metadata-present inspection'
grep -F 'module version=1.11 profile=hosted name=' "$work/Present.txt" >/dev/null || fail 'metadata-present module report'
grep -F 'capability index=0 name="process.argument_count"' "$work/Present.txt" >/dev/null || fail 'metadata-present capability report'
echo 'PASS  WVB inspector reconstruction metadata-present module'

echo 'native WVB inspector reconstruction step=identity item=4/4'
fixture="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Metadata.wvb"
[[ $(wc -c < "$fixture") -eq 369 ]] || fail 'metadata fixture length'
fixture_hash=$(sha256sum -- "$fixture") || fail 'metadata fixture digest'
[[ ${fixture_hash%% *} == 94b41f5016722c9e5bf16ace5ec933acc35c14efdd4e08fe11fd582a62b58ffa ]] || fail 'metadata fixture identity'
echo 'native WVB inspector reconstruction status=Passed profile=4 metadata=Present cases=4'
echo 'Tests: 4, Passed: 4, Failed: 0'
