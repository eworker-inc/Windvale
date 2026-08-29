#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Echo-Command-Launch.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-echo-command-launch.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-echo-command-launch.*) rm -rf -- "$work" ;;
        *) echo "Refusing to remove unexpected temporary path: $work" >&2; return 1 ;;
    esac
}
trap cleanup EXIT

verify_file() {
    local file=$1 expected_bytes=$2 expected_sha256=$3
    [[ -f $file && $(wc -c <"$file") -eq $expected_bytes ]] || return 1
    [[ $(sha256sum -- "$file" | cut -d ' ' -f 1) == "$expected_sha256" ]]
}

echo 'native echo command launch step=build-tools item=1/3'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Installation-Command-Resolver.wvproj" \
    "$work/Resolver.wvb" || exit $?
verify_file "$work/Resolver.wvb" 60732 \
    521cd77ee53f20cec3157208e4f0b9c93841c212dcabec88f4e7cbc6a9229679 || exit 1
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$work/Resolver.wvb" "$work/Resolver.elf" linux || exit $?
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Package-Bundle-Writer.wvproj" \
    "$work/Writer.wvb" || exit $?
verify_file "$work/Writer.wvb" 510498 \
    7bc577ac157fc20c301699e5cd08286b736017922871f5206b045d6c46b93a1d || exit 1
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 6 \
    "$work/Writer.wvb" "$work/Writer.elf" || exit $?
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Package-Bundle-Verifier.wvproj" \
    "$work/Verifier.wvb" || exit $?
verify_file "$work/Verifier.wvb" 529791 \
    218e8939a6e0686c6d2086e2ce977c405abb77728280b332bdf15277f8fa606b || exit 1
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 6 \
    "$work/Verifier.wvb" "$work/Verifier.elf" || exit $?

echo 'native echo command launch step=construct-package item=2/3'
"$script_directory/Build-Echo-Package.sh" \
    "$repository_root/Distribution/Applications/Echo/Windvale-Echo.wvpack" \
    "$repository_root/Distribution/Applications/Echo/Windvale-Echo.wvlock" \
    "$work/Echo.wvb" || exit $?
node -e "const fs=require('node:fs');const x=fs.readFileSync(process.argv[1],'utf8').replaceAll('\\r\\n','\\n');if(x.includes('\\r')||x.includes('\\0'))process.exit(1);fs.writeFileSync(process.argv[2],x);" \
    "$repository_root/LICENSE.md" "$work/LICENSE.md" || exit $?
"$work/Writer.elf" \
    "$repository_root/Distribution/Applications/Echo/Windvale-Echo.wvpack" \
    "$repository_root/Distribution/Applications/Echo/Windvale-Echo.wvlock" \
    "$work/Echo.wvb" "$work/LICENSE.md" \
    "$repository_root/Distribution/Applications/Echo/Windvale-Echo.wvprov" \
    "$work/Echo.wvbundle" >/dev/null || exit $?
verify_file "$work/Echo.wvbundle" 17009 \
    a649a98c6d6f8dd2873f1b5097f74f613f7e3422929ef8838a7f6522bc464a0e || exit 1
bundle_report=$("$work/Verifier.elf" "$work/Echo.wvbundle") || exit $?
[[ $bundle_report == 'bundle status=Valid bytes=17009 package=windvale.echo version=0.1.0 target=hosted-wvb-v1 items=3 blobs=5 sha256=a649a98c6d6f8dd2873f1b5097f74f613f7e3422929ef8838a7f6522bc464a0e' ]] || exit 1

echo 'native echo command launch step=package-and-dispatch item=3/3 cases=10'
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$work/Echo.wvb" "$work/Echo.elf" linux || exit $?
verify_file "$work/Echo.elf" 24576 \
    0e5a91887381adb23a84d745ce06902be99e53d70e58a598465939881638b576 || exit 1
node "$repository_root/Tools/Package/Verify-Echo-Command-Launch.mjs" \
    "$work/Resolver.elf" linux-x64 "$work/Echo.wvbundle" "$work/Echo.elf"
