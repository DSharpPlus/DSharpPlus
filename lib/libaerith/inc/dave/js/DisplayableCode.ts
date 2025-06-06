const MAX_GROUP_SIZE = 8;

export function generateDisplayableCode(data: Uint8Array, desiredLength: number, groupSize: number): string {
  if (data.byteLength < desiredLength) {
    throw new Error('data.byteLength must be greater than or equal to desiredLength');
  }

  if (desiredLength % groupSize !== 0) {
    throw new Error('desiredLength must be a multiple of groupSize');
  }

  if (groupSize > MAX_GROUP_SIZE) {
    throw new Error(`groupSize must be less than or equal to ${MAX_GROUP_SIZE}`);
  }

  const groupModulus = BigInt(10 ** groupSize);

  let result = '';

  for (let i = 0; i < desiredLength; i += groupSize) {
    let groupValue = BigInt(0);

    for (let j = groupSize; j > 0; --j) {

      const nextByte = data[i + (groupSize - j)]
      if (nextByte === undefined) {
        throw new Error('Out of bounds access from data array');
      }

      groupValue = (groupValue << 8n) | BigInt(nextByte);
    }

    groupValue %= groupModulus;

    result += groupValue.toString().padStart(groupSize, '0');
  }

  return result;
}
