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

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Windvale-Native-X64-Lowering-Staging-Tool.wvproj" \
    "$wvo_staging_wvb" \
    >"$temporary_directory/Build-Wvo-Staging.txt" 2>"$temporary_directory/Build-Wvo-Staging.err" || exit $?
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Windvale-Compiler-Image-Staging.wvproj" \
    "$image_staging_wvb" \
    >"$temporary_directory/Build-Image-Staging.txt" 2>"$temporary_directory/Build-Image-Staging.err" || exit $?
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Windvale-Compiler-Image-Canonical-Transport.wvproj" \
    "$transport_wvb" \
    >"$temporary_directory/Build-Transport.txt" 2>"$temporary_directory/Build-Transport.err" || exit $?

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
    fragment_count=$(printf '%s\n' "$transport_line" | sed -n 's/^.* chunks=\([1-8]\) manifest-bytes=.*$/\1/p')
    case "$native_entry" in
        ''|*[!0-9]*) return 1 ;;
    esac
    case "$fragment_count" in
        [1-8]) ;;
        *) return 1 ;;
    esac

    "$script_directory/Package-Hosted-Wvb.sh" image 6 \
        "$wvb" "$canonical_prefix" "$fragment_count" "$native_entry" \
        "$windows" windows \
        >"$work_directory/Windows.txt" 2>"$work_directory/Windows.err" || return $?
    "$script_directory/Package-Hosted-Wvb.sh" image 6 \
        "$wvb" "$canonical_prefix" "$fragment_count" "$native_entry" \
        "$linux" linux \
        >"$work_directory/Linux.txt" 2>"$work_directory/Linux.err"
}

construct_pair Wvo-Staging "$wvo_staging_wvb" \
    "$output_root/windows-x64-wvstage.exe" \
    "$output_root/linux-x64-wvstage.elf" || exit $?
construct_pair Image-Staging "$image_staging_wvb" \
    "$output_root/windows-x64-wvlinkstage.exe" \
    "$output_root/linux-x64-wvlinkstage.elf" || exit $?
construct_pair Transport "$transport_wvb" \
    "$output_root/windows-x64-wvimagetransport.exe" \
    "$output_root/linux-x64-wvimagetransport.elf" || exit $?

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

verify_file "$wvo_staging_wvb" 442593 \
    7a0463df0f9278174182e799974a3eba8ea3721d7d756c0acd092401f8577f18 \
    'WVO staging producer WVB' || exit 1
verify_file "$output_root/windows-x64-wvstage.exe" 6438912 \
    2e3a1f8057b71c6eafca9c457a2dae5aac099f4b4fdd280fcd534ae1303acb00 \
    'Windows WVO staging producer' || exit 1
verify_file "$output_root/linux-x64-wvstage.elf" 6438912 \
    f8fbc55776bc8573bec74b3610cf1d5c918286befcfde17069f1e39194c214d6 \
    'Linux WVO staging producer' || exit 1
verify_file "$image_staging_wvb" 75553 \
    14521acae6052d08add386833a35dd22c36e0dd07a1fad494961ee8064119d1c \
    'compiler-image staging WVB' || exit 1
verify_file "$output_root/windows-x64-wvlinkstage.exe" 852480 \
    f6c9f4ca4bf5983a72888305d42622969aa861d31def21eee980800781e1d3f1 \
    'Windows compiler-image staging application' || exit 1
verify_file "$output_root/linux-x64-wvlinkstage.elf" 851968 \
    bc703c9893107019f19d8e1d1cefd138f7d1b2acfa83d2b70054c8c6063bd2b8 \
    'Linux compiler-image staging application' || exit 1
verify_file "$transport_wvb" 23836 \
    dc5f460ce89bcce2678092030376c8ddc928e682b263af2a73ba2a57034b6d4d \
    'compiler-image transport WVB' || exit 1
verify_file "$output_root/windows-x64-wvimagetransport.exe" 269312 \
    1a5e4c7e232f30a1b97c90d7fef2ef03fa98de4a968fac2d07cc497a03248729 \
    'Windows compiler-image transport application' || exit 1
verify_file "$output_root/linux-x64-wvimagetransport.elf" 270336 \
    30386b1e571b5b444befbfb7c15ee9ce5cb30e7744cf84ddfee89cbf1e2e8108 \
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
