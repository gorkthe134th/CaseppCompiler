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
li a0, 0
li a7, 93
ecall            # halt
p_f:             # start of function "f"
sw ra, 4(sp)     # save the return address
p_f_0:
li t1, 9         # load 9
mv t0, sp
addi t0, t0, 16
sw t1, 0(t0)     # set the local variable x (*(sp + 16)) to 9, instead of the parameter x (*(sp + 12))
p_f_1:
li t1, 0
mv t0, sp
addi t0, t0, 8
lw t0, 0(t0)     # get the address of the return variable (*(sp + 8))
sw t1, 0(t0)     # store 0 into the return variable, as a default
lw ra, 4(sp)     # load the return address
jr ra            # return
_exit:
lw a0, 0(sp)
li a7, 93
ecall            # final halt