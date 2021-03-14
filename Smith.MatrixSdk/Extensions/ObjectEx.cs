using System;

namespace Smith.MatrixSdk.Extensions
{
    public static class ObjectEx
    {
        public static T NotNull<T>(this T? t) where T : class =>
            t ?? throw new ArgumentNullException(nameof(t));
    }
}
