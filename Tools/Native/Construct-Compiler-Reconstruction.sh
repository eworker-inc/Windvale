#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || ! -d $1 ]]; then
    echo 'Usage: ./Tools/Native/Construct-Compiler-Reconstruction.sh <existing-output-directory>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
output_root=$(CDPATH= cd -- "$1" && pwd -P) || exit 64
wvb_directory="$output_root/Wvb"
windows_directory="$output_root/windows-x64"
linux_directory="$output_root/linux-x64"
mkdir -p -- "$wvb_directory" "$windows_directory" "$linux_directory" || exit 1

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-compiler-reconstruction.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-compiler-reconstruction.*)
            rm -f -- "$temporary_directory"/*
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

wvb="$wvb_directory/Windvale-Compiler.wvb"
windows="$windows_directory/wvcompiler.exe"
linux="$linux_directory/wvcompiler.elf"
driver_wvb="$wvb_directory/Compiler-Build-Driver.wvb"
driver_windows="$windows_directory/wvbuild.exe"
driver_linux="$linux_directory/wvbuild.elf"
frozen_build_driver="$repository_root/Artifacts/Native-Front-Door/linux-x64/wvbuild.elf"
object_prefix="$temporary_directory/Object"
object_manifest="$temporary_directory/Object.wvop"
image_prefix="$temporary_directory/Image"
image_manifest="$temporary_directory/Image.wvli"
canonical_prefix="$temporary_directory/Canonical"
canonical_manifest="$temporary_directory/Canonical.wvli"

"$script_directory/Bootstrap-Compiler.sh" \
    "$repository_root/Artifacts" "$repository_root" "$wvb" \
    >"$temporary_directory/Bootstrap.txt" 2>"$temporary_directory/Bootstrap.err" || exit $?
"$script_directory/Stage-Compiler-Wvb.sh" \
    "$wvb" "$object_prefix" "$object_manifest" \
    >"$temporary_directory/Stage.txt" 2>"$temporary_directory/Stage.err" || exit $?
"$script_directory/Link-Staged-Compiler-Wvo.sh" \
    "$object_prefix" "$object_manifest" "$image_prefix" "$image_manifest" \
    >"$temporary_directory/Link.txt" 2>"$temporary_directory/Link.err" || exit $?
"$script_directory/Transport-Compiler-Image.sh" \
    "$image_prefix" "$image_manifest" "$canonical_prefix" "$canonical_manifest" \
    >"$temporary_directory/Transport.txt" 2>"$temporary_directory/Transport.err" || exit $?

transport_line=$(sed -n '/^compiler image transport status=Complete /p' "$temporary_directory/Transport.txt")
native_entry=$(printf '%s\n' "$transport_line" | sed -n 's/^.* entry-offset=\([0-9][0-9]*\) chunks=.*$/\1/p')
fragment_count=$(printf '%s\n' "$transport_line" | sed -n 's/^.* chunks=\([1-8]\) manifest-bytes=.*$/\1/p')
if [[ $native_entry != 51356 || $fragment_count != 7 ]]; then
    echo 'The canonical compiler image identity is invalid.' >&2
    exit 1
fi

"$script_directory/Package-Hosted-Wvb.sh" image 1 \
    "$wvb" "$canonical_prefix" "$fragment_count" "$native_entry" "$windows" windows \
    >"$temporary_directory/Windows.txt" 2>"$temporary_directory/Windows.err" || exit $?
"$script_directory/Package-Hosted-Wvb.sh" image 1 \
    "$wvb" "$canonical_prefix" "$fragment_count" "$native_entry" "$linux" linux \
    >"$temporary_directory/Linux.txt" 2>"$temporary_directory/Linux.err" || exit $?

"$frozen_build_driver" \
    --workspace "$repository_root/Windvale.wvws" \
    --project "$repository_root/Projects/Tools/Windvale-Compiler-Build-Driver.wvproj" \
    "$driver_wvb" \
    >"$temporary_directory/Driver-Build.txt" 2>"$temporary_directory/Driver-Build.err" || exit $?
driver_object_prefix="$temporary_directory/Driver-Object"
driver_object_manifest="$temporary_directory/Driver-Object.wvop"
driver_image_prefix="$temporary_directory/Driver-Image"
driver_image_manifest="$temporary_directory/Driver-Image.wvli"
driver_canonical_prefix="$temporary_directory/Driver-Canonical"
driver_canonical_manifest="$temporary_directory/Driver-Canonical.wvli"
"$script_directory/Stage-Compiler-Wvb.sh" \
    "$driver_wvb" "$driver_object_prefix" "$driver_object_manifest" \
    >"$temporary_directory/Driver-Stage.txt" 2>"$temporary_directory/Driver-Stage.err" || exit $?
"$script_directory/Link-Staged-Compiler-Wvo.sh" \
    "$driver_object_prefix" "$driver_object_manifest" \
    "$driver_image_prefix" "$driver_image_manifest" \
    >"$temporary_directory/Driver-Link.txt" 2>"$temporary_directory/Driver-Link.err" || exit $?
"$script_directory/Transport-Compiler-Image.sh" \
    "$driver_image_prefix" "$driver_image_manifest" \
    "$driver_canonical_prefix" "$driver_canonical_manifest" \
    >"$temporary_directory/Driver-Transport.txt" 2>"$temporary_directory/Driver-Transport.err" || exit $?
driver_transport_line=$(sed -n '/^compiler image transport status=Complete /p' "$temporary_directory/Driver-Transport.txt")
driver_native_entry=$(printf '%s\n' "$driver_transport_line" | sed -n 's/^.* entry-offset=\([0-9][0-9]*\) chunks=.*$/\1/p')
driver_fragment_count=$(printf '%s\n' "$driver_transport_line" | sed -n 's/^.* chunks=\([1-8]\) manifest-bytes=.*$/\1/p')
if [[ $driver_native_entry != 220460 || $driver_fragment_count != 8 ]]; then
    echo 'The canonical compiler build-driver image identity is invalid.' >&2
    exit 1
fi
"$script_directory/Package-Hosted-Wvb.sh" image 2 \
    "$driver_wvb" "$driver_canonical_prefix" "$driver_fragment_count" \
    "$driver_native_entry" "$driver_windows" windows \
    >"$temporary_directory/Driver-Windows.txt" 2>"$temporary_directory/Driver-Windows.err" || exit $?
"$script_directory/Package-Hosted-Wvb.sh" image 2 \
    "$driver_wvb" "$driver_canonical_prefix" "$driver_fragment_count" \
    "$driver_native_entry" "$driver_linux" linux \
    >"$temporary_directory/Driver-Linux.txt" 2>"$temporary_directory/Driver-Linux.err" || exit $?

verify_file() {
    local path=$1
    local expected_bytes=$2
    local expected_sha256=$3
    [[ -f $path ]] || return 1
    local actual_bytes
    actual_bytes=$(wc -c < "$path") || return 1
    [[ $actual_bytes -eq $expected_bytes ]] || return 1
    local digest_line
    local actual_sha256
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    [[ $actual_sha256 == "$expected_sha256" ]]
}

verify_file "$wvb" 931035 \
    13558d9dbc0d185b161b770824aa29ff90b8873903b2b5d7184a23950a6fc1e4 || exit 1
verify_file "$windows" 27898368 \
    4009e6747bbf9a6d2b0b2ec90e2368ca50fda863d445534f15ef96e22a657b34 || exit 1
verify_file "$linux" 27897856 \
    c266adf20fe2927a446483f68880ef323c480f011b0c26384716ea2f651bcd65 || exit 1
verify_file "$driver_wvb" 1162338 \
    a214662da422443cd70c4be12c8f0bd06cbb5bce9fe3a56e2a52c46a37445a20 || exit 1
verify_file "$driver_windows" 30381568 \
    b0d58f4d8d6d32e09d45358035ce4521410ec1280f11fa72b66245e621ba49a3 || exit 1
verify_file "$driver_linux" 30380032 \
    b4fdc30a7242e03f7166491bf6b415aa3b5dce8ff0e16444f6ccb24c5bcb03a0 || exit 1

echo 'native compiler reconstruction status=Complete compiler-bytes=931035 native-bytes=27867015 entry-offset=51356 chunks=7 build-driver-bytes=1162338 build-driver-entry-offset=220460 build-driver-chunks=8'
