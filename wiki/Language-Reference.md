# Language Reference

Complete reference for Applesoft BASIC commands and syntax.

## Table of Contents

- [Program Control](#program-control)
- [Flow Control](#flow-control)
- [Input/Output](#inputoutput)
- [Memory/System](#memorysystem)
- [Text Display](#text-display)
- [Graphics](#graphics)
- [Variables and Data Types](#variables-and-data-types)
- [Arrays](#arrays)
- [Operators](#operators)

## Program Control

### REM - Remark (Comment)

Adds a comment to your program. The interpreter ignores everything after REM on that line.

**Syntax:**
```basic
REM comment text
```

**Example:**
```basic
10 REM THIS IS A COMMENT
20 REM PROGRAM TO CALCULATE FACTORIAL
30 PRINT "STARTING..."
```

**Notes:**
- Comments do not affect program execution
- Use comments to document your code

---

### LET - Variable Assignment

Assigns a value to a variable. The `LET` keyword is optional in Applesoft BASIC.

**Syntax:**
```basic
LET variable = expression
variable = expression
```

**Examples:**
```basic
10 LET X = 42
20 Y = 100
30 Z$ = "HELLO"
40 A% = 50
```

**Notes:**
- `LET` keyword is optional
- Variable type must match value type

---

### DIM - Dimension Arrays

Declares an array and allocates memory for it.

**Syntax:**
```basic
DIM array(max_index)
DIM array(max_index1, max_index2)
```

**Examples:**
```basic
10 DIM A(10)           : REM 11 ELEMENTS (0-10)
20 DIM B(5,5)          : REM 6x6 2D ARRAY
30 DIM NAMES$(100)     : REM STRING ARRAY
```

**Notes:**
- Arrays are 0-based (start at index 0)
- `DIM A(10)` creates elements 0 through 10 (11 total)
- Maximum of 2 dimensions
- Must declare arrays before using them

---

### DEF FN - Define Function

Defines a user function that can be called with `FN`.

**Syntax:**
```basic
DEF FN name(parameter) = expression
```

**Examples:**
```basic
10 DEF FN SQUARE(X) = X * X
20 DEF FN DOUBLE(N) = N * 2
30 DEF FN AVG(A,B) = (A + B) / 2
40 PRINT FN SQUARE(5)
50 PRINT FN AVG(10, 20)
```

**Notes:**
- Function names start with `FN`
- Can have multiple parameters
- Returns single value
- Function body is a single expression

---

### END - End Program

Terminates program execution.

**Syntax:**
```basic
END
```

**Example:**
```basic
10 PRINT "HELLO"
20 END
30 PRINT "THIS NEVER EXECUTES"
```

**Notes:**
- Optional but good practice
- Stops execution immediately
- Any lines after END are not executed

---

### STOP - Stop Execution

Stops program execution and displays "BREAK IN LINE n".

**Syntax:**
```basic
STOP
```

**Example:**
```basic
10 FOR I = 1 TO 100
20 IF I = 50 THEN STOP
30 PRINT I
40 NEXT I
```

**Notes:**
- Similar to END but indicates intentional breakpoint
- Useful for debugging

---

### CLEAR - Clear Variables

Clears all variables and resets the program state.

**Syntax:**
```basic
CLEAR
```

**Example:**
```basic
10 X = 100
20 PRINT X
30 CLEAR
40 PRINT X     : REM PRINTS 0
```

**Notes:**
- Resets all variables to 0 or empty string
- Clears arrays
- Does not affect program code

---

## Flow Control

### GOTO - Jump to Line

Jumps program execution to the specified line number.

**Syntax:**
```basic
GOTO line_number
```

**Example:**
```basic
10 PRINT "START"
20 GOTO 40
30 PRINT "SKIPPED"
40 PRINT "END"
```

**Output:**
```
START
END
```

---

### GOSUB / RETURN - Subroutine Call

`GOSUB` calls a subroutine. `RETURN` returns to the line after the `GOSUB`.

**Syntax:**
```basic
GOSUB line_number
RETURN
```

**Example:**
```basic
10 GOSUB 100
20 PRINT "BACK IN MAIN"
30 END
100 REM SUBROUTINE
110 PRINT "IN SUBROUTINE"
120 RETURN
```

**Output:**
```
IN SUBROUTINE
BACK IN MAIN
```

**Notes:**
- Can nest subroutines
- Must have matching RETURN

---

### ON...GOTO - Computed GOTO

Jumps to one of several line numbers based on a value.

**Syntax:**
```basic
ON expression GOTO line1, line2, line3, ...
```

**Example:**
```basic
10 INPUT "ENTER 1, 2, OR 3: "; N
20 ON N GOTO 100, 200, 300
100 PRINT "YOU CHOSE 1": END
200 PRINT "YOU CHOSE 2": END
300 PRINT "YOU CHOSE 3": END
```

**Notes:**
- If value is 1, goes to first line
- If value is 2, goes to second line, etc.
- If value is out of range, continues to next line

---

### ON...GOSUB - Computed GOSUB

Calls one of several subroutines based on a value.

**Syntax:**
```basic
ON expression GOSUB line1, line2, line3, ...
```

**Example:**
```basic
10 INPUT "CHOICE: "; C
20 ON C GOSUB 100, 200, 300
30 END
100 PRINT "SUBROUTINE 1": RETURN
200 PRINT "SUBROUTINE 2": RETURN
300 PRINT "SUBROUTINE 3": RETURN
```

---

### IF...THEN - Conditional

Executes statement(s) if condition is true.

**Syntax:**
```basic
IF condition THEN statement
IF condition THEN line_number
```

**Examples:**
```basic
10 IF X > 10 THEN PRINT "BIG"
20 IF Y = 0 THEN GOTO 100
30 IF A$ = "YES" THEN GOSUB 200
40 IF N < 0 THEN N = 0: PRINT "NEGATIVE"
```

**Notes:**
- Can chain statements with `:`
- Can jump to line number
- No ELSE in Applesoft BASIC (use separate IF for else case)

---

### FOR...TO...STEP...NEXT - Loop

Repeats a block of code with a counter variable.

**Syntax:**
```basic
FOR variable = start TO end [STEP increment]
  statements
NEXT variable
```

**Examples:**
```basic
10 REM COUNT 1 TO 10
20 FOR I = 1 TO 10
30 PRINT I
40 NEXT I

50 REM COUNT BY 2'S
60 FOR I = 0 TO 20 STEP 2
70 PRINT I
80 NEXT I

90 REM COUNT BACKWARDS
100 FOR I = 10 TO 1 STEP -1
110 PRINT I
120 NEXT I
```

**Notes:**
- STEP is optional (defaults to 1)
- Can use negative STEP to count down
- Variable in NEXT is optional but recommended

---

## Input/Output

### PRINT - Print Output

Outputs text and values to the console.

**Syntax:**
```basic
PRINT [expression] [;|,] [expression] ...
```

**Examples:**
```basic
10 PRINT "HELLO"
20 PRINT X
30 PRINT "X ="; X
40 PRINT X, Y, Z        : REM TAB SEPARATED
50 PRINT A; B; C        : REM NO SPACE
60 PRINT                : REM BLANK LINE
```

**Notes:**
- `;` separates with no space
- `,` separates with tab spacing
- Trailing `;` suppresses newline

---

### INPUT - Get Input

Reads input from the user.

**Syntax:**
```basic
INPUT [prompt;] variable
INPUT variable1, variable2, ...
```

**Examples:**
```basic
10 INPUT "NAME: "; N$
20 INPUT "AGE: "; A
30 INPUT X, Y, Z
```

**Notes:**
- Prompt is optional
- Use `;` after prompt (`,` adds `?`)
- Can input multiple values separated by commas

---

### GET - Get Single Character

Reads a single character without waiting for Enter.

**Syntax:**
```basic
GET variable$
```

**Example:**
```basic
10 PRINT "PRESS ANY KEY"
20 GET K$
30 PRINT "YOU PRESSED: "; K$
```

**Notes:**
- Waits for a key press
- Returns single character in string variable
- Does not echo to screen

---

### DATA - Define Data

Defines data values to be read by READ statements.

**Syntax:**
```basic
DATA value1, value2, value3, ...
```

**Example:**
```basic
10 DATA 10, 20, 30, 40, 50
20 DATA "APPLE", "BANANA", "CHERRY"
```

---

### READ - Read Data

Reads values from DATA statements.

**Syntax:**
```basic
READ variable1, variable2, ...
```

**Example:**
```basic
10 DATA 100, 200, 300
20 READ A, B, C
30 PRINT A, B, C
```

**Output:**
```
100    200    300
```

---

### RESTORE - Reset Data Pointer

Resets the DATA pointer to the beginning or a specific line.

**Syntax:**
```basic
RESTORE [line_number]
```

**Example:**
```basic
10 DATA 1, 2, 3
20 READ A: PRINT A
30 READ B: PRINT B
40 RESTORE
50 READ C: PRINT C
```

**Output:**
```
1
2
1
```

---

## Memory/System

### PEEK - Read Memory

Reads a byte from memory address.

**Syntax:**
```basic
PEEK(address)
```

**Example:**
```basic
10 X = PEEK(768)
20 PRINT "VALUE AT 768:"; X
```

**Notes:**
- Returns value 0-255
- Address range: 0-65535 ($0000-$FFFF)
- Reads from emulated memory

---

### POKE - Write Memory

Writes a byte to memory address.

**Syntax:**
```basic
POKE address, value
```

**Example:**
```basic
10 POKE 768, 42
20 PRINT PEEK(768)
```

**Output:**
```
42
```

**Notes:**
- Value must be 0-255
- Address range: 0-65535
- Writes to emulated memory

---

### CALL - Execute Machine Code

Calls a machine code routine at the specified address.

**Syntax:**
```basic
CALL address
```

**Example:**
```basic
10 REM LOAD MACHINE CODE
20 FOR I = 0 TO 10
30 READ B
40 POKE 768 + I, B
50 NEXT I
60 CALL 768
100 DATA 169, 0, 141, 48, 192, 96
```

**Notes:**
- Executes 6502 machine code
- Uses emulated 6502 CPU
- Address typically in page 3 ($300+)

---

### HIMEM: - Set High Memory

Sets the high memory limit.

**Syntax:**
```basic
HIMEM: address
```

**Example:**
```basic
10 HIMEM: 8192
20 PRINT HIMEM:
```

---

### LOMEM: - Set Low Memory

Sets the low memory limit.

**Syntax:**
```basic
LOMEM: address
```

**Example:**
```basic
10 LOMEM: 2048
20 PRINT LOMEM:
```

---

## Text Display

### HOME - Clear Screen

Clears the text screen and moves cursor to top-left.

**Syntax:**
```basic
HOME
```

**Example:**
```basic
10 HOME
20 PRINT "SCREEN CLEARED"
```

---

### HTAB - Horizontal Tab

Moves cursor to column position (1-40).

**Syntax:**
```basic
HTAB column
```

**Example:**
```basic
10 HTAB 10
20 PRINT "INDENTED"
```

---

### VTAB - Vertical Tab

Moves cursor to row position (1-24).

**Syntax:**
```basic
VTAB row
```

**Example:**
```basic
10 VTAB 12
20 PRINT "MIDDLE OF SCREEN"
```

---

### INVERSE - Inverse Video

Subsequent text displays in inverse video (background/foreground swapped).

**Syntax:**
```basic
INVERSE
```

**Example:**
```basic
10 INVERSE
20 PRINT "INVERSE TEXT"
30 NORMAL
40 PRINT "NORMAL TEXT"
```

---

### FLASH - Flashing Text

Subsequent text flashes.

**Syntax:**
```basic
FLASH
```

**Example:**
```basic
10 FLASH
20 PRINT "FLASHING!"
30 NORMAL
```

---

### NORMAL - Normal Text

Returns text display to normal (cancels INVERSE or FLASH).

**Syntax:**
```basic
NORMAL
```

---

## Graphics

**Note:** Graphics commands are stubbed for future UI implementation. They execute without error but do not produce visual output in the console version.

### GR - Low-Resolution Graphics

Switches to low-resolution graphics mode (40×40).

**Syntax:**
```basic
GR
```

---

### HGR - High-Resolution Graphics Page 1

Switches to high-resolution graphics mode page 1 (280×192).

**Syntax:**
```basic
HGR
```

---

### HGR2 - High-Resolution Graphics Page 2

Switches to high-resolution graphics mode page 2.

**Syntax:**
```basic
HGR2
```

---

### TEXT - Text Mode

Returns to text mode.

**Syntax:**
```basic
TEXT
```

---

### COLOR= - Set Color

Sets the low-resolution graphics color (0-15).

**Syntax:**
```basic
COLOR= value
```

---

### HCOLOR= - Set High-Resolution Color

Sets the high-resolution graphics color (0-7).

**Syntax:**
```basic
HCOLOR= value
```

---

### PLOT - Plot Pixel (Low-Res)

Plots a pixel in low-resolution graphics mode.

**Syntax:**
```basic
PLOT x, y
```

---

### HPLOT - Plot Pixel (High-Res)

Plots a pixel in high-resolution graphics mode.

**Syntax:**
```basic
HPLOT x, y
HPLOT x, y TO x2, y2
```

---

### DRAW - Draw Shape

Draws a predefined shape.

**Syntax:**
```basic
DRAW shape_number
```

---

### XDRAW - Exclusive-OR Draw

Draws a shape using XOR operation.

**Syntax:**
```basic
XDRAW shape_number
```

---

## Variables and Data Types

### Variable Types

Applesoft BASIC has three variable types:

#### Numeric Variables

Store floating-point numbers.

```basic
10 X = 3.14159
20 COUNT = 100
30 TEMP = -40.5
```

**Notes:**
- Default type
- Approximately 9 decimal digits of precision
- Range: ±1E-38 to ±1E+38

#### String Variables

Store text. Variable names end with `$`.

```basic
10 NAME$ = "JOHN"
20 CITY$ = "NEW YORK"
30 EMPTY$ = ""
```

**Notes:**
- End with `$`
- Maximum length: 255 characters
- Can be concatenated with `+`

#### Integer Variables

Store integers (-32768 to 32767). Variable names end with `%`.

```basic
10 COUNT% = 100
20 INDEX% = -50
```

**Notes:**
- End with `%`
- Faster than floating-point
- Range: -32768 to 32767

### Variable Names

- Start with a letter
- Can contain letters and digits
- **Only first 2 characters are significant!**
  - `COUNT` and `COUNTER` are the same variable
  - `X1` and `X100` are the same variable

---

## Arrays

### Single-Dimension Arrays

```basic
10 DIM SCORES(10)      : REM 11 ELEMENTS (0-10)
20 SCORES(0) = 95
30 SCORES(5) = 87
40 PRINT SCORES(0), SCORES(5)
```

### Two-Dimension Arrays

```basic
10 DIM BOARD(8,8)      : REM 9x9 ARRAY
20 BOARD(4,4) = 1
30 PRINT BOARD(4,4)
```

### Array Notes

- Must use DIM before accessing arrays
- 0-based indexing
- DIM A(10) creates indices 0-10 (11 elements total)
- Maximum 2 dimensions
- Can have numeric, string, or integer arrays

---

## Operators

### Arithmetic Operators

| Operator | Operation | Example | Result |
|----------|-----------|---------|--------|
| `+` | Addition | `5 + 3` | `8` |
| `-` | Subtraction | `10 - 4` | `6` |
| `*` | Multiplication | `6 * 7` | `42` |
| `/` | Division | `15 / 3` | `5` |
| `^` | Exponentiation | `2 ^ 3` | `8` |

### Comparison Operators

| Operator | Meaning | Example |
|----------|---------|---------|
| `=` | Equal to | `X = 10` |
| `<>` | Not equal to | `X <> 10` |
| `><` | Not equal to (alternate) | `X >< 10` |
| `<` | Less than | `X < 10` |
| `>` | Greater than | `X > 10` |
| `<=` | Less than or equal | `X <= 10` |
| `>=` | Greater than or equal | `X >= 10` |

### Logical Operators

| Operator | Operation | Example |
|----------|-----------|---------|
| `AND` | Logical AND | `X > 0 AND X < 10` |
| `OR` | Logical OR | `X = 1 OR X = 2` |
| `NOT` | Logical NOT | `NOT (X = 0)` |

**Notes:**
- In Applesoft, 0 is false, non-zero is true
- AND, OR, NOT also work as bitwise operators

### String Operator

| Operator | Operation | Example | Result |
|----------|-----------|---------|--------|
| `+` | Concatenation | `"HE" + "LLO"` | `"HELLO"` |

### Operator Precedence

From highest to lowest:

1. `^` (Exponentiation)
2. `*`, `/` (Multiplication, Division)
3. `+`, `-` (Addition, Subtraction)
4. `=`, `<>`, `<`, `>`, `<=`, `>=` (Comparison)
5. `NOT` (Logical NOT)
6. `AND` (Logical AND)
7. `OR` (Logical OR)

Use parentheses to override precedence:

```basic
10 X = 2 + 3 * 4        : REM = 14 (NOT 20)
20 Y = (2 + 3) * 4      : REM = 20
```

---

## Next Topics

- **[Built-in Functions](Built-in-Functions.md)** - Math, string, and utility functions
- **[Custom Extensions](Custom-Extensions.md)** - SLEEP and other modern additions
- **[Sample Programs](Sample-Programs.md)** - Complete example programs
- **[Memory Map](Memory-Map.md)** - Detailed memory layout
