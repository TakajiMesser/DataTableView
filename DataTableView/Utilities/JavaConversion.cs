using System;

namespace Messert.Controls.Droid.Utilities
{
    public static class JavaConversion
    {
        private class JavaHolder : Java.Lang.Object
        {
            public readonly object Instance;

            public JavaHolder(object instance) => Instance = instance;
        }

        /// <summary>
        /// Converts from a Java object to a .NET object, using a wrapper class
        /// </summary>
        /// <typeparam name="TObject">The .NET type to convert to</typeparam>
        /// <param name="value">The Java object to convert, which MUST have been initially created with the complimentary ToJavaObject call</param>
        /// <returns>Returns the converted .NET type</returns>
        public static TObject ToNetObject<TObject>(this Java.Lang.Object value)
        {
            if (value == null)
            {
                return default(TObject);
            }

            if (!(value is JavaHolder))
            {
                throw new InvalidOperationException("Unable to convert to .NET object. Only Java.Lang.Object created with .ToJavaObject() can be converted.");
            }

            TObject returnVal;
            try
            {
                returnVal = (TObject)((JavaHolder)value).Instance;
            }
            finally
            {
                value.Dispose();
            }

            return returnVal;
        }

        /// <summary>
        /// Converts from a.NET object to a Java object, using a wrapper class
        /// </summary>
        /// <typeparam name="TObject">The .NET type to convert</typeparam>
        /// <param name="value">The .NET object to convert</param>
        /// <returns>Returns the converted Java wrapper object</returns>
        public static Java.Lang.Object ToJavaObject<TObject>(this TObject value)
        {
            if (Equals(value, default(TObject)) && !typeof(TObject).IsValueType)
            {
                return null;
            }

            return new JavaHolder(value);
        }

        /// <summary>
        /// Returns a new Java String equivalent of a .NET string.
        /// </summary>
        /// <param name="value">The value of the .NET string.</param>
        /// <returns>A new Java String.</returns>
        public static Java.Lang.String ToJavaString(this string value) => new Java.Lang.String(value);
    }
}