#pragma once

#include<cstdint>
#include<cstdlib>
#include<cstring>
#include<vector>

// represents a wrapper around a vector<uint8_t> that C# can use to get the contents of a vector.
struct VectorWrapper 
{
public:
    uint8_t* data;
    int32_t length;
    int32_t oom;

    VectorWrapper(std::vector<uint8_t> vec)
    {
        this->length = vec.size();
        this->oom = 0;
        this->data = (uint8_t*)malloc(this->length);

        if (!this->data)
        {
            this->oom = 1;
            return;
        }

        memcpy(this->data, &vec[0], vec.size());
    }

    ~VectorWrapper()
    {
        free(this->data);
    }
};

// destroys a given vector wrapper. the vector is not destroyed (since we don't ever actually control the vector, that's always managed by dave/mlspp).
extern "C" __declspec(dllexport) void AerithDestroyVectorWrapper(VectorWrapper* wrapper)
{
    delete wrapper;
}
