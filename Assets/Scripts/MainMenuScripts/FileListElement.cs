using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenuScripts
{

    public class FileListElement : MonoBehaviour
    {
        public string Filepath;
        public Text Filename;
        public Text ModifiedDate;
        public Text Filesize;

        public Color EvenColor;
        public Color OddColor;
        public Color SelectedColor;
        public Image DirectoryImage;

        public float LastClicked;//Necessary for double click
        public float DoubleClickTime;

        public bool IsEven;
        public bool Upwards = false;//Browse to upper file
        public bool IsDirectory = false;

        public event EventHandler SelectionEvent;
        public event EventHandler PickEvent;

        public void Select()
        {

            if (Time.fixedTime - LastClicked < DoubleClickTime)
            {
                //fire pick event
                if (PickEvent != null)
                {
                    PickEvent(this, EventArgs.Empty);
                }
            }
            else
            {
                //fire selection event
                if (SelectionEvent != null)
                {
                    SelectionEvent(this, EventArgs.Empty);
                }
                Image img = gameObject.GetComponent<Image>();
                img.color = SelectedColor;
                LastClicked = Time.fixedTime;
            }

        }

        public void Deselect()
        {
            LastClicked = 0;
            SetEven(IsEven);
        }

        public void SetEven(bool isEven)
        {
            Image img = gameObject.GetComponent<Image>();
            if (isEven)
            {
                img.color = EvenColor;
            }
            else
            {
                img.color = OddColor;
            }
        }

        public void SetFilename(string filepath, bool isDirectory, bool isUpwards)
        {
            Filepath = filepath;
            if (isUpwards)
            {
                Upwards = true;
                IsDirectory = true;
                Filename.text = "..";
                Filesize.text = "";
                ModifiedDate.text = "";
            }
            else if (isDirectory)
            {
                Filename.text = Path.GetFileName(Filepath);
                Filesize.text = "";
                ModifiedDate.text = "";
                Upwards = false;
                IsDirectory = true;
                DirectoryImage.gameObject.SetActive(true);
            }
            else
            {
                DirectoryImage.gameObject.SetActive(false);
                Filename.text = Path.GetFileName(Filepath);
                FileInfo info = new FileInfo(Filepath);
                long size = info.Length;

                Upwards = false;
                IsDirectory = false;
                if (size > 1000000)
                {
                    Filesize.text = (size / 1000000) + " MB";
                }
                else if (size > 1000)
                {
                    Filesize.text = (size / 1000) + " KB";
                }
                else
                {
                    Filesize.text = (size) + " B";
                }

                ModifiedDate.text = info.LastWriteTime.ToString("dd.MM.yyyy - hh:mm:ss");
            }
        }

    }
}