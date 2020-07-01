using MainMenuScripts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

// For xml parsing
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Globalization;

public class CreditWindow : MonoBehaviour {

    private string pathToXmlFile = "Credits.xml";

    private float Offset = 0 ;
    private float OffsetPerElement;
    
    public string PathToCreditsFile;
    
    public CreditElement CreditPrefab;
    public GameObject CreditContent;
    public Text HeadingPrefab;
    public GameObject LegendBar;
    
    public GameObject AuthorsWindow;
    public GameObject AuthorsContent; 
    public Text AuthorsText; 
    private string AuthorsTextString = "";
    private string AuthorsFilePath = "AUTHORS.md";
    
    public List<CreditElement> Models = new List<CreditElement>();
    public List<CreditElement> Sounds = new List<CreditElement>();
    public List<CreditElement> Textures = new List<CreditElement>();

    void Start()
    {
       ParseCreditsXml();
       ShowCredits();
       ReadAuthorsFile();
    }
    
    private void ParseCreditsXml()
    {
        using (XmlReader reader = XmlReader.Create(pathToXmlFile))
		{
			while (reader.Read())
			{
				if (reader.IsStartElement())
				{
					switch (reader.Name)
					{
						case "Model":
                            ParseCreditElement(reader, "Model");
							break;
						case "Sound":
                            ParseCreditElement(reader, "Sound");
							break;
						case "Texture":
                            ParseCreditElement(reader, "Texture");
							break;
					}
				}
			}
        }
    }
    
    private void ParseCreditElement(XmlReader outerReader, string creditType)
    {
        CreditElement NewCreditElement = GameObject.Instantiate(CreditPrefab).GetComponent<CreditElement>();
        XmlReader inner = outerReader.ReadSubtree();
        while(inner.Read()){
            
			if (inner.IsStartElement() == true)
			{
                switch (inner.Name)
                {
                    case "Title":
                        if (inner.Read())
                        {
                            NewCreditElement.Title.text = inner.Value.Trim();
                        }
                        break;
                    case "License":
                        if (inner.Read())
                        {
                            NewCreditElement.License.text = inner.Value.Trim();
                        }
                        break;
                    case "Source":
                        if (inner.Read())
                        {
                            NewCreditElement.Source = inner.Value.Trim();
                        }
                        break;
                    case "Author":
                        if (inner.Read())
                        {
                            NewCreditElement.Author.text = inner.Value.Trim();
                        }
                        break;
                    case "Modifications":
                        if (inner.Read())
                        {
                            NewCreditElement.Modifications = inner.Value.Trim();
                        }
                        break;
                }
			}
        } // End While
        
        switch (creditType)
        {
            case "Model":
                Models.Add(NewCreditElement);
            break;
            case "Sound":
                Sounds.Add(NewCreditElement);
            break;
            case "Texture":
                Textures.Add(NewCreditElement);
            break;
        }
        
    }
    
    public void ShowCredits()
    {
        OffsetPerElement = CreditPrefab.GetComponent<RectTransform>().sizeDelta.y *1.5f;
        Offset  = CreditPrefab.GetComponent<RectTransform>().sizeDelta.y*0.5f;
        
        CreditPrefab.gameObject.SetActive(false);
        
        PrintModelCredits();
        
        PrintSoundCredits();
        
        PrintTextureCredits();
        
        SetPrefabsToInactive();
        
        
        // Make the content of the scroll view long enough to contain all the Elements
        CreditContent.GetComponent<RectTransform>().sizeDelta = new Vector2(CreditContent.GetComponent<RectTransform>().sizeDelta.x, Offset);
    }
    
    void PrintModelCredits()
    {
        PrintCategoryHeading("3D Models");
        
        PrintColumnNaming();
        
        Offset +=OffsetPerElement;
        
        PrintCreditList(Models);
    }
    
    void PrintSoundCredits()
    {
        PrintCategoryHeading("Sounds");
        
        PrintColumnNaming();
        
        Offset += OffsetPerElement;
        
        PrintCreditList(Sounds);
    }
    
    void PrintTextureCredits()
    {
        if (Textures.Count != 0){
            
            PrintCategoryHeading("Textures");
            
            PrintColumnNaming();
            
            Offset +=OffsetPerElement;
            
            PrintCreditList(Textures);
        }
    }
    
    void PrintCategoryHeading(string Category)
    {
        Text Heading = GameObject.Instantiate(HeadingPrefab).GetComponent<Text>();
        Heading.transform.SetParent(CreditContent.transform);
        Heading.GetComponent<RectTransform>().localPosition = new Vector2(HeadingPrefab.GetComponent<RectTransform>().localPosition.x, -Offset);
        Heading.GetComponent<RectTransform>().sizeDelta = new Vector2(HeadingPrefab.GetComponent<RectTransform>().sizeDelta.x, HeadingPrefab.GetComponent<RectTransform>().sizeDelta.y);
        Heading.GetComponent<RectTransform>().localScale = new Vector3(1f,1f,1f);
        Heading.GetComponent<Text>().text = Category;
    }
    
    void PrintColumnNaming()
    {
        GameObject ColumnNaming = GameObject.Instantiate(LegendBar);
        ColumnNaming.transform.SetParent(CreditContent.transform);
        ColumnNaming.GetComponent<RectTransform>().localPosition = new Vector2(LegendBar.GetComponent<RectTransform>().localPosition.x, -(Offset+OffsetPerElement/2));
        ColumnNaming.GetComponent<RectTransform>().sizeDelta = new Vector2(LegendBar.GetComponent<RectTransform>().sizeDelta.x, LegendBar.GetComponent<RectTransform>().sizeDelta.y);
        ColumnNaming.GetComponent<RectTransform>().localScale = new Vector3(1f,1f,1f);
    }
    
    void PrintCreditList(List<CreditElement> CreditListToPrint)
    {
        foreach (CreditElement element in CreditListToPrint)
        {
            element.transform.SetParent(CreditContent.transform);
            element.GetComponent<RectTransform>().localPosition = new Vector2(CreditPrefab.GetComponent<RectTransform>().localPosition.x, -Offset);
            element.GetComponent<RectTransform>().sizeDelta = new Vector2(CreditPrefab.GetComponent<RectTransform>().sizeDelta.x, CreditPrefab.GetComponent<RectTransform>().sizeDelta.y);
            element.GetComponent<RectTransform>().localScale = new Vector3(1f,1f,1f);
            
            Offset +=OffsetPerElement;
        }
    }
    
    void SetPrefabsToInactive()
    {
        HeadingPrefab.GetComponent<Text>().enabled = false;
        LegendBar.SetActive(false);
    }
    
    /* ******* Authors Window *********/
    
    public void DisplayAuthorsWindow()
    {
        AuthorsWindow.gameObject.SetActive(true);
    }
    
    public void CloseAuthorsWindow()
    {
        AuthorsWindow.gameObject.SetActive(false);
    }

    void ReadAuthorsFile()
    {
        StreamReader streamReader = new StreamReader(AuthorsFilePath);
        float TextHeight = 0;

        while(!streamReader.EndOfStream)
        {
           AuthorsText.text += streamReader.ReadLine();
           AuthorsText.text += "\n";
           // Found by trial and error. It's the height of one line.
           // We'll accumulate the height of all lines to calculate the size of the scroll area and textfield
           TextHeight += 31.0f; 
        }
        streamReader.Close();  
    
        // Set the Position and size of the Text containing the Authors.md file
        // and the size of the Scroll view content containing the Text
        AuthorsText.transform.SetParent(AuthorsContent.transform);
        AuthorsContent.GetComponent<RectTransform>().sizeDelta = new Vector2(AuthorsContent.GetComponent<RectTransform>().sizeDelta.x, TextHeight);
        AuthorsText.GetComponent<RectTransform>().sizeDelta = new Vector2(AuthorsText.GetComponent<RectTransform>().sizeDelta.x, TextHeight);
        AuthorsText.GetComponent<RectTransform>().localPosition = new Vector2(AuthorsText.GetComponent<RectTransform>().sizeDelta.x/2, -AuthorsText.GetComponent<RectTransform>().sizeDelta.y/2);
        
    }

}
