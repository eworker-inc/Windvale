#!/usr/bin/env sh
set -eu

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
if [[ ${1:-} == chat && -z ${WINDVALE_MODEL_CHAT_APPLICATION:-} ]]; then
    repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
    native_application="$repository_root/Artifacts/Applications/Model-Chat/Windvale-Model-Chat.elf"
    if [[ -f $native_application ]]; then
        WINDVALE_MODEL_CHAT_APPLICATION=$native_application
        export WINDVALE_MODEL_CHAT_APPLICATION
    fi
fi
exec node "$script_directory/Windvale-Model-Chat.mjs" "$@"
