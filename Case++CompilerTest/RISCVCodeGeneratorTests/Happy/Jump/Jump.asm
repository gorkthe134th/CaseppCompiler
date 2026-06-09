.data
_new_line: .asciz "\n"
.text
j _main         # skip function code (not necessary here)
_main:
la ra, _exit    # set the return address to the final halt, in case the main function returns (not necessary here)
sw sp, 8(sp)    # use the first position in the stack to store the program return value (instead of the parent frame reference)
p:
sw ra, 4(sp)    # save the return address (not necessary here)
p_0:
j p_7           # jump
p_1:
li t1, 1
li t2, 2
beq t1, t2, p_7 # equal
p_2:
li t1, 1
li t2, 2
bgt t1, t2, p_7 # greater than
p_3:
li t1, 1
li t2, 2
blt t1, t2, p_7 # less than
p_4:
li t1, 1
li t2, 2
bne t1, t2, p_7 # not equal
p_5:
li t1, 1
li t2, 2
bge t1, t2, p_7 # greater than or equal to
p_6:
li t1, 1
li t2, 2
ble t1, t2, p_7 # less than or equal to
p_7:
li a0, 0
li a7, 93
ecall           # halt
_exit:
lw a0, 0(sp)
li a7, 93
ecall           # final halt