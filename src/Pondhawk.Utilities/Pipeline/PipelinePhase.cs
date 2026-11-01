// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// Indicates whether a pipeline step is executing before or after the main action.
/// </summary>
public enum PipelinePhase
{
    /// <summary>
    /// The pipeline is executing steps before the main action.
    /// </summary>
    Before,

    /// <summary>
    /// The pipeline is executing steps after the main action.
    /// </summary>
    After
}
