#include "dave_test.h"

namespace discord {
namespace dave {
namespace test {

std::vector<uint8_t> GetBufferFromHex(const std::string_view& hex)
{
    auto hexLength = hex.length();

    if (hexLength % 2 != 0) {
        return {};
    }

    auto buffer = std::vector<uint8_t>(hexLength / 2);

    for (unsigned int i = 0; i < hexLength; i += 2) {
        auto byte = std::string(hex.substr(i, 2));
        buffer[i / 2] = static_cast<uint8_t>(std::stoi(byte, nullptr, 16));
    }

    return buffer;
}

} // namespace test
} // namespace dave
} // namespace discord
