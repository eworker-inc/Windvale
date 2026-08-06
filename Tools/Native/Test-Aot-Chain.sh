#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Aot-Chain.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-aot-chain.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-aot-chain.*)
            rm -f -- \
                "$temporary_directory/Return-42.wvb" \
                "$temporary_directory/Return-42.wvo" \
                "$temporary_directory/Return-42.bin" \
                "$temporary_directory/Return-42.elf" \
                "$temporary_directory/Return-42.wvmap" \
                "$temporary_directory/Application.err"
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

check_hash() {
    local path=$1
    local digest=$2
    local label=$3
    local directory
    directory=$(CDPATH= cd -- "$(dirname -- "$path")" && pwd -P) || return 1
    if ! (cd -- "$directory" && printf '%s  %s\n' \
        "$digest" "$(basename -- "$path")" | sha256sum --check --strict --quiet); then
        echo "The native AOT chain $label identity differs." >&2
        return 1
    fi
}

wvb="$temporary_directory/Return-42.wvb"
wvo="$temporary_directory/Return-42.wvo"
image="$temporary_directory/Return-42.bin"
application="$temporary_directory/Return-42.elf"
map="$temporary_directory/Return-42.wvmap"
application_error="$temporary_directory/Application.err"

"$repository_root/Tools/Native/Build-Wvb.sh" \
    "$repository_root/Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj" \
    "$wvb" >/dev/null || exit $?
check_hash "$wvb" \
    '7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31' \
    'WVB' || exit $?

"$repository_root/Tools/Native/Lower-Wvb-To-Wvo.sh" "$wvb" "$wvo" \
    >/dev/null || exit $?
"$repository_root/Tools/Native/Verify-Wvo.sh" "$wvo" >/dev/null || exit $?
check_hash "$wvo" \
    '0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5' \
    'WVO' || exit $?

"$repository_root/Tools/Native/Link-Wvo.sh" 1048576 Main "$image" "$wvo" \
    > "$map" || exit $?
check_hash "$image" \
    '7c05565142850adab1d63d999479977a23ef50c7264c03ee55ce5b323df26408' \
    'flat image' || exit $?
check_hash "$map" \
    '857710249807d2fed4da847729d0244f08ccdc70156c043fdaa0516de394e2dc' \
    'link map' || exit $?

"$repository_root/Tools/Native/Package-Console.sh" linux-x64-console-v1 \
    "$image" 0 "$application" >/dev/null || exit $?
check_hash "$application" \
    'fe525b84b9bf902677a5c7beb36872dfd72e7d6d0f12bfb5c95d491c4e1cd3f7' \
    'Linux application' || exit $?

"$application" >/dev/null 2> "$application_error"
application_result=$?
if ((application_result != 42)); then
    echo "The native AOT application result is $application_result, expected 42." >&2
    if [[ -s $application_error ]]; then
        cat -- "$application_error" >&2
    fi
    exit 1
fi
if [[ -s $application_error ]]; then
    echo 'The native AOT application wrote a diagnostic.' >&2
    cat -- "$application_error" >&2
    exit 1
fi

echo 'native aot chain status=Passed result=42'
