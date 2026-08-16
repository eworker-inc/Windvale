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
verify_file "$work/Build-Driver.wvb" 1155121 \
    0cd519556a1cf59321b9418bfbf01643283e10e3dd111c8e2083ec0e51c4ce02 \
    'current build driver WVB' || exit 1
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 2 \
    "$work/Build-Driver.wvb" "$work/Build-Driver.elf" --development-cache \
    >/dev/null || exit $?
"$work/Build-Driver.elf" --workspace "$workspace" --project \
    "$repository_root/Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj" \
    "$work/Lowerer.wvb" >/dev/null || exit $?
verify_file "$work/Lowerer.wvb" 522025 \
    318717a608ba37360b9c39f53b9720944ab4463af4ab6a1ec9a267a6ceb85bf6 \
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
verify_file "$work/File-Read.wvb" 76348 \
    4ef96f317c0ac0ca57d60c1c2b6533e6d51cc36b8adb5b481e8ec04b61b69a73 \
    'file-read WVB' || exit 1
echo 'native file read compile step=lower'
"$work/Lowerer.elf" "$work/File-Read.wvb" "$work/File-Read.wvo" >/dev/null || exit $?
verify_file "$work/File-Read.wvo" 2408301 \
    93553d1c06f19c52d276bc7150d7939b5da4d5b5f8b260deb742f00da123b429 \
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
[[ $windows_entry == 2405696 && $linux_entry == 2405696 ]] || exit 1
verify_file "$work/Windows-Image.chunk-0" 2409512 d4d58f79cff5bd4a2066567116f40c8b7128d4578c0959bb2ff08b2be0d2b38e 'Windows linked image' || exit 1
verify_file "$work/Linux-Image.chunk-0" 2408462 9a0a6736ce61c468faf270ab392aa41991ae0763f0a53f9cb42b56641e1f0186 'Linux linked image' || exit 1
echo 'PASS  native file read phase=link item=4/6'

echo 'START native file read phase=package item=5/6'
"$script_directory/Build-Cached-Hosted-Application.sh" 6 "$work/File-Read.wvb" \
    "$work/Windows-Image" 1 2405696 "$work/File-Read.exe" windows >/dev/null || exit $?
"$script_directory/Build-Cached-Hosted-Application.sh" 6 "$work/File-Read.wvb" \
    "$work/Linux-Image" 1 2405696 "$work/File-Read.elf" linux >/dev/null || exit $?
verify_file "$work/File-Read.exe" 2430464 16085cd263600822f693d1f57f14315f47fe4102b76b59a64e333bdcf98615b9 'Windows file-read application' || exit 1
verify_file "$work/File-Read.elf" 2428928 547c311b1f5398d7cc5f67d31782ccb992e98c02dd90edfe0a560b47de575beb 'Linux file-read application' || exit 1
echo 'PASS  native file read phase=package item=5/6'

echo 'START native file read phase=execute item=6/6 cases=32'
"$work/Response.elf" >/dev/null 2>&1
[[ $? -eq 42 ]] || exit 1
node "$script_directory/Verify-File-Read-Application.mjs" linux \
    "$work/File-Read.wvb" "$work/File-Read.exe" "$work/File-Read.elf" >/dev/null || exit $?
echo 'PASS  native file read phase=execute item=6/6 cases=32'
echo 'native file read application status=Passed cases=32 capabilities=5 wvb=4ef96f317c0ac0ca57d60c1c2b6533e6d51cc36b8adb5b481e8ec04b61b69a73 cross-host-images=Verified'
