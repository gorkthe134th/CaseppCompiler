.data
p_f:
mv t0, sp
lw t0, 0(t0)
addi t0, t0, 8
sw s0, 0(t0)
.text
mv gp, sp
p_f:
mv t0, gp
addi t0, t0, 8
sw s0, 0(t0)