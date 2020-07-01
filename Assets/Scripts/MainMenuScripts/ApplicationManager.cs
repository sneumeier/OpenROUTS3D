using UnityEngine;
using System.Collections;

namespace MainMenuScripts
{

    public class ApplicationManager : MonoBehaviour
    {


        public void Quit()
        {
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
