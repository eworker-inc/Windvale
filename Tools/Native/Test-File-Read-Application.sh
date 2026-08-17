#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-File-Read-Application.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-file-read-application.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-file-read-application.*)
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
    local candidate=$1 expected_bytes=$2 expected_sha256=$3 description=$4
    [[ -f $candidate && ! -L $candidate ]] || {
        echo "Missing $description: $candidate" >&2
        return 1
    }
    [[ $(wc -c < "$candidate") -eq $expected_bytes ]] || return 1
    [[ $(sha256sum -- "$candidate" | awk '{ print $1 }') == "$expected_sha256" ]]
}

workspace="$repository_root/Windvale.wvws"
front_door="$repository_root/Artifacts/Native-Front-Door/linux-x64/wvbuild.elf"

echo 'START native file read phase=self-host item=1/6'
"$front_door" --workspace "$workspace" --project \
    "$repository_root/Projects/Tools/Windvale-Compiler-Build-Driver.wvproj" \
    "$work/Build-Driver.wvb" >/dev/null || exit $?
verify_file "$work/Build-Driver.wvb" 1142818 \
    125d2b4080889615877d843a36b2f9f6b50d049d011cc06fa8ab426ab83c0574 \
    'current build driver WVB' || exit 1
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 2 \
    "$work/Build-Driver.wvb" "$work/Build-Driver.elf" --development-cache \
    >/dev/null || exit $?
"$work/Build-Driver.elf" --workspace "$workspace" --project \
    "$repository_root/Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj" \
    "$work/Lowerer.wvb" >/dev/null || exit $?
verify_file "$work/Lowerer.wvb" 523087 \
    6b56da9c4ee12917fc4e59f1745ebbfd854335c011f1a5c2c27613abedc1db41 \
    'current lowerer WVB' || exit 1
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 6 \
    "$work/Lowerer.wvb" "$work/Lowerer.elf" --development-cache >/dev/null || exit $?
echo 'PASS  native file read phase=self-host item=1/6'

echo 'START native file read phase=compile item=2/6'
echo 'native file read compile step=source'
"$work/Build-Driver.elf" --workspace "$workspace" --project \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Standard-Byte-Output-Response-Core.wvproj" \
    "$work/Response.wvb" >/dev/null || exit $?
"$work/Build-Driver.elf" --workspace "$workspace" --project \
    "$repository_root/Projects/Applications/Windvale-File-Read.wvproj" \
    "$work/File-Read.wvb" >/dev/null || exit $?
verify_file "$work/Response.wvb" 8417 \
    868c9967432b3b5b2859de26bb3caf76dcbcc113d4a9c678625eecde73fd8193 \
    'response-core self-test WVB' || exit 1
verify_file "$work/File-Read.wvb" 76474 \
    95eed93bf74b10214711efe9a8780c4c289c06bbf8b46e835c00119a36190dfb \
    'file-read WVB' || exit 1
echo 'native file read compile step=lower'
"$work/Lowerer.elf" "$work/File-Read.wvb" "$work/File-Read.wvo" >/dev/null || exit $?
verify_file "$work/File-Read.wvo" 2410255 \
    8ad63e3dbe87daccf6a9a94407ee0a661f177d6f812b300587b77fe36f7dd323 \
    'file-read WVO' || exit 1
echo 'native file read compile step=response-package'
"$script_directory/Package-Hosted-Wvb.sh" 2 "$work/Response.wvb" \
    "$work/Response.exe" windows >/dev/null || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 2 "$work/Response.wvb" \
    "$work/Response.elf" linux >/dev/null || exit $?
echo 'native file read compile step=response-identity'
verify_file "$work/Response.exe" 91648 \
    e6ed27e2a4946f09d0846ddc3a6cb61301b0ccf311b0f17d090d140bb6ddf9a6 \
    'Windows response self-test' || exit 1
verify_file "$work/Response.elf" 90112 \
    bac55c2f144447501979aeba617435611fd777917fb9c1331d6bfb82419fbbdb \
    'Linux response self-test' || exit 1
echo 'PASS  native file read phase=compile item=2/6'

echo 'START native file read phase=providers item=3/6'
"$script_directory/Assemble-Wva.sh" "$repository_root/Runtime/Native/X64-File-Read-Host.wva" "$work/Host.wvo" >/dev/null || exit $?
"$script_directory/Assemble-Wva.sh" "$repository_root/Runtime/Native/Windows-X64-Read-Only-Directory.wva" "$work/Directory-Windows.wvo" >/dev/null || exit $?
"$script_directory/Assemble-Wva.sh" "$repository_root/Runtime/Native/Linux-X64-Read-Only-Directory.wva" "$work/Directory-Linux.wvo" >/dev/null || exit $?
"$script_directory/Assemble-Wva.sh" "$repository_root/Runtime/Native/Windows-X64-Standard-Byte-Output.wva" "$work/Output-Windows.wvo" >/dev/null || exit $?
"$script_directory/Assemble-Wva.sh" "$repository_root/Runtime/Native/Linux-X64-Standard-Byte-Output.wva" "$work/Output-Linux.wvo" >/dev/null || exit $?
verify_file "$work/Host.wvo" 2569 ec306b202ba9820a6ccecdc188abb12e54a0c07166c7d3dd2a97a4921c14af20 'file-read host WVO' || exit 1
verify_file "$work/Directory-Windows.wvo" 1951 d2da1c67864c242aeb9797661028295922486de2cf7d37aa41024189afb10f34 'Windows directory WVO' || exit 1
verify_file "$work/Directory-Linux.wvo" 681 0ccbcda71b20eaa024946e4fbb2016853952a39f1fe58ed0a183bde502335d86 'Linux directory WVO' || exit 1
verify_file "$work/Output-Windows.wvo" 430 68f7701dfc1065d8adfe65028ee52d6e4879f41ef4399318123cbc1870629c2f 'Windows output WVO' || exit 1
verify_file "$work/Output-Linux.wvo" 389 8d28e2a7913f647f105991a7c6112f2f63d014dfa7d5723ad7625b2fb5560ee0 'Linux output WVO' || exit 1
echo 'PASS  native file read phase=providers item=3/6'

echo 'START native file read phase=link item=4/6'
"$script_directory/Link-Wvo.sh" 0 File_read_host_entry "$work/Windows-Image.chunk-0" \
    "$work/File-Read.wvo" "$work/Host.wvo" "$work/Directory-Windows.wvo" \
    "$work/Output-Windows.wvo" >"$work/Windows-Link.txt" || exit $?
"$script_directory/Link-Wvo.sh" 0 File_read_host_entry "$work/Linux-Image.chunk-0" \
    "$work/File-Read.wvo" "$work/Host.wvo" "$work/Directory-Linux.wvo" \
    "$work/Output-Linux.wvo" >"$work/Linux-Link.txt" || exit $?
windows_entry=$(sed -n 's/^entry name=File_read_host_entry address=\([0-9][0-9]*\)$/\1/p' "$work/Windows-Link.txt")
linux_entry=$(sed -n 's/^entry name=File_read_host_entry address=\([0-9][0-9]*\)$/\1/p' "$work/Linux-Link.txt")
[[ $windows_entry == 2407616 && $linux_entry == 2407616 ]] || exit 1
verify_file "$work/Windows-Image.chunk-0" 2411432 7905ace13aaea2715c622380177b3a4bdb7470d122d143729603c6ead0d17cfb 'Windows linked image' || exit 1
verify_file "$work/Linux-Image.chunk-0" 2410382 748d356ac947e2eb52fbe7b186a90f0b22aed6bbef6da10f908eff472f22ab05 'Linux linked image' || exit 1
echo 'PASS  native file read phase=link item=4/6'

echo 'START native file read phase=package item=5/6'
"$script_directory/Build-Cached-Hosted-Application.sh" 6 "$work/File-Read.wvb" \
    "$work/Windows-Image" 1 2407616 "$work/File-Read.exe" windows >/dev/null || exit $?
"$script_directory/Build-Cached-Hosted-Application.sh" 6 "$work/File-Read.wvb" \
    "$work/Linux-Image" 1 2407616 "$work/File-Read.elf" linux >/dev/null || exit $?
verify_file "$work/File-Read.exe" 2432000 98c8ae185f9508d7ac6473b433cc8cb21429fe77cf3b218196bf25032e7ba7d5 'Windows file-read application' || exit 1
verify_file "$work/File-Read.elf" 2433024 e24332de44b14766049742941742e31d8a6b55c62ee31510e95ef9a128de0f24 'Linux file-read application' || exit 1
echo 'PASS  native file read phase=package item=5/6'

echo 'START native file read phase=execute item=6/6 cases=32'
"$work/Response.elf" >/dev/null 2>&1
[[ $? -eq 42 ]] || exit 1
node "$script_directory/Verify-File-Read-Application.mjs" linux \
    "$work/File-Read.wvb" "$work/File-Read.exe" "$work/File-Read.elf" >/dev/null || exit $?
echo 'PASS  native file read phase=execute item=6/6 cases=32'
echo 'native file read application status=Passed cases=32 capabilities=5 wvb=95eed93bf74b10214711efe9a8780c4c289c06bbf8b46e835c00119a36190dfb cross-host-images=Verified'
