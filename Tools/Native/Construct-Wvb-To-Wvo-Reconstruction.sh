#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || ! -d $1 ]]; then
    echo 'Usage: ./Tools/Native/Construct-Wvb-To-Wvo-Reconstruction.sh <existing-separate-output-directory>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
output_root=$(CDPATH= cd -- "$1" && pwd -P) || exit 64
candidate_root=$(CDPATH= cd -- "$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate" && pwd -P) || exit 1
if [[ $output_root == "$candidate_root" ]]; then
    echo 'The WVB-to-WVO reconstruction must use a separate output directory.' >&2
    exit 64
fi

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-wvb-to-wvo-reconstruction.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-wvb-to-wvo-reconstruction.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

lowerer_wvb="$output_root/Wvb-To-Wvo.wvb"
windows_application="$output_root/Wvb-To-Wvo.exe"
linux_application="$output_root/Wvb-To-Wvo.elf"
return_wvb="$output_root/Return-42.wvb"
return_wvo="$output_root/Return-42.wvo"
metadata_wvb="$output_root/Metadata.wvb"
metadata_wvo="$output_root/Metadata.wvo"
metadata_test_wvb="$temporary_directory/Metadata-Self-Test.wvb"
object_prefix="$temporary_directory/Object"
object_manifest="$temporary_directory/Object.wvop"
image_prefix="$temporary_directory/Image"
image_manifest="$temporary_directory/Image.wvli"
canonical_prefix="$temporary_directory/Canonical"
canonical_manifest="$temporary_directory/Canonical.wvli"

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj" \
    "$lowerer_wvb" \
    >"$temporary_directory/Build-Lowerer.txt" 2>"$temporary_directory/Build-Lowerer.err" || exit $?
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-X64-Lowering-Metadata.wvproj" \
    "$metadata_test_wvb" \
    >"$temporary_directory/Build-Metadata-Test.txt" 2>"$temporary_directory/Build-Metadata-Test.err" || exit $?
"$script_directory/Run-Wvb.sh" "$metadata_test_wvb" \
    >"$temporary_directory/Run-Metadata-Test.txt" 2>"$temporary_directory/Run-Metadata-Test.err" || exit $?
grep -Fx 'Result: 0' "$temporary_directory/Run-Metadata-Test.txt" >/dev/null || exit 1
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj" \
    "$return_wvb" \
    >"$temporary_directory/Build-Return-42.txt" 2>"$temporary_directory/Build-Return-42.err" || exit $?
"$repository_root/Artifacts/Native-Compiler-Seed/linux-x64/wvcompiler.elf" \
    "$repository_root/Tests/Fixtures/Native-X64/Wvb-To-Wvo-Metadata.wv" \
    "$metadata_wvb" \
    >"$temporary_directory/Build-Metadata.txt" 2>"$temporary_directory/Build-Metadata.err" || exit $?

"$script_directory/Stage-Compiler-Wvb.sh" \
    "$lowerer_wvb" "$object_prefix" "$object_manifest" \
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
case "$native_entry" in
    ''|*[!0-9]*) exit 1 ;;
esac
case "$fragment_count" in
    [1-8]) ;;
    *) exit 1 ;;
esac

"$script_directory/Package-Hosted-Wvb.sh" image 6 \
    "$lowerer_wvb" "$canonical_prefix" "$fragment_count" "$native_entry" \
    "$windows_application" windows \
    >"$temporary_directory/Windows.txt" 2>"$temporary_directory/Windows.err" || exit $?
"$script_directory/Package-Hosted-Wvb.sh" image 6 \
    "$lowerer_wvb" "$canonical_prefix" "$fragment_count" "$native_entry" \
    "$linux_application" linux \
    >"$temporary_directory/Linux.txt" 2>"$temporary_directory/Linux.err" || exit $?

if [[ ! -x $linux_application ]]; then
    echo 'The Linux WVB-to-WVO application is not executable.' >&2
    exit 1
fi
"$linux_application" "$return_wvb" "$return_wvo" \
    >"$temporary_directory/Return-42.txt" 2>"$temporary_directory/Return-42.err" || exit $?
"$linux_application" "$metadata_wvb" "$metadata_wvo" \
    >"$temporary_directory/Metadata.txt" 2>"$temporary_directory/Metadata.err" || exit $?

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

verify_file "$lowerer_wvb" 747997 \
    d5a514e72203ab530c6df6da8f444e6bd7f93130921e02042e70c7a7723942dc \
    'WVB-to-WVO tool WVB' || exit 1
verify_file "$windows_application" 10661888 \
    a46d73ada72fba9561e9db1fcfc5477bf19be2518ad9db2d8487184112923dfd \
    'Windows WVB-to-WVO application' || exit 1
verify_file "$linux_application" 10661888 \
    9c331308e5afe852d4c0441e22c1ff68a0ac0c86793c2e403f38556302c90fd3 \
    'Linux WVB-to-WVO application' || exit 1
verify_file "$return_wvb" 174 \
    7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31 \
    'Return-42 WVB' || exit 1
verify_file "$return_wvo" 479 \
    0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5 \
    'Return-42 WVO' || exit 1
verify_file "$metadata_wvb" 369 \
    94b41f5016722c9e5bf16ace5ec933acc35c14efdd4e08fe11fd582a62b58ffa \
    'metadata WVB' || exit 1
verify_file "$metadata_wvo" 1151 \
    6f1cb53ec55448a7552f2ff5b380446964d16ed32a60aa28b8e55a9ca590845d \
    'metadata WVO' || exit 1

echo 'native WVB-to-WVO reconstruction status=Complete artifacts=7'
