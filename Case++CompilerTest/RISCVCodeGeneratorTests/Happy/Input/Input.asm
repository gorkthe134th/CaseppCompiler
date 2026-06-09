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
li a7, 5
ecall            # wait for input
mv t0, sp
addi t0, t0, 12  # load the address of x (sp + 12)
sw a0, 0(t0)     # set x to the provided value
p_1:
li a0, 0
li a7, 93
ecall            # halt
_exit:
lw a0, 0(sp)
li a7, 93
ecall            # final halt