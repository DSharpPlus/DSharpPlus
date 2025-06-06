#include <vector>
#include "gtest/gtest.h"

#include "dave/codec_utils.h"
#include "dave/decryptor.h"
#include "dave/encryptor.h"
#include "dave/frame_processors.h"
#include "dave/utils/array_view.h"

#include "dave_test.h"
#include "static_key_ratchet.h"

namespace discord {
namespace dave {
namespace test {

TEST_F(DaveTests, RandomOpusFrame)
{
    constexpr std::string_view randomBytes =
      "0dc5aedd5bdc3f20be5697e54dd1f437b896a36f858c6f20bbd69e2a493ca170c4f0c1b9acd4"
      "9d324b92afa788d09b12b29115a2feb3552b60fff983234a6c9608af3933683efc6b0f5579a9";

    // load the hex encoded sample frame to a buffer
    auto incomingFrame = GetBufferFromHex(randomBytes);

    auto encryptedFrame = std::make_unique<uint8_t[]>(incomingFrame.size() * 2);

    OutboundFrameProcessor frameProcessor;

    frameProcessor.ProcessFrame(
      MakeArrayView<const uint8_t>(incomingFrame.data(), incomingFrame.size()), Codec::Opus);
    auto& unencryptedBytes = frameProcessor.GetUnencryptedBytes();
    auto& encryptedBytes = frameProcessor.GetEncryptedBytes();
    auto unencryptedRanges = frameProcessor.GetUnencryptedRanges();

    EXPECT_EQ(incomingFrame.size(), 76u);
    EXPECT_EQ(unencryptedBytes.size(), 0u);
    EXPECT_EQ(encryptedBytes.size(), incomingFrame.size());
    EXPECT_EQ(unencryptedRanges.size(), 0u);
}

TEST_F(DaveTests, SplitReconstruct)
{
    std::string randomBytes =
      "0dc5aedd5bdc3f20be5697e54dd1f437b896a36f858c6f20bbd69e2a493ca170c4f0c1b9acd4"
      "9d324b92afa788d09b12b29115a2feb3552b60fff983234a6c9608af3933683efc6b0f5579a9"
      "0000000000000000 00 000a 140a 280a 3c0a 14 fafa";
    randomBytes.erase(std::remove(randomBytes.begin(), randomBytes.end(), ' '), randomBytes.end());

    // load the hex encoded sample frame to a buffer
    auto incomingFrame = GetBufferFromHex(randomBytes);

    auto reconstructedFrame = std::make_unique<uint8_t[]>(incomingFrame.size());

    InboundFrameProcessor frameProcessor;

    frameProcessor.ParseFrame(
      MakeArrayView<const uint8_t>(incomingFrame.data(), incomingFrame.size()));
    memcpy(frameProcessor.GetPlaintext().data(),
           frameProcessor.GetCiphertext().data(),
           frameProcessor.GetCiphertext().size());
    auto bytesWritten = frameProcessor.ReconstructFrame(
      MakeArrayView<uint8_t>(reconstructedFrame.get(), incomingFrame.size()));

    EXPECT_EQ(bytesWritten, 76u);
    EXPECT_EQ(memcmp(incomingFrame.data(), reconstructedFrame.get(), bytesWritten), 0);
}

TEST_F(DaveTests, H264SliceOneByteExpGolomb)
{
    // start code, nal unit header
    // 3 exponential golomb values (first_mb_in_slice, slice_type, pic_parameter_set_id)
    // then slice payloads
    constexpr std::string_view kH264SliceHex = "0000000161e0fafafa";

    // load the hex encoded sample frame to a buffer
    auto incomingFrame = GetBufferFromHex(kH264SliceHex);

    auto encryptedFrame = std::make_unique<uint8_t[]>(incomingFrame.size() * 2);

    OutboundFrameProcessor frameProcessor;
    frameProcessor.ProcessFrame(
      MakeArrayView<const uint8_t>(incomingFrame.data(), incomingFrame.size()), Codec::H264);

    auto unencryptedRanges = frameProcessor.GetUnencryptedRanges();

    EXPECT_EQ(unencryptedRanges.size(), 1u);
    EXPECT_EQ(unencryptedRanges.front().offset, 0u);
    EXPECT_EQ(unencryptedRanges.front().size, 6u);
}

TEST_F(DaveTests, H264ShortIDROneByteExpGolomb)
{
    // SPS NAL UNIT, PPS NAL UNIT, then IDR NAL Unit
    // for IDR: nal unit header, then 3 exponential golomb values (first_mb_in_slice, slice_type,
    // pic_parameter_set_id) then IDR payloads
    constexpr std::string_view kH264ShortIDR =
      "000000016742c00d8c8d40d0fbc900f08846a00000000168ce3c800000000165b8fafafa";

    // load the hex encoded sample frame to a buffer
    auto incomingFrame = GetBufferFromHex(kH264ShortIDR);
    auto encryptedFrame = std::make_unique<uint8_t[]>(incomingFrame.size() * 2);

    OutboundFrameProcessor frameProcessor;
    frameProcessor.ProcessFrame(
      MakeArrayView<const uint8_t>(incomingFrame.data(), incomingFrame.size()), Codec::H264);

    auto unencryptedRanges = frameProcessor.GetUnencryptedRanges();

    EXPECT_EQ(unencryptedRanges.size(), 1u);
    EXPECT_EQ(unencryptedRanges.front().offset, 0u);
    EXPECT_EQ(unencryptedRanges.front().size, 33u);
}

TEST_F(DaveTests, H264ShortIDRTwoByteExpGolomb)
{
    // SPS NAL UNIT, PPS NAL UNIT, then IDR NAL Unit
    // for IDR: nal unit header, then 3 exponential golomb values (first_mb_in_slice, slice_type,
    // pic_parameter_set_id) then IDR payloads
    constexpr std::string_view kH264ShortIDR =
      "000000016742c00d8c8d40d0fbc900f08846a00000000168ce3c8000000001654760fafafa";

    // load the hex encoded sample frame to a buffer
    auto incomingFrame = GetBufferFromHex(kH264ShortIDR);
    auto encryptedFrame = std::make_unique<uint8_t[]>(incomingFrame.size() * 2);

    OutboundFrameProcessor frameProcessor;
    frameProcessor.ProcessFrame(
      MakeArrayView<const uint8_t>(incomingFrame.data(), incomingFrame.size()), Codec::H264);

    auto unencryptedRanges = frameProcessor.GetUnencryptedRanges();

    EXPECT_EQ(unencryptedRanges.size(), 1u);
    EXPECT_EQ(unencryptedRanges.front().offset, 0u);
    EXPECT_EQ(unencryptedRanges.front().size, 34u);
}

TEST_F(DaveTests, H264LongIDROneByteExpGolomb)
{
    // SPS NAL UNIT, PPS NAL UNIT, SEI NAL unit, then IDR NAL Unit
    // which has nal unit header,
    // then 3 exponential golomb values (first_mb_in_slice, slice_type, pic_parameter_set_id)
    // then IDR payloads
    constexpr std::string_view kH264LongIDR =
      "00000001274d0033ab402802dd00da08846a000000000128ee3c800000000106051a47564adc5c4c433f94efc511"
      "3cd143a801ffccccff020004ca90800000000125b8fafafa";

    // load the hex encoded sample frame to a buffer
    auto incomingFrame = GetBufferFromHex(kH264LongIDR);
    auto encryptedFrame = std::make_unique<uint8_t[]>(incomingFrame.size() * 2);

    OutboundFrameProcessor frameProcessor;
    frameProcessor.ProcessFrame(
      MakeArrayView<const uint8_t>(incomingFrame.data(), incomingFrame.size()), Codec::H264);

    auto unencryptedRanges = frameProcessor.GetUnencryptedRanges();

    EXPECT_EQ(unencryptedRanges.size(), 1u);
    EXPECT_EQ(unencryptedRanges.front().offset, 0u);
    EXPECT_EQ(unencryptedRanges.front().size, 67u);
}

TEST_F(DaveTests, H264LongIDRTwoByteExpGolomb)
{
    // SPS NAL UNIT, PPS NAL UNIT, SEI NAL unit, then IDR NAL Unit
    // which has nal unit header, then 3 exponential golomb values
    // (first_mb_in_slice, slice_type, pic_parameter_set_id) then IDR payloads
    constexpr std::string_view kH264LongIDR =
      "00000001274d0033ab402802dd00da08846a000000000128ee3c800000000106051a47564adc5c4c433f94efc5"
      "11"
      "3cd143a801ffccccff020004ca908000000001254760fafafa";

    // load the hex encoded sample frame to a buffer
    auto incomingFrame = GetBufferFromHex(kH264LongIDR);
    auto encryptedFrame = std::make_unique<uint8_t[]>(incomingFrame.size() * 2);

    OutboundFrameProcessor frameProcessor;
    frameProcessor.ProcessFrame(
      MakeArrayView<const uint8_t>(incomingFrame.data(), incomingFrame.size()), Codec::H264);

    auto unencryptedRanges = frameProcessor.GetUnencryptedRanges();

    EXPECT_EQ(unencryptedRanges.size(), 1u);
    EXPECT_EQ(unencryptedRanges.front().offset, 0u);
    EXPECT_EQ(unencryptedRanges.front().size, 68u);
}

TEST_F(DaveTests, H264EmulationPreventionInEarlyExpGolomb)
{
    constexpr std::string_view kH264SliceHex = "00000001610000038000e0fafafa";

    // load the hex encoded sample frame to a buffer
    auto incomingFrame = GetBufferFromHex(kH264SliceHex);

    auto encryptedFrame = std::make_unique<uint8_t[]>(incomingFrame.size() * 2);

    OutboundFrameProcessor frameProcessor;
    frameProcessor.ProcessFrame(
      MakeArrayView<const uint8_t>(incomingFrame.data(), incomingFrame.size()), Codec::H264);

    auto unencryptedRanges = frameProcessor.GetUnencryptedRanges();

    EXPECT_EQ(unencryptedRanges.size(), 1u);
    EXPECT_EQ(unencryptedRanges.front().offset, 0u);
    EXPECT_EQ(unencryptedRanges.front().size, 11u);
}

TEST_F(DaveTests, H264ThreeByteShortCodeExtension)
{
    constexpr std::string_view kH264MixedShortCodes =
      "000000012764001fac2b602802dd8088000003000800000301b46d0e1970"
      "00000128ee3cb0000001258880ababab";

    // load the hex encoded sample frame to a buffer
    auto incomingFrame = GetBufferFromHex(kH264MixedShortCodes);
    auto encryptedFrame = std::make_unique<uint8_t[]>(incomingFrame.size() * 2);

    OutboundFrameProcessor frameProcessor;
    frameProcessor.ProcessFrame(
      MakeArrayView<const uint8_t>(incomingFrame.data(), incomingFrame.size()), Codec::H264);

    auto unencryptedRanges = frameProcessor.GetUnencryptedRanges();

    EXPECT_EQ(unencryptedRanges.size(), 1u);
    EXPECT_EQ(unencryptedRanges.front().offset, 0u);
    EXPECT_EQ(unencryptedRanges.front().size, 45u);

    auto bytesToEncrypt = frameProcessor.GetEncryptedBytes();
    auto encryptedBytes = frameProcessor.GetCiphertextBytes();
    EXPECT_EQ(bytesToEncrypt.size(), encryptedBytes.size());
    memcpy(encryptedFrame.get(), bytesToEncrypt.data(), bytesToEncrypt.size());

    frameProcessor.ReconstructFrame(MakeArrayView<uint8_t>(
      encryptedFrame.get(), bytesToEncrypt.size() + frameProcessor.GetUnencryptedBytes().size()));

    constexpr std::string_view kExpectedUnencryptedHeaderHex =
      "000000012764001fac2b602802dd8088000003000800000301b46d0e19700000000128ee3cb000000001258880";
    auto expectedUnencryptedHeader = GetBufferFromHex(kExpectedUnencryptedHeaderHex);

    auto compareResultExpected = memcmp(
      encryptedFrame.get(), expectedUnencryptedHeader.data(), expectedUnencryptedHeader.size());

    EXPECT_EQ(compareResultExpected, 0);
}

TEST_F(DaveTests, H264TwoSliceTest)
{
    // start code, nal unit header
    // 3 exponential golomb values (first_mb_in_slice, slice_type, pic_parameter_set_id)
    // then slice payload
    // and repeated again
    constexpr std::string_view kH264TwoSliceHex = "0000000161e0fafafa0000000161e0fafafa";

    // load the hex encoded sample frame to a buffer
    auto incomingFrame = GetBufferFromHex(kH264TwoSliceHex);

    auto encryptedFrame = std::make_unique<uint8_t[]>(incomingFrame.size() * 2);

    OutboundFrameProcessor frameProcessor;
    frameProcessor.ProcessFrame(
      MakeArrayView<const uint8_t>(incomingFrame.data(), incomingFrame.size()), Codec::H264);

    auto unencryptedRanges = frameProcessor.GetUnencryptedRanges();

    EXPECT_EQ(unencryptedRanges.size(), 2u);
    EXPECT_EQ(unencryptedRanges[0].offset, 0u);
    EXPECT_EQ(unencryptedRanges[0].size, 6u);
    EXPECT_EQ(unencryptedRanges[1].offset, 9u);
    EXPECT_EQ(unencryptedRanges[1].size, 6u);
}

TEST_F(DaveTests, H265IdrSlice)
{
    constexpr std::string_view kH265IdrSliceHex =
      "0000000140010c01ffff016000000300b0000003000003005d17024"
      "000000001420101016000000300b0000003000003005da00280802d16205ee45914bff2e7f13fa2"
      "000000014401c072f05324000000014e01051a47564adc5c4c433f94efc5113cd143a803ee0000ee02001fc8b88"
      "0000000012801abab";

    // load the hex encoded sample frame to a buffer
    auto incomingFrame = GetBufferFromHex(kH265IdrSliceHex);
    auto encryptedFrame = std::make_unique<uint8_t[]>(incomingFrame.size() * 2);

    OutboundFrameProcessor frameProcessor;
    frameProcessor.ProcessFrame(
      MakeArrayView<const uint8_t>(incomingFrame.data(), incomingFrame.size()), Codec::H265);

    auto unencryptedRanges = frameProcessor.GetUnencryptedRanges();

    EXPECT_EQ(unencryptedRanges.size(), 1u);
    EXPECT_EQ(unencryptedRanges.front().offset, 0u);
    EXPECT_EQ(unencryptedRanges.front().size, 119u);
}

TEST_F(DaveTests, H265TsaSlice)
{
    constexpr std::string_view kH265TsaSliceHex = "000000010201abab";

    // load the hex encoded sample frame to a buffer
    auto incomingFrame = GetBufferFromHex(kH265TsaSliceHex);
    auto encryptedFrame = std::make_unique<uint8_t[]>(incomingFrame.size() * 2);

    OutboundFrameProcessor frameProcessor;
    frameProcessor.ProcessFrame(
      MakeArrayView<const uint8_t>(incomingFrame.data(), incomingFrame.size()), Codec::H265);

    auto unencryptedRanges = frameProcessor.GetUnencryptedRanges();

    EXPECT_EQ(unencryptedRanges.size(), 1u);
    EXPECT_EQ(unencryptedRanges.front().offset, 0u);
    EXPECT_EQ(unencryptedRanges.front().size, 6u);
}

TEST_F(DaveTests, H265SimpleThreeByteCodeExtension)
{
    constexpr std::string_view kH265TsaSliceHexShort = "0000010201abab";

    // load the hex encoded sample frame to a buffer
    auto incomingFrame = GetBufferFromHex(kH265TsaSliceHexShort);
    auto encryptedFrame = std::make_unique<uint8_t[]>(incomingFrame.size() * 2);

    OutboundFrameProcessor frameProcessor;
    frameProcessor.ProcessFrame(
      MakeArrayView<const uint8_t>(incomingFrame.data(), incomingFrame.size()), Codec::H265);

    auto unencryptedRanges = frameProcessor.GetUnencryptedRanges();

    EXPECT_EQ(unencryptedRanges.size(), 1u);
    EXPECT_EQ(unencryptedRanges.front().offset, 0u);
    EXPECT_EQ(unencryptedRanges.front().size, 6u);
}

TEST_F(DaveTests, H265MultipleThreeByteCodeExtensions)
{
    constexpr std::string_view kH265IdrSliceHex =
      "00000140010c01ffff016000000300b0000003000003005d17024"
      "0000001420101016000000300b0000003000003005da00280802d16205ee45914bff2e7f13fa2"
      "000000014401c072f05324000000014e01051a47564adc5c4c433f94efc5113cd143a803ee0000ee02001fc8b88"
      "00000012801abab";

    // load the hex encoded sample frame to a buffer
    auto incomingFrame = GetBufferFromHex(kH265IdrSliceHex);
    auto encryptedFrame = std::make_unique<uint8_t[]>(incomingFrame.size() * 2);

    OutboundFrameProcessor frameProcessor;
    frameProcessor.ProcessFrame(
      MakeArrayView<const uint8_t>(incomingFrame.data(), incomingFrame.size()), Codec::H265);

    auto unencryptedRanges = frameProcessor.GetUnencryptedRanges();

    EXPECT_EQ(unencryptedRanges.size(), 1u);
    EXPECT_EQ(unencryptedRanges.front().offset, 0u);
    EXPECT_EQ(unencryptedRanges.front().size, 119u);
}

TEST_F(DaveTests, H265TwoIdrSlice)
{
    constexpr std::string_view kH265TwoIdrSliceHex = "0000010201abab0000010201abab";

    // load the hex encoded sample frame to a buffer
    auto incomingFrame = GetBufferFromHex(kH265TwoIdrSliceHex);
    auto encryptedFrame = std::make_unique<uint8_t[]>(incomingFrame.size() * 2);

    OutboundFrameProcessor frameProcessor;
    frameProcessor.ProcessFrame(
      MakeArrayView<const uint8_t>(incomingFrame.data(), incomingFrame.size()), Codec::H265);

    auto unencryptedRanges = frameProcessor.GetUnencryptedRanges();

    EXPECT_EQ(unencryptedRanges.size(), 2u);
    EXPECT_EQ(unencryptedRanges[0].offset, 0u);
    EXPECT_EQ(unencryptedRanges[0].size, 6u);
    EXPECT_EQ(unencryptedRanges[1].offset, 8u);
    EXPECT_EQ(unencryptedRanges[1].size, 6u);
}

} // namespace test
} // namespace dave
} // namespace discord
