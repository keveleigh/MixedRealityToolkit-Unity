using TMPro;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

//public class FontBinding : ScriptableObject, IBindable
//{
//    private TMP_Text test;

//    private TextField test2;

//    public IBinding binding { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

//    [field: SerializeField]
//    public string bindingPath { get; set; }

//}


[CreateAssetMenu]
public class FontBinding : ScriptableObject, IBindable
{
    [Header("Bind to multiple properties")]

    [CreateProperty]
    public Vector3 vector3Value;

    [CreateProperty]
    public float sumOfVector3Properties => vector3Value.x + vector3Value.y + vector3Value.z;

    IBinding IBindable.binding { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    string IBindable.bindingPath { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
}
