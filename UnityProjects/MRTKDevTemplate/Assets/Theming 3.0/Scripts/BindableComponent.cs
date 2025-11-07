using UnityEngine.UIElements;

public class BindableComponent : IBindable
{
    IBinding IBindable.binding { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    string IBindable.bindingPath { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
}
