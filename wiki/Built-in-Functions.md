# Built-in Functions

Complete reference for all built-in functions in Applesoft BASIC.

## Table of Contents

- [Mathematical Functions](#mathematical-functions)
- [String Functions](#string-functions)
- [Utility Functions](#utility-functions)
- [User-Defined Functions](#user-defined-functions)

## Mathematical Functions

### ABS - Absolute Value

Returns the absolute value of a number.

**Syntax:**
```basic
ABS(expression)
```

**Examples:**
```basic
10 PRINT ABS(5)        : REM 5
20 PRINT ABS(-5)       : REM 5
30 PRINT ABS(-3.14)    : REM 3.14
40 X = -10
50 PRINT ABS(X)        : REM 10
```

**Returns:** Non-negative value

---

### ATN - Arctangent

Returns the arctangent (inverse tangent) in radians.

**Syntax:**
```basic
ATN(expression)
```

**Example:**
```basic
10 PRINT ATN(1)         : REM 0.785398163 (π/4)
20 PRINT ATN(0)         : REM 0
30 REM CONVERT TO DEGREES
40 DEG = ATN(1) * 180 / 3.14159
50 PRINT DEG            : REM 45
```

**Returns:** Value in radians between -π/2 and π/2

**Note:** To get arctangent of Y/X accounting for quadrant, use:
```basic
10 DEF FN ATAN2(Y,X) = ATN(Y/X) + (X<0) * 3.14159
```

---

### COS - Cosine

Returns the cosine of an angle in radians.

**Syntax:**
```basic
COS(expression)
```

**Examples:**
```basic
10 PRINT COS(0)              : REM 1
20 PRINT COS(3.14159)        : REM -1 (π)
30 PRINT COS(3.14159/2)      : REM 0 (π/2)
40 REM DEGREES TO RADIANS
50 DEG = 90
60 RAD = DEG * 3.14159 / 180
70 PRINT COS(RAD)            : REM 0
```

**Returns:** Value between -1 and 1

---

### EXP - Exponential

Returns e raised to the given power.

**Syntax:**
```basic
EXP(expression)
```

**Examples:**
```basic
10 PRINT EXP(0)        : REM 1
20 PRINT EXP(1)        : REM 2.718281828... (e)
30 PRINT EXP(2)        : REM 7.389...
```

**Returns:** e^x where e ≈ 2.71828

**Note:** Inverse of LOG (natural logarithm)

---

### INT - Integer Part

Returns the integer part of a number (rounds towards negative infinity).

**Syntax:**
```basic
INT(expression)
```

**Examples:**
```basic
10 PRINT INT(3.7)      : REM 3
20 PRINT INT(3.2)      : REM 3
30 PRINT INT(-3.7)     : REM -4 (not -3!)
40 PRINT INT(-3.2)     : REM -4
```

**Returns:** Integer value (floor function)

**Important:** For negative numbers, INT rounds down (towards negative infinity):
- `INT(-3.2)` = -4 (not -3)

---

### LOG - Natural Logarithm

Returns the natural logarithm (base e).

**Syntax:**
```basic
LOG(expression)
```

**Examples:**
```basic
10 PRINT LOG(1)        : REM 0
20 PRINT LOG(2.71828)  : REM 1 (approximately)
30 PRINT LOG(10)       : REM 2.302585...
```

**Returns:** Natural logarithm of x

**Note:** For logarithm base 10:
```basic
10 DEF FN LOG10(X) = LOG(X) / LOG(10)
20 PRINT FN LOG10(100)    : REM 2
```

---

### RND - Random Number

Returns a random number between 0 and 1.

**Syntax:**
```basic
RND(expression)
```

**Examples:**
```basic
10 REM RANDOM NUMBER 0 TO 1
20 PRINT RND(1)

30 REM RANDOM INTEGER 1 TO 100
40 N = INT(RND(1) * 100) + 1
50 PRINT N

60 REM RANDOM INTEGER 1 TO 6 (DICE)
70 DICE = INT(RND(1) * 6) + 1
80 PRINT DICE
```

**Parameter:**
- `RND(1)` or positive: Returns random number
- `RND(0)`: Returns last random number again
- `RND(negative)`: Seeds generator with value

**Returns:** Random value 0 ≤ x < 1

**Seeding:**
```basic
10 REM SEED WITH NEGATIVE VALUE
20 X = RND(-1)
30 REM NOW GET RANDOM NUMBERS
40 PRINT RND(1), RND(1), RND(1)
```

---

### SGN - Sign

Returns the sign of a number.

**Syntax:**
```basic
SGN(expression)
```

**Examples:**
```basic
10 PRINT SGN(10)       : REM 1
20 PRINT SGN(-10)      : REM -1
30 PRINT SGN(0)        : REM 0
```

**Returns:**
- `1` if positive
- `-1` if negative
- `0` if zero

---

### SIN - Sine

Returns the sine of an angle in radians.

**Syntax:**
```basic
SIN(expression)
```

**Examples:**
```basic
10 PRINT SIN(0)              : REM 0
20 PRINT SIN(3.14159/2)      : REM 1 (π/2)
30 PRINT SIN(3.14159)        : REM 0 (π)
40 REM CONVERT DEGREES TO RADIANS
50 DEG = 30
60 RAD = DEG * 3.14159 / 180
70 PRINT SIN(RAD)            : REM 0.5
```

**Returns:** Value between -1 and 1

---

### SQR - Square Root

Returns the square root of a number.

**Syntax:**
```basic
SQR(expression)
```

**Examples:**
```basic
10 PRINT SQR(4)        : REM 2
20 PRINT SQR(9)        : REM 3
30 PRINT SQR(2)        : REM 1.414213...
40 PRINT SQR(0)        : REM 0
```

**Returns:** Non-negative square root

**Note:** Argument must be non-negative (≥ 0)

---

### TAN - Tangent

Returns the tangent of an angle in radians.

**Syntax:**
```basic
TAN(expression)
```

**Examples:**
```basic
10 PRINT TAN(0)              : REM 0
20 PRINT TAN(3.14159/4)      : REM 1 (π/4 = 45°)
30 REM DEGREES TO RADIANS
40 DEG = 45
50 RAD = DEG * 3.14159 / 180
60 PRINT TAN(RAD)            : REM 1
```

**Returns:** Tangent value

**Note:** Undefined at π/2, 3π/2, etc. (±90°, ±270°, ...)

---

## String Functions

### LEN - String Length

Returns the number of characters in a string.

**Syntax:**
```basic
LEN(string_expression)
```

**Examples:**
```basic
10 PRINT LEN("HELLO")      : REM 5
20 A$ = "APPLESOFT"
30 PRINT LEN(A$)           : REM 9
40 PRINT LEN("")           : REM 0
```

**Returns:** Integer length (0 or more)

---

### VAL - Convert String to Number

Converts a string to a numeric value.

**Syntax:**
```basic
VAL(string_expression)
```

**Examples:**
```basic
10 PRINT VAL("123")        : REM 123
20 PRINT VAL("3.14")       : REM 3.14
30 PRINT VAL("-42")        : REM -42
40 PRINT VAL("12ABC")      : REM 12
50 PRINT VAL("ABC")        : REM 0
```

**Returns:** Numeric value

**Note:** Converts until it encounters a non-numeric character

---

### ASC - ASCII Code

Returns the ASCII code of the first character in a string.

**Syntax:**
```basic
ASC(string_expression)
```

**Examples:**
```basic
10 PRINT ASC("A")          : REM 65
20 PRINT ASC("a")          : REM 97
30 PRINT ASC("0")          : REM 48
40 PRINT ASC(" ")          : REM 32
50 A$ = "HELLO"
60 PRINT ASC(A$)           : REM 72 (H)
```

**Returns:** ASCII value (0-255)

**Note:** Only examines first character

---

### MID$ - Substring (Middle)

Returns a substring from the middle of a string.

**Syntax:**
```basic
MID$(string_expression, start)
MID$(string_expression, start, length)
```

**Examples:**
```basic
10 A$ = "APPLESOFT"
20 PRINT MID$(A$, 1, 5)    : REM "APPLE"
30 PRINT MID$(A$, 6)       : REM "SOFT"
40 PRINT MID$(A$, 1, 1)    : REM "A"
50 PRINT MID$(A$, 6, 4)    : REM "SOFT"
```

**Parameters:**
- `start`: Starting position (1-based)
- `length`: Number of characters (optional, defaults to rest of string)

**Returns:** Substring

**Note:** Positions are 1-based (first character is position 1)

---

### LEFT$ - Left Substring

Returns the leftmost characters of a string.

**Syntax:**
```basic
LEFT$(string_expression, length)
```

**Examples:**
```basic
10 A$ = "APPLESOFT"
20 PRINT LEFT$(A$, 5)      : REM "APPLE"
30 PRINT LEFT$(A$, 1)      : REM "A"
40 PRINT LEFT$(A$, 9)      : REM "APPLESOFT"
50 PRINT LEFT$(A$, 0)      : REM ""
```

**Returns:** Leftmost n characters

---

### RIGHT$ - Right Substring

Returns the rightmost characters of a string.

**Syntax:**
```basic
RIGHT$(string_expression, length)
```

**Examples:**
```basic
10 A$ = "APPLESOFT"
20 PRINT RIGHT$(A$, 4)     : REM "SOFT"
30 PRINT RIGHT$(A$, 1)     : REM "T"
40 PRINT RIGHT$(A$, 9)     : REM "APPLESOFT"
50 PRINT RIGHT$(A$, 0)     : REM ""
```

**Returns:** Rightmost n characters

---

### STR$ - Convert Number to String

Converts a number to a string.

**Syntax:**
```basic
STR$(numeric_expression)
```

**Examples:**
```basic
10 N = 123
20 N$ = STR$(N)
30 PRINT N$                : REM " 123" (note leading space)
40 PRINT LEN(N$)           : REM 4
50 PRINT STR$(-456)        : REM "-456"
```

**Returns:** String representation

**Note:** Positive numbers have a leading space (for the sign)

---

### CHR$ - ASCII to Character

Returns the character for an ASCII code.

**Syntax:**
```basic
CHR$(numeric_expression)
```

**Examples:**
```basic
10 PRINT CHR$(65)          : REM "A"
20 PRINT CHR$(72)          : REM "H"
30 PRINT CHR$(32)          : REM " " (space)
40 PRINT CHR$(13)          : REM (carriage return)
50 REM BUILD A STRING
60 A$ = CHR$(72) + CHR$(73)
70 PRINT A$                : REM "HI"
```

**Returns:** Single character string

**Common Codes:**
- 7: Bell/beep
- 13: Carriage return
- 32-126: Printable ASCII characters

---

## Utility Functions

### PEEK - Read Memory Byte

See [Language Reference - Memory/System](Language-Reference.md#peek---read-memory)

**Syntax:**
```basic
PEEK(address)
```

**Example:**
```basic
10 X = PEEK(768)
20 PRINT "VALUE:"; X
```

---

### FRE - Free Memory

Returns the amount of free memory available.

**Syntax:**
```basic
FRE(dummy)
```

**Example:**
```basic
10 PRINT FRE(0)
20 PRINT "FREE MEMORY:"; FRE(0); "BYTES"
```

**Note:** Parameter is ignored but required

---

### POS - Cursor Position

Returns the current horizontal cursor position (0-39).

**Syntax:**
```basic
POS(dummy)
```

**Example:**
```basic
10 PRINT "HELLO";
20 PRINT "CURSOR AT:"; POS(0)
```

**Returns:** Column position (0-39)

---

### SCRN - Read Screen Character

Returns the color or character at screen coordinates.

**Syntax:**
```basic
SCRN(x, y)
```

**Example:**
```basic
10 HOME
20 PRINT "X"
30 C = SCRN(0, 0)
40 PRINT "COLOR:"; C
```

**Note:** In text mode, returns character code

---

### PDL - Paddle/Joystick

Reads paddle/joystick position.

**Syntax:**
```basic
PDL(paddle_number)
```

**Example:**
```basic
10 X = PDL(0)
20 PRINT "PADDLE:"; X
```

**Returns:** Value 0-255

**Note:** Returns 0 in emulated environment

---

### TAB - Tab Function

Used in PRINT to move to a specific column.

**Syntax:**
```basic
PRINT TAB(column)
```

**Example:**
```basic
10 PRINT TAB(10); "INDENTED"
20 PRINT TAB(5); "A"; TAB(10); "B"; TAB(15); "C"
```

**Output:**
```
          INDENTED
     A    B    C
```

---

### SPC - Space Function

Outputs specified number of spaces.

**Syntax:**
```basic
PRINT SPC(count)
```

**Example:**
```basic
10 PRINT "A"; SPC(5); "B"
20 PRINT "X"; SPC(10); "Y"
```

**Output:**
```
A     B
X          Y
```

---

## User-Defined Functions

You can create your own functions with `DEF FN`.

### Syntax

```basic
DEF FN name(parameter) = expression
```

### Examples

#### Single Parameter Function

```basic
10 DEF FN SQUARE(X) = X * X
20 PRINT FN SQUARE(5)        : REM 25
30 PRINT FN SQUARE(10)       : REM 100
```

#### Multiple Parameters

```basic
10 DEF FN MAX(A,B) = (A > B) * A + (A <= B) * B
20 PRINT FN MAX(10, 20)      : REM 20
30 PRINT FN MAX(50, 30)      : REM 50
```

#### Using Built-in Functions

```basic
10 DEF FN DISTANCE(X1,Y1,X2,Y2) = SQR((X2-X1)^2 + (Y2-Y1)^2)
20 D = FN DISTANCE(0, 0, 3, 4)
30 PRINT D                   : REM 5
```

#### String Functions

```basic
10 DEF FN FIRST$(S$) = LEFT$(S$, 1)
20 DEF FN LAST$(S$) = RIGHT$(S$, 1)
30 A$ = "HELLO"
40 PRINT FN FIRST$(A$)       : REM "H"
50 PRINT FN LAST$(A$)        : REM "O"
```

### Common User-Defined Functions

#### Min/Max

```basic
10 DEF FN MIN(A,B) = (A < B) * A + (A >= B) * B
20 DEF FN MAX(A,B) = (A > B) * A + (A <= B) * B
```

#### Rounding

```basic
10 DEF FN ROUND(X) = INT(X + 0.5)
20 PRINT FN ROUND(3.4)       : REM 3
30 PRINT FN ROUND(3.6)       : REM 4
```

#### Clamping

```basic
10 DEF FN CLAMP(X,MIN,MAX) = FN MIN(FN MAX(X,MIN),MAX)
```

#### Degrees/Radians

```basic
10 DEF FN RAD(D) = D * 3.14159 / 180
20 DEF FN DEG(R) = R * 180 / 3.14159
30 PRINT FN RAD(90)          : REM 1.5708 (π/2)
40 PRINT FN DEG(3.14159)     : REM 180
```

---

## Function Summary Table

### Mathematical Functions

| Function | Purpose | Example | Result |
|----------|---------|---------|--------|
| `ABS(x)` | Absolute value | `ABS(-5)` | `5` |
| `ATN(x)` | Arctangent (radians) | `ATN(1)` | `0.785398` |
| `COS(x)` | Cosine (radians) | `COS(0)` | `1` |
| `EXP(x)` | e to the power x | `EXP(1)` | `2.71828` |
| `INT(x)` | Integer part (floor) | `INT(3.7)` | `3` |
| `LOG(x)` | Natural logarithm | `LOG(2.71828)` | `1` |
| `RND(x)` | Random number 0-1 | `RND(1)` | `0.xxxxx` |
| `SGN(x)` | Sign (-1, 0, or 1) | `SGN(-5)` | `-1` |
| `SIN(x)` | Sine (radians) | `SIN(0)` | `0` |
| `SQR(x)` | Square root | `SQR(9)` | `3` |
| `TAN(x)` | Tangent (radians) | `TAN(0)` | `0` |

### String Functions

| Function | Purpose | Example | Result |
|----------|---------|---------|--------|
| `LEN(s$)` | String length | `LEN("HI")` | `2` |
| `VAL(s$)` | String to number | `VAL("123")` | `123` |
| `ASC(s$)` | First char to ASCII | `ASC("A")` | `65` |
| `MID$(s$,p,n)` | Substring | `MID$("HELLO",2,3)` | `"ELL"` |
| `LEFT$(s$,n)` | Left n characters | `LEFT$("HELLO",2)` | `"HE"` |
| `RIGHT$(s$,n)` | Right n characters | `RIGHT$("HELLO",2)` | `"LO"` |
| `STR$(n)` | Number to string | `STR$(123)` | `" 123"` |
| `CHR$(n)` | ASCII to character | `CHR$(65)` | `"A"` |

### Utility Functions

| Function | Purpose | Example |
|----------|---------|---------|
| `PEEK(a)` | Read memory byte | `PEEK(768)` |
| `FRE(0)` | Free memory | `FRE(0)` |
| `POS(0)` | Cursor column | `POS(0)` |
| `SCRN(x,y)` | Read screen | `SCRN(0,0)` |
| `PDL(n)` | Paddle position | `PDL(0)` |
| `TAB(n)` | Tab to column | `TAB(10)` |
| `SPC(n)` | Output n spaces | `SPC(5)` |

---

## Next Topics

- **[Language Reference](Language-Reference)** - Commands and syntax
- **[Custom Extensions](Custom-Extensions)** - SLEEP and other additions
- **[Sample Programs](Sample-Programs)** - See functions in action
