#!/usr/bin/env sh
set -eu

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
if [[ ${1:-} == chat && -z ${WINDVALE_MODEL_CHAT_APPLICATION:-} ]]; then
    repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
    WINDVALE_MODEL_CHAT_APPLICATION="$repository_root/Artifacts/Applications/Model-Chat/Windvale-Model-Chat.elf"
    export WINDVALE_MODEL_CHAT_APPLICATION
    if [[ ! -f $WINDVALE_MODEL_CHAT_APPLICATION ]]; then
        "$repository_root/Tools/Native/Build-Windvale-Model-Chat.sh" \
            "$WINDVALE_MODEL_CHAT_APPLICATION"
    fi
fi
exec node "$script_directory/Windvale-Model-Chat.mjs" "$@"
