// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Pondhawk.Mediator;

namespace Pondhawk.Api;

/// <summary>
/// Pagination metadata lifted from an <see cref="IPagedResult"/> into <see cref="ResponseMeta.Pagination"/>.
/// </summary>
/// <param name="Page">The 1-based page number.</param>
/// <param name="PageSize">The page size.</param>
/// <param name="Total">The total item count across all pages.</param>
/// <param name="TotalPages">The total number of pages.</param>
public sealed record PaginationMeta(int Page, int PageSize, int Total, int TotalPages);
