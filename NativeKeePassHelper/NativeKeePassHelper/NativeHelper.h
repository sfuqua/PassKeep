#pragma once

using Windows::Storage::Streams::IBuffer;

namespace NativeKeePassHelper
{
    public ref class NativeHelper sealed
    {
    public:
		// Native implementation of key transformation algorithm
        static IBuffer^ TransformKey(uint64 numRounds, IBuffer^ key, IBuffer^ iv, IBuffer^ data);
    };
}