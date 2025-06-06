#include "dave/mls/detail/persisted_key_pair.h"

namespace discord {
namespace dave {
namespace mls {
namespace detail {

std::shared_ptr<::mlspp::SignaturePrivateKey> GetNativePersistedKeyPair(
  [[maybe_unused]] KeyPairContextType ctx,
  [[maybe_unused]] const std::string& keyID,
  [[maybe_unused]] ::mlspp::CipherSuite suite,
  bool& supported)
{
    supported = false;
    return nullptr;
}

bool DeleteNativePersistedKeyPair([[maybe_unused]] KeyPairContextType ctx,
                                  [[maybe_unused]] const std::string& keyID)
{
    return false;
}

} // namespace detail
} // namespace mls
} // namespace dave
} // namespace discord
