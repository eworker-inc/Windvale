#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || ! -d $1 ]]; then
    echo 'Usage: ./Tools/Native/Construct-Wvo-Inspector-Reconstruction.sh <existing-separate-output-directory>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
output_root=$(CDPATH= cd -- "$1" && pwd -P) || exit 64
candidate_root=$(CDPATH= cd -- "$repository_root/Artifacts/Native-Wvo-Object-Candidate" && pwd -P) || exit 1
if [[ $output_root == "$candidate_root" ]]; then
    echo 'The WVO inspector reconstruction must use a separate output directory.' >&2
    exit 64
fi

hosted_toolset="$repository_root/Artifacts/Native-Hosted-Container-Toolset-Candidate"
construction="$repository_root/Artifacts/Native-Hosted-Verifier-Publisher-Construction-Candidate"
hosted_tools="$hosted_toolset/linux-x64"
construction_tools="$construction/linux-x64"
service_root="$repository_root/Runtime/Windvale.Native/Consumers"
startup_root="$repository_root/Linker/Startup"

verify_file() {
    local path=$1 expected_bytes=$2 expected_sha256=$3 label=$4
    local actual_bytes actual_line actual_sha256
    [[ -f $path ]] || return 1
    actual_bytes=$(wc -c < "$path") || return 1
    if [[ $actual_bytes -ne $expected_bytes ]]; then
        echo "The $label identity is invalid." >&2
        return 1
    fi
    actual_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${actual_line%% *}
    if [[ $actual_sha256 != "$expected_sha256" ]]; then
        echo "The $label identity is invalid." >&2
        return 1
    fi
}

verify_file "$hosted_toolset/SHA256SUMS" 6927 \
    dc1899b252a8ad0f75eeee33cdec82d9cbbba40c7ba8115bb55aaad0b9dd00c8 \
    'hosted toolset inventory' || exit 1
(cd -- "$hosted_toolset" && sha256sum --check --strict --quiet SHA256SUMS) || exit 1
verify_file "$construction/SHA256SUMS" 5064 \
    161787321f0741cc5007bd263afb23f11a7c697a557c70e8c1c09e04877c071a \
    'publisher construction inventory' || exit 1
(cd -- "$construction" && sha256sum --check --strict --quiet SHA256SUMS) || exit 1
verify_file "$startup_root/Windows-X64-Hosted-Inspector.wva" 9617 \
    865c29d2f83740e70be173f6116b29b0fa9eb4836f52e96200eb508f6fdbb789 \
    'Windows inspector startup source' || exit 1
verify_file "$startup_root/Linux-X64-Hosted-Inspector.wva" 5214 \
    01603c6b945b4e03ebef1d3d5bf691a5e05bf2e2630d6466e1db1028b8c9c005 \
    'Linux inspector startup source' || exit 1

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-wvo-inspector-reconstruction.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-wvo-inspector-reconstruction.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

wvb="$output_root/Wvo-Object.wvb"
wvo="$output_root/Wvo-Object.wvo"
windows_application="$output_root/Wvo-Object.exe"
linux_application="$output_root/Wvo-Object.elf"
fragment="$temporary_directory/Wvo-Object.bin"
enum_request="$temporary_directory/Wvo-Object.wveq"
enum_service="$temporary_directory/Enum-Service.bin"
windows_startup="$temporary_directory/Windows-Startup.wvo"
linux_startup="$temporary_directory/Linux-Startup.wvo"

"$script_directory/Build-Wvb.sh" "$repository_root/Windvale-Wvo-Object.wvproj" "$wvb" \
    >"$temporary_directory/Build.out" 2>"$temporary_directory/Build.err" || exit $?
verify_file "$wvb" 61008 \
    a630d49f0549c865644d8052fbff7e8bf2b6a6dcd013e1187d4356d49cd188db \
    'WVO inspector WVB' || exit 1

"$hosted_tools/wvhostenumrequest.elf" "$wvb" "$enum_request" \
    >"$temporary_directory/Enum-Request.out" \
    2>"$temporary_directory/Enum-Request.err" || exit $?
verify_file "$enum_request" 945 \
    7129a003ae3d0e795f5aea61e4e8d8f25ba4fb93180f2538bea9f04a3c0bdab6 \
    'WVO inspector enum request' || exit 1
"$hosted_tools/wvhostenumservice.elf" "$enum_request" "$enum_service" \
    >"$temporary_directory/Enum-Service.out" \
    2>"$temporary_directory/Enum-Service.err" || exit $?
verify_file "$enum_service" 1244 \
    577ffaee02e64b0956f73d5ca44d65afa262cf476ae5eee86a899ffc575788d1 \
    'WVO inspector enum service' || exit 1

"$script_directory/Lower-Wvb-To-Wvo.sh" "$wvb" "$wvo" \
    >"$temporary_directory/Lower.out" 2>"$temporary_directory/Lower.err" || exit $?
verify_file "$wvo" 591723 \
    f45b14c33a7615209a2a16f6caf0bee041bdb5e2f46fd868792222e774fdb30c \
    'WVO inspector native object' || exit 1

"$script_directory/Link-Wvo.sh" 0 Main "$fragment" "$wvo" \
    >"$temporary_directory/Link.out" 2>"$temporary_directory/Link.err" || exit $?
grep -Fx 'entry name=Main address=82280' "$temporary_directory/Link.out" >/dev/null || exit 1
verify_file "$fragment" 587529 \
    f318ee573b149aac169b67369e90dbacc6451fc129022bfb4e62b2ceff9cfba4 \
    'WVO inspector linked fragment' || exit 1

"$script_directory/Assemble-Wva.sh" \
    "$startup_root/Windows-X64-Hosted-Inspector.wva" "$windows_startup" \
    >"$temporary_directory/Windows-Assemble.out" 2>"$temporary_directory/Windows-Assemble.err" || exit $?
verify_file "$windows_startup" 4017 \
    95ff213a8e59f28d148eb8223a100a5b24dcbc3eb1b444264783a860f159fe49 \
    'Windows inspector startup WVO' || exit 1
"$script_directory/Assemble-Wva.sh" \
    "$startup_root/Linux-X64-Hosted-Inspector.wva" "$linux_startup" \
    >"$temporary_directory/Linux-Assemble.out" 2>"$temporary_directory/Linux-Assemble.err" || exit $?
verify_file "$linux_startup" 2291 \
    5d316c109b5c8964c019c44f96f42370408820c7db1ec278268cef541ba17ebb \
    'Linux inspector startup WVO' || exit 1

construct_target() {
    local target_name=$1 target=$2 console_leaf=$3 file_input_leaf=$4 diagnostic_leaf=$5 startup=$6 application=$7
    local target_directory="$temporary_directory/$target_name"
    mkdir -- "$target_directory" || return 1
    "$hosted_tools/wvhostverifierbundle.elf" wvo-inspector "$fragment" \
        "$console_leaf" \
        "$service_root/Native-X64-Argument-Count-Service.bin" \
        "$service_root/Native-X64-Argument-Service.bin" \
        "$file_input_leaf" \
        "$service_root/Native-X64-Utf8-Service.bin" \
        "$diagnostic_leaf" \
        "$enum_service" \
        "$service_root/Native-X64-Text-Concat-Service.bin" \
        "$service_root/Native-X64-Text-Quote-Service.bin" \
        "$service_root/Native-X64-I32-Format-Service.bin" \
        "$service_root/Native-X64-U32-Format-Service.bin" \
        "$target_directory/Bundle.wvsq" \
        >"$temporary_directory/$target_name-Bundle-Request.out" \
        2>"$temporary_directory/$target_name-Bundle-Request.err" || return 1
    "$construction_tools/wvhostverifierpublisherbasemetadata.elf" wvo-inspector \
        "$target" 82280 "$target_directory/Bundle.wvsq" "$target_directory/Metadata.wvhv" \
        >"$temporary_directory/$target_name-Metadata.out" \
        2>"$temporary_directory/$target_name-Metadata.err" || return 1
    "$construction_tools/wvhostverifierpublisherbaseruntime.elf" \
        "$target_directory/Metadata.wvhv" "$target_directory/Runtime.wvhr" \
        >"$temporary_directory/$target_name-Runtime.out" \
        2>"$temporary_directory/$target_name-Runtime.err" || return 1
    "$hosted_tools/wvhostbundle.elf" \
        "$target_directory/Bundle.wvsq" "$target_directory/Bundle.wvsi" \
        >"$temporary_directory/$target_name-Bundle.out" \
        2>"$temporary_directory/$target_name-Bundle.err" || return 1
    "$hosted_tools/wvhostverifierbytes.elf" wvo-inspector \
        "$target_directory/Runtime.wvhr" "$target_directory/Platform.wvhb" \
        >"$temporary_directory/$target_name-Platform.out" \
        2>"$temporary_directory/$target_name-Platform.err" || return 1
    "$hosted_tools/wvhostverifierstartup.elf" wvo-inspector \
        "$target_directory/Runtime.wvhr" "$startup" "$target_directory/Startup.wvsd" \
        >"$temporary_directory/$target_name-Startup.out" \
        2>"$temporary_directory/$target_name-Startup.err" || return 1
    "$hosted_tools/wvhostverifiercompose.elf" wvo-inspector \
        "$target_directory/Runtime.wvhr" "$target_directory/Platform.wvhb" \
        "$target_directory/Startup.wvsd" "$target_directory/Bundle.wvsi" "$application" \
        >"$temporary_directory/$target_name-Compose.out" \
        2>"$temporary_directory/$target_name-Compose.err"
}

construct_target windows 1 \
    "$service_root/Native-X64-Windows-Console-Output-Service.bin" \
    "$service_root/Native-X64-Windows-File-Input-Service.bin" \
    "$service_root/Native-X64-Windows-Diagnostic-Output-Service.bin" \
    "$windows_startup" "$windows_application" || exit 1
verify_file "$windows_application" 606720 \
    8c6f30b0b55898776d8dc394ea763313527650a361ceb6f478ffad48979084f1 \
    'Windows WVO inspector application' || exit 1

construct_target linux 2 \
    "$service_root/Native-X64-Linux-Console-Output-Service.bin" \
    "$service_root/Native-X64-Linux-File-Input-Service.bin" \
    "$service_root/Native-X64-Linux-Diagnostic-Output-Service.bin" \
    "$linux_startup" "$linux_application" || exit 1
chmod +x "$linux_application" || exit 1
verify_file "$linux_application" 606208 \
    f94d2e16da76c949e15978bd879bff38205685be08d7afa1670f48d3f6592ea1 \
    'Linux WVO inspector application' || exit 1
[[ -x $linux_application ]] || {
    echo 'The Linux WVO inspector application is not executable.' >&2
    exit 1
}

echo 'native WVO inspector reconstruction status=Complete artifacts=4'
