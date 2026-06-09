.data
_new_line: .asciz "\n"
.text
j _main          # skip function code (not necessary here)
_main:
la ra, _exit     # set the return address to the final halt, in case the main function returns (not necessary here)
sw sp, 8(sp)     # use the first position in the stack to store the program return value (instead of the parent frame reference)
p:
sw ra, 4(sp)     # save the return address (not necessary here)
p_0:
li t1, 1
li t2, 2
add t1, t1, t2   # addition
mv t0, sp
addi t0, t0, 12
sw t1, 0(t0)     # store the result to _T0 (sp + 12)
p_1:
mv t0, sp
addi t0, t0, 12
lw t1, 0(t0)     # load _T0
li t2, 3
sub t1, t1, t2   # subtraction
mv t0, sp
addi t0, t0, 16
sw t1, 0(t0)     # store the result to _T1 (sp + 16)
p_2:
mv t0, sp
addi t0, t0, 16
lw t1, 0(t0)     # load _T1
li t2, 4
mul t1, t1, t2   # multiplication
mv t0, sp
addi t0, t0, 20
sw t1, 0(t0)     # store the result to _T2 (sp + 20)
p_3:
mv t0, sp
addi t0, t0, 20
lw t1, 0(t0)     # load _T2
li t2, 5
div t1, t1, t2   # division
mv t0, sp
addi t0, t0, 24
sw t1, 0(t0)     # store the result to _T3 (sp + 24)
p_4:
mv t0, sp
addi t0, t0, 24
lw t1, 0(t0)     # load the return value (from _T3)
mv t0, sp
addi t0, t0, 8
lw t0, 0(t0)     # get the address of the return variable (*(sp + 8))
sw t1, 0(t0)     # store the value of _T3 into the return variable
lw ra, 4(sp)     # load the return address
jr ra            # return
p_5:
li a0, 0
li a7, 93
ecall            # halt
_exit:
lw a0, 0(sp)
li a7, 93
ecall            # final halt