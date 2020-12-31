5460: set r0 28844
5463: set r1 1531
5466: add r2 3397 8356
5470: call 1458
5472: pop r2
5474: pop r1
5476: pop r0
5478: nop
5479: nop
5480: nop
5481: nop
5482: nop
5483: set r0 4
5486: set r1 1
5489: call 6027
5491: eq r1 r0 6
5495: jf r1 5579
5498: push r0
5500: push r1


6027: jt r0 6035          // if !r0:
6030: add r0 r1 1         //   r0 = r1 + 1
6034: ret                 //   return
6035: jt r1 6048          // if !r1:
6038: add r0 r0 32767     //   r0 = r0 - 1
6042: set r1 r7           //   r1 = r7
6045: call 6027           //   _6027()
6047: ret                 //   return
6048: push r0             // push(r0)
6050: add r1 r1 32767     // r1 = r1 - 1
6054: call 6027           // _6027()
6056: set r1 r0           // r1 = r0
6059: pop r0              // r0 = pop()
6061: add r0 r0 32767     // r0 = r0 - 1
6065: call 6027           // _6027()
6067: ret                 // return


if x == 0:
  return y + 1
if y == 0:
  return f(x - 1, r7)
return f(x - 1, f(x, y - 1))