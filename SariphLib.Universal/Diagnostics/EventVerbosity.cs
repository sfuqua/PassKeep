// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

/// <summary>
/// Indicates the logging level of an event.
/// </summary>
public enum EventVerbosity
{
    /// <summary>
    /// Errors and other sparse events.
    /// </summary>
    Critical,

    /// <summary>
    /// General events.
    /// </summary>
    Info,

    /// <summary>
    /// Debug-only level tracing.
    /// </summary>
    Verbose
}