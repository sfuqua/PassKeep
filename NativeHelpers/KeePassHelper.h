// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

#pragma once

using Windows::Storage::Streams::IBuffer;

namespace NativeHelpers
{
    public delegate bool ConditionChecker();

    // Managed helper class for KeePass algorithms
    public ref class KeePassHelper sealed
    {
    public:
        // Transforms data until cancellation is requested - useful for computing how many rounds fit into a given time span.
        static uint64 TransformUntilCancelled(IBuffer^ key, IBuffer^ data, ConditionChecker^ cancellationToken);

        // Native implementation of key transformation algorithm
        static IBuffer^ TransformKey(uint64 numRounds, IBuffer^ key, IBuffer^ iv, IBuffer^ data, ConditionChecker^ cancellationToken);
    };
}