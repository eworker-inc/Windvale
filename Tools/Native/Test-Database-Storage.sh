#!/usr/bin/env bash
set -uo pipefail

development=0
prepare_only=0
if [[ $# -eq 1 && $1 == --development ]]; then
    development=1
elif [[ $# -eq 1 && $1 == --prepare-development-tools ]]; then
    development=1
    prepare_only=1
elif [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Database-Storage.sh [--development|--prepare-development-tools]' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-database-storage.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-database-storage.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

build_driver_wvb="$temporary_directory/Build-Driver.wvb"
build_driver="$temporary_directory/Build-Driver.elf"
lowerer_wvb="$temporary_directory/Lowerer.wvb"
lowerer="$temporary_directory/Lowerer.elf"
workspace_path="$repository_root/Windvale.wvws"

verify_file() {
    local path=$1 expected_size=$2 expected_digest=$3
    [[ -f $path && $(wc -c < "$path") -eq $expected_size ]] || return 1
    local directory
    directory=$(CDPATH= cd -- "$(dirname -- "$path")" && pwd -P) || return 1
    (cd -- "$directory" && printf '%s  %s\n' \
        "$expected_digest" "$(basename -- "$path")" | sha256sum --check --strict --quiet)
}

sha256_file() {
    sha256sum -- "$1" | awk '{ print $1 }'
}

accept_build_driver_checkpoint() {
    local directory=$1 expected_key=$2 expected_input=$3
    local manifest="$directory/Checkpoint.txt"
    local application="$directory/Build-Driver.elf"
    [[ -f $manifest && -f $application && ! -L $directory &&
        ! -L $manifest && ! -L $application ]] || return 1
    [[ $(wc -c < "$manifest") -le 512 ]] || return 1
    local actual_bytes actual_sha256 expected_manifest
    actual_bytes=$(wc -c < "$application") || return 1
    [[ $actual_bytes -gt 0 && $actual_bytes -le 67108864 ]] || return 1
    actual_sha256=$(sha256_file "$application") || return 1
    expected_manifest="$temporary_directory/Checkpoint-Expected.txt"
    printf '%s\n' \
        'windvale-native-tool-checkpoint 1' \
        "key $expected_key" \
        "input-sha256 $expected_input" \
        "output-bytes $actual_bytes" \
        "output-sha256 $actual_sha256" \
        > "$expected_manifest" || return 1
    cmp --silent -- "$expected_manifest" "$manifest" || return 1
    build_driver=$application
}

prepare_cached_build_driver() {
    local input=$1
    local input_sha256 package_sha256 stage_sha256 link_sha256 transport_sha256
    local hosted_sha256 inventory_sha256 material key
    input_sha256=$(sha256_file "$input") || return 1
    package_sha256=$(sha256_file "$script_directory/Package-Segmented-Compiler-Wvb.sh") || return 1
    stage_sha256=$(sha256_file "$script_directory/Stage-Compiler-Wvb.sh") || return 1
    link_sha256=$(sha256_file "$script_directory/Link-Staged-Compiler-Wvo.sh") || return 1
    transport_sha256=$(sha256_file "$script_directory/Transport-Compiler-Image.sh") || return 1
    hosted_sha256=$(sha256_file "$script_directory/Package-Hosted-Wvb.sh") || return 1
    inventory_sha256=$(sha256_file \
        "$repository_root/Artifacts/Native-Hosted-Container-Toolset-Candidate/SHA256SUMS") || return 1
    material="build-driver-v1-linux-profile-2-$input_sha256-$package_sha256-$stage_sha256-$link_sha256-$transport_sha256-$hosted_sha256-$inventory_sha256"
    key=$(printf '%s\n' "$material" | sha256sum | awk '{ print $1 }') || return 1

    local cache_root_input
    if [[ -n ${WINDVALE_NATIVE_CACHE_ROOT:-} ]]; then
        cache_root_input=$WINDVALE_NATIVE_CACHE_ROOT
    else
        cache_root_input="${XDG_CACHE_HOME:-$HOME/.cache}/windvale/native-tool-cache"
    fi
    [[ ! -L $cache_root_input ]] || return 1
    mkdir -p -- "$cache_root_input" || return 1
    local cache_root
    cache_root=$(CDPATH= cd -- "$cache_root_input" && pwd -P) || return 1
    [[ -z $(find "$cache_root" -type l -print -quit) ]] || return 1
    local family="$cache_root/build-driver-v1/linux-profile-2"
    mkdir -p -- "$family" || return 1
    [[ ! -L $family ]] || return 1
    local directory="$family/$key"
    if [[ -e $directory ]]; then
        accept_build_driver_checkpoint "$directory" "$key" "$input_sha256" || return 1
        tool_checkpoint=Hit
        return 0
    fi

    local temporary
    temporary=$(mktemp -d "$family/.new-$key.XXXXXXXX") || return 1
    local candidate="$temporary/Build-Driver.elf"
    if ! "$script_directory/Package-Segmented-Compiler-Wvb.sh" \
        2 "$input" "$candidate" >/dev/null; then
        rm -f -- "$candidate"
        rmdir -- "$temporary"
        return 1
    fi
    local output_sha256 output_bytes
    output_sha256=$(sha256_file "$candidate") || return 1
    output_bytes=$(wc -c < "$candidate") || return 1
    [[ $output_bytes -gt 0 && $output_bytes -le 67108864 ]] || return 1
    printf '%s\n' \
        'windvale-native-tool-checkpoint 1' \
        "key $key" \
        "input-sha256 $input_sha256" \
        "output-bytes $output_bytes" \
        "output-sha256 $output_sha256" \
        > "$temporary/Checkpoint.txt" || return 1
    mv -- "$temporary" "$directory" || return 1
    accept_build_driver_checkpoint "$directory" "$key" "$input_sha256" || return 1
    tool_checkpoint=Created
}

if ((development == 1)); then
    "$script_directory/Build-Wvb.sh" \
        "$repository_root/Projects/Tools/Windvale-Compiler-Build-Driver.wvproj" \
        "$build_driver_wvb" >/dev/null || exit $?
    prepare_cached_build_driver "$build_driver_wvb" || exit $?
    lowerer="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo.elf"
    verify_file "$lowerer" 6500352 \
        de7bdb40637208ee05a7987aba0ea88366638e132fb3f7ba5d9730befde316b5 || exit $?
else
    "$script_directory/Build-Wvb.sh" \
        "$repository_root/Projects/Tools/Windvale-Compiler-Build-Driver.wvproj" \
        "$build_driver_wvb" >/dev/null || exit $?
    "$script_directory/Package-Segmented-Compiler-Wvb.sh" \
        2 "$build_driver_wvb" "$build_driver" >/dev/null || exit $?

    "$script_directory/Build-Wvb.sh" \
        "$repository_root/Projects/Compiler/Windvale-Native-X64-Lowering-Tool.wvproj" \
        "$lowerer_wvb" >/dev/null || exit $?
    "$script_directory/Package-Segmented-Compiler-Wvb.sh" \
        6 "$lowerer_wvb" "$lowerer" >/dev/null || exit $?
fi

if ((prepare_only == 1)); then
    echo "native database storage development tools status=Passed checkpoint=$tool_checkpoint"
    exit 0
fi

verify_target() {
    local label=$1 project_path=$2
    local first_wvb="$temporary_directory/$label-First.wvb"
    local second_wvb="$temporary_directory/$label-Second.wvb"
    local first_wvo="$temporary_directory/$label-First.wvo"
    local second_wvo="$temporary_directory/$label-Second.wvo"
    local image="$temporary_directory/$label.bin"
    local image_prefix="$temporary_directory/$label-Image"
    local map="$temporary_directory/$label.map"
    local linux_application="$temporary_directory/$label.elf"
    local windows_application="$temporary_directory/$label.exe"

    "$build_driver" --workspace "$workspace_path" --project "$project_path" "$first_wvb" >/dev/null || return $?
    if ((development == 0)); then
        "$build_driver" --workspace "$workspace_path" --project "$project_path" "$second_wvb" >/dev/null || return $?
        cmp --silent -- "$first_wvb" "$second_wvb" || return 1
    fi

    "$lowerer" "$first_wvb" "$first_wvo" >/dev/null || return $?
    if ((development == 0)); then
        "$lowerer" "$second_wvb" "$second_wvo" >/dev/null || return $?
        cmp --silent -- "$first_wvo" "$second_wvo" || return 1
    fi
    "$script_directory/Verify-Wvo.sh" "$first_wvo" >/dev/null || return $?

    "$script_directory/Link-Wvo.sh" 0 Main "$image" "$first_wvo" >"$map" || return $?
    local entry_offset
    entry_offset=$(sed -n 's/^entry name=Main address=\([0-9][0-9]*\)$/\1/p' "$map")
    case "$entry_offset" in
        ''|*[!0-9]*) return 1 ;;
    esac
    cp -- "$image" "$image_prefix.chunk-0" || return $?

    "$script_directory/Package-Hosted-Wvb.sh" image 6 \
        "$first_wvb" "$image_prefix" 1 "$entry_offset" "$linux_application" linux \
        >/dev/null || return $?
    "$linux_application" >/dev/null
    local application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The $label database-storage case returned $application_result, expected 0." >&2
        return 1
    fi

    "$script_directory/Package-Hosted-Wvb.sh" image 6 \
        "$first_wvb" "$image_prefix" 1 "$entry_offset" "$windows_application" windows \
        >/dev/null || return $?
}

verify_storage_lowering() {
    local project_path=$1
    local first_wvb="$temporary_directory/StorageLowering-First.wvb"
    local second_wvb="$temporary_directory/StorageLowering-Second.wvb"
    local first_wvo="$temporary_directory/StorageLowering-First.wvo"
    local second_wvo="$temporary_directory/StorageLowering-Second.wvo"
    local first_report="$temporary_directory/StorageLowering-First.txt"
    local second_report="$temporary_directory/StorageLowering-Second.txt"
    local first_bridge="$temporary_directory/StorageLowering-Bridge-First.wvo"
    local second_bridge="$temporary_directory/StorageLowering-Bridge-Second.wvo"
    local image="$temporary_directory/StorageLowering.bin"
    local image_prefix="$temporary_directory/StorageLowering-Image"
    local map="$temporary_directory/StorageLowering.map"
    local linux_application="$temporary_directory/StorageLowering.elf"
    local windows_application="$temporary_directory/StorageLowering.exe"

    "$build_driver" --workspace "$workspace_path" --project "$project_path" "$first_wvb" >/dev/null || return $?
    "$build_driver" --workspace "$workspace_path" --project "$project_path" "$second_wvb" >/dev/null || return $?
    cmp --silent -- "$first_wvb" "$second_wvb" || return 1

    "$lowerer" "$first_wvb" "$first_wvo" >"$first_report" || return $?
    "$lowerer" "$second_wvb" "$second_wvo" >"$second_report" || return $?
    grep -q '^native x64 status=Valid abi=23 ' "$first_report" || return 1
    cmp --silent -- "$first_report" "$second_report" || return 1
    cmp --silent -- "$first_wvo" "$second_wvo" || return 1
    "$script_directory/Verify-Wvo.sh" "$first_wvo" >/dev/null || return $?

    "$script_directory/Assemble-Wva.sh" \
        "$repository_root/Runtime/Native/X64-Random-Access-Storage-Describe-Probe.wva" \
        "$first_bridge" >/dev/null || return $?
    "$script_directory/Assemble-Wva.sh" \
        "$repository_root/Runtime/Native/X64-Random-Access-Storage-Describe-Probe.wva" \
        "$second_bridge" >/dev/null || return $?
    cmp --silent -- "$first_bridge" "$second_bridge" || return 1
    "$script_directory/Verify-Wvo.sh" "$first_bridge" >/dev/null || return $?

    "$script_directory/Link-Wvo.sh" 0 Storage_describe_probe_entry \
        "$image" "$first_wvo" "$first_bridge" >"$map" || return $?
    local entry_offset
    entry_offset=$(sed -n \
        's/^entry name=Storage_describe_probe_entry address=\([0-9][0-9]*\)$/\1/p' \
        "$map")
    case "$entry_offset" in
        ''|*[!0-9]*) return 1 ;;
    esac
    cp -- "$image" "$image_prefix.chunk-0" || return $?

    "$script_directory/Package-Hosted-Wvb.sh" image 6 \
        "$first_wvb" "$image_prefix" 1 "$entry_offset" \
        "$linux_application" linux >/dev/null || return $?
    "$linux_application" >/dev/null
    local application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The ABI-23 storage describe execution returned $application_result, expected 0." >&2
        return 1
    fi

    "$script_directory/Package-Hosted-Wvb.sh" image 6 \
        "$first_wvb" "$image_prefix" 1 "$entry_offset" \
        "$windows_application" windows >/dev/null || return $?
}

verify_host_storage_interruption() {
    local application=$1 initial=$2 step=$3 scenario_root=$4
    local scenario_directory="$scenario_root/HostStorage-Interruption-$step"
    local scenario_storage="$scenario_directory/Windvale-Database-Storage.bin"
    mkdir -- "$scenario_directory" || return $?
    cp -- "$initial" "$scenario_storage" || return $?
    truncate -s $((4609 + step)) -- "$scenario_storage" || return $?

    (cd -- "$scenario_directory" && "$application" >/dev/null)
    local application_result=$?
    local expected_result=$((90 + step))
    if [[ $application_result -ne $expected_result ]]; then
        echo "The native host-storage interruption $step returned $application_result, expected $expected_result." >&2
        return 1
    fi
    (cd -- "$scenario_directory" && "$application" >/dev/null)
    application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host-storage restart $step returned $application_result, expected 0." >&2
        return 1
    fi
    local scenario_bytes
    scenario_bytes=$(wc -c < "$scenario_storage") || return 1
    if ((step <= 2 && scenario_bytes != 4608)); then return 1; fi
    if ((step == 3 && scenario_bytes != 4608 && scenario_bytes != 12800)); then
        return 1
    fi
    if ((step == 4 && scenario_bytes != 12800)); then return 1; fi
}

verify_host_storage() {
    local project_path=$1
    local first_wvb="$temporary_directory/HostStorage-First.wvb"
    local second_wvb="$temporary_directory/HostStorage-Second.wvb"
    local first_wvo="$temporary_directory/HostStorage-First.wvo"
    local second_wvo="$temporary_directory/HostStorage-Second.wvo"
    local common_first="$temporary_directory/HostStorage-Common-First.wvo"
    local common_second="$temporary_directory/HostStorage-Common-Second.wvo"
    local windows_platform="$temporary_directory/HostStorage-Windows.wvo"
    local linux_platform="$temporary_directory/HostStorage-Linux.wvo"
    local linux_image="$temporary_directory/HostStorage-Linux.bin"
    local linux_image_prefix="$temporary_directory/HostStorage-Linux-Image"
    local linux_map="$temporary_directory/HostStorage-Linux.map"
    local linux_application="$temporary_directory/HostStorage.elf"
    local windows_image="$temporary_directory/HostStorage-Windows.bin"
    local windows_image_prefix="$temporary_directory/HostStorage-Windows-Image"
    local windows_map="$temporary_directory/HostStorage-Windows.map"
    local windows_application="$temporary_directory/HostStorage.exe"
    local run_directory="$temporary_directory/HostStorage-Run"
    local storage_file="$run_directory/Windvale-Database-Storage.bin"
    local initial_file="$run_directory/Windvale-Database-Storage.initial"

    "$build_driver" --workspace "$workspace_path" --project "$project_path" "$first_wvb" >/dev/null || return $?
    if ((development == 0)); then
        "$build_driver" --workspace "$workspace_path" --project "$project_path" "$second_wvb" >/dev/null || return $?
        cmp --silent -- "$first_wvb" "$second_wvb" || return 1
    fi

    "$lowerer" "$first_wvb" "$first_wvo" >/dev/null || return $?
    if ((development == 0)); then
        "$lowerer" "$second_wvb" "$second_wvo" >/dev/null || return $?
        cmp --silent -- "$first_wvo" "$second_wvo" || return 1
    fi
    "$script_directory/Verify-Wvo.sh" "$first_wvo" >/dev/null || return $?

    "$script_directory/Assemble-Wva.sh" \
        "$repository_root/Runtime/Native/X64-Random-Access-Storage-Host.wva" \
        "$common_first" >/dev/null || return $?
    if ((development == 0)); then
        "$script_directory/Assemble-Wva.sh" \
            "$repository_root/Runtime/Native/X64-Random-Access-Storage-Host.wva" \
            "$common_second" >/dev/null || return $?
        cmp --silent -- "$common_first" "$common_second" || return 1
    fi
    "$script_directory/Verify-Wvo.sh" "$common_first" >/dev/null || return $?

    "$script_directory/Assemble-Wva.sh" \
        "$repository_root/Runtime/Native/Linux-X64-Random-Access-Storage.wva" \
        "$linux_platform" >/dev/null || return $?
    "$script_directory/Verify-Wvo.sh" "$linux_platform" >/dev/null || return $?
    "$script_directory/Assemble-Wva.sh" \
        "$repository_root/Runtime/Native/Windows-X64-Random-Access-Storage.wva" \
        "$windows_platform" >/dev/null || return $?
    "$script_directory/Verify-Wvo.sh" "$windows_platform" >/dev/null || return $?

    "$script_directory/Link-Wvo.sh" 0 Storage_host_entry \
        "$linux_image" "$first_wvo" "$common_first" "$linux_platform" \
        >"$linux_map" || return $?
    local linux_entry
    linux_entry=$(sed -n \
        's/^entry name=Storage_host_entry address=\([0-9][0-9]*\)$/\1/p' \
        "$linux_map")
    case "$linux_entry" in
        ''|*[!0-9]*) return 1 ;;
    esac
    cp -- "$linux_image" "$linux_image_prefix.chunk-0" || return $?
    "$script_directory/Package-Hosted-Wvb.sh" image 6 \
        "$first_wvb" "$linux_image_prefix" 1 "$linux_entry" \
        "$linux_application" linux >/dev/null || return $?

    mkdir -- "$run_directory" || return $?
    (cd -- "$run_directory" && "$linux_application" >/dev/null)
    local application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host-storage create run returned $application_result, expected 0." >&2
        return 1
    fi
    [[ $(wc -c < "$storage_file") -eq 4608 ]] || return 1
    cp -- "$storage_file" "$initial_file" || return $?

    truncate -s 4625 -- "$storage_file" || return $?
    [[ $(wc -c < "$storage_file") -eq 4625 ]] || return 1
    (cd -- "$run_directory" && "$linux_application" >/dev/null)
    application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host-storage recovery run returned $application_result, expected 0." >&2
        return 1
    fi
    [[ $(wc -c < "$storage_file") -eq 4608 ]] || return 1
    cmp --silent -- "$initial_file" "$storage_file" || return 1

    (cd -- "$run_directory" && "$linux_application" >/dev/null)
    application_result=$?
    if [[ $application_result -ne 0 ]]; then
        echo "The native host-storage stable reopen returned $application_result, expected 0." >&2
        return 1
    fi
    cmp --silent -- "$initial_file" "$storage_file" || return 1

    local step
    for step in 0 1 2 3 4; do
        verify_host_storage_interruption \
            "$linux_application" "$initial_file" "$step" "$temporary_directory" \
            || return $?
    done

    if ((development == 1)); then
        return 0
    fi

    "$script_directory/Link-Wvo.sh" 0 Storage_host_entry \
        "$windows_image" "$first_wvo" "$common_first" "$windows_platform" \
        >"$windows_map" || return $?
    local windows_entry
    windows_entry=$(sed -n \
        's/^entry name=Storage_host_entry address=\([0-9][0-9]*\)$/\1/p' \
        "$windows_map")
    case "$windows_entry" in
        ''|*[!0-9]*) return 1 ;;
    esac
    cp -- "$windows_image" "$windows_image_prefix.chunk-0" || return $?
    "$script_directory/Package-Hosted-Wvb.sh" image 6 \
        "$first_wvb" "$windows_image_prefix" 1 "$windows_entry" \
        "$windows_application" windows >/dev/null || return $?
}

if ((development == 0)); then
    verify_target Nested \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Nested-Record-Fields.wvproj" || exit $?
    verify_target Publication \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Storage-Publication.wvproj" || exit $?
    verify_target Recovery \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Storage-Recovery.wvproj" || exit $?
    verify_target SingleWriter \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Single-Writer-Commit.wvproj" || exit $?
    verify_target ProviderTable \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Capability-Provider-Table.wvproj" || exit $?
    verify_target ProviderCall \
        "$repository_root/Projects/Tests/Windvale-Native-Test-X64-Provider-Call.wvproj" || exit $?
    verify_target Context9 \
        "$repository_root/Projects/Tests/Windvale-Native-Test-Execution-Context-9.wvproj" || exit $?
    verify_storage_lowering \
        "$repository_root/Projects/Tests/Windvale-Native-Test-X64-Storage-Random-Access.wvproj" || exit $?
fi
verify_host_storage \
    "$repository_root/Projects/Tests/Windvale-Native-Test-Database-Host-Storage.wvproj" || exit $?

if ((development == 1)); then
    echo "native database storage development status=Passed cases=1 local-results=0 tools=$tool_checkpoint"
    exit 0
fi
echo 'native database storage status=Passed cases=10 local-results=0 cross-host-images=Verified'
