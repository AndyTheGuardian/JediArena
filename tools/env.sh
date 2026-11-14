# Entwicklungsumgebung fuer Lazer Arena (alles im Benutzerverzeichnis, kein sudo).
# Verwendung:  source tools/env.sh
export DOTNET_ROOT="$HOME/.dotnet"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export ANDROID_HOME="$HOME/Android/Sdk"
export JAVA_HOME="$(ls -d "$HOME"/Android/jdk/jdk-17*/ 2>/dev/null | head -1)"
export JAVA_HOME="${JAVA_HOME%/}"
export PATH="$DOTNET_ROOT:$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/platform-tools:$JAVA_HOME/bin:$PATH"
