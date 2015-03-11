/*
 * This code is derived from MyJavaLibrary (http://somelinktomycoollibrary)
 * 
 * If this is an open source Java library, include the proper license and copyright attributions here!
 */

using System;

namespace Supremes.Helper
{
    /// <summary>
    /// Simple validation methods.
    /// </summary>
    /// <remarks>Designed for jsoup internal use</remarks>
    internal static class Validate
    {
        /// <summary>
        /// Validates that the object is not null
        /// </summary>
        /// <param name="obj">object to test</param>
        public static void NotNull(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentException("Object must not be null");
            }
        }

        /// <summary>
        /// Validates that the object is not null
        /// </summary>
        /// <param name="obj">object to test</param>
        /// <param name="msg">message to output if validation fails</param>
        public static void NotNull(object obj, string msg)
        {
            if (obj == null)
            {
                throw new ArgumentException(msg);
            }
        }

        /// <summary>
        /// Validates that the value is true
        /// </summary>
        /// <param name="val">object to test</param>
        public static void IsTrue(bool val)
        {
            if (!val)
            {
                throw new ArgumentException("Must be true");
            }
        }

        /// <summary>
        /// Validates that the value is true
        /// </summary>
        /// <param name="val">object to test</param>
        /// <param name="msg">message to output if validation fails</param>
        public static void IsTrue(bool val, string msg)
        {
            if (!val)
            {
                throw new ArgumentException(msg);
            }
        }

        /// <summary>
        /// Validates that the value is false
        /// </summary>
        /// <param name="val">object to test</param>
        public static void IsFalse(bool val)
        {
            if (val)
            {
                throw new ArgumentException("Must be false");
            }
        }

        /// <summary>
        /// Validates that the value is false
        /// </summary>
        /// <param name="val">object to test</param>
        /// <param name="msg">message to output if validation fails</param>
        public static void IsFalse(bool val, string msg)
        {
            if (val)
            {
                throw new ArgumentException(msg);
            }
        }

        /// <summary>
        /// Validates that the array contains no null elements
        /// </summary>
        /// <param name="objects">the array to test</param>
        public static void NoNullElements(object[] objects)
        {
            NoNullElements(objects, "Array must not contain any null objects");
        }

        /// <summary>
        /// Validates that the array contains no null elements
        /// </summary>
        /// <param name="objects">the array to test</param>
        /// <param name="msg">message to output if validation fails</param>
        public static void NoNullElements(object[] objects, string msg)
        {
            foreach (object obj in objects)
            {
                if (obj == null)
                {
                    throw new ArgumentException(msg);
                }
            }
        }

        /// <summary>
        /// Validates that the string is not empty
        /// </summary>
        /// <param name="string">the string to test</param>
        public static void NotEmpty(string @string)
        {
			if (string.IsNullOrEmpty(@string)) {
				throw new ArgumentException("String must not be empty");
			}
        }

        /// <summary>
        /// Validates that the string is not empty
        /// </summary>
        /// <param name="string">the string to test</param>
        /// <param name="msg">message to output if validation fails</param>
        public static void NotEmpty(string @string, string msg)
        {
			if (string.IsNullOrEmpty(@string)) {
				throw new ArgumentException(msg);
			}
        }

        /// <summary>
        /// Cause a failure.
        /// </summary>
        /// <param name="msg">message to output.</param>
        public static void Fail(string msg)
        {
            throw new ArgumentException(msg);
        }
    }
}
