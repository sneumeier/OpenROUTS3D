using MainMenuScripts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CreditElement : MonoBehaviour {

    public Text Title;
    public Image Picture;
    public Text License;
    public Text Author;
    public string Source;
    public string Modifications;
    
    public Button SourceButton;
    public Button ModificationsButton;
    
    public CreditPopupWindow PopupWindow;
    
    public void ShowMoreInformation()
    {
        PopupWindowFillFirstLine();
        PopupWindow.SourceContent.text = Source;
        PopupWindow.ModificationsContent.text = Modifications;
        
    }
    
    private void PopupWindowFillFirstLine()
    {
        PopupWindow.gameObject.SetActive(true);
        PopupWindow.Title.text = Title.text;
        PopupWindow.License.text = License.text;
        PopupWindow.Author.text = Author.text;
        
    }
    
    public void ClosePopupWindow()
    {
        PopupWindow.gameObject.SetActive(false);
    }


}
