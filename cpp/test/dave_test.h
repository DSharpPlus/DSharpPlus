
#include "gtest/gtest.h"

#include "dave/common.h"

namespace discord {
namespace dave {
namespace test {

std::vector<uint8_t> GetBufferFromHex(const std::string_view& hex);

class DaveTests : public ::testing::Test {
public:
    void SetUp() override {}

    void TearDown() override {}
};

} // namespace test
} // namespace dave
} // namespace discord
