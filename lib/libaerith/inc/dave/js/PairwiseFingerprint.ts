import {generateKeyFingerprint} from './KeyFingerprint';
import {scryptAsync} from '@noble/hashes/scrypt';

const salt = Uint8Array.of(
  0x24,
  0xca,
  0xb1,
  0x7a,
  0x7a,
  0xf8,
  0xec,
  0x2b,
  0x82,
  0xb4,
  0x12,
  0xb9,
  0x2d,
  0xab,
  0x19,
  0x2e,
);
const scryptParams = {
  N: 16384,
  r: 8,
  p: 2,
  dkLen: 64,
};

function compareArrays(a: Uint8Array, b: Uint8Array) {
  for (let i = 0; i < a.length && i < b.length; i++) {
    if (a[i] != b[i]) return a[i]! - b[i]!;
  }

  return a.length - b.length;
}

export async function generatePairwiseFingerprint(
  version: number,
  keyA: Uint8Array,
  userIdA: string,
  keyB: Uint8Array,
  userIdB: string,
): Promise<Uint8Array> {
  const fingerprints = await Promise.all([
    generateKeyFingerprint(version, keyA, userIdA),
    generateKeyFingerprint(version, keyB, userIdB),
  ]);

  fingerprints.sort(compareArrays);

  const input = new Uint8Array(fingerprints[0].byteLength + fingerprints[1].byteLength);
  input.set(fingerprints[0], 0);
  input.set(fingerprints[1], fingerprints[0].byteLength);

  const ret = await scryptAsync(input, salt, scryptParams);

  return new Uint8Array(ret);
}
