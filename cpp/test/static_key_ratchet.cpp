#include "static_key_ratchet.h"

#include <algorithm>

#include <bytes/bytes.h>

#include "dave/common.h"
#include "dave/logger.h"

namespace discord {
namespace dave {
namespace test {

EncryptionKey MakeStaticSenderKey(const std::string& userID)
{
    auto u64userID = strtoull(userID.c_str(), nullptr, 10);
    return MakeStaticSenderKey(u64userID);
}

EncryptionKey MakeStaticSenderKey(uint64_t u64userID)
{
    static_assert(kAesGcm128KeyBytes == 2 * sizeof(u64userID));
    EncryptionKey senderKey(kAesGcm128KeyBytes);
    const uint8_t* bytePtr = reinterpret_cast<const uint8_t*>(&u64userID);
    std::copy_n(bytePtr, sizeof(u64userID), senderKey.begin());
    std::copy_n(bytePtr, sizeof(u64userID), senderKey.begin() + sizeof(u64userID));
    return senderKey;
}

StaticKeyRatchet::StaticKeyRatchet(const std::string& userId) noexcept
  : u64userID_(strtoull(userId.c_str(), nullptr, 10))
{
}

EncryptionKey StaticKeyRatchet::GetKey(KeyGeneration generation) noexcept
{
    DISCORD_LOG(LS_INFO) << "Retrieving static key for generation " << generation << " for user "
                         << u64userID_;
    return MakeStaticSenderKey(u64userID_);
}

void StaticKeyRatchet::DeleteKey([[maybe_unused]] KeyGeneration generation) noexcept
{
    // noop
}

} // namespace test
} // namespace dave
} // namespace discord
