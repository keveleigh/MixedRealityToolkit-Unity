using MixedReality.Toolkit.Data;
using UnityEngine;
using UnityEngine.UIElements;

public class ThemeDefinition : ScriptableObject, IBindable, IDataSourceProvider
{
    IBinding IBindable.binding { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    string IBindable.bindingPath { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    IDataSource IDataSourceProvider.GetDataSource(string dataSourceType)
    {
        throw new System.NotImplementedException();
    }

    string[] IDataSourceProvider.GetDataSourceTypes()
    {
        throw new System.NotImplementedException();
    }
}
