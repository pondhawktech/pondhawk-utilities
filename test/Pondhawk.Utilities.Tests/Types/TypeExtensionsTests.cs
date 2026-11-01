// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Pondhawk.Utilities.Types;
using Shouldly;
using Xunit;

namespace Pondhawk.Utilities.Tests.Types;

public class TypeExtensionsTests
{

    [Fact]
    public void ToHexString_KnownBytes_ReturnsLowercaseHex()
    {
        var bytes = new byte[] { 0xAB, 0xCD };

        bytes.ToHexString().ShouldBe("abcd");
    }

    [Fact]
    public void ToHexString_EmptyArray_ReturnsEmpty()
    {
        var bytes = Array.Empty<byte>();

        bytes.ToHexString().ShouldBe("");
    }

    [Fact]
    public void ToHexString_Null_Throws()
    {
        byte[] bytes = null;

        Should.Throw<ArgumentNullException>(() => bytes.ToHexString());
    }

    [Fact]
    public void ToTimestampString_KnownUtcDate_ReturnsExpectedFormat()
    {
        var date = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        var result = date.ToTimestampString();

        result.ShouldStartWith("20250115");
        result.Length.ShouldBeGreaterThan(8);
    }

    [Fact]
    public void GetConciseName_NonGeneric_ReturnsName()
    {
        typeof(string).GetConciseName().ShouldBe("String");
    }

    [Fact]
    public void GetConciseName_GenericType_ReturnsReadableName()
    {
        typeof(List<string>).GetConciseName().ShouldBe("List<String>");
    }

    [Fact]
    public void GetConciseName_NestedGeneric_ReturnsNestedName()
    {
        typeof(Dictionary<string, List<int>>).GetConciseName()
            .ShouldBe("Dictionary<String, List<Int32>>");
    }

    [Fact]
    public void GetConciseFullName_NonGeneric_ReturnsFullName()
    {
        typeof(string).GetConciseFullName().ShouldBe("System.String");
    }

    [Fact]
    public void GetConciseFullName_GenericType_IncludesGenericParams()
    {
        var result = typeof(List<string>).GetConciseFullName();

        result.ShouldStartWith("System.Collections.Generic.List<String>");
    }

    [Fact]
    public void GetConciseFullName_NullFullName_ReturnsEmpty()
    {
        // Generic type parameters (e.g. T) have null FullName
        var genericParam = typeof(List<>).GetGenericArguments()[0];

        genericParam.GetConciseFullName().ShouldBe("");
    }

}
