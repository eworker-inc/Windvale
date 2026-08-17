# Workload 11 strict reference oracle

## Status

This document is the simple correctness oracle for the paper workload. It uses
only exact package bytes and Language 1.0 strict numeric rules. It is deliberately
not optimized and does not define physical accelerator performance.

## Input decoding

The tokenizer payload contains these finite f16 interchange encodings:

| Token | f16 bits | Exact f32 value |
| ---: | ---: | ---: |
| 1 | `0x4000` | `2.0` |
| 2 | `0xBC00` | `-1.0` |
| 3 | `0x3800` | `0.5` |
| 4 | `0x4200` | `3.0` |

For a finite normal f16 value, the paper decoder moves the sign, rebiases the
five-bit exponent by 112, shifts the ten fraction bits into f32 position, and
uses `Bitsˉu32ˉtoˉf32`. It performs no host cast. This first tokenizer format
rejects exponent zero and 31 rather than assigning incomplete subnormal, infinity,
or NaN behavior.

The input vector is therefore:

```text
X = [2.0, -1.0, 0.5, 3.0]
```

## Weight decoding

The four bytes `E1 03 2F 4D` are read in row-major order. The low nibble precedes
the high nibble in each byte. Four-bit two's-complement decoding yields:

```text
Wq[0] = [ 1, -2,  3, 0 ]
Wq[1] = [-1,  2, -3, 4 ]
```

Row scales and biases are strict f32 values:

```text
Scale = [0.5, 0.25]
Bias  = [0.25, -0.125]
```

## Operation order

The reference path performs these operations exactly:

1. convert each signed-I4 mathematical value exactly to f32;
2. multiply each f32 input by its f32 weight value;
3. accumulate four products from column zero through three using separate f32
   additions, roundTiesToEven, no contraction, and no reassociation;
4. multiply the row accumulator by its one f32 scale;
5. add its f32 bias in the custom-kernel reference; and
6. return the biased value when it is greater than positive zero, otherwise
   return positive zero.

### Row 0

```text
Accumulator = (((2.0 * 1.0) + (-1.0 * -2.0)) + (0.5 * 3.0)) + (3.0 * 0.0)
            = 5.5
Scaled      = 5.5 * 0.5
            = 2.75
Biased      = 2.75 + 0.25
            = 3.0
ReLU        = 3.0
```

### Row 1

```text
Accumulator = (((2.0 * -1.0) + (-1.0 * 2.0)) + (0.5 * -3.0)) + (3.0 * 4.0)
            = 6.5
Scaled      = 6.5 * 0.25
            = 1.625
Biased      = 1.625 + -0.125
            = 1.5
ReLU        = 1.5
```

All intermediate values in this fixture are exactly representable in f32.

## Expected output

| Index | Value | f32 bits | Little-endian bytes |
| ---: | ---: | ---: | --- |
| 0 | `3.0` | `0x40400000` | `00 00 40 40` |
| 1 | `1.5` | `0x3FC00000` | `00 00 C0 3F` |

The exact eight-byte output is:

```text
00 00 40 40 00 00 C0 3F
```

Its SHA-256 identity is
`a86c2d68f31d3d97629e347fdb19445648c5a7e11df505e95fd5ac7bcdfed076`.

Class selection compares the two finite admitted f32 scores. The larger score
wins; equality selects the lower index. This fixture selects class `0`.

## Accelerated comparison contract

The paper `Boundedˉf32ˉv1` mode requires signed-I4 inputs, finite f16 activations,
f32 accumulation, finite f32 output, no narrower hidden accumulator, and the
declared scale/bias order. A provider may use a different fixed reduction tree
only when each final output passes:

```text
absolute_error <= absolute_limit + relative_limit * abs(expected)
absolute_limit = 1 / 1024 = 0.0009765625
relative_limit = 1 / 1024 = 0.0009765625
```

The resulting fixture limits are:

| Index | Expected | Maximum allowed absolute error |
| ---: | ---: | ---: |
| 0 | 3.0 | 0.00390625 |
| 1 | 1.5 | 0.00244140625 |

The software provider must produce the exact bytes above. A physical provider
may produce a different finite value only within the formula. NaN, infinity,
mis-sized output, a different element order, or a narrower accumulator rejects
regardless of tolerance. The custom bias/ReLU kernel itself uses strict f32
addition and comparison and has no approximate operation.

## Differential procedure

For every admitted provider:

1. verify the four package digests;
2. run `Inferenceˉreference.Run` once;
3. submit the six-command accelerator batch once;
4. collect outputs in index order only after terminal completion;
5. validate output length and finite f32 bits;
6. apply the comparison formula independently per index; and
7. record provider identity, generation, attachment mode, numeric mode, exact
   observed bits, and comparison result.

Wall-clock time, scheduler order, native handles, and device addresses never
enter correctness evidence.
