#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Installation-Command-Dispatch.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-command-dispatch.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-command-dispatch.*) rm -rf -- "$work" ;;
        *) echo "Refusing to remove unexpected temporary path: $work" >&2; return 1 ;;
    esac
}
trap cleanup EXIT

verify_file() {
    local file=$1 expected_bytes=$2 expected_sha256=$3
    [[ -f $file && $(wc -c <"$file") -eq $expected_bytes ]] || return 1
    [[ $(sha256sum -- "$file" | cut -d ' ' -f 1) == "$expected_sha256" ]]
}

echo 'native installation command dispatch step=build-selection-and-bundle-tools item=1/7'
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
verify_file "$work/Writer.wvb" 283725 \
    6cf19d10d49cd27496ea7a3aa4ea11dec4baa792001697bf6e2835c0ed2c3a14 || exit 1
"$script_directory/Package-Hosted-Wvb.sh" 6 \
    "$work/Writer.wvb" "$work/Writer.elf" linux || exit $?

echo 'native installation command dispatch step=build-package-payloads item=2/7 packages=2'
"$script_directory/Build-Wvdb-Query-Package.sh" \
    "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvpack" \
    "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvlock" \
    "$work/Wvdb-Query.wvb" || exit $?
"$script_directory/Build-Wvb-Inspector-Package.sh" \
    "$repository_root/Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvpack" \
    "$repository_root/Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvlock" \
    "$work/Wvb-Inspector.wvb" || exit $?

echo 'native installation command dispatch step=construct-exact-bundles item=3/7 packages=2'
node -e "const fs=require('node:fs');const x=fs.readFileSync(process.argv[1],'utf8').replaceAll('\\r\\n','\\n');if(x.includes('\\r')||x.includes('\\0'))process.exit(1);fs.writeFileSync(process.argv[2],x);" \
    "$repository_root/LICENSE.md" "$work/LICENSE.md" || exit $?
"$work/Writer.elf" \
    "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvpack" \
    "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvlock" \
    "$work/Wvdb-Query.wvb" "$work/LICENSE.md" \
    "$repository_root/Distribution/Applications/Wvdb-Query/Windvale-Wvdb-Query.wvprov" \
    "$work/Wvdb-Query.wvbundle" >/dev/null || exit $?
verify_file "$work/Wvdb-Query.wvbundle" 43725 \
    3d7f035e15fa839d9a7a3f8df6a7fa152e115aba42c1b48bdd1ae0b1ba998474 || exit 1
"$work/Writer.elf" \
    "$repository_root/Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvpack" \
    "$repository_root/Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvlock" \
    "$work/Wvb-Inspector.wvb" "$work/LICENSE.md" \
    "$repository_root/Distribution/Applications/Wvb-Inspector/Windvale-Wvb-Inspector.wvprov" \
    "$work/Wvb-Inspector.wvbundle" >/dev/null || exit $?
verify_file "$work/Wvb-Inspector.wvbundle" 92781 \
    a9be069d9eaab7a612a8833d8ce621d1598e01d250ba53a62a2ab4b2126fc4a9 || exit 1

echo 'native installation command dispatch step=lower-wvdb-host item=4/7'
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj" \
    "$work/Lowerer.wvb" || exit $?
verify_file "$work/Lowerer.wvb" 522025 \
    318717a608ba37360b9c39f53b9720944ab4463af4ab6a1ec9a267a6ceb85bf6 || exit 1
"$script_directory/Package-Segmented-Compiler-Wvb.sh" 6 \
    "$work/Lowerer.wvb" "$work/Lowerer.elf" || exit $?
"$work/Lowerer.elf" "$work/Wvdb-Query.wvb" "$work/Wvdb-Query.wvo" >/dev/null || exit $?
verify_file "$work/Wvdb-Query.wvo" 237210 \
    b3d3bbde00136c230f6804215c352490bae9603b338d25186dba827be137edbf || exit 1

echo 'native installation command dispatch step=bind-rights-reduced-provider item=5/7 target=linux-x64'
"$script_directory/Assemble-Wva.sh" \
    "$repository_root/Runtime/Native/X64-Read-Only-Directory-Host.wva" \
    "$work/Directory-Host.wvo" || exit $?
"$script_directory/Assemble-Wva.sh" \
    "$repository_root/Runtime/Native/Linux-X64-Read-Only-Directory.wva" \
    "$work/Directory-Linux.wvo" || exit $?
"$script_directory/Link-Wvo.sh" 0 Directory_host_entry \
    "$work/Linux-Image.chunk-0" "$work/Wvdb-Query.wvo" \
    "$work/Directory-Host.wvo" "$work/Directory-Linux.wvo" \
    >"$work/Link.txt" || exit $?
verify_file "$work/Linux-Image.chunk-0" 237517 \
    cae8aee6da474d2acb0a976047c689511a22269377b58114a56e8616fecc708d || exit 1

echo 'native installation command dispatch step=package-exact-host item=6/7 target=linux-x64'
"$script_directory/Build-Cached-Hosted-Application.sh" 6 \
    "$work/Wvdb-Query.wvb" "$work/Linux-Image" 1 235440 \
    "$work/Wvdb-Query.elf" linux || exit $?
verify_file "$work/Wvdb-Query.elf" 258048 \
    b21095d6ab62209b67053b7dfe1cf5a2f0130b3722a09a8e48284fc1aa988b3f || exit 1

echo 'native installation command dispatch step=dispatch-and-reject item=7/7 cases=9 executions=2'
node "$repository_root/Tools/Package/Verify-Installation-Command-Dispatcher.mjs" \
    "$work/Resolver.elf" linux-x64 "$work/Wvb-Inspector.wvbundle" \
    "$work/Wvdb-Query.wvbundle" "$work/Wvdb-Query.elf"
