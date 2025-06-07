#include<cstdint>
#include<vector>

// represents a wrapper around a vector<uint8_t> that C# can use to get the contents of a vector in two native calls
struct VectorWrapper 
{
public:
    std::vector<uint8_t> vector;

    VectorWrapper(std::vector<uint8_t> vec)
    {
        this->vector = vec;
    }
};

// gets the size of the vector we wrapped so that C# knows how much memory needs to be allocated.
extern "C" __declspec(__dllexport__) int32_t AerithGetWrappedVectorSize(VectorWrapper* __restrict__ wrapped);

// copies the wrapped vector into memory provided by C#
extern "C" __declspec(__dllexport__) void AerithGetWrappedVectorData(VectorWrapper* __restrict__ wrapper, uint8_t* buffer);

// destroys a given vector wrapper. the vector is not destroyed (since we don't ever actually control the vector, that's always managed by dave/mlspp).
extern "C" __declspec(__dllexport__) void AerithDestroyVectorWrapper(VectorWrapper* wrapper);
