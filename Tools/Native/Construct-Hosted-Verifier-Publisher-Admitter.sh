#!/usr/bin/env bash
set -uo pipefail

usage() {
    echo 'Usage: ./Tools/Native/Construct-Hosted-Verifier-Publisher-Admitter.sh <windows|linux> <output.exe|output.elf>' >&2
    exit 64
}

[[ $# -eq 2 ]] || usage
target_name=$1
output=$2
case "$target_name:$output" in
    windows:*.exe)
        target=1
        console_leaf=Native-X64-Windows-Console-Output-Service.bin
        file_input_leaf=Native-X64-Windows-File-Input-Service.bin
        diagnostic_leaf=Native-X64-Windows-Diagnostic-Output-Service.bin
        hosted_startup=Windows-X64-Hosted-Verifier.wvo
        application_bytes=570368
        application_sha256=1407ed428387986e170b4d8394e9a0a6295408ef668d5d6e16d719102428dd4f
        ;;
    linux:*.elf)
        target=2
        console_leaf=Native-X64-Linux-Console-Output-Service.bin
        file_input_leaf=Native-X64-Linux-File-Input-Service.bin
        diagnostic_leaf=Native-X64-Linux-Diagnostic-Output-Service.bin
        hosted_startup=Linux-X64-Hosted-Verifier.wvo
        application_bytes=569344
        application_sha256=27fff54e139228586a6948aa234de60e5d4f5439e6b0616a55c057d4ad8661c2
        ;;
    *) usage ;;
esac
[[ ! -e "$output" ]] || {
    echo 'Refusing to replace an existing publisher-admitter construction output.' >&2
    exit 1
}

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
hosted_toolset="$repository_root/Artifacts/Native-Hosted-Container-Toolset-Candidate"
construction="$repository_root/Artifacts/Native-Hosted-Verifier-Publisher-Construction-Candidate"
tools="$construction/linux-x64"
service_root="$repository_root/Runtime/Windvale.Native/Consumers"
consumer_root="$repository_root/Linker/Reference/Consumers"

verify_file() {
    local path=$1 bytes=$2 digest=$3 description=$4
    [[ -f "$path" && $(wc -c < "$path") -eq $bytes ]] || {
        echo "The $description byte length is invalid." >&2
        return 1
    }
    local actual
    actual=$(sha256sum -- "$path") || return 1
    [[ ${actual%% *} == "$digest" ]] || {
        echo "The $description digest is invalid: $path expected=$digest actual=${actual%% *}" >&2
        return 1
    }
}

verify_file "$hosted_toolset/SHA256SUMS" 6927 c32a885162aff80da67a839c6c6c247f8d2da5ed420337823fccd62dcfc26c89 'hosted toolset inventory' || exit 1
(cd -- "$hosted_toolset" && sha256sum --check --strict --quiet SHA256SUMS) || exit 1
verify_file "$construction/SHA256SUMS" 5064 15502d44e9578a1ce332fe390764c811a82fee8b3a0f8d9ee80aa158c9bbb334 'publisher construction inventory' || exit 1
(cd -- "$construction" && sha256sum --check --strict --quiet SHA256SUMS) || exit 1

temporary_directory=$(mktemp -d "${TMPDIR:-/tmp}/windvale-hosted-verifier-publisher-admitter.XXXXXXXX") || exit 1
phase=link
result=1
cleanup() {
    if [[ $result -ne 0 ]]; then
        echo "publisher admitter construction status=Rejected phase=$phase" >&2
        rm -f -- "$output"
    fi
    rm -rf -- "$temporary_directory"
}
trap cleanup EXIT

"$repository_root/Tools/Native/Link-Wvo.sh" 0 Main "$temporary_directory/Admission.bin" "$construction/Publisher-Application-Admission-Tool.wvo" > "$temporary_directory/Link.txt" || exit 1
grep -Fx 'entry name=Main address=0' "$temporary_directory/Link.txt" >/dev/null || exit 1
verify_file "$temporary_directory/Admission.bin" 554354 7a91f2db5e4c42b88f696cd4ef7cc31eb668f88b811c856ef09c5819911fa453 'publisher-admission fragment' || exit 1

phase=bundle-request
"$hosted_toolset/linux-x64/wvhostverifierbundle.elf" "$temporary_directory/Admission.bin" "$service_root/$console_leaf" "$service_root/Native-X64-Argument-Count-Service.bin" "$service_root/Native-X64-Argument-Service.bin" "$service_root/$file_input_leaf" "$service_root/Native-X64-Utf8-Service.bin" "$service_root/$diagnostic_leaf" "$temporary_directory/Bundle.wvsq" >/dev/null || exit 1
phase=metadata
"$tools/wvhostverifierpublisherbasemetadata.elf" publisher-admission "$target" 0 "$temporary_directory/Bundle.wvsq" "$temporary_directory/Metadata.wvhv" || exit 1
phase=runtime
"$tools/wvhostverifierpublisherbaseruntime.elf" "$temporary_directory/Metadata.wvhv" "$temporary_directory/Runtime.wvhr" || exit 1
phase=bundle
"$hosted_toolset/linux-x64/wvhostbundle.elf" "$temporary_directory/Bundle.wvsq" "$temporary_directory/Bundle.wvsi" >/dev/null || exit 1
phase=platform
"$hosted_toolset/linux-x64/wvhostverifierbytes.elf" publisher-admission "$temporary_directory/Runtime.wvhr" "$temporary_directory/Platform.wvhb" >/dev/null || exit 1
phase=startup
"$hosted_toolset/linux-x64/wvhostverifierstartup.elf" publisher-admission "$temporary_directory/Runtime.wvhr" "$consumer_root/$hosted_startup" "$temporary_directory/Startup.wvsd" >/dev/null || exit 1
phase=compose
"$hosted_toolset/linux-x64/wvhostverifiercompose.elf" publisher-admission "$temporary_directory/Runtime.wvhr" "$temporary_directory/Platform.wvhb" "$temporary_directory/Startup.wvsd" "$temporary_directory/Bundle.wvsi" "$temporary_directory/Admitter.application" >/dev/null || exit 1
verify_file "$temporary_directory/Admitter.application" "$application_bytes" "$application_sha256" 'completed publisher admitter' || exit 1
cp -- "$temporary_directory/Admitter.application" "$output" || exit 1
verify_file "$output" "$application_bytes" "$application_sha256" 'published construction output' || exit 1
echo "publisher admitter construction status=Valid target=$target_name bytes=$application_bytes"
result=0
