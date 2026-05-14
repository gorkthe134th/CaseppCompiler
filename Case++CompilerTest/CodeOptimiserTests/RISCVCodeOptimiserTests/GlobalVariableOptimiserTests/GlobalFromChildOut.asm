mv gp, sp
p_f_g:
mv t0, gp
addi t0, t0, 12
sw s0, 0(t0)
mv t0, sp
lw t0, 0(t0)
addi t0, t0, 16
lw s0, 0(t0)