.data
_new_line: .asciz "\n"
.text
j _main          # skip function code (not necessary here)
_main:
la ra, _exit     # set the return address to the final halt, in case the main function returns (not necessary here)
sw sp, 8(sp)     # use the first position in the stack to store the program return value (instead of the parent frame reference)
p:
sw ra, 4(sp)     # save the return address (not necessary here)
p_0:             # "par" does not produce code
p_1:             # "par" does not produce code
p_2:
addi fp, sp, 20  # set the frame pointer to the start of the next frame
mv t0, sp
sw t0, 0(fp)     # since this is a call to a child, the current function is the parent
mv t0, sp
addi t0, t0, 12
mv t1, t0
sw t1, 12(fp)    # store a reference to x (sp + 12) to the out parameter (fp + 12)
mv t0, sp
addi t0, t0, 16
mv t1, t0
sw t1, 8(fp)     # set the address of the return variable to sp + 16 (_T1)
mv sp, fp        # move the stack pointer forward to the start of the next frame
jal ra, p_f      # jump to the start of "f"
addi sp, sp, -20 # move the stack pointer back to the start of the this frame
p_3:
mv t0, sp
addi t0, t0, 16
lw t1, 0(t0)     # load the return value (from _T1)
mv t0, sp
addi t0, t0, 8
lw t0, 0(t0)     # get the address of the return variable (*(sp + 8))
sw t1, 0(t0)     # store the value of _T1 into the return variable
lw ra, 4(sp)     # load the return address
jr ra            # return
p_4:
li a0, 0
li a7, 93
ecall            # halt
p_f:             # start of function "f"
sw ra, 4(sp)     # save the return address
p_f_0:
li t1, 9         # load 9
mv t0, sp
addi t0, t0, 12
lw t0, 0(t0)     # load x (*(sp + 12))
sw t1, 0(t0)     # store 9 to the address pointed to by x
p_f_1:
mv t0, sp
addi t0, t0, 12
lw t0, 0(t0)     # load x (*(sp + 12))
lw t1, 0(t0)     # dereference x
mv t0, sp
addi t0, t0, 8
lw t0, 0(t0)     # get the address of the return variable (*(sp + 8))
sw t1, 0(t0)     # store x into the return variable, as a default
lw ra, 4(sp)     # load the return address
jr ra            # return
p_f_2:
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