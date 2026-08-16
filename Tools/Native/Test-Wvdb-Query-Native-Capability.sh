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
verify_file "$work/Lowerer.wvb" 520966 \
    ce190159783b48912ff71326d937a72a27b5178b07b7e52de71742a53cd12b56 \
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
verify_file "$work/Directory-Windows.wvo" 1951 \
    d2da1c67864c242aeb9797661028295922486de2cf7d37aa41024189afb10f34 \
    'Windows directory leaf WVO' || exit 1
verify_file "$work/Directory-Linux.wvo" 681 \
    0ccbcda71b20eaa024946e4fbb2016853952a39f1fe58ed0a183bde502335d86 \
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
verify_file "$work/Windows-Image.chunk-0" 238536 \
    fe51adddc364f9ec32d9ae0a7925417e1fa6304e930fd42ec9106f31f73d35bc \
    'Windows linked image' || exit 1
verify_file "$work/Linux-Image.chunk-0" 237517 \
    cae8aee6da474d2acb0a976047c689511a22269377b58114a56e8616fecc708d \
    'Linux linked image' || exit 1

echo 'native wvdb query step=package-cross-host-applications'
"$script_directory/Build-Cached-Hosted-Application.sh" 6 \
    "$work/Wvdb-Query.wvb" "$work/Windows-Image" 1 235440 \
    "$work/Wvdb-Query.exe" windows || exit $?
"$script_directory/Build-Cached-Hosted-Application.sh" 6 \
    "$work/Wvdb-Query.wvb" "$work/Linux-Image" 1 235440 \
    "$work/Wvdb-Query.elf" linux || exit $?
verify_file "$work/Wvdb-Query.exe" 258048 \
    198d44b49db6765792c835c6419da88f0cbcc0de0422748b0d15cb4ae5e6ba32 \
    'Windows hosted application' || exit 1
verify_file "$work/Wvdb-Query.elf" 258048 \
    b21095d6ab62209b67053b7dfe1cf5a2f0130b3722a09a8e48284fc1aa988b3f \
    'Linux hosted application' || exit 1

echo 'native wvdb query step=create-fixture'
node "$script_directory/Create-Wvdb-Query-Fixture.mjs" \
    "$work/Run/Windvale-Database-Storage.bin" || exit $?
verify_file "$work/Run/Windvale-Database-Storage.bin" 288 \
    b0a940dca77a4b018f66d3be66023880746f077ff78446e88671688d5ad31892 \
    'WVDB query fixture' || exit 1

echo 'native wvdb query step=execute-linux-cases cases=6'
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
ln -s "$work/Run/Windvale-Database-Storage.bin" \
    "$work/Empty/Windvale-Database-Storage.bin" || exit $?
(cd -- "$work/Empty" && "$work/Wvdb-Query.elf" Windvale-Database-Storage.bin 7) \
    >"$work/NoLink.txt" 2>&1
no_link_exit=$?
printf 'native wvdb query cases status found=%s negative=%s missing=%s denied=%s unavailable=%s no-link=%s\n' \
    "$found_exit" "$negative_exit" "$missing_exit" "$denied_exit" "$unavailable_exit" \
    "$no_link_exit"
[[ $found_exit -eq 0 && $negative_exit -eq 0 && $missing_exit -eq 2 &&
    $denied_exit -eq 3 && $unavailable_exit -eq 3 && $no_link_exit -eq 3 ]] || exit 1
echo 'native wvdb query output item=1/6 case=found'
if ! grep -Fx 'found key=7 value=42' "$work/Found.txt" >/dev/null; then
    echo 'native wvdb query output status=Mismatch case=found' >&2
    sed -n '1,8p' "$work/Found.txt" >&2
    exit 1
fi
echo 'native wvdb query output item=2/6 case=negative'
if ! grep -Fx 'found key=9 value=-5' "$work/Negative.txt" >/dev/null; then
    echo 'native wvdb query output status=Mismatch case=negative' >&2
    sed -n '1,8p' "$work/Negative.txt" >&2
    exit 1
fi
echo 'native wvdb query output item=3/6 case=missing'
if ! grep -Fx 'missing key=8' "$work/Missing.txt" >/dev/null; then
    echo 'native wvdb query output status=Mismatch case=missing' >&2
    sed -n '1,8p' "$work/Missing.txt" >&2
    exit 1
fi
echo 'native wvdb query output item=4/6 case=denied'
if ! grep -F 'storage-failure status=' "$work/Denied.txt" >/dev/null; then
    echo 'native wvdb query output status=Mismatch case=denied' >&2
    sed -n '1,8p' "$work/Denied.txt" >&2
    exit 1
fi
echo 'native wvdb query output item=5/6 case=unavailable'
if ! grep -F 'storage-failure status=' "$work/Unavailable.txt" >/dev/null; then
    echo 'native wvdb query output status=Mismatch case=unavailable' >&2
    sed -n '1,8p' "$work/Unavailable.txt" >&2
    exit 1
fi
echo 'native wvdb query output item=6/6 case=no-link'
if ! grep -F 'storage-failure status=' "$work/NoLink.txt" >/dev/null; then
    echo 'native wvdb query output status=Mismatch case=no-link' >&2
    sed -n '1,8p' "$work/NoLink.txt" >&2
    exit 1
fi

echo 'native wvdb query identity host=linux wvb=61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2 windows-application=198d44b49db6765792c835c6419da88f0cbcc0de0422748b0d15cb4ae5e6ba32 linux-application=b21095d6ab62209b67053b7dfe1cf5a2f0130b3722a09a8e48284fc1aa988b3f'
echo 'native wvdb query capability status=Passed cases=6 capabilities=5 wvb=61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2 cross-host-images=Verified'
