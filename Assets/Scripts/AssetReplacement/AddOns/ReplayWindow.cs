using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.AssetReplacement.AddOns
{
    public class ReplayWindow : MonoBehaviour
    {

        #region Properties and Fields
        public InputField savePath;
        public InputField lidarOutputPath;

        public Toggle lidarToggle;
        public Toggle cameraScanToggle;
        public InputField lidarColumns;
        public InputField lidarRows;
        public InputField lidarAngle;
        public InputField lidarDelay;

        public ReplayScript Replay
        {
            get { return AnchorMapping.GetAnchor("ReplayScript").GetComponent<ReplayScript>(); }
        }
        #endregion Properties and Fields
        #region Methods

        public void LoadReplay()
        {
            ReplayScript rc = Replay;
            string path;
            try
            {
                path = (savePath.text);
                savePath.GetComponent<Image>().color = Color.white;
            }
            catch (Exception)
            {
                savePath.GetComponent<Image>().color = Color.red;
                return;
            }

            double delay;
            try
            {
                delay = double.Parse(lidarDelay.text);
                lidarDelay.GetComponent<Image>().color = Color.white;
            }
            catch (Exception)
            {
                lidarDelay.GetComponent<Image>().color = Color.red;
                return;
            }
            rc.lidarDelay = (float)delay;
            rc.lidarDumpPath = lidarOutputPath.text;

            if (lidarToggle.isOn)
            {
                int columns;
                int rows;
                double angle;
                
                try
                {
                    columns = int.Parse(lidarColumns.text);
                    lidarColumns.GetComponent<Image>().color = Color.white;
                }
                catch (Exception)
                {
                    lidarColumns.GetComponent<Image>().color = Color.red;
                    return;
                }
                try
                {
                    rows = int.Parse(lidarRows.text);
                    lidarRows.GetComponent<Image>().color = Color.white;
                }
                catch (Exception)
                {
                    lidarRows.GetComponent<Image>().color = Color.red;
                    return;
                }
                try
                {
                    angle = double.Parse(lidarAngle.text);
                    lidarAngle.GetComponent<Image>().color = Color.white;
                }
                catch (Exception)
                {
                    lidarAngle.GetComponent<Image>().color = Color.red;
                    return;
                }
                

                rc.lidarActive = true;
                rc.lidarAngle = (float)angle;
                
                rc.lidarRows = rows;
                rc.lidarColums = columns;

            }
            else
            {
                rc.lidarActive = false;
            }
            
            rc.cameraScanActive = cameraScanToggle.isOn;
            
            rc.savePath = path;
            rc.started = true;
            this.gameObject.SetActive(false);
        }
        #endregion Methods
    }
}
