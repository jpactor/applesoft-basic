# emudbg — Emulator Debug Console

`emudbg` is the primary interactive (and scriptable) debug console for the BackPocketBASIC emulator. It provides a REPL for inspecting and controlling a live emulated 65C02 / Apple II machine, including:

- CPU registers, memory, disassembly
- Stepping, running, breakpoints, watchpoints, tracing
- Disk image management (insert/eject, create, inspect)
- Video window control and soft switches
- Profile switching
- Many other diagnostic commands

It is designed to be useful for both human developers and AI agents / automation.

## Building and Launching

From the repository root:

```bash
# Build
dotnet build BackPocketBasic.slnx

# Run (interactive by default)
dotnet run --project src/BadMango.Emulator.Debug

# Or directly after build
src/BadMango.Emulator.Debug/bin/Debug/net10.0/emudbg.exe
```

On startup it loads a default profile (usually `pocket2e-a2-enh`) and presents a prompt.

## Command-Line Options

```text
emudbg [options]
```

| Option              | Description |
|---------------------|-------------|
| `-p, --profile <name>` | Load the named machine profile at startup (e.g. `pocket2e-lite`, `simple-65c02`) |
| `-e, --exec <commands>` | Execute semicolon- or newline-separated commands then exit (non-interactive) |
| `-f, --file <path>` | Read and run commands from a text file (one per line) |
| `--json`, `-j`      | Enable structured JSON output for supported commands (e.g. `regs`, `dasm`) |
| `--no-banner`       | Suppress the startup banner and input prompts |
| `-h, --help`        | Show usage and exit |

### Examples

```powershell
# Show help
emudbg --help

# Start with a minimal profile
emudbg -p simple-65c02

# Run a short non-interactive session
emudbg --exec "boot;regs;step 20;regs;exit" --no-banner

# Get structured JSON output (for agents/tools)
emudbg --json --exec "regs;exit" --no-banner

# Use a script file
emudbg --file debug-sequence.emudbg --no-banner
```

When using `--exec` or `--file`, `emudbg` will process the supplied commands using the normal command infrastructure and then exit.

Supported commands (regs, dasm, etc.) accept `--json` / `-j` to emit machine-readable output instead of formatted text. Global `--json` enables it for the session.

## Interactive Usage

Type commands at the `> ` prompt. Use `help` or `help <command>` for information.

Common workflow:

```
> boot
> regs
> step 5
> dasm
> bp add $c030
> run
```

Press Ctrl+C or type `exit` (or `quit`) to leave.

## Key Commands

See `help` inside the console for the full live list. Highlights:

- `regs`, `pc`, `mem <addr> [len]`, `dasm [addr] [count]`
- `step [n]`, `run`, `halt`, `pause`, `resume`, `reset`, `boot`
- `bp ...` / `watch ...` (breakpoints and watchpoints)
- `trace ...` (instruction tracing)
- `profile list|load <name>`
- `disk list|info|insert|create|...`
- `video open|close|scale ...`
- `switches`, `regions`, `pages`, `fault`

Many commands accept hex addresses with `$` or `0x` prefixes.

## Profiles

Profiles describe complete machine configurations (CPU type, ROMs, devices, memory map). They live in the `profiles/` directory (copied next to the executable at build time).

Use `--profile` on the command line or the `profile load` command at runtime.

## For Agents and Scripting

See `AGENTS.md` for recommended patterns when driving `emudbg` from AI tools:

- Prefer `--exec`, `--file`, and `--no-banner` for non-interactive runs.
- Combine with piping to stdin when needed.
- Always end sequences with `exit` (or let input EOF terminate).

Example agent-friendly invocation:

```powershell
$cmds = "profile list`nboot`nregs`nstep 50`nregs`nexit"
$cmds | & "src\BadMango.Emulator.Debug\bin\Debug\net10.0\emudbg.exe" --no-banner
```

## Related Documentation

- `AGENTS.md` — Guidance for AI agents working with the codebase and `emudbg`
- `emudbg-agent-plan.md` / `PRD-emudbg-agent.md` — Plans for structured agent mode / MCP
- `profiles/` + `schemas/machine-profile.schema.json`
- In-console `help` and `help <command>`

## Logging

`emudbg` writes detailed logs to `logs/emudbg-<date>.log` (Serilog).

---

This tool is the main lever for diagnosing and improving the emulator. Happy debugging!