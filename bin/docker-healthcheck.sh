#!/bin/sh

curl --head --fail --silent --connect-timeout 3 --output /dev/null "http://127.0.0.1:${SERVER_PORT}" || exit 1
