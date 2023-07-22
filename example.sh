#!/bin/bash
set -eo pipefail

script_title="Example Script"
script_version=1.0
script_opts_args=(-o '-a,--abc;No-value option'
    -o '-m,--multi;Multiple values option;o:m'
    -o '-n,--num;Number option;o:s;t:n'
    -a 'file;File that must exist;f:f'
    -a 'dir;Directory that must not exist;f:!d'
    -a 'cbe;Required but can be empty;e:1'
    -a 'xyz;Optional number with default value;r:0;t:n;d:123.456'
    -b 'Aditional help text here

Created By — Source URL — License')

# --- GotOpts ---
script_provided_args="$(printf %s "${@/#/|}")"
if [ ${#script_provided_args} -gt 0 ]; then script_provided_args="[${script_provided_args:1}]"; else script_provided_args=""; fi
script_gotopts="$(./src/gotopts/bin/Debug/net7.0/gotopts "$(basename "$0")" "$script_title" "$script_provided_args" -v "$script_version" "${script_opts_args[@]}")" || (echo "$script_gotopts" && exit 1)
eval $(echo $script_gotopts)
shift $(($# - 1))
# --- End of GotOps ---

echo "GotOpts return (variables now available in script):"
echo "$script_gotopts"
