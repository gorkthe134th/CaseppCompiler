mv gp, sp
p_f:
mv t0, gp
addi t0, t0, 8
sw s0, 0(t0)
mv t0, gp
addi t0, t0, 8
lw s0, 0(t0)
mv t0, sp
addi t0, t0, 12
lw t0, 0(t0)
sw s0, 0(t0)