#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || ! -d $1 ]]; then
    echo 'Usage: ./Tools/Native/Construct-Segmented-Compiler-Toolset.sh <existing-separate-output-directory>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
output_root=$(CDPATH= cd -- "$1" && pwd -P) || exit 64
candidate_root="$repository_root/Artifacts/Native-Segmented-Compiler-Toolset-Candidate"
if [[ $output_root == "$candidate_root" ]]; then
    echo 'The segmented compiler toolset must be constructed in a separate output directory.' >&2
    exit 64
fi

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-segmented-toolset-construction.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-segmented-toolset-construction.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

wvo_staging_wvb="$output_root/Wvo-Staging-Producer.wvb"
image_staging_wvb="$output_root/Compiler-Image-Staging.wvb"
transport_wvb="$output_root/Compiler-Image-Canonical-Transport.wvb"

echo 'START segmented compiler toolset construction phase=build item=1/3 project=WVO-staging'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Compiler/Windvale-Native-X64-Lowering-Staging-Tool.wvproj" \
    "$wvo_staging_wvb" \
    >"$temporary_directory/Build-Wvo-Staging.txt" 2>"$temporary_directory/Build-Wvo-Staging.err" || exit $?
echo 'PASS  segmented compiler toolset construction phase=build item=1/3 project=WVO-staging'
echo 'START segmented compiler toolset construction phase=build item=2/3 project=compiler-image-staging'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Linker/Windvale-Compiler-Image-Staging.wvproj" \
    "$image_staging_wvb" \
    >"$temporary_directory/Build-Image-Staging.txt" 2>"$temporary_directory/Build-Image-Staging.err" || exit $?
echo 'PASS  segmented compiler toolset construction phase=build item=2/3 project=compiler-image-staging'
echo 'START segmented compiler toolset construction phase=build item=3/3 project=canonical-transport'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Linker/Windvale-Compiler-Image-Canonical-Transport.wvproj" \
    "$transport_wvb" \
    >"$temporary_directory/Build-Transport.txt" 2>"$temporary_directory/Build-Transport.err" || exit $?
echo 'PASS  segmented compiler toolset construction phase=build item=3/3 project=canonical-transport'

construct_pair() {
    local name=$1
    local wvb=$2
    local windows=$3
    local linux=$4
    local work_directory="$temporary_directory/$name"
    local object_prefix="$work_directory/Object"
    local object_manifest="$work_directory/Object.wvop"
    local image_prefix="$work_directory/Image"
    local image_manifest="$work_directory/Image.wvli"
    local canonical_prefix="$work_directory/Canonical"
    local canonical_manifest="$work_directory/Canonical.wvli"
    local transport_line
    local native_entry
    local fragment_count

    mkdir -- "$work_directory" || return 1
    "$script_directory/Stage-Compiler-Wvb.sh" \
        "$wvb" "$object_prefix" "$object_manifest" \
        >"$work_directory/Stage.txt" 2>"$work_directory/Stage.err" || return $?
    "$script_directory/Link-Staged-Compiler-Wvo.sh" \
        "$object_prefix" "$object_manifest" "$image_prefix" "$image_manifest" \
        >"$work_directory/Link.txt" 2>"$work_directory/Link.err" || return $?
    "$script_directory/Transport-Compiler-Image.sh" \
        "$image_prefix" "$image_manifest" "$canonical_prefix" "$canonical_manifest" \
        >"$work_directory/Transport.txt" 2>"$work_directory/Transport.err" || return $?

    transport_line=$(sed -n '/^compiler image transport status=Complete /p' "$work_directory/Transport.txt")
    native_entry=$(printf '%s\n' "$transport_line" | sed -n 's/^.* entry-offset=\([0-9][0-9]*\) chunks=.*$/\1/p')
    fragment_count=$(printf '%s\n' "$transport_line" | sed -n 's/^.* chunks=\([0-9][0-9]*\) manifest-bytes=.*$/\1/p')
    case "$native_entry" in
        ''|*[!0-9]*) return 1 ;;
    esac
    ((fragment_count >= 1 && fragment_count <= 16)) || return 1

    "$script_directory/Package-Hosted-Wvb.sh" image 6 \
        "$wvb" "$canonical_prefix" "$fragment_count" "$native_entry" \
        "$windows" windows \
        >"$work_directory/Windows.txt" 2>"$work_directory/Windows.err" || return $?
    "$script_directory/Package-Hosted-Wvb.sh" image 6 \
        "$wvb" "$canonical_prefix" "$fragment_count" "$native_entry" \
        "$linux" linux \
        >"$work_directory/Linux.txt" 2>"$work_directory/Linux.err"
}

echo 'START segmented compiler toolset construction phase=package item=1/3 family=WVO-staging'
construct_pair Wvo-Staging "$wvo_staging_wvb" \
    "$output_root/windows-x64-wvstage.exe" \
    "$output_root/linux-x64-wvstage.elf" || exit $?
echo 'PASS  segmented compiler toolset construction phase=package item=1/3 family=WVO-staging'
echo 'START segmented compiler toolset construction phase=package item=2/3 family=compiler-image-staging'
construct_pair Image-Staging "$image_staging_wvb" \
    "$output_root/windows-x64-wvlinkstage.exe" \
    "$output_root/linux-x64-wvlinkstage.elf" || exit $?
echo 'PASS  segmented compiler toolset construction phase=package item=2/3 family=compiler-image-staging'
echo 'START segmented compiler toolset construction phase=package item=3/3 family=canonical-transport'
construct_pair Transport "$transport_wvb" \
    "$output_root/windows-x64-wvimagetransport.exe" \
    "$output_root/linux-x64-wvimagetransport.elf" || exit $?
echo 'PASS  segmented compiler toolset construction phase=package item=3/3 family=canonical-transport'

verify_file() {
    local path=$1
    local expected_bytes=$2
    local expected_sha256=$3
    local label=$4
    local actual_bytes
    local digest_line
    local actual_sha256
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

verify_file "$wvo_staging_wvb" 728718 \
    80694188b3f62f27851f8e21d04bcd9450bea01f2fc5fb4e67dfe9b137f77d2b \
    'WVO staging producer WVB' || exit 1
verify_file "$output_root/windows-x64-wvstage.exe" 10601984 \
    e7ce71d35c2439ecf592206cd76b3b1d884bffc6f464e865a228cbf7c3230aae \
    'Windows WVO staging producer' || exit 1
verify_file "$output_root/linux-x64-wvstage.elf" 10604544 \
    131b50ed4da1b3e9514a846730495c2341b1fad62c5ff13d9547953eab503e0e \
    'Linux WVO staging producer' || exit 1
verify_file "$image_staging_wvb" 81530 \
    825445b022cfd8a6b75fc6e0a63df548707bf5251f840d7cf0c33e2cf2ac15c9 \
    'compiler-image staging WVB' || exit 1
verify_file "$output_root/windows-x64-wvlinkstage.exe" 931840 \
    969bc653c765e3d2e24f62afaa50717268df51fcb805f66e927f0f16ab47838f \
    'Windows compiler-image staging application' || exit 1
verify_file "$output_root/linux-x64-wvlinkstage.elf" 933888 \
    d5909f461c10c6529f881350e86d288cdb40a6ed0b600b75ada86037265af4b0 \
    'Linux compiler-image staging application' || exit 1
verify_file "$transport_wvb" 23836 \
    d4bdfa7588e4431432a300e0da257507d73846931f5dd1296855b03714d218c8 \
    'compiler-image transport WVB' || exit 1
verify_file "$output_root/windows-x64-wvimagetransport.exe" 269312 \
    e724a5efbffc233fda76f55bfb5cc01c044e221882b5de5f247b0ab236726f81 \
    'Windows compiler-image transport application' || exit 1
verify_file "$output_root/linux-x64-wvimagetransport.elf" 270336 \
    9ff5401eca1ffd93a49077dd6ebc56c446c59939379a481f22662465fc3cf6db \
    'Linux compiler-image transport application' || exit 1

for application in \
    "$output_root/linux-x64-wvstage.elf" \
    "$output_root/linux-x64-wvlinkstage.elf" \
    "$output_root/linux-x64-wvimagetransport.elf"; do
    if [[ ! -x $application ]]; then
        echo "The Linux application is not executable: $application" >&2
        exit 1
    fi
done

echo 'native segmented compiler toolset construction status=Complete artifacts=9'
