#!/usr/bin/env sh
set -eu

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
exec node "$script_directory/Windvale-Model-Chat.mjs" "$@"
