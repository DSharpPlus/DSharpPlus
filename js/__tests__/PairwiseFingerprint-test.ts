import {describe, expect, test} from '@jest/globals';

const DAVE = require('../libdave');

describe('PairwiseFingerprint', () => {
  test('expectedOutput', async () => {
    const data1 = new Uint8Array(33);
    const data2 = new Uint8Array(65);
    expect(DAVE.generatePairwiseFingerprint(0, data1, '1234', data2, '5678')).resolves.toEqual(
      new Uint8Array([
        133, 129, 241, 44, 36, 135, 79, 195, 27, 28, 151, 69, 124, 197, 189, 41, 192, 7, 16, 45, 79, 247, 138, 58, 126,
        161, 178, 136, 12, 109, 96, 164, 169, 92, 2, 232, 136, 174, 74, 156, 173, 144, 191, 184, 34, 45, 242, 136, 41,
        133, 14, 158, 119, 79, 204, 48, 6, 220, 121, 6, 242, 11, 164, 60,
      ]),
    );
  });

  test('badSort', async () => {
    const data1 = new Uint8Array([0, 100]);
    const data2 = new Uint8Array([0, 20]);
    expect(DAVE.generatePairwiseFingerprint(0, data1, '1', data2, '2')).resolves.toEqual(
      new Uint8Array([
        141, 169, 194, 143, 22, 72, 22, 245, 13, 140, 66, 228, 159, 195, 101, 106, 119, 240, 69, 191, 178, 227, 194,
        126, 162, 255, 222, 148, 138, 5, 33, 215, 240, 167, 234, 245, 149, 182, 46, 20, 4, 83, 191, 31, 165, 74, 253,
        165, 199, 16, 29, 71, 193, 205, 169, 154, 255, 154, 34, 30, 94, 171, 247, 43,
      ]),
    );
  });

  test('expectedFailure', async () => {
    const data = new Uint8Array(33);
    await expect(DAVE.generatePairwiseFingerprint(1, data, '1234', data, '5678')).rejects.toThrow();

    await expect(DAVE.generatePairwiseFingerprint(0, data, 'abcd', data, '5678')).rejects.toThrow();

    await expect(DAVE.generatePairwiseFingerprint(0, new Uint8Array(0), '1234', data, '5678')).rejects.toThrow();
  });
});
