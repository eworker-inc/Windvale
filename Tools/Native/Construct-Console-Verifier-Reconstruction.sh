#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || ! -d $1 ]]; then
    echo 'Usage: ./Tools/Native/Construct-Console-Verifier-Reconstruction.sh <existing-separate-output-directory>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
output_root=$(CDPATH= cd -- "$1" && pwd -P) || exit 64
candidate_root=$(CDPATH= cd -- "$repository_root/Artifacts/Native-Console-Application-Verifier-Candidate" && pwd -P) || exit 1
if [[ $output_root == "$candidate_root" ]]; then
    echo 'The console-verifier reconstruction must use a separate output directory.' >&2
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
    local actual_bytes digest_line actual_sha256
    [[ -f $path ]] || return 1
    actual_bytes=$(wc -c < "$path") || return 1
    if [[ $actual_bytes -ne $expected_bytes ]]; then
        echo "The $label identity is invalid." >&2
        return 1
    fi
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    if [[ $actual_sha256 != "$expected_sha256" ]]; then
        echo "The $label identity is invalid." >&2
        return 1
    fi
}

verify_file "$hosted_toolset/SHA256SUMS" \
    6927 60f66c785c8dc7352ad394dee5ffd4da4b0f62370c47bdf2978ff0d7a34abd67 \
    'hosted toolset inventory' || exit 1
(cd -- "$hosted_toolset" && sha256sum --check --strict --quiet SHA256SUMS) || exit 1
verify_file "$construction/SHA256SUMS" \
    5064 4c69c2e03e5f9ff5810d3e494167da7b6e8c34c5f630f5af8dbdcebfe0205779 \
    'publisher construction inventory' || exit 1
(cd -- "$construction" && sha256sum --check --strict --quiet SHA256SUMS) || exit 1
verify_file "$startup_root/Windows-X64-Hosted-Inspector.wva" 9437 \
    f706848709e9c217f31dce6733b8aa3e94518b6f371cbd5ccc8af63603edb495 \
    'Windows inspector startup source' || exit 1
verify_file "$startup_root/Linux-X64-Hosted-Inspector.wva" 5214 \
    01603c6b945b4e03ebef1d3d5bf691a5e05bf2e2630d6466e1db1028b8c9c005 \
    'Linux inspector startup source' || exit 1

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-console-verifier-reconstruction.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-console-verifier-reconstruction.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

wvb="$output_root/Console-Application-Verifier.wvb"
wvo="$output_root/Console-Application-Verifier.wvo"
fragment="$temporary_directory/Console-Application-Verifier.bin"
windows_application="$output_root/windows-x64-wvappverify.exe"
linux_application="$output_root/linux-x64-wvappverify.elf"
windows_startup="$temporary_directory/Windows-Startup.wvo"
linux_startup="$temporary_directory/Linux-Startup.wvo"

"$script_directory/Build-Wvb.sh" "$repository_root/Windvale-Console-Application-Verifier.wvproj" "$wvb" \
    >"$temporary_directory/Build.out" 2>"$temporary_directory/Build.err" || exit $?
verify_file "$wvb" 105006 \
    1dcd5f2aeebd974649e64c90d9f473e1e75f7d13dbcde2814de1dded72cf2c0c \
    'console-verifier WVB' || exit 1

"$script_directory/Lower-Wvb-To-Wvo.sh" "$wvb" "$wvo" \
    >"$temporary_directory/Lower.out" 2>"$temporary_directory/Lower.err" || exit $?
verify_file "$wvo" 1049519 \
    51292e4d300d4a6bb6ce4879915bba5304de70c9deafdf4eb6ff6a54a6dbf150 \
    'console-verifier WVO' || exit 1

"$script_directory/Link-Wvo.sh" 0 Main "$fragment" "$wvo" \
    >"$temporary_directory/Link.out" 2>"$temporary_directory/Link.err" || exit $?
grep -Fx 'entry name=Main address=19221' "$temporary_directory/Link.out" >/dev/null || exit 1
verify_file "$fragment" 1045627 \
    96fee2a235db667b161db2eff71625dc714f842f82e74dcf22c0aa03b1cdbffa \
    'console-verifier linked fragment' || exit 1

"$script_directory/Assemble-Wva.sh" "$startup_root/Windows-X64-Hosted-Inspector.wva" "$windows_startup" \
    >"$temporary_directory/Windows-Assemble.out" 2>"$temporary_directory/Windows-Assemble.err" || exit $?
verify_file "$windows_startup" 3927 \
    1bb785d5a06c40b91e45ebdc26b33ae33cb8ee7b244daffaa30ee59b9509edf3 \
    'Windows inspector startup WVO' || exit 1
"$script_directory/Assemble-Wva.sh" "$startup_root/Linux-X64-Hosted-Inspector.wva" "$linux_startup" \
    >"$temporary_directory/Linux-Assemble.out" 2>"$temporary_directory/Linux-Assemble.err" || exit $?
verify_file "$linux_startup" 2291 \
    5d316c109b5c8964c019c44f96f42370408820c7db1ec278268cef541ba17ebb \
    'Linux inspector startup WVO' || exit 1

construct_target() {
    local target_name=$1 target=$2 console_leaf=$3 file_input_leaf=$4 diagnostic_leaf=$5 startup=$6 application=$7
    local target_directory="$temporary_directory/$target_name"
    mkdir -- "$target_directory" || return 1
    "$hosted_tools/wvhostverifierbundle.elf" console-verifier "$fragment" \
        "$console_leaf" \
        "$service_root/Native-X64-Argument-Count-Service.bin" \
        "$service_root/Native-X64-Argument-Service.bin" \
        "$file_input_leaf" \
        "$service_root/Native-X64-Utf8-Service.bin" \
        "$diagnostic_leaf" \
        "$service_root/Native-X64-Enum-Name-Service.bin" \
        "$service_root/Native-X64-Text-Concat-Service.bin" \
        "$service_root/Native-X64-Text-Quote-Service.bin" \
        "$service_root/Native-X64-I32-Format-Service.bin" \
        "$service_root/Native-X64-U32-Format-Service.bin" \
        "$target_directory/Bundle.wvsq" \
        >"$temporary_directory/$target_name-Bundle-Request.out" \
        2>"$temporary_directory/$target_name-Bundle-Request.err" || return 1
    "$construction_tools/wvhostverifierpublisherbasemetadata.elf" console-verifier \
        "$target" 19221 "$target_directory/Bundle.wvsq" "$target_directory/Metadata.wvhv" \
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
    "$hosted_tools/wvhostverifierbytes.elf" console-verifier \
        "$target_directory/Runtime.wvhr" "$target_directory/Platform.wvhb" \
        >"$temporary_directory/$target_name-Platform.out" \
        2>"$temporary_directory/$target_name-Platform.err" || return 1
    "$hosted_tools/wvhostverifierstartup.elf" console-verifier \
        "$target_directory/Runtime.wvhr" "$startup" "$target_directory/Startup.wvsd" \
        >"$temporary_directory/$target_name-Startup.out" \
        2>"$temporary_directory/$target_name-Startup.err" || return 1
    "$hosted_tools/wvhostverifiercompose.elf" console-verifier \
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
verify_file "$windows_application" \
    1063936 05b5f5b3e3999a0ef3537f0908967069a12f17de09753fc90e8a4c7542dc9d3f \
    'Windows console-verifier application' || exit 1

construct_target linux 2 \
    "$service_root/Native-X64-Linux-Console-Output-Service.bin" \
    "$service_root/Native-X64-Linux-File-Input-Service.bin" \
    "$service_root/Native-X64-Linux-Diagnostic-Output-Service.bin" \
    "$linux_startup" "$linux_application" || exit 1
verify_file "$linux_application" \
    1064960 c2700e5e68711d7b8e8a8f7e9573d87dfa27c3676a034a314310ef59045e5f1a \
    'Linux console-verifier application' || exit 1
[[ -x $linux_application ]] || {
    echo 'The Linux console-verifier application is not executable.' >&2
    exit 1
}

echo 'native console verifier reconstruction status=Complete artifacts=4'
