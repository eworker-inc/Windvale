#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Development-Installers.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-development-installers.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-development-installers.*) rm -rf -- "$work" ;;
        *) echo "Refusing to remove unexpected temporary path: $work" >&2; return 1 ;;
    esac
}
trap cleanup EXIT

mkdir -- "$work/First" "$work/Second" "$work/Corrupt" "$work/Tampered-Extract" "$work/Clean-Extract" || exit 1
printf '%s\n' preserve >"$work/sentinel.txt"
builder=$repository_root/Tools/Release/Build-Development-Installers.mjs
windows_archive=windvale-0.1.0-dev.1-windows-x64.zip
linux_archive=windvale-0.1.0-dev.1-linux-x64.tar.gz
package_directory=windvale-0.1.0-dev.1-linux-x64
generation=0.1.0-dev.1-linux-x64-f6f96c6df5fc

verify_file() {
    local file=$1 expected_bytes=$2 expected_sha256=$3 description=$4
    [[ -f "$file" ]] || { echo "Missing $description: $file" >&2; return 1; }
    local observed_bytes observed_sha256
    observed_bytes=$(wc -c <"$file" | tr -d '[:space:]') || return 1
    [[ "$observed_bytes" == "$expected_bytes" ]] || { echo "Invalid $description length." >&2; return 1; }
    observed_sha256=$(sha256sum -- "$file" | cut -d ' ' -f 1) || return 1
    [[ "$observed_sha256" == "$expected_sha256" ]] || { echo "Invalid $description digest." >&2; return 1; }
}

echo 'native development installer step=construct-candidates item=1/8 targets=2 attempts=2'
node "$builder" build "$work/First" || exit $?
node "$builder" build "$work/Second" || exit $?

echo 'native development installer step=prove-reproducibility item=2/8'
cmp --silent "$work/First/$windows_archive" "$work/Second/$windows_archive" || exit 1
cmp --silent "$work/First/$linux_archive" "$work/Second/$linux_archive" || exit 1
verify_file "$work/First/$windows_archive" 38351998 \
    2c2112bef12e89b0594e2510b5ea71318b4c9ff8979b35c7fa7c20ca8703a186 \
    'Windows installer' || exit 1
verify_file "$work/First/$linux_archive" 38362500 \
    dc65a1091e918b8d73106cc6c4bb9bd1a3a905b42601eacd32453e0a073e5937 \
    'Linux installer' || exit 1

echo 'native development installer step=verify-and-reject item=3/8'
node "$builder" verify "$work/First/$windows_archive" >/dev/null || exit $?
node "$builder" verify "$work/First/$linux_archive" >/dev/null || exit $?
cp -- "$work/First/$linux_archive" "$work/Corrupt/$linux_archive" || exit 1
printf x >>"$work/Corrupt/$linux_archive"
if node "$builder" verify "$work/Corrupt/$linux_archive" >/dev/null 2>&1; then exit 1; fi

echo 'native development installer step=extract-host-package item=4/8'
tar -xzf "$work/First/$linux_archive" -C "$work/Tampered-Extract" || exit $?
tar -xzf "$work/First/$linux_archive" -C "$work/Clean-Extract" || exit $?

echo 'native development installer step=reject-tampered-package item=5/8'
printf '\0' >>"$work/Tampered-Extract/$package_directory/bin/wvbuild"
if "$work/Tampered-Extract/$package_directory/install.sh" \
    --root "$work/Rejected-Install" --bin-dir "$work/Rejected-Bin" >/dev/null 2>&1; then
    exit 1
fi
[[ ! -e "$work/Rejected-Install" ]] || exit 1

echo 'native development installer step=install-and-run item=6/8 attempts=2'
"$work/Clean-Extract/$package_directory/install.sh" \
    --root "$work/Installed" --bin-dir "$work/User-Bin" >/dev/null || exit $?
"$work/Clean-Extract/$package_directory/install.sh" \
    --root "$work/Installed" --bin-dir "$work/User-Bin" >/dev/null || exit $?
"$work/User-Bin/wv" version | grep -Fx 'Windvale 0.1.0-dev.1' >/dev/null || exit 1
"$work/User-Bin/wv" doctor >/dev/null || exit $?
"$work/User-Bin/wvverify" \
    "$repository_root/Artifacts/Native-Front-Door/Wvb/Wvb-Runner.wvb" |
    grep -Fx 'wvb status=Valid profile=compiler-aligned' >/dev/null || exit 1

echo 'native development installer step=detect-installed-tamper item=7/8'
printf '\0' >>"$work/Installed/generations/$generation/bin/wvbuild"
tampered_hash=$(sha256sum -- "$work/Installed/generations/$generation/bin/wvbuild" | cut -d ' ' -f 1) || exit 1
tampered_bytes=$(wc -c <"$work/Installed/generations/$generation/bin/wvbuild" | tr -d '[:space:]') || exit 1
awk -v hash="$tampered_hash" -v bytes="$tampered_bytes" \
    '$5 == "bin/wvbuild" { $2 = hash; $3 = bytes } { print }' \
    "$work/Installed/generations/$generation/Payload-Manifest.txt" \
    >"$work/Installed/generations/$generation/Payload-Manifest.candidate" || exit 1
mv -- "$work/Installed/generations/$generation/Payload-Manifest.candidate" \
    "$work/Installed/generations/$generation/Payload-Manifest.txt" || exit 1
if "$work/User-Bin/wv" doctor >/dev/null 2>&1; then exit 1; fi

echo 'native development installer step=uninstall-preserve-external item=8/8'
"$work/Clean-Extract/$package_directory/uninstall.sh" \
    --root "$work/Installed" --bin-dir "$work/User-Bin" >/dev/null || exit $?
[[ ! -e "$work/Installed" && -f "$work/sentinel.txt" ]] || exit 1
for command in wv wvbuild wvasm wvlink wvrun wvdump wvverify wvpublish; do
    [[ ! -e "$work/User-Bin/$command" && ! -L "$work/User-Bin/$command" ]] || exit 1
done

echo 'native development installer status=Passed cases=8 archives=2 reproducible=Verified host-install=Verified'
