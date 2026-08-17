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
build_compiler="$temporary_directory/wvcompiler-build.elf"
object_prefix="$temporary_directory/Object"
object_manifest="$temporary_directory/Object.wvop"
image_prefix="$temporary_directory/Image"
image_manifest="$temporary_directory/Image.wvli"
canonical_prefix="$temporary_directory/Canonical"
canonical_manifest="$temporary_directory/Canonical.wvli"

echo 'native compiler reconstruction step=bootstrap item=1/14'
"$script_directory/Bootstrap-Compiler.sh" \
    "$repository_root/Artifacts" "$repository_root" "$wvb" \
    >"$temporary_directory/Bootstrap.txt" 2>"$temporary_directory/Bootstrap.err" || exit $?
echo 'native compiler reconstruction step=stage-compiler item=2/14'
"$script_directory/Stage-Compiler-Wvb.sh" \
    "$wvb" "$object_prefix" "$object_manifest" \
    >"$temporary_directory/Stage.txt" 2>"$temporary_directory/Stage.err" || exit $?
echo 'native compiler reconstruction step=link-compiler item=3/14'
"$script_directory/Link-Staged-Compiler-Wvo.sh" \
    "$object_prefix" "$object_manifest" "$image_prefix" "$image_manifest" \
    >"$temporary_directory/Link.txt" 2>"$temporary_directory/Link.err" || exit $?
echo 'native compiler reconstruction step=transport-compiler item=4/14'
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

echo 'native compiler reconstruction step=package-compiler-windows item=5/14'
"$script_directory/Package-Hosted-Wvb.sh" image 1 \
    "$wvb" "$canonical_prefix" "$fragment_count" "$native_entry" "$windows" windows \
    >"$temporary_directory/Windows.txt" 2>"$temporary_directory/Windows.err" || exit $?
echo 'native compiler reconstruction step=package-build-compiler item=6/14'
"$script_directory/Package-Hosted-Wvb.sh" image 2 \
    "$wvb" "$canonical_prefix" "$fragment_count" "$native_entry" "$build_compiler" linux \
    >"$temporary_directory/Build-Compiler.txt" 2>"$temporary_directory/Build-Compiler.err" || exit $?
echo 'native compiler reconstruction step=compile-build-driver item=7/14'
"$script_directory/Compile-Compiler-Build-Driver-Source-Set.sh" \
    "$build_compiler" "$repository_root" "$driver_wvb" \
    >"$temporary_directory/Driver-Build.txt" 2>"$temporary_directory/Driver-Build.err" || exit $?
driver_object_prefix="$temporary_directory/Driver-Object"
driver_object_manifest="$temporary_directory/Driver-Object.wvop"
driver_image_prefix="$temporary_directory/Driver-Image"
driver_image_manifest="$temporary_directory/Driver-Image.wvli"
driver_canonical_prefix="$temporary_directory/Driver-Canonical"
driver_canonical_manifest="$temporary_directory/Driver-Canonical.wvli"
echo 'native compiler reconstruction step=stage-build-driver item=8/14'
"$script_directory/Stage-Compiler-Wvb.sh" \
    "$driver_wvb" "$driver_object_prefix" "$driver_object_manifest" \
    >"$temporary_directory/Driver-Stage.txt" 2>"$temporary_directory/Driver-Stage.err" || exit $?
echo 'native compiler reconstruction step=link-build-driver item=9/14'
"$script_directory/Link-Staged-Compiler-Wvo.sh" \
    "$driver_object_prefix" "$driver_object_manifest" \
    "$driver_image_prefix" "$driver_image_manifest" \
    >"$temporary_directory/Driver-Link.txt" 2>"$temporary_directory/Driver-Link.err" || exit $?
echo 'native compiler reconstruction step=transport-build-driver item=10/14'
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
echo 'native compiler reconstruction step=package-build-driver-windows item=11/14'
"$script_directory/Package-Hosted-Wvb.sh" image 2 \
    "$driver_wvb" "$driver_canonical_prefix" "$driver_fragment_count" \
    "$driver_native_entry" "$driver_windows" windows \
    >"$temporary_directory/Driver-Windows.txt" 2>"$temporary_directory/Driver-Windows.err" || exit $?
echo 'native compiler reconstruction step=package-build-driver-linux item=12/14'
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

echo 'native compiler reconstruction step=package-compiler-linux item=13/14'
"$script_directory/Package-Hosted-Wvb.sh" image 1 \
    "$wvb" "$canonical_prefix" "$fragment_count" "$native_entry" "$linux" linux \
    >"$temporary_directory/Linux.txt" 2>"$temporary_directory/Linux.err" || exit $?

echo 'native compiler reconstruction step=verify-identities item=14/14'
verify_file "$wvb" 935163 \
    a7d47b2de29faee089c7a22ef23eac4657f719331dc02044eb2d818457dac5b6 || exit 1
verify_file "$windows" 28172800 \
    a5db938a814471fdacda75efcf57d28934ae52b3b2290732627c14ba173fd70d || exit 1
verify_file "$linux" 28172288 \
    da11ab3b70b428087cbcb9de5614a2dbdccd31afc6861cc15881fd65c12ff19b || exit 1
verify_file "$driver_wvb" 1142818 \
    125d2b4080889615877d843a36b2f9f6b50d049d011cc06fa8ab426ab83c0574 || exit 1
verify_file "$driver_windows" 30071296 \
    f556f0e2c794d9424cbcd9f5e3f8e5aee54f49373c7c18ea1d4829facea7dc6f || exit 1
verify_file "$driver_linux" 30072832 \
    628fd60ea702c4a3b3ffb01d32cba7ba9708477acccf190cc6506a56f159d7a9 || exit 1

echo 'native compiler reconstruction status=Complete compiler-bytes=935163 native-bytes=28141686 entry-offset=51356 chunks=7 build-driver-bytes=1142818 build-driver-entry-offset=220460 build-driver-chunks=8'
