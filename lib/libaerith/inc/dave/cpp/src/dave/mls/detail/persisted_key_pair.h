#pragma once

#include <memory>
#include <string>

#include <mls/crypto.h>

#include "dave/mls/persisted_key_pair.h"

namespace discord {
namespace dave {
namespace mls {
namespace detail {

std::shared_ptr<::mlspp::SignaturePrivateKey> GetNativePersistedKeyPair(KeyPairContextType ctx,
                                                                        const std::string& keyID,
                                                                        ::mlspp::CipherSuite suite,
                                                                        bool& supported);
std::shared_ptr<::mlspp::SignaturePrivateKey> GetGenericPersistedKeyPair(
  KeyPairContextType ctx,
  const std::string& keyID,
  ::mlspp::CipherSuite suite);

bool DeleteNativePersistedKeyPair(KeyPairContextType ctx, const std::string& keyID);
bool DeleteGenericPersistedKeyPair(KeyPairContextType ctx, const std::string& keyID);

} // namespace detail
} // namespace mls
} // namespace dave
} // namespace discord
