#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Offline-Generation-Lifecycle.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-generation-lifecycle.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-generation-lifecycle.*) rm -rf -- "$work" ;;
        *) echo "Refusing to remove unexpected temporary path: $work" >&2; return 1 ;;
    esac
}
trap cleanup EXIT

echo 'native offline generation lifecycle step=build-tools item=1/4 tools=2'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Installation-Activation-Planner.wvproj" \
    "$work/Planner.wvb" || exit $?
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Installation-Command-Resolver.wvproj" \
    "$work/Resolver.wvb" || exit $?

echo 'native offline generation lifecycle step=package-tools item=2/4 target=linux-x64'
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$work/Planner.wvb" "$work/Planner.elf" linux || exit $?
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$work/Resolver.wvb" "$work/Resolver.elf" linux || exit $?

echo 'native offline generation lifecycle step=verify-planner item=3/4 cases=12'
node "$repository_root/Tools/Package/Verify-Installation-Activation-Planner.mjs" \
    "$work/Planner.elf" || exit $?

echo 'native offline generation lifecycle step=compose-lifecycle item=4/4 cases=12'
node "$repository_root/Tools/Package/Verify-Offline-Generation-Lifecycle.mjs" \
    "$work/Planner.elf" "$work/Resolver.elf" linux-x64 || exit $?
echo 'native offline lifecycle composition status=Passed cases=24 planner=12 lifecycle=12 generations=2 activations=3 rollback=Verified'
