#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Installers.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-installers.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-installers.*) rm -rf -- "$work" ;;
        *) echo "Refusing to remove unexpected temporary path: $work" >&2; return 1 ;;
    esac
}
trap cleanup EXIT

mkdir -- "$work/First-Development" "$work/Second-Development" \
    "$work/First-Release" "$work/Second-Release" "$work/Corrupt" \
    "$work/Development-Extract" "$work/Tampered-Extract" \
    "$work/Clean-Extract" || exit 1
printf '%s\n' preserve >"$work/sentinel.txt"
builder=$repository_root/Tools/Release/Build-Installers.mjs
release_input=Distribution/Installers/Windvale-Release-Installer.json
development_windows_archive=windvale-0.2.0-dev.1-windows-x64.zip
development_linux_archive=windvale-0.2.0-dev.1-linux-x64.tar.gz
development_package_directory=windvale-0.2.0-dev.1-linux-x64
development_payload=738778db11f628d55beb8dcb7c4f8a60cdcf72c075814a5d8786a3d3f4948ffa
windows_archive=windvale-0.1.0-windows-x64.zip
linux_archive=windvale-0.1.0-linux-x64.tar.gz
package_directory=windvale-0.1.0-linux-x64
generation=0.1.0-linux-x64-1321e44a32b3

verify_file() {
    local file=$1 expected_bytes=$2 expected_sha256=$3 description=$4
    [[ -f "$file" ]] || { echo "Missing $description: $file" >&2; return 1; }
    local observed_bytes observed_sha256
    observed_bytes=$(wc -c <"$file" | tr -d '[:space:]') || return 1
    [[ "$observed_bytes" == "$expected_bytes" ]] || { echo "Invalid $description length." >&2; return 1; }
    observed_sha256=$(sha256sum -- "$file" | cut -d ' ' -f 1) || return 1
    [[ "$observed_sha256" == "$expected_sha256" ]] || { echo "Invalid $description digest." >&2; return 1; }
}

echo 'native installer step=construct-candidates item=1/8 channels=2 targets=2 attempts=2'
node "$builder" build "$work/First-Development" || exit $?
node "$builder" build "$work/Second-Development" || exit $?
node "$builder" build "$work/First-Release" "$release_input" || exit $?
node "$builder" build "$work/Second-Release" "$release_input" || exit $?

echo 'native installer step=prove-reproducibility item=2/8'
cmp --silent "$work/First-Development/$development_windows_archive" \
    "$work/Second-Development/$development_windows_archive" || exit 1
cmp --silent "$work/First-Development/$development_linux_archive" \
    "$work/Second-Development/$development_linux_archive" || exit 1
cmp --silent "$work/First-Release/$windows_archive" \
    "$work/Second-Release/$windows_archive" || exit 1
cmp --silent "$work/First-Release/$linux_archive" \
    "$work/Second-Release/$linux_archive" || exit 1
verify_file "$work/First-Development/$development_windows_archive" 5469526 \
    e32b1dadc813d9f220d19f8dad9c641950b391532b0a20f69de7d78b7ed51d57 \
    'Windows development installer' || exit 1
verify_file "$work/First-Development/$development_linux_archive" 5463313 \
    24898661ca443b50761c6cc9f03a92a3f33f881f423394093a12b51c8e3932fa \
    'Linux development installer' || exit 1
verify_file "$work/First-Release/$windows_archive" 43234311 \
    3f6f633e17fd5d29799ffcec91a6855f7076df9102f8a61c95bdaf760ddb54a1 \
    'Windows release installer' || exit 1
verify_file "$work/First-Release/$linux_archive" 43246838 \
    17d0982384ac73c251aeb5ee4fa5cfd7e3b82070abee77c321be6be07d068a63 \
    'Linux release installer' || exit 1

echo 'native installer step=verify-and-reject item=3/8 channel=stable'
node "$builder" verify "$work/First-Release/$windows_archive" "$release_input" >/dev/null || exit $?
node "$builder" verify "$work/First-Release/$linux_archive" "$release_input" >/dev/null || exit $?
cp -- "$work/First-Release/$linux_archive" "$work/Corrupt/$linux_archive" || exit 1
printf x >>"$work/Corrupt/$linux_archive"
if node "$builder" verify "$work/Corrupt/$linux_archive" "$release_input" >/dev/null 2>&1; then exit 1; fi

echo 'native installer step=extract-host-packages item=4/8 channels=2'
tar -xzf "$work/First-Development/$development_linux_archive" \
    -C "$work/Development-Extract" || exit $?
"$work/Development-Extract/$development_package_directory/bin/wv-verify-installation" \
    "$work/Development-Extract/$development_package_directory" linux-x64 \
    "$development_payload" >/dev/null || exit $?
tar -xzf "$work/First-Release/$linux_archive" -C "$work/Tampered-Extract" || exit $?
tar -xzf "$work/First-Release/$linux_archive" -C "$work/Clean-Extract" || exit $?

echo 'native installer step=reject-tampered-package item=5/8 channel=stable'
printf '\0' >>"$work/Tampered-Extract/$package_directory/bin/wvbuild"
if "$work/Tampered-Extract/$package_directory/install.sh" \
    --root "$work/Rejected-Install" --bin-dir "$work/Rejected-Bin" >/dev/null 2>&1; then
    exit 1
fi
[[ ! -e "$work/Rejected-Install" ]] || exit 1

echo 'native installer step=install-and-run item=6/8 channel=stable attempts=2'
"$work/Clean-Extract/$package_directory/install.sh" \
    --root "$work/Installed" --bin-dir "$work/User-Bin" >/dev/null || exit $?
"$work/Clean-Extract/$package_directory/install.sh" \
    --root "$work/Installed" --bin-dir "$work/User-Bin" >/dev/null || exit $?
"$work/User-Bin/wv" version | grep -Fx 'Windvale 0.1.0' >/dev/null || exit 1
"$work/User-Bin/wv" doctor >/dev/null || exit $?
"$work/User-Bin/wvverify" \
    "$repository_root/Artifacts/Native-Front-Door/Wvb/Wvb-Runner.wvb" |
    grep -Fx 'wvb status=Valid profile=compiler-aligned' >/dev/null || exit 1
set +e
"$work/User-Bin/wv" run \
    "$repository_root/Tests/Fixtures/Scripting/Arguments-And-Output.wv" \
    -flag 'snow day' >"$work/Script.out" 2>"$work/Script.err"
status=$?
set -e
[[ $status -eq 7 ]] || exit 1
grep -Fqx 'first=-flag' "$work/Script.out" || exit 1
grep -Fqx 'second=snow day' "$work/Script.err" || exit 1

echo 'native installer step=detect-installed-tamper item=7/8 channel=stable'
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

echo 'native installer step=uninstall-preserve-external item=8/8 channel=stable'
"$work/Clean-Extract/$package_directory/uninstall.sh" \
    --root "$work/Installed" --bin-dir "$work/User-Bin" >/dev/null || exit $?
[[ ! -e "$work/Installed" && -f "$work/sentinel.txt" ]] || exit 1
for command in wv wvbuild wvasm wvlink wvrun wvdump wvverify wvpublish; do
    [[ ! -e "$work/User-Bin/$command" && ! -L "$work/User-Bin/$command" ]] || exit 1
done

echo 'native installer status=Passed cases=8 channels=2 archives=4 reproducible=Verified host-install=Verified'
