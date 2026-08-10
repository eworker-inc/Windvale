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

source_wvb="$repository_root/Artifacts/Native-Front-Door/Wvb/Wvb-Runner.wvb"
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

check_file "$source_wvb" 90009 3b881147e5e6c8298cf249e6e02c9f18ed4a677d49ef0a307427465795a1c626 || exit 1
check_file "$hosted_toolset/SHA256SUMS" 6927 430171a9157560acb57e6f84aa772429b436059867892ee2408839057e0eeebc || exit 1
(cd -- "$hosted_toolset" && sha256sum --check --strict --quiet SHA256SUMS) || exit 1
check_file "$construction/SHA256SUMS" 5064 12b7cafbfeafcf1fc667e074ea0670f353bc883131d8a2f180008019f07d03d5 || exit 1
(cd -- "$construction" && sha256sum --check --strict --quiet SHA256SUMS) || exit 1
check_file "$startup_root/Windows-X64-Hosted-Inspector.wva" 9437 f706848709e9c217f31dce6733b8aa3e94518b6f371cbd5ccc8af63603edb495 || exit 1
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

cp -- "$source_wvb" "$wvb" || exit 1
check_file "$wvb" 90009 3b881147e5e6c8298cf249e6e02c9f18ed4a677d49ef0a307427465795a1c626 || exit 1
"$repository_root/Tools/Native/Lower-Wvb-To-Wvo.sh" "$wvb" "$wvo" >"$temporary_directory/Lower.out" 2>"$temporary_directory/Lower.err" || exit 1
check_file "$wvo" 761854 e92eed5006a7a98609173c0ed73e66a7aec5e152d8556c9174cab928b946a505 || exit 1
"$repository_root/Tools/Native/Link-Wvo.sh" 0 Main "$fragment" "$wvo" >"$temporary_directory/Link.out" 2>"$temporary_directory/Link.err" || exit 1
[[ $(grep -Fxc 'entry name=Main address=10049' "$temporary_directory/Link.out") -eq 1 ]] || exit 1
check_file "$fragment" 761278 d602b50d9057f0aad1bb7dca32e624cf78a78244e53ec1a053455caf66a02212 || exit 1

"$repository_root/Tools/Native/Assemble-Wva.sh" "$startup_root/Windows-X64-Hosted-Inspector.wva" "$windows_startup" >"$temporary_directory/Windows-Assemble.out" 2>"$temporary_directory/Windows-Assemble.err" || exit 1
check_file "$windows_startup" 3927 1bb785d5a06c40b91e45ebdc26b33ae33cb8ee7b244daffaa30ee59b9509edf3 || exit 1
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
    "$construction_tools/wvhostverifierpublisherbasemetadata.elf" wvb-runner "$target" 10049 \
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
check_file "$windows_application" 778240 578ddd302da5fbd8d8e14c9410787f5aa05378429a1aca738ee2057e2f9ac1a5 || exit 1
construct_target linux 2 \
    "$service_root/Native-X64-Linux-Console-Output-Service.bin" \
    "$service_root/Native-X64-Linux-File-Input-Service.bin" \
    "$service_root/Native-X64-Linux-Diagnostic-Output-Service.bin" \
    "$linux_startup" "$linux_application" || exit 1
check_file "$linux_application" 778240 16f39270c239609c6f58b086d0648609fad46860ba9bdd198fa7e6668b628047 || exit 1
chmod +x "$linux_application" || exit 1

echo 'native WVB runner reconstruction status=Complete artifacts=4'
result=0
