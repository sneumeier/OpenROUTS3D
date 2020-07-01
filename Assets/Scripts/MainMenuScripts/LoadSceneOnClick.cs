using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenuScripts
{

    public class LoadSceneOnClick : MonoBehaviour
    {

        public void LoadByIndex(int sceneIndex)
        {     
            SceneManager.LoadSceneAsync(sceneIndex);
        }
    }
}