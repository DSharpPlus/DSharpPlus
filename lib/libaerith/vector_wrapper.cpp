#include "vector_wrapper.h"

#include<cstring>

extern "C" __declspec(__dllexport__) int32_t AerithGetWrappedVectorSize(VectorWrapper* __restrict__ wrapped)
{
    return wrapped->vector.size();
}

extern "C" __declspec(__dllexport__) void AerithGetWrappedVectorData(VectorWrapper* __restrict__ wrapper, uint8_t* buffer)
{
    uint8_t* raw = &wrapper->vector[0];
    memcpy(buffer, raw, wrapper->vector.size());
}

extern "C" __declspec(__dllexport__) void AerithDestroyVectorWrapper(VectorWrapper* wrapper)
{
    delete wrapper;
}