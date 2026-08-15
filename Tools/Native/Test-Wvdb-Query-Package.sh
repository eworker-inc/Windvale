#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Wvdb-Query-Package.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
manifest="$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvpack"
lock="$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvlock"
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-wvdb-package-test.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-wvdb-package-test.*)
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

"$script_directory/Build-Wvdb-Query-Package.sh" \
    "$manifest" "$lock" "$temporary_directory/First.wvb" >/dev/null || exit $?
"$script_directory/Build-Wvdb-Query-Package.sh" \
    "$manifest" "$lock" "$temporary_directory/Second.wvb" >/dev/null || exit $?
cmp --silent "$temporary_directory/First.wvb" "$temporary_directory/Second.wvb" || exit 1
first_bytes=$(wc -c < "$temporary_directory/First.wvb") || exit 1
first_digest_line=$(sha256sum -- "$temporary_directory/First.wvb") || exit 1
first_sha256=${first_digest_line%% *}
[[ $first_bytes -eq 26294 && $first_sha256 == 61f7b9d739a0f4ac9eece1cb79e554e373f49375109cf23d332921395ae37dc2 ]] || exit 1

"$script_directory/Inspect-Wvb.sh" "$temporary_directory/First.wvb" >"$temporary_directory/Inspect.txt" || exit $?
capability_count=$(grep -c '^capability index=' "$temporary_directory/Inspect.txt") || exit 1
[[ $capability_count -eq 5 ]] || exit 1
for capability in \
    console.write_line \
    diagnostic.write_line \
    filesystem.directory_read_v1 \
    process.argument \
    process.argument_count; do
    grep '^capability index=' "$temporary_directory/Inspect.txt" | \
        grep -F "name=\"$capability\"" >/dev/null || exit 1
done

printf '%s\n' 'windvale-lock 1' >"$temporary_directory/Bad.wvlock"
printf '%s\n' 'preserved-output' >"$temporary_directory/Preserved.wvb"
cp -- "$temporary_directory/Preserved.wvb" "$temporary_directory/Expected.wvb" || exit 1
if "$script_directory/Build-Wvdb-Query-Package.sh" \
    "$manifest" "$temporary_directory/Bad.wvlock" "$temporary_directory/Preserved.wvb" \
    >"$temporary_directory/Bad.out" 2>"$temporary_directory/Bad.err"; then
    exit 1
fi
cmp --silent "$temporary_directory/Expected.wvb" "$temporary_directory/Preserved.wvb" || exit 1

if "$script_directory/Build-Wvdb-Query-Package.sh" \
    "$manifest" "$temporary_directory/Missing.wvlock" "$temporary_directory/Missing.wvb" \
    >"$temporary_directory/Missing.out" 2>"$temporary_directory/Missing.err"; then
    exit 1
fi
[[ ! -e $temporary_directory/Missing.wvb ]] || exit 1

cp -- "$manifest" "$temporary_directory/Alias.wvpack" || exit 1
if "$script_directory/Build-Wvdb-Query-Package.sh" \
    "$temporary_directory/Alias.wvpack" "$lock" "$temporary_directory/Alias.wvb" \
    >"$temporary_directory/Alias.out" 2>"$temporary_directory/Alias.err"; then
    exit 1
fi
[[ ! -e $temporary_directory/Alias.wvb ]] || exit 1

echo 'native package status=Passed builds=2 inspection=1 negative=3 preservation=1 cases=8'
