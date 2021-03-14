using System;

namespace Smith.MatrixSdk
{
    public static class ObjectExtensions
    {
        public static T NotNull<T>(this T? t) where T : class =>
            t ?? throw new ArgumentNullException(nameof(t));
    }
}
