// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// Base implementation of <see cref="IPipelineContext"/> with JSON-serializable success state and failure details.
/// </summary>
public abstract class BasePipelineContext
{

    /// <summary>
    /// Gets or sets a value indicating whether the pipeline is still in a successful state.
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Gets or sets the current phase of the pipeline (before or after the main action).
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<PipelinePhase>))]
    public PipelinePhase Phase { get; set; }

    /// <summary>
    /// Gets or sets the name of the step that caused the pipeline to fail.
    /// </summary>
    public string FailedStep { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exception that caused the pipeline failure, if any.
    /// </summary>
    [JsonIgnore]
    public Exception? Cause { get; set; }

    /// <summary>
    /// Gets the type name of the exception that caused the failure, or an empty string if no exception occurred.
    /// </summary>
    public string ExceptionType => Cause?.GetType().Name ?? string.Empty;

    /// <summary>
    /// Gets the message of the exception that caused the failure, or an empty string if no exception occurred.
    /// </summary>
    public string ExceptionMessage => Cause?.Message ?? string.Empty;

}
