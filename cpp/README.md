## libdave C++

Contains the libdave C++ library, which handles the bulk of the DAVE protocol implementation for Discord's native clients.

### Dependencies

- [mlspp](https://github.com/cisco/mlspp)
  - Configured with `-DMLS_CXX_NAMESPACE="mlspp"` and `-DDISABLE_GREASE=ON`
- [boringssl](https://boringssl.googlesource.com/boringssl)

#### Testing

- [googletest](https://github.com/google/googletest)
- [AFLplusplus](https://github.com/AFLplusplus/AFLplusplus)
