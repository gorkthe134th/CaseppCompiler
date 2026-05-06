j _main         # skip function code
p_f:            # start of function "f"
sw ra, 4(sp)    # save the return address
p_f_0:
li t1, 0
mv t0, sp
addi t0, t0, 8
lw t0, 0(t0)    # get the address of the return variable (*(sp + 8))
sw t1, 0(t0)    # store 0 into the return variable
lw ra, 4(sp)    # load the return address
jr ra           # return
p_g_h:          # start of function "h"
sw ra, 4(sp)    # save the return address
p_g_h_0:        # "par" does not produce code
p_g_h_1:
addi fp, sp, 32 # set the frame pointer to the start of the next frame
mv t0, sp
lw t0, 0(t0)
lw t0, 0(t0)
sw t0, 0(fp)    # since this is a call to an uncle, it has the same parent as the parent of the current function
mv t0, sp
addi t0, t0, 12
mv t1, t0
sw t1, 8(fp)    # set the address of the return variable to sp + 12 (_T0)
mv sp, fp       # realign sp and fp
jal ra, p_f     # jump to the start of "f"
p_g_h_2:
mv t0, sp
addi t0, t0, 12
lw t1, 0(t0)    # load the return value (from _T0)
mv t0, sp
addi t0, t0, 8
lw t0, 0(t0)    # get the address of the return variable (*(sp + 8))
sw t1, 0(t0)    # store the value of _T0 into the return variable
lw ra, 4(sp)    # load the return address
jr ra           # return
p_g_h_3:
li t1, 0
mv t0, sp
addi t0, t0, 8
lw t0, 0(t0)    # get the address of the return variable (*(sp + 8))
sw t1, 0(t0)    # store 0 into the return variable, as a default
lw ra, 4(sp)    # load the return address
jr ra           # return
p_g:            # start of function "g"
sw ra, 4(sp)    # save the return address
p_g_0:
li t1, 0
mv t0, sp
addi t0, t0, 8
lw t0, 0(t0)    # get the address of the return variable (*(sp + 8))
sw t1, 0(t0)    # store 0 into the return variable
lw ra, 4(sp)    # load the return address
jr ra           # return
_main:
p:
sw ra, 4(sp)    # save the return address (not necessary here)
p_0:
li a0, 0
li a7, 93
ecall           # halt