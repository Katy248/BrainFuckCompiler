# Brainfuck to Il

Program start:

```il
.locals init ([0] int32 index, [1] int32[] array) // allocate index, should be zero by default
ldc.i4.s 3000 // load array size to stack
newarr int32 // allocate array
stlock.1 // move array to its variable
```

Increment `+`:

```il
ldloc.1 // allocate array
ldloc.0 // load index variable to stack
ldloc.1 // allocate array
ldloc.0 // load index variable to stack
ldelema // array[index] pointer to stack
ldc.i4.1 // load 1 to stack
add // increment
stelem.i4 // move stack value to array[index]
```

Decrement `-`:

```il
ldloc.1 // allocate array
ldloc.0 // load index variable to stack
ldloc.1 // allocate array
ldloc.0 // load index variable to stack
ldelema // array[index] pointer to stack
ldc.i4.1 // load 1 to stack
sub // decrement
stelem.i4 // move stack value to array[index]
```

Next `>`:

```il
ldloc.0 // load index variable to stack
ldc.i4.1 // load 1 to stack
add // increment
stloc.0 // load stack value to index variable
```

Previous `<`:

```il
ldloc.0 // load index variable to stack
ldc.i4.1 // load 1 to stack
sub // decrement
stloc.0 // load stack value to index variable
```

Input `,`:

```il
ldloc.1 // allocate array
ldloc.0 // load index variable to stack
call int32 [System.Console]System.Console::Read() // read from console
stelem.i4 // move stack value to array[index]
```

Output `.`:

```il
ldloc.1 // allocate array
ldloc.0 // load index variable to stack
ldelema // array[index] pointer to stack
call void [System.Console]System.Console::Write(int32) // write to console
```

Start loop `[`:

```il
START_LOOP_$X:
ldloc.1 // allocate array
ldloc.0 // load index variable to stack
ldelema // array[index] pointer to stack
ldc.i4.0 // 0 to stack
beq.s END_LOOP_$X //if current value is 0 end loop
```

Where `$X` is index of loop in all program

End loop `]`:

```il
br.s START_LOOP_$X
END_LOOP_$X:
```
