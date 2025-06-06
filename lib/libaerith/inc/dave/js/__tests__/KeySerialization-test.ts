import {describe, expect, test} from '@jest/globals';

const DAVE = require('../libdave');

describe('KeySerialization', () => {
  test('expectedOutput', async () => {
    const zeroData = new Uint8Array(6);
    expect(DAVE.serializeKey(zeroData)).toBe('AAAAAAAA');

    const moreData = new Uint8Array([0, 1, 0xff, 0x7f, 0x80]);
    expect(DAVE.serializeKey(moreData)).toBe('AAH/f4A=');
  });
});
