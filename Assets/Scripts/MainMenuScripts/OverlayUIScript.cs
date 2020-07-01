using UnityEngine;

namespace MainMenuScripts
{
    public class OverlayUIScript : MonoBehaviour
    {
        public GameObject BackgroundPanel;
        
        private bool cursorState;
		private bool mb_current_paused_state = false;

        // Use this for initialization
        void Start()
        {
            Settings.paused = false;
            BackgroundPanel.SetActive(false);
            cursorState = Cursor.visible;
        }

        public void SwitchVisibility()
        {
            Debug.Log("Switched Overlay Menu");
            mb_current_paused_state = Settings.paused;
            if(Settings.paused)
            {
                Cursor.visible = true;
                Time.timeScale = 0;
            }
            else
            {
                // Failsafe for Scenes where the cursor is not hidden by default
                Cursor.visible = cursorState;
                Time.timeScale  = 1;
            }
            BackgroundPanel.SetActive(Settings.paused);
        }

        public void ResumeVisibility()
        {
            Settings.paused = false;
            SwitchVisibility();
        }

        // Update is called once per frame
        void Update()
        {
            if (Settings.paused != mb_current_paused_state)
            {
                SwitchVisibility();
            }
        }
    }
}