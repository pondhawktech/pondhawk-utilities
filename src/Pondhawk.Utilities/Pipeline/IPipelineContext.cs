// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// Context shared across pipeline steps, tracking success state and failure information.
/// </summary>
public interface IPipelineContext
{

    /// <summary>
    /// Gets or sets a value indicating whether the pipeline is still in a successful state.
    /// </summary>
    bool Success { get; set; }

    /// <summary>
    /// Gets or sets the current phase of the pipeline (before or after the main action).
    /// </summary>
    PipelinePhase Phase { get; set; }

    /// <summary>
    /// Gets or sets the name of the step that caused the pipeline to fail.
    /// </summary>
    string FailedStep { get; set; }

    /// <summary>
    /// Gets or sets the exception that caused the pipeline failure, if any.
    /// </summary>
    Exception? Cause { get; set; }

}
