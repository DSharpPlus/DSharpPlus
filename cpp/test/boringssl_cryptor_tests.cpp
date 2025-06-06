#include "gtest/gtest.h"

#include <bytes/bytes.h>

#include "dave/boringssl_cryptor.h"

#include "dave_test.h"
#include "static_key_ratchet.h"

namespace discord {
namespace dave {
namespace test {

TEST_F(DaveTests, BoringSSLEncryptDecrypt)
{
    constexpr size_t PLAINTEXT_SIZE = 1024;
    auto plaintextBufferIn = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto additionalDataBuffer = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto plaintextBufferOut = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto ciphertextBuffer = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto nonceBuffer = std::vector<uint8_t>(kAesGcm128NonceBytes, 0);
    auto tagBuffer = std::vector<uint8_t>(kAesGcm128TruncatedTagBytes, 0);

    auto plaintextIn =
      MakeArrayView<const uint8_t>(plaintextBufferIn.data(), plaintextBufferIn.size());
    auto additionalData =
      MakeArrayView<const uint8_t>(additionalDataBuffer.data(), additionalDataBuffer.size());
    auto plaintextOut =
      MakeArrayView<uint8_t>(plaintextBufferOut.data(), plaintextBufferOut.size());
    auto ciphertextOut = MakeArrayView<uint8_t>(ciphertextBuffer.data(), ciphertextBuffer.size());
    auto ciphertextIn =
      MakeArrayView<const uint8_t>(ciphertextBuffer.data(), ciphertextBuffer.size());
    auto nonce = MakeArrayView<const uint8_t>(nonceBuffer.data(), nonceBuffer.size());
    auto tagOut = MakeArrayView<uint8_t>(tagBuffer.data(), tagBuffer.size());
    auto tagIn = MakeArrayView<const uint8_t>(tagBuffer.data(), tagBuffer.size());

    BoringSSLCryptor cryptor(MakeStaticSenderKey("12345678901234567890"));

    EXPECT_TRUE(cryptor.Encrypt(ciphertextOut, plaintextIn, nonce, additionalData, tagOut));

    // The ciphertext should not be the same as the plaintext
    EXPECT_FALSE(memcmp(plaintextBufferIn.data(), ciphertextBuffer.data(), PLAINTEXT_SIZE) == 0);

    EXPECT_TRUE(cryptor.Decrypt(plaintextOut, ciphertextIn, tagIn, nonce, additionalData));

    // The plaintext should be the same as the original plaintext
    EXPECT_TRUE(memcmp(plaintextBufferIn.data(), plaintextBufferOut.data(), PLAINTEXT_SIZE) == 0);
}

TEST_F(DaveTests, BoringSSLAdditionalDataAuth)
{
    constexpr size_t PLAINTEXT_SIZE = 1024;
    auto plaintextBufferIn = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto additionalDataBuffer = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto plaintextBufferOut = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto ciphertextBuffer = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto nonceBuffer = std::vector<uint8_t>(kAesGcm128NonceBytes, 0);
    auto tagBuffer = std::vector<uint8_t>(kAesGcm128TruncatedTagBytes, 0);

    auto plaintextIn =
      MakeArrayView<const uint8_t>(plaintextBufferIn.data(), plaintextBufferIn.size());
    auto additionalData =
      MakeArrayView<const uint8_t>(additionalDataBuffer.data(), additionalDataBuffer.size());
    auto plaintextOut =
      MakeArrayView<uint8_t>(plaintextBufferOut.data(), plaintextBufferOut.size());
    auto ciphertextOut = MakeArrayView<uint8_t>(ciphertextBuffer.data(), ciphertextBuffer.size());
    auto ciphertextIn =
      MakeArrayView<const uint8_t>(ciphertextBuffer.data(), ciphertextBuffer.size());
    auto nonce = MakeArrayView<const uint8_t>(nonceBuffer.data(), nonceBuffer.size());
    auto tagOut = MakeArrayView<uint8_t>(tagBuffer.data(), tagBuffer.size());
    auto tagIn = MakeArrayView<const uint8_t>(tagBuffer.data(), tagBuffer.size());

    BoringSSLCryptor cryptor(MakeStaticSenderKey("12345678901234567890"));

    EXPECT_TRUE(cryptor.Encrypt(ciphertextOut, plaintextIn, nonce, additionalData, tagOut));

    // We modify the additional data before decryption
    additionalDataBuffer[0] = 1;

    EXPECT_FALSE(cryptor.Decrypt(plaintextOut, ciphertextIn, tagIn, nonce, additionalData));
}

TEST_F(DaveTests, BoringSSLKeyDiff)
{
    constexpr size_t PLAINTEXT_SIZE = 1024;
    auto plaintextBuffer1 = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto additionalDataBuffer1 = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto plaintextBuffer2 = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto additionalDataBuffer2 = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto ciphertextBuffer1 = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto ciphertextBuffer2 = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto nonceBuffer = std::vector<uint8_t>(kAesGcm128NonceBytes, 0);
    auto tagBuffer = std::vector<uint8_t>(kAesGcm128TruncatedTagBytes, 0);

    auto plaintext1 =
      MakeArrayView<const uint8_t>(plaintextBuffer1.data(), plaintextBuffer1.size());
    auto additionalData1 =
      MakeArrayView<const uint8_t>(additionalDataBuffer1.data(), additionalDataBuffer1.size());
    auto plaintext2 =
      MakeArrayView<const uint8_t>(plaintextBuffer2.data(), plaintextBuffer2.size());
    auto additionalData2 =
      MakeArrayView<const uint8_t>(additionalDataBuffer2.data(), additionalDataBuffer2.size());
    auto ciphertext1 = MakeArrayView<uint8_t>(ciphertextBuffer1.data(), ciphertextBuffer1.size());
    auto ciphertext2 = MakeArrayView<uint8_t>(ciphertextBuffer2.data(), ciphertextBuffer2.size());
    auto nonce = MakeArrayView<const uint8_t>(nonceBuffer.data(), nonceBuffer.size());
    auto tag = MakeArrayView<uint8_t>(tagBuffer.data(), tagBuffer.size());

    BoringSSLCryptor cryptor1(MakeStaticSenderKey("12345678901234567890"));
    BoringSSLCryptor cryptor2(MakeStaticSenderKey("09876543210987654321"));

    EXPECT_TRUE(cryptor1.Encrypt(ciphertext1, plaintext1, nonce, additionalData1, tag));
    EXPECT_TRUE(cryptor2.Encrypt(ciphertext2, plaintext2, nonce, additionalData2, tag));

    EXPECT_FALSE(memcmp(ciphertextBuffer1.data(), ciphertextBuffer2.data(), PLAINTEXT_SIZE) == 0);
}

TEST_F(DaveTests, BoringSSLNonceDiff)
{
    constexpr size_t PLAINTEXT_SIZE = 1024;
    auto plaintextBuffer1 = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto additionalDataBuffer1 = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto plaintextBuffer2 = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto additionalDataBuffer2 = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto ciphertextBuffer1 = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto ciphertextBuffer2 = std::vector<uint8_t>(PLAINTEXT_SIZE, 0);
    auto nonceBuffer1 = std::vector<uint8_t>(kAesGcm128NonceBytes, 0);
    auto nonceBuffer2 = std::vector<uint8_t>(kAesGcm128NonceBytes, 1);
    auto tagBuffer = std::vector<uint8_t>(kAesGcm128TruncatedTagBytes, 0);

    auto plaintext1 =
      MakeArrayView<const uint8_t>(plaintextBuffer1.data(), plaintextBuffer1.size());
    auto additionalData1 =
      MakeArrayView<const uint8_t>(additionalDataBuffer1.data(), additionalDataBuffer1.size());
    auto plaintext2 =
      MakeArrayView<const uint8_t>(plaintextBuffer2.data(), plaintextBuffer2.size());
    auto additionalData2 =
      MakeArrayView<const uint8_t>(additionalDataBuffer2.data(), additionalDataBuffer2.size());
    auto ciphertext1 = MakeArrayView<uint8_t>(ciphertextBuffer1.data(), ciphertextBuffer1.size());
    auto ciphertext2 = MakeArrayView<uint8_t>(ciphertextBuffer2.data(), ciphertextBuffer2.size());
    auto nonce1 = MakeArrayView<const uint8_t>(nonceBuffer1.data(), nonceBuffer1.size());
    auto nonce2 = MakeArrayView<const uint8_t>(nonceBuffer2.data(), nonceBuffer2.size());
    auto tag = MakeArrayView<uint8_t>(tagBuffer.data(), tagBuffer.size());

    BoringSSLCryptor cryptor(MakeStaticSenderKey("12345678901234567890"));

    EXPECT_TRUE(cryptor.Encrypt(ciphertext1, plaintext1, nonce1, additionalData1, tag));
    EXPECT_TRUE(cryptor.Encrypt(ciphertext2, plaintext2, nonce2, additionalData2, tag));

    EXPECT_FALSE(memcmp(ciphertextBuffer1.data(), ciphertextBuffer2.data(), PLAINTEXT_SIZE) == 0);
}

} // namespace test
} // namespace dave
} // namespace discord
