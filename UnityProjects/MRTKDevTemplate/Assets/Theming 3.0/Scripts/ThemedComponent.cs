using UnityEngine;

namespace MixedReality.Toolkit.Themes
{
    public class ThemedComponent : MonoBehaviour
    {
        [SerializeReference, InterfaceSelector]
        [Tooltip("The list of bound theme entries.")]
        private IBinder[] binders;

        private void Awake()
        {
            foreach (IBinder binder in binders)
            {
                binder.Subscribe();
            }
        }
    }
}
