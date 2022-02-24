#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Utilities
{
    public static class SystemExtensions
    {
        public static void ThrowWhenNull([NotNull] object? value, string valueExpression = "") => _ = value ?? throw new ArgumentNullException(valueExpression);
    }
}