#include "persisted_key_pair.h"

namespace discord {
namespace dave {
namespace mls {

std::shared_ptr<::mlspp::SignaturePrivateKey> GetPersistedKeyPair(
  [[maybe_unused]] KeyPairContextType,
  const std::string&,
  ProtocolVersion)
{
    return nullptr;
}

bool DeletePersistedKeyPair([[maybe_unused]] KeyPairContextType,
                            [[maybe_unused]] const std::string&,
                            [[maybe_unused]] SignatureVersion version)
{
    return false;
}

} // namespace mls
} // namespace dave
} // namespace discord
