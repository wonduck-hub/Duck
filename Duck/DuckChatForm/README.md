# DuckChatForm

DuckChatForm uses the native `DuckChatCommon.dll` through P/Invoke. Build with
Visual Studio MSBuild and the C++ toolset required by
`DuckChatServer\DuckChatCommon.vcxproj`. The build creates the native DLL for the
selected platform and copies it to the application output and publish folders.
Supported platforms are x86, x64, and ARM64; AnyCPU defaults to x64. Specify a
matching platform when supplying a runtime identifier.

The form connects to `localhost:27015` after login and sends UTF-8 messages of
up to 1024 bytes. The current server prints received packets but does not
broadcast them to clients, so the form displays only messages it sends. The
existing DuckChat client converts characters to single bytes rather than UTF-8;
non-ASCII text is not interoperable between the two encodings.
