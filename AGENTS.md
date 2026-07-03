# AGENTS.md

Guidance for AI coding agents, LLM tools, and automated assistants working in this repository.

This file focuses on practical workflows, especially **interacting with `emudbg`** (the emulator debug console / REPL). See also `.github/copilot-instructions.md` for broader coding standards, testing requirements, logging rules (Serilog), StyleCop, dependency injection, etc.

## Project Context

- **Solution**: `BackPocketBasic.slnx` (modern .NET solution file). All projects target **net10.0**.
- **Core**: Cycle-accurate 65C02 (and future 65816/65832) emulator for Apple II, plus a full Applesoft BASIC implementation.
- **Primary Debug Tool**: `emudbg` — an interactive REPL for inspecting and controlling a live emulated machine. It is the main lever for debugging the emulator, devices, storage, video, BASIC runtime, etc.
- The long-term vision (documented in `PRD-emudbg-agent.md` and `emudbg-agent-plan.md`) is to make `emudbg` excellent for **both humans and AI agents**.

## Building and Running

```powershell
# From repo root
dotnet build BackPocketBasic.slnx
# or just the debug host
dotnet build src/BadMango.Emulator.Debug/BadMango.Emulator.Debug.csproj
```

The output executable name is `emudbg`:

- Direct: `src\BadMango.Emulator.Debug\bin\Debug\net10.0\emudbg.exe`
- Dev-friendly: `dotnet run --project src/BadMango.Emulator.Debug`

On launch, `emudbg` immediately creates a machine using the default profile (currently `pocket2e-a2-enh` — an Apple IIe Enhanced configuration) and enters the REPL.

## Interacting with the emudbg REPL (Critical for Agents)

### Recommended Interaction Pattern (Piping / Batch Commands)

The REPL is line-oriented: it prints a banner + prompt, reads lines from stdin, dispatches via `CommandDispatcher`, and writes results to `Output` / `Error`.

**True live interactive REPL is difficult for agents.** The proven, reliable pattern is to send a batch of commands via stdin and capture everything:

**PowerShell (this environment):**
```powershell
$cmds = @(
    "profile list"
    "boot"
    "regs"
    "step 20"
    "regs"
    "dasm"
    "mem $300 64"
    "exit"
) -join "`n"

$cmds | & "src\BadMango.Emulator.Debug\bin\Debug\net10.0\emudbg.exe" 2>&1
```

**Unix-style shells:**
```bash
printf "boot\nregs\nstep 5\nregs\nexit\n" | dotnet run --project src/BadMango.Emulator.Debug -- 2>&1
```

**Rules:**
- Always terminate the command list with `exit`.
- Capture both stdout and stderr.
- Send related commands together (mutate → immediately inspect).
- Keep individual sequences focused (one logical debugging task per invocation).

### Core Commands for Debugging the Emulator

Use `help` and `help <command>` liberally inside sequences — they are self-documenting via `ICommandHelp`.

**Essential control & inspection:**
- `boot` (or alias `startup`) — Reset and start the machine (background execution begins). Supports holding modifier keys briefly.
- `reset`, `pause`, `resume`, `halt`, `stop`
- `step [count]` (alias `s`) — Single-step or step N instructions. Preferred for controlled progress.
- `run [limit]` — Run until halt or limit. Use cautiously with agents (consider bounded `step` instead for now).
- `regs` (aliases: `r`, `registers`) — Full CPU register dump (PC, SP, A/X/Y, flags, E/M/X, etc.).
- `pc [value]` — Get or set program counter.
- `mem <addr> [length]` — Classic hex dump.
- `dasm [addr] [count]` (or `--instructions=N`, `--range`) — Disassemble from PC or address.
- `peek`, `poke`, `read`, `write` — Lower-level access with/without side effects.

**Debugging features:**
- `bp ...` (breakpoints) — add/list/remove/clear
- `watch ...` (watchpoints)
- `trace on|off|configure|dump|...` — Instruction tracing (very useful for agents)
- `fault`, `buslog`

**Machine / profile:**
- `profile` — Show current
- `profile list`
- `profile load <name>` — Switch and rebuild machine (e.g. `pocket2e-lite`, `pocket2e`, `simple-65c02`)
- `profile default [name]`

Profiles live in `profiles/*.json`. The default is resolved in `DebugConsoleModule` (respects `.default-profile` file if present).

**Disk & storage (important for real Apple II software):**
- `disk list`, `disk info`, `disk create`, `disk insert <slot:drive> <path>`, `disk eject`, etc.

**Video / GUI:**
- `video open|close|scale N|color ...|fps|refresh`
- Various `*mon` commands open debug windows (`statmon`, `schedmon`, `trapmon`).

**Notes on GUI for agents:**
- Commands are accepted and the `IDebugWindowManager` (Avalonia) is optional.
- In headless or tool-driven sessions, windows often do not render visibly even if the command says "opened".
- Use `video` subcommands for configuration; fall back to `print`, `plot`, `hplot`, memory inspection, and `dasm` for visibility.

### Example Productive Agent Sequence

```powershell
$seq = "profile list`nboot`nregs`nstep 100`nregs`ndasm $fa00 32`ntrace dump`nexit"
$seq | & ...emudbg.exe...
```

Follow every significant state change or run with `regs`, `dasm`, or targeted `mem`.

### Current Limitations (Be Aware)

- All output is human-formatted text (boxes, tables, disassembly listings). Agents must parse it.
- No built-in JSON output mode yet (future enhancement opportunity).
- Long-running execution (`boot` / `run`) happens on the machine thread; the REPL stays responsive for other commands in the human design.
- The current host always starts the REPL (`DebugRepl.Run()`). There is no native "run a script and exit" or "execute one command" CLI mode today.
- Some output may go directly to console streams (Serilog is at Warning+ by default for console).

See the agent planning documents for the intended future direction (structured tools + generic `exec` + MCP stdio host).

## When Enhancing emudbg

### Key Code Locations
- `src/BadMango.Emulator.Debug/Program.cs` — Host setup (Autofac + `DebugConsoleModule` + `DebugUiModule`), starts `DebugRepl`.
- `src/BadMango.Emulator.Debug.Infrastructure/DebugRepl.cs` — REPL loop, `ProcessLine`, banner.
- `src/BadMango.Emulator.Debug.Infrastructure/Commands/CommandDispatcher.cs` — Registration and dispatch.
- `src/BadMango.Emulator.Debug.Infrastructure/Commands/` — Individual handlers (most implement `ICommandHelp`).
- `DebugContext.cs`, `MachineFactory.cs`, `DebugConsoleModule.cs` — Machine creation and wiring.
- UI windows and monitors live under `BadMango.Emulator.Debug.UI`.

### Improvement Ideas (Prioritize Making It Agent-Friendly)
- Add CLI arguments (e.g. `--profile`, `--exec "cmd1;cmd2"`, `--script file.emudbg`, `--json`).
- Support a non-interactive / batch mode that doesn't require the full REPL loop.
- Optional structured output (JSON) for key commands (`regs --json`, `dasm --json`).
- Better output capture abstraction (currently uses `ICommandContext.Output`).
- Separate lightweight agent host project (as planned in `emudbg-agent-plan.md`).
- Headless-friendly defaults and fake audio/video providers.
- Scriptable "run until condition" helpers that are safe for agents.

When you change commands, **always verify** by running representative piped sequences (boot + inspect + step + trace + exit) and confirm no regressions in output.

### General Rules
- Follow all rules in `.github/copilot-instructions.md` (especially Serilog injection, XML docs, no suppressed warnings, tests required).
- New commands should implement `ICommandHelp` for discoverability (`help <cmd>`).
- Prefer extending the existing dispatcher and context rather than bypassing them.
- Update this `AGENTS.md` and the planning docs when behavior or recommended workflows change.

## Related Documentation

- `emudbg-agent-plan.md` — Technical approach for exposing emudbg to LLM agents.
- `PRD-emudbg-agent.md` — Requirements for agent mode.
- `specs/video/Pocket2e Debug Video Window (Avalonia) — Specification.md`
- `profiles/` + `schemas/machine-profile.schema.json`
- Root `README.md`, `SETUP_GUIDE.md`, wiki pages.

## Humans vs. Agents

- **Humans**: Just run `emudbg` in a terminal. Type commands interactively. Use the video window for visual debugging.
- **Agents**: Use the batch/piping pattern above. Focus on reproducible, inspect-after-mutate sequences. Help improve the REPL so the same commands become even more powerful for tooling.

Start by exploring with `help` and small sequences. The existing command surface is rich — leverage it.

Welcome! Let's make `emudbg` the best possible debugger for this emulator, usable by both people and AI.
