#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || ! -d $1 ]]; then
    echo 'Usage: ./Tools/Native/Construct-Wvb-Runner-Reconstruction.sh <existing-output-directory>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
output_root=$(CDPATH= cd -- "$1" && pwd -P) || exit 64
candidate_root=$(CDPATH= cd -- "$repository_root/Artifacts/Native-Wvb-Runner-Candidate" && pwd -P)
if [[ $output_root == "$candidate_root" ]]; then
    echo 'The WVB-runner reconstruction must use a separate output directory.' >&2
    exit 64
fi

source_project="$repository_root/Projects/Tools/Windvale-Wvb-Runner.wvproj"
hosted_toolset="$repository_root/Artifacts/Native-Hosted-Container-Toolset-Candidate"
construction="$repository_root/Artifacts/Native-Hosted-Verifier-Publisher-Construction-Candidate"
hosted_tools="$hosted_toolset/linux-x64"
construction_tools="$construction/linux-x64"
service_root="$repository_root/Runtime/Windvale.Native/Consumers"
startup_root="$repository_root/Linker/Startup"

check_file() {
    local path=$1 expected_bytes=$2 expected_sha=$3
    [[ -f $path ]] || return 1
    [[ $(wc -c < "$path") -eq $expected_bytes ]] || return 1
    printf '%s  %s\n' "$expected_sha" "$path" | sha256sum --check --strict --quiet
}

check_file "$hosted_toolset/SHA256SUMS" 6927 3051a9c328c04a53dd0f0a54a8f83c7d1f12c3947df3bd19d7ad066ac3f09954 || exit 1
(cd -- "$hosted_toolset" && sha256sum --check --strict --quiet SHA256SUMS) || exit 1
check_file "$construction/SHA256SUMS" 5064 15502d44e9578a1ce332fe390764c811a82fee8b3a0f8d9ee80aa158c9bbb334 || exit 1
(cd -- "$construction" && sha256sum --check --strict --quiet SHA256SUMS) || exit 1
check_file "$startup_root/Windows-X64-Hosted-Inspector.wva" 9617 865c29d2f83740e70be173f6116b29b0fa9eb4836f52e96200eb508f6fdbb789 || exit 1
check_file "$startup_root/Linux-X64-Hosted-Inspector.wva" 5214 01603c6b945b4e03ebef1d3d5bf691a5e05bf2e2630d6466e1db1028b8c9c005 || exit 1

temporary_directory=$(mktemp -d "${TMPDIR:-/tmp}/windvale-wvb-runner-reconstruction.XXXXXX") || exit 1
result=1
cleanup() {
    rm -rf -- "$temporary_directory"
    exit "$result"
}
trap cleanup EXIT HUP INT TERM

wvb="$output_root/Wvb-Runner.wvb"
wvo="$output_root/Wvb-Runner.wvo"
fragment="$temporary_directory/Wvb-Runner.bin"
windows_application="$output_root/windows-x64-wvrun.exe"
linux_application="$output_root/linux-x64-wvrun.elf"
windows_startup="$temporary_directory/Windows-Startup.wvo"
linux_startup="$temporary_directory/Linux-Startup.wvo"

"$repository_root/Tools/Native/Build-Wvb.sh" "$source_project" "$wvb" >"$temporary_directory/Build.out" 2>"$temporary_directory/Build.err" || exit 1
check_file "$wvb" 151488 e5948f52146a5c3be9901e2dc8c3b9e4f1ba7b2fdc75624c43f2a3a7b807d264 || exit 1
"$repository_root/Tools/Native/Lower-Wvb-To-Wvo.sh" "$wvb" "$wvo" >"$temporary_directory/Lower.out" 2>"$temporary_directory/Lower.err" || exit 1
check_file "$wvo" 1371883 f482eface9f6857e6a851a4503b343c6c848aa99fdbe28385aa951bc8e463905 || exit 1
"$repository_root/Tools/Native/Link-Wvo.sh" 0 Main "$fragment" "$wvo" >"$temporary_directory/Link.out" 2>"$temporary_directory/Link.err" || exit 1
[[ $(grep -Fxc 'entry name=Main address=60426' "$temporary_directory/Link.out") -eq 1 ]] || exit 1
check_file "$fragment" 1369921 f367c6047d696f1a939bba8aedf489f9def4e229512accfd7bb5de1e7d85345a || exit 1

"$repository_root/Tools/Native/Assemble-Wva.sh" "$startup_root/Windows-X64-Hosted-Inspector.wva" "$windows_startup" >"$temporary_directory/Windows-Assemble.out" 2>"$temporary_directory/Windows-Assemble.err" || exit 1
check_file "$windows_startup" 4017 95ff213a8e59f28d148eb8223a100a5b24dcbc3eb1b444264783a860f159fe49 || exit 1
"$repository_root/Tools/Native/Assemble-Wva.sh" "$startup_root/Linux-X64-Hosted-Inspector.wva" "$linux_startup" >"$temporary_directory/Linux-Assemble.out" 2>"$temporary_directory/Linux-Assemble.err" || exit 1
check_file "$linux_startup" 2291 5d316c109b5c8964c019c44f96f42370408820c7db1ec278268cef541ba17ebb || exit 1

construct_target() {
    local name=$1 target=$2 console_leaf=$3 file_leaf=$4 diagnostic_leaf=$5 startup=$6 application=$7
    local directory="$temporary_directory/$name"
    mkdir -- "$directory" || return 1
    "$hosted_tools/wvhostverifierbundle.elf" wvb-runner "$fragment" "$console_leaf" \
        "$service_root/Native-X64-Argument-Count-Service.bin" \
        "$service_root/Native-X64-Argument-Service.bin" "$file_leaf" \
        "$service_root/Native-X64-Utf8-Service.bin" "$diagnostic_leaf" \
        "$service_root/Native-X64-Text-Concat-Service.bin" \
        "$service_root/Native-X64-I32-Format-Service.bin" \
        "$service_root/Native-X64-U32-Format-Service.bin" \
        "$directory/Bundle.wvsq" >"$temporary_directory/$name-Bundle-Request.out" 2>"$temporary_directory/$name-Bundle-Request.err" || return 1
    "$construction_tools/wvhostverifierpublisherbasemetadata.elf" wvb-runner "$target" 60426 \
        "$directory/Bundle.wvsq" "$directory/Metadata.wvhv" >"$temporary_directory/$name-Metadata.out" 2>"$temporary_directory/$name-Metadata.err" || return 1
    "$construction_tools/wvhostverifierpublisherbaseruntime.elf" \
        "$directory/Metadata.wvhv" "$directory/Runtime.wvhr" >"$temporary_directory/$name-Runtime.out" 2>"$temporary_directory/$name-Runtime.err" || return 1
    "$hosted_tools/wvhostbundle.elf" "$directory/Bundle.wvsq" "$directory/Bundle.wvsi" >"$temporary_directory/$name-Bundle.out" 2>"$temporary_directory/$name-Bundle.err" || return 1
    "$hosted_tools/wvhostverifierbytes.elf" wvb-runner "$directory/Runtime.wvhr" \
        "$directory/Platform.wvhb" >"$temporary_directory/$name-Platform.out" 2>"$temporary_directory/$name-Platform.err" || return 1
    "$hosted_tools/wvhostverifierstartup.elf" wvb-runner "$directory/Runtime.wvhr" \
        "$startup" "$directory/Startup.wvsd" >"$temporary_directory/$name-Startup.out" 2>"$temporary_directory/$name-Startup.err" || return 1
    "$hosted_tools/wvhostverifiercompose.elf" wvb-runner "$directory/Runtime.wvhr" \
        "$directory/Platform.wvhb" "$directory/Startup.wvsd" "$directory/Bundle.wvsi" \
        "$application" >"$temporary_directory/$name-Compose.out" 2>"$temporary_directory/$name-Compose.err"
}

construct_target windows 1 \
    "$service_root/Native-X64-Windows-Console-Output-Service.bin" \
    "$service_root/Native-X64-Windows-File-Input-Service.bin" \
    "$service_root/Native-X64-Windows-Diagnostic-Output-Service.bin" \
    "$windows_startup" "$windows_application" || exit 1
check_file "$windows_application" 1387008 57b91dae115d14da470b265f3ce1f59a44fe94c06f0de4ae99b1c13418118ae4 || exit 1
construct_target linux 2 \
    "$service_root/Native-X64-Linux-Console-Output-Service.bin" \
    "$service_root/Native-X64-Linux-File-Input-Service.bin" \
    "$service_root/Native-X64-Linux-Diagnostic-Output-Service.bin" \
    "$linux_startup" "$linux_application" || exit 1
check_file "$linux_application" 1388544 b6914c6b4d5c3bb069b219ce2cb329b179faf032c8b204648628775fbdfbd25e || exit 1
chmod +x "$linux_application" || exit 1

echo 'native WVB runner reconstruction status=Complete artifacts=4'
result=0
