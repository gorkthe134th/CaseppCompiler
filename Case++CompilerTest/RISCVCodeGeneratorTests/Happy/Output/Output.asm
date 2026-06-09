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
li a0, 9         # load 9
li a7, 1
ecall            # output
la a0, _new_line  # load new line character
li a7, 4
ecall            # change line
p_1:
li a0, 0
li a7, 93
ecall            # halt
_exit:
lw a0, 0(sp)
li a7, 93
ecall            # final halt