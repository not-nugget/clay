
# How to compile clay.h into clay.dll

I have never compiled a C program into a .dll before, so I am all ears on improvements and suggesstions, as well as ways to make this more platform-agnostic (I only develop on my personal Windows machine)

cp ../../clay.h ./clay.c
clang -shared -o clay.dll clay.c -target x86_64-windows-gnu --define-macro CLAY_IMPLEMENTATION

# Preprocessing a C file

This is very helpful for debugging the binding implementations, and seeing exactly how things are done when the macros are used

clang -E -P ./clay.c -o clay-expanded.c

# TODO Clay C# Bindings Documentation: