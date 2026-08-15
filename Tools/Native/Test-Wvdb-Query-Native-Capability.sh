#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Wvdb-Query-Native-Capability.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-wvdb-query-native.XXXXXXXX") || exit 1
mkdir -- "$work/Run" "$work/Empty" || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-wvdb-query-native.*)
            rm -rf -- "$work"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $work" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

verify_file() {
    local path=$1 expected_bytes=$2 expected_sha256=$3 description=$4
    [[ -f $path && ! -L $path ]] || {
        echo "Missing $description: $path" >&2
        return 1
    }
    local actual_bytes actual_sha256
    actual_bytes=$(wc -c < "$path") || return 1
    actual_sha256=$(sha256sum -- "$path" | awk '{ print $1 }') || return 1
    [[ $actual_bytes -eq $expected_bytes && $actual_sha256 == "$expected_sha256" ]] || {
        echo "Invalid identity for $description." >&2
        return 1
    }
}

echo 'native wvdb query step=locked-package'
"$script_directory/Build-Wvdb-Query-Package.sh" \
    "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvpack" \
    "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvlock" \
    "$work/Wvdb-Query.wvb" || exit $?
verify_file "$work/Wvdb-Query.wvb" 26294 \
    61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2 \
    'locked WVDB query WVB' || exit 1

echo 'native wvdb query step=capability-directory'
"$script_directory/Inspect-Wvb.sh" "$work/Wvdb-Query.wvb" >"$work/Inspect.txt" || exit $?
capability_count=$(grep -c '^capability index=' "$work/Inspect.txt") || exit 1
[[ $capability_count -eq 5 ]] || exit 1
for capability in \
    console.write_line \
    diagnostic.write_line \
    filesystem.directory_read_v1 \
    process.argument \
    process.argument_count; do
    grep '^capability index=' "$work/Inspect.txt" | \
        grep -F "name=\"$capability\"" >/dev/null || exit 1
done

echo 'native wvdb query step=build-current-lowerer'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj" \
    "$work/Lowerer.wvb" || exit $?
verify_file "$work/Lowerer.wvb" 501344 \
    4ef35324a2e5ba3bd0cf8751fb2b6beb3a8c6108767734ea719b5dab063c8746 \
    'variant-capable lowerer WVB' || exit 1

echo 'native wvdb query step=package-current-lowerer'
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 6 \
    "$work/Lowerer.wvb" "$work/Lowerer.elf" || exit $?

echo 'native wvdb query step=lower-application'
"$work/Lowerer.elf" "$work/Wvdb-Query.wvb" "$work/Wvdb-Query.wvo" || exit $?
verify_file "$work/Wvdb-Query.wvo" 237210 \
    b3d3bbde00136c230f6804215c352490bae9603b338d25186dba827be137edbf \
    'WVDB query WVO' || exit 1

echo 'native wvdb query step=assemble-rights-reduced-hosts'
"$script_directory/Assemble-Wva.sh" \
    "$repository_root/Runtime/Native/X64-Read-Only-Directory-Host.wva" \
    "$work/Directory-Host.wvo" || exit $?
"$script_directory/Assemble-Wva.sh" \
    "$repository_root/Runtime/Native/Windows-X64-Read-Only-Directory.wva" \
    "$work/Directory-Windows.wvo" || exit $?
"$script_directory/Assemble-Wva.sh" \
    "$repository_root/Runtime/Native/Linux-X64-Read-Only-Directory.wva" \
    "$work/Directory-Linux.wvo" || exit $?
verify_file "$work/Directory-Host.wvo" 2010 \
    7ab58a817fe5dbc8e8f91b910654487ba62e10bc5aa5d1ae74b6bb07f2f6ca09 \
    'directory host WVO' || exit 1
verify_file "$work/Directory-Windows.wvo" 1731 \
    d1dc38e751ab7a04cb115f2fc6f0e62a5452e2937cc1dd56867f3da8fe2ddc03 \
    'Windows directory leaf WVO' || exit 1
verify_file "$work/Directory-Linux.wvo" 608 \
    53136d316adec7f6b7667ecc853764fc5207d25fc52e60d2175cd8e0f49c4c64 \
    'Linux directory leaf WVO' || exit 1

echo 'native wvdb query step=link-cross-host-images'
"$script_directory/Link-Wvo.sh" 0 Directory_host_entry \
    "$work/Windows-Image.chunk-0" "$work/Wvdb-Query.wvo" \
    "$work/Directory-Host.wvo" "$work/Directory-Windows.wvo" \
    >"$work/Windows-Link.txt" || exit $?
"$script_directory/Link-Wvo.sh" 0 Directory_host_entry \
    "$work/Linux-Image.chunk-0" "$work/Wvdb-Query.wvo" \
    "$work/Directory-Host.wvo" "$work/Directory-Linux.wvo" \
    >"$work/Linux-Link.txt" || exit $?
windows_entry=$(sed -n 's/^entry name=Directory_host_entry address=\([0-9][0-9]*\)$/\1/p' "$work/Windows-Link.txt")
linux_entry=$(sed -n 's/^entry name=Directory_host_entry address=\([0-9][0-9]*\)$/\1/p' "$work/Linux-Link.txt")
[[ $windows_entry == 235440 && $linux_entry == 235440 ]] || exit 1
verify_file "$work/Windows-Image.chunk-0" 238413 \
    60bdf794d8fba0889a077eeec35fab75de9fd174a5a894eb78ef316ad1c8872c \
    'Windows linked image' || exit 1
verify_file "$work/Linux-Image.chunk-0" 237437 \
    76b8327d6f970c467d76a4e9c2f64d7473897d2afe2a444c007f840e42a35632 \
    'Linux linked image' || exit 1

echo 'native wvdb query step=package-cross-host-applications'
"$script_directory/Build-Cached-Hosted-Application.sh" 6 \
    "$work/Wvdb-Query.wvb" "$work/Windows-Image" 1 235440 \
    "$work/Wvdb-Query.exe" windows || exit $?
"$script_directory/Build-Cached-Hosted-Application.sh" 6 \
    "$work/Wvdb-Query.wvb" "$work/Linux-Image" 1 235440 \
    "$work/Wvdb-Query.elf" linux || exit $?
verify_file "$work/Wvdb-Query.exe" 258048 \
    7cd60860e07294d9a45064495da33a42cc752849accfc672c35a69454cd963d8 \
    'Windows hosted application' || exit 1
verify_file "$work/Wvdb-Query.elf" 258048 \
    29b4d4db7505daec94865d423e3805b02bde95751343b1fb7e4ceee8045a202d \
    'Linux hosted application' || exit 1

echo 'native wvdb query step=create-fixture'
node "$script_directory/Create-Wvdb-Query-Fixture.mjs" \
    "$work/Run/Windvale-Database-Storage.bin" || exit $?
verify_file "$work/Run/Windvale-Database-Storage.bin" 288 \
    b0a940dca77a4b018f66d3be66023880746f077ff78446e88671688d5ad31892 \
    'WVDB query fixture' || exit 1

echo 'native wvdb query step=execute-linux-cases cases=5'
(cd -- "$work/Run" && "$work/Wvdb-Query.elf" Windvale-Database-Storage.bin 7) \
    >"$work/Found.txt" 2>&1
found_exit=$?
(cd -- "$work/Run" && "$work/Wvdb-Query.elf" Windvale-Database-Storage.bin 9) \
    >"$work/Negative.txt" 2>&1
negative_exit=$?
(cd -- "$work/Run" && "$work/Wvdb-Query.elf" Windvale-Database-Storage.bin 8) \
    >"$work/Missing.txt" 2>&1
missing_exit=$?
(cd -- "$work/Run" && "$work/Wvdb-Query.elf" Xindvale-Database-Storage.bin 7) \
    >"$work/Denied.txt" 2>&1
denied_exit=$?
(cd -- "$work/Empty" && "$work/Wvdb-Query.elf" Windvale-Database-Storage.bin 7) \
    >"$work/Unavailable.txt" 2>&1
unavailable_exit=$?
printf 'native wvdb query cases status found=%s negative=%s missing=%s denied=%s unavailable=%s\n' \
    "$found_exit" "$negative_exit" "$missing_exit" "$denied_exit" "$unavailable_exit"
[[ $found_exit -eq 0 && $negative_exit -eq 0 && $missing_exit -eq 2 &&
    $denied_exit -eq 3 && $unavailable_exit -eq 3 ]] || exit 1
echo 'native wvdb query output item=1/5 case=found'
if ! grep -Fx 'found key=7 value=42' "$work/Found.txt" >/dev/null; then
    echo 'native wvdb query output status=Mismatch case=found' >&2
    sed -n '1,8p' "$work/Found.txt" >&2
    exit 1
fi
echo 'native wvdb query output item=2/5 case=negative'
if ! grep -Fx 'found key=9 value=-5' "$work/Negative.txt" >/dev/null; then
    echo 'native wvdb query output status=Mismatch case=negative' >&2
    sed -n '1,8p' "$work/Negative.txt" >&2
    exit 1
fi
echo 'native wvdb query output item=3/5 case=missing'
if ! grep -Fx 'missing key=8' "$work/Missing.txt" >/dev/null; then
    echo 'native wvdb query output status=Mismatch case=missing' >&2
    sed -n '1,8p' "$work/Missing.txt" >&2
    exit 1
fi
echo 'native wvdb query output item=4/5 case=denied'
if ! grep -F 'storage-failure status=' "$work/Denied.txt" >/dev/null; then
    echo 'native wvdb query output status=Mismatch case=denied' >&2
    sed -n '1,8p' "$work/Denied.txt" >&2
    exit 1
fi
echo 'native wvdb query output item=5/5 case=unavailable'
if ! grep -F 'storage-failure status=' "$work/Unavailable.txt" >/dev/null; then
    echo 'native wvdb query output status=Mismatch case=unavailable' >&2
    sed -n '1,8p' "$work/Unavailable.txt" >&2
    exit 1
fi

echo 'native wvdb query identity host=linux wvb=61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2 windows-application=7cd60860e07294d9a45064495da33a42cc752849accfc672c35a69454cd963d8 linux-application=29b4d4db7505daec94865d423e3805b02bde95751343b1fb7e4ceee8045a202d'
echo 'native wvdb query capability status=Passed cases=5 capabilities=5 wvb=61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2 cross-host-images=Verified'
