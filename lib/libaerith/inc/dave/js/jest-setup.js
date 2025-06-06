const crypto = require('crypto');

function convertAlgorithm(name) {
  switch (name) {
    case 'SHA-512':
      return 'sha512';
    default:
      return name;
  }
}

Object.defineProperty(globalThis, 'crypto', {
  value: {
    getRandomValues: (arr) => crypto.randomBytes(arr.length),
    subtle: {
      digest: (algorithm, data) => {
        return crypto.hash(convertAlgorithm(algorithm), data, 'buffer').buffer;
      },
    },
  },
});
