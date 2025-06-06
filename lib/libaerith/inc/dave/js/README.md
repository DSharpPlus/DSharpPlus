## libdave JS

Contains the package @discordapp/libdave. This is leveraged by Discord clients to enable out-of-band verifications of DAVE protocol call members and the MLS epoch authenticator.

### Testing

Testing uses [Jest](https://jestjs.io/). You can run the tests with `pnpm jest`.

### Dependencies

- [@noble/hashes](https://github.com/paulmillr/noble-hashes)
- [base64-js](https://www.npmjs.com/package/base64-js)