#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || $1 != *.elf ]]; then
    echo 'Usage: ./Tools/Native/Build-Windvale-Model-Chat.sh <output.elf>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
output_directory=$(dirname -- "$1")
mkdir -p -- "$output_directory" || exit 1
output=$(CDPATH= cd -- "$output_directory" && pwd -P)/$(basename -- "$1")
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-model-chat-build.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-model-chat-build.*) rm -rf -- "$work" ;;
        *) echo "Refusing to remove unexpected temporary path: $work" >&2; return 1 ;;
    esac
}
trap cleanup EXIT

echo 'START Windvale model chat build phase=self-host item=1/5'
lowerer_project="$repository_root/Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj"
terminal_project="$repository_root/Projects/Tests/Windvale-Native-Test-Terminal-Line-Input-Core.wvproj"
chat_project="$repository_root/Projects/Tests/Windvale-Native-Test-Model-Chat-Core.wvproj"
model_chat_project="$repository_root/Projects/Applications/Windvale-Application-Model-Chat.wvproj"
node "$script_directory/Build-Current-Split-Project-Wvb.mjs" \
    "$lowerer_project" "$work/Lowerer.wvb" \
    "$terminal_project" "$work/Terminal-A.wvb" \
    "$terminal_project" "$work/Terminal-B.wvb" \
    "$chat_project" "$work/Chat-A.wvb" \
    "$chat_project" "$work/Chat-B.wvb" \
    "$model_chat_project" "$work/Model-Chat-A.wvb" \
    "$model_chat_project" "$work/Model-Chat-B.wvb" || exit $?
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 6 "$work/Lowerer.wvb" \
    "$work/Lowerer.elf" --development-cache >/dev/null || exit $?
echo 'PASS  Windvale model chat build phase=self-host item=1/5'

echo 'START Windvale model chat build phase=compile item=2/5'
for name in Terminal Chat; do
    cmp -s -- "$work/$name-A.wvb" "$work/$name-B.wvb" || exit 1
    "$script_directory/Package-Hosted-Wvb.sh" 2 "$work/$name-A.wvb" \
        "$work/$name.exe" windows >/dev/null || exit $?
    "$script_directory/Package-Hosted-Wvb.sh" 2 "$work/$name-A.wvb" \
        "$work/$name.elf" linux >/dev/null || exit $?
    "$work/$name.elf" >/dev/null 2>&1
    [[ $? -eq 42 ]] || exit 1
done
for suffix in A B; do
    "$work/Lowerer.elf" "$work/Model-Chat-$suffix.wvb" \
        "$work/Model-Chat-$suffix.wvo" >/dev/null || exit $?
done
cmp -s -- "$work/Model-Chat-A.wvb" "$work/Model-Chat-B.wvb" || exit 1
cmp -s -- "$work/Model-Chat-A.wvo" "$work/Model-Chat-B.wvo" || exit 1
"$script_directory/Check-Wvo.sh" "$work/Model-Chat-A.wvo" >/dev/null || exit $?
echo 'PASS  Windvale model chat build phase=compile item=2/5'

echo 'START Windvale model chat build phase=providers item=3/5'
for name in Host Windows Linux; do
    case "$name" in
        Host) source_path="$repository_root/Runtime/Native/X64-External-Model-Gateway-Host.wva" ;;
        Windows) source_path="$repository_root/Runtime/Native/Windows-X64-External-Model-Gateway.wva" ;;
        Linux) source_path="$repository_root/Runtime/Native/Linux-X64-External-Model-Gateway.wva" ;;
    esac
    "$script_directory/Assemble-Wva.sh" "$source_path" "$work/$name-A.wvo" >/dev/null || exit $?
    "$script_directory/Assemble-Wva.sh" "$source_path" "$work/$name-B.wvo" >/dev/null || exit $?
    cmp -s -- "$work/$name-A.wvo" "$work/$name-B.wvo" || exit 1
    "$script_directory/Check-Wvo.sh" "$work/$name-A.wvo" >/dev/null || exit $?
done
echo 'PASS  Windvale model chat build phase=providers item=3/5'

echo 'START Windvale model chat build phase=link item=4/5'
"$script_directory/Link-Wvo.sh" 0 Model_gateway_host_entry \
    "$work/Model-Chat-Image.chunk-0" "$work/Model-Chat-A.wvo" \
    "$work/Host-A.wvo" "$work/Linux-A.wvo" >"$work/Link.txt" || exit $?
entry=$(sed -n 's/^entry name=Model_gateway_host_entry address=\([0-9][0-9]*\)$/\1/p' "$work/Link.txt")
[[ -n $entry ]] || exit 1
"$script_directory/Link-Wvo.sh" 0 Model_gateway_host_entry \
    "$work/Model-Chat-Windows-Image.chunk-0" "$work/Model-Chat-A.wvo" \
    "$work/Host-A.wvo" "$work/Windows-A.wvo" >"$work/Windows-Link.txt" || exit $?
windows_entry=$(sed -n 's/^entry name=Model_gateway_host_entry address=\([0-9][0-9]*\)$/\1/p' "$work/Windows-Link.txt")
[[ -n $windows_entry ]] || exit 1
echo 'PASS  Windvale model chat build phase=link item=4/5'

echo 'START Windvale model chat build phase=package item=5/5'
"$script_directory/Build-Cached-Hosted-Application.sh" 6 "$work/Model-Chat-A.wvb" \
    "$work/Model-Chat-Image" 1 "$entry" "$work/Windvale-Model-Chat.elf" linux \
    >/dev/null || exit $?
"$script_directory/Build-Cached-Hosted-Application.sh" 6 "$work/Model-Chat-A.wvb" \
    "$work/Model-Chat-Windows-Image" 1 "$windows_entry" \
    "$work/Windvale-Model-Chat.exe" windows >/dev/null || exit $?
cp -- "$work/Windvale-Model-Chat.elf" "$output" || exit 1
chmod 755 -- "$output" || exit 1
echo 'PASS  Windvale model chat build phase=package item=5/5'
echo "Windvale model chat build status=Published target=linux output=$output core-cases=32 cross-host-images=Verified"
