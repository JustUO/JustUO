#!/bin/bash

until mono ./Server.exe; do
	echo "Server exited with code $?.  Respawning in 5s..." >&2
	sleep 5
done
