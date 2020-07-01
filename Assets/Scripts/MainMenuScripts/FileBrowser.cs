using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenuScripts
{

    public class FileBrowser : MonoBehaviour
    {

        private string currentDir = null;

        public string FileExtension;
        public float OffsetPerElement;
        public bool DirectorySelector;

        public RectTransform ScrollRect;
        public Text FileExtensionText;
        public InputField BrowserField;
        public GameObject Content;

        public GameObject FilePrefab;
        public Button LoadButton;

        public List<FileListElement> Files = new List<FileListElement>();
        public FileListElement SelectedElement;

        public delegate void FileSelectedHandler(object sender, string path);

        public event FileSelectedHandler FileSelected;


        // Use this for initialization
        void Start()
        {
            if (!DirectorySelector)
            {
                FileExtensionText.text = "File Extension : " + FileExtension;
            }
            else {
                FileExtensionText.text = "Select a Directory";
            }
            if (currentDir == null || ! Directory.Exists(currentDir))
            {
                Browse(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }
        }

        public void BrowseInputField()
        {
            if (Directory.Exists(BrowserField.text))
            {
                BrowserField.GetComponent<Image>().color = Color.white;

                Browse(BrowserField.text);
            }
            else
            {

                BrowserField.GetComponent<Image>().color = Color.red;
            }
        }

        public void InputChanged()
        {
            if (Directory.Exists(BrowserField.text))
            {
                BrowserField.GetComponent<Image>().color = Color.white;
            }
            else
            {
                BrowserField.GetComponent<Image>().color = Color.red;
            }
        }

        public void Browse(string path)
        {
            if (!Directory.Exists(path))
            {
                Debug.Log("Path: "+path+" does not exist!");
                return;
            }
            currentDir = path;
            foreach (FileListElement fle in Files)
            {
                fle.SelectionEvent -= Select;
                fle.PickEvent -= Pick;
                GameObject.Destroy(fle.gameObject);
            }
            bool even = true;
            Files.Clear();
            float Offset = 0;
            SelectedElement = null;
            Content.GetComponent<RectTransform>().sizeDelta = new Vector2(ScrollRect.sizeDelta.x, -(Offset - (OffsetPerElement * 2)));

            foreach (string directory in Directory.GetDirectories(path))
            {
                FileListElement fileObject = GameObject.Instantiate(FilePrefab).GetComponent<FileListElement>();
                fileObject.transform.SetParent(Content.transform);
                fileObject.GetComponent<RectTransform>().localPosition = new Vector2(0, Offset);
                fileObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, FilePrefab.GetComponent<RectTransform>().sizeDelta.y);

                fileObject.SetFilename(directory, true, false);
                fileObject.SetEven(even);

                fileObject.SelectionEvent += Select;
                fileObject.PickEvent += Pick;

                Files.Add(fileObject);
                even = !even;
                Offset -= OffsetPerElement;
            }

            if (!DirectorySelector)
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    if (!file.EndsWith(FileExtension))
                    {
                        continue;
                    }
                    FileListElement fileObject = GameObject.Instantiate(FilePrefab).GetComponent<FileListElement>();
                    fileObject.transform.SetParent(Content.transform);
                    fileObject.GetComponent<RectTransform>().localPosition = new Vector2(0, Offset);
                    fileObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, FilePrefab.GetComponent<RectTransform>().sizeDelta.y);

                    fileObject.SetFilename(file, false, false);
                    fileObject.SetEven(even);

                    fileObject.SelectionEvent += Select;
                    fileObject.PickEvent += Pick;

                    Files.Add(fileObject);
                    even = !even;
                    Offset -= OffsetPerElement;
                }
            }
            BrowserField.text = path;
            Content.GetComponent<RectTransform>().sizeDelta = new Vector2(ScrollRect.sizeDelta.x, -(Offset - (OffsetPerElement * 2)));

        }

        public void Cancel()
        {
            if (FileSelected != null)
            {
                FileSelected(this, "");
            }
        }

        public void BrowseUpwards()
        {
            Browse(Directory.GetParent(currentDir).FullName);
        }

        public void Load()
        {
            if (FileSelected != null)
            {
                FileSelected(this, SelectedElement.Filepath);
                CheckStreetwidthExistence();
            }
        }

        public void CheckStreetwidthExistence()
        {
            if (!File.Exists(Settings.mapPath))
            {
                Debug.LogError("SumoUnityConnection - No sumonet found");
                return;
            }
            else
            {
                XElement rootElement = XElement.Load(Settings.mapPath);
                IEnumerable<XElement> sumoStreetWidth =
                    from lane in rootElement.Elements("edge").Elements("lane")
                    where lane.Attribute("width") == null
                    select lane;
                if (sumoStreetWidth.Count() > 0)
                    Debug.Log("At least one streetwidth is not set in .xml-File!");
            }
        }

        private void Pick(object sender, EventArgs e)
        {
            FileListElement file = sender as FileListElement;
            LoadButton.interactable = false || DirectorySelector;
            if (file.Upwards)
            {
                BrowseUpwards();
                return;
            }
            if (file.IsDirectory)
            {
                Browse(file.Filepath);
            }
            else
            {
                FileSelected(this, file.Filepath);
            }
        }

        public void Select(object sender, EventArgs args)
        {
            FileListElement file = sender as FileListElement;
            if (SelectedElement != null)
            {
                SelectedElement.Deselect();
                SelectedElement = null;
            }
            SelectedElement = file;
            LoadButton.interactable = !file.IsDirectory || DirectorySelector;
        }

        public void LoadDirectory()
        {
            if (FileSelected != null)
            {
                if (SelectedElement == null)
                {
                    FileSelected(this, this.currentDir);
                }
                else
                {
                    FileSelected(this, this.SelectedElement.Filepath);
                }
            }
        }

    }
}