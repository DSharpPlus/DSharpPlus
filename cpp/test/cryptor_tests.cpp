#include <gtest/gtest.h>

#include "dave/decryptor.h"
#include "dave/encryptor.h"
#include "dave/frame_processors.h"

#include "dave_test.h"
#include "static_key_ratchet.h"

using namespace testing;
using namespace std::chrono_literals;

namespace discord {
namespace dave {
namespace test {

constexpr std::string_view RandomBytes =
  "0dc5aedd5bdc3f20be5697e54dd1f437b896a36f858c6f20bbd69e2a493ca170c4f0c1b9acd4"
  "9d324b92afa788d09b12b29115a2feb3552b60fff983234a6c9608af3933683efc6b0f5579a9";

TEST_F(DaveTests, PassthroughInOutBuffer)
{
    auto incomingFrame = GetBufferFromHex(RandomBytes);
    auto frameCopy = incomingFrame;

    auto frameViewIn = MakeArrayView<const uint8_t>(incomingFrame.data(), incomingFrame.size());
    auto frameViewOut = MakeArrayView<uint8_t>(incomingFrame.data(), incomingFrame.size());

    EXPECT_NE(incomingFrame.data(), frameCopy.data());

    Encryptor encryptor;
    encryptor.AssignSsrcToCodec(0, Codec::Opus);
    encryptor.SetPassthroughMode(true);

    size_t bytesWritten = 0;
    auto encryptResult =
      encryptor.Encrypt(MediaType::Audio, 0, frameViewIn, frameViewOut, &bytesWritten);

    EXPECT_EQ(encryptResult, 0);
    EXPECT_EQ(bytesWritten, frameCopy.size());
    EXPECT_EQ(memcmp(incomingFrame.data(), frameCopy.data(), bytesWritten), 0);

    Decryptor decryptor;
    decryptor.TransitionToPassthroughMode(true, 0s);

    auto decryptResult = decryptor.Decrypt(MediaType::Audio, frameViewIn, frameViewOut);

    EXPECT_EQ(decryptResult, frameCopy.size());
    EXPECT_EQ(memcmp(incomingFrame.data(), frameCopy.data(), bytesWritten), 0);
}

TEST_F(DaveTests, PassthroughTwoBuffers)
{
    auto incomingFrame = GetBufferFromHex(RandomBytes);
    auto encryptedFrame = std::vector<uint8_t>(incomingFrame.size() * 2);
    auto decryptedFrame = std::vector<uint8_t>(incomingFrame.size());

    Encryptor encryptor;
    encryptor.AssignSsrcToCodec(0, Codec::Opus);
    encryptor.SetPassthroughMode(true);

    size_t bytesWritten = 0;
    auto encryptResult = encryptor.Encrypt(MediaType::Audio,
                                           0,
                                           {incomingFrame.data(), incomingFrame.size()},
                                           {encryptedFrame.data(), encryptedFrame.size()},
                                           &bytesWritten);

    EXPECT_EQ(encryptResult, 0);
    EXPECT_EQ(bytesWritten, incomingFrame.size());
    EXPECT_EQ(memcmp(incomingFrame.data(), encryptedFrame.data(), bytesWritten), 0);

    Decryptor decryptor;
    decryptor.TransitionToPassthroughMode(true, 0s);

    auto decryptResult = decryptor.Decrypt(MediaType::Audio,
                                           {encryptedFrame.data(), bytesWritten},
                                           {decryptedFrame.data(), decryptedFrame.size()});

    EXPECT_EQ(decryptResult, incomingFrame.size());
    EXPECT_EQ(memcmp(encryptedFrame.data(), decryptedFrame.data(), decryptResult), 0);
}

TEST_F(DaveTests, SilencePacketPassthrough)
{
    const std::vector<uint8_t> WorkerSilencePacket = {248, 255, 254};

    Decryptor decryptor;
    decryptor.TransitionToKeyRatchet(std::make_unique<StaticKeyRatchet>("0123456789876543210"), 0s);

    auto decryptedFrame = std::vector<uint8_t>(WorkerSilencePacket.size());
    auto decryptResult = decryptor.Decrypt(MediaType::Audio,
                                           {WorkerSilencePacket.data(), WorkerSilencePacket.size()},
                                           {decryptedFrame.data(), decryptedFrame.size()});

    EXPECT_EQ(decryptResult, WorkerSilencePacket.size());
    EXPECT_EQ(memcmp(WorkerSilencePacket.data(), decryptedFrame.data(), decryptResult), 0);
}

TEST_F(DaveTests, RandomOpusFrameEncryptDecrypt)
{
    Encryptor encryptor;
    Decryptor decryptor;

    // set static key ratchet for testing
    encryptor.SetKeyRatchet(std::make_unique<StaticKeyRatchet>("0123456789876543210"));
    decryptor.TransitionToKeyRatchet(std::make_unique<StaticKeyRatchet>("0123456789876543210"), 0s);

    // load the hex encoded sample frame to a buffer
    auto incomingFrame = GetBufferFromHex(RandomBytes);
    auto encryptedFrame = std::vector<uint8_t>(incomingFrame.size() * 2);
    auto decryptedFrame = std::vector<uint8_t>(incomingFrame.size());

    for (size_t i = 0; i < 1; i++) {
        // encrypt frame
        size_t bytesWritten = 0;
        encryptor.AssignSsrcToCodec(0, Codec::Opus);
        auto encryptResult = encryptor.Encrypt(MediaType::Audio,
                                               0,
                                               {incomingFrame.data(), incomingFrame.size()},
                                               {encryptedFrame.data(), encryptedFrame.size()},
                                               &bytesWritten);

        EXPECT_EQ(encryptResult, 0);
        EXPECT_GE(bytesWritten, incomingFrame.size());

        // decrypt frame
        auto decryptResult = decryptor.Decrypt(MediaType::Audio,
                                               {encryptedFrame.data(), bytesWritten},
                                               {decryptedFrame.data(), decryptedFrame.size()});
        EXPECT_EQ(decryptResult, incomingFrame.size());
        EXPECT_EQ(memcmp(incomingFrame.data(), decryptedFrame.data(), incomingFrame.size()), 0);
    }
}

} // namespace test
} // namespace dave
} // namespace discord
