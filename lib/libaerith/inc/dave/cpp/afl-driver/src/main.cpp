#include <array>
#include <cassert>
#include <iostream>
#include <memory>
#include <unistd.h>

#include <fuzzer/FuzzedDataProvider.h>

#include "dave/common.h"
#include "dave/utils/array_view.h"
#include "dave/decryptor.h"

using namespace discord::dave;

extern "C" int LLVMFuzzerTestOneInput(const uint8_t* data, size_t size)
{
    FuzzedDataProvider provider(data, size);
    MediaType mediaType = static_cast<MediaType>(provider.ConsumeIntegralInRange(0, 2));
    const auto InFrame = provider.ConsumeRemainingBytes<uint8_t>();

    Decryptor decryptor;
    const auto OutFrameSize = decryptor.GetMaxPlaintextByteSize(mediaType, InFrame.size());
    auto outFrame = std::make_unique<uint8_t[]>(OutFrameSize);
    [[maybe_unused]] auto res = decryptor.Decrypt(mediaType,
                                                  MakeArrayView(InFrame.data(), InFrame.size()),
                                                  MakeArrayView(outFrame.get(), OutFrameSize));
    return 0;
}
