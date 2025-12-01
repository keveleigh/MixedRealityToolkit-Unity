// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    [Serializable]
    public abstract class BaseThemeItemData<T>
    {
        [field: SerializeField]
        public T Value { get; set; }
    }
}
