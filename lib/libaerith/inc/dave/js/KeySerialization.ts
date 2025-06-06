import base64 from 'base64-js';

export function serializeKey(data: Uint8Array): string {
  return base64.fromByteArray(data);
}
