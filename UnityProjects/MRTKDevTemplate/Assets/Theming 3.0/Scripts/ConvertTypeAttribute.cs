// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// Draws the Type name converted from the string contents, if possible.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ConvertTypeAttribute : PropertyAttribute
    {
    }
}
