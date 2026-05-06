j _main         # skip function code (not necessary here)
_main:
p:
sw ra, 4(sp)    # save the return address (not necessary here)
p_0:
li a0, 0
li a7, 93
ecall           # halt