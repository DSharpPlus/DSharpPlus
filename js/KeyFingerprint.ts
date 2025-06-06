const VERSION_LEN = 2;
const UID_LEN = 8;

export async function generateKeyFingerprint(version: number, key: Uint8Array, userId: string): Promise<Uint8Array> {
  if (version !== 0) {
    throw new Error('unsupported fingerprint format version');
  }

  if (key.byteLength === 0) {
    throw new Error('zero-length key');
  }

  if (userId.length === 0) {
    throw new Error('zero-length user ID');
  }

  const userIdInt = BigInt(userId);
  if (userIdInt < 0n || userIdInt >= 2n ** 64n) {
    throw new Error('user ID out of range');
  }

  let lbuf = new Uint8Array(VERSION_LEN + key.byteLength + UID_LEN);
  lbuf.set(key, VERSION_LEN);

  const dv = new DataView(lbuf.buffer);
  dv.setUint16(0, version);
  dv.setBigUint64(VERSION_LEN + key.byteLength, userIdInt);

  return lbuf;
}
