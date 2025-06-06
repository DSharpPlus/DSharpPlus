import {describe, expect, test} from '@jest/globals';

const DAVE = require('../libdave');

describe('DisplayableCode', () => {
  test('expectedOutput', () => {
    const shortData = new Uint8Array([0xaa, 0xbb, 0xcc, 0xdd, 0xee]);
    expect(DAVE.generateDisplayableCode(shortData, 5, 5)).toBe('05870');

    const longDataBuffer = Buffer.from('aabbccddeebbccddeeffccddeeffaaddeeffaabbeeffaabbccffaabbccdd', 'hex');
    const longData = Uint8Array.from(longDataBuffer);
    expect(DAVE.generateDisplayableCode(longData, 30, 5)).toBe('058708105556138052119572494877');
  });

  test('expectedFailure', () => {
    const tooShortData = new Uint8Array([0xaa, 0xbb, 0xcc, 0xdd]);
    expect(() => {
      DAVE.generateDisplayableCode(tooShortData, 5, 5);
    }).toThrow();

    const goodData = new Uint8Array([0xaa, 0xbb, 0xcc, 0xdd]);
    expect(() => {
      DAVE.generateDisplayableCode(goodData, 4, 3);
    }).toThrow();

    const randomData = new Uint8Array(1024);
    globalThis.crypto.getRandomValues(randomData);
    expect(() => {
      DAVE.generateDisplayableCode(randomData, 1024, 11);
    }).toThrow();
  });
});
