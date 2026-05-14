lb t0, 9(sp)
lh t0, 9(sp)
la t0, 9(sp)
lw t0, 9(sp)
ld t0, 9(sp)
lbu t0, 9(sp)
lhu t0, 9(sp)
lwu t0, 9(sp)
sb t0, 9(sp)
sh t0, 9(sp)
sw t0, 9(sp)
sd t0, 9(sp)
li t0, 9
lui t0, 9
auipc t0, 9
mv t0, sp
addi t0, sp, 9
add t0, sp, s0
sub t0, sp, s0
mul t0, sp, s0
mulh t0, sp, s0
mulhu t0, sp, s0
mulhsu t0, sp, s0
div t0, sp, s0
divu t0, sp, s0
rem t0, sp, s0
remu t0, sp, s0
seqz t0, sp, s0
snez t0, sp, s0
slti t0, sp, 9
slt t0, sp, s0
sltiu t0, sp, 9
sltu t0, sp, s0
andi t0, sp, 9
and t0, sp, s0
ori t0, sp, 9
or t0, sp, s0
xori t0, sp, 9
xor t0, sp, s0
not t0, sp, s0
slli t0, sp, 9
sll t0, sp, s0
srli t0, sp, 9
srl t0, sp, s0
srai t0, sp, 9
sra t0, sp, s0
j label
jal ra, label
jr a0
jalr a0, label
beq t0, s0, label
bne t0, s0, label
blt t0, s0, label
bgt t0, s0, label
bge t0, s0, label
ble t0, s0, label
bltu t0, s0, label
bgtu t0, s0, label
bgeu t0, s0, label
bleu t0, s0, label
label:
nop
ecall
ebreak