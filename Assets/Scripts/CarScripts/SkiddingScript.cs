using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CarScripts
{
    /// <summary>
    /// Implementation of drawing skidding marks and playing skidding sound.
    /// </summary>
    public class SkiddingScript : MonoBehaviour
    {
        /// Friction value from when skidding is started
        public float skidAt = 0.5f;
        /// Sound file wich is used for skidding sound
        public GameObject skidSound;
        /// The distance the sound for the skidding is played
        public float soundEmission = 12f;
        /// The width of the mark
        public float markWidth = 0.2f;
        /// Actual friction value
        private float currentFrictionValue;
        private float soundWait;
        /// Skidding marks are drawed (1) or not (0)
        private int skidding;
        private Vector3[] lastPos = new Vector3[2];
        /// Material wich is used for skidding marks
        Material newMat;
        public GameObject marksParent;
        public GameObject soundsParent;
        /// <summary>
        /// Initialisation
        /// </summary>
        void Start()
        {
            newMat = Resources.Load("Materials/Tyre", typeof(Material)) as Material;
        }

        /// <summary>
        /// Desicion if skidding marks has to be drawn
        /// </summary>
        void Update()
        {
            WheelHit hit;
            transform.GetComponent<WheelCollider>().GetGroundHit(out hit);
            currentFrictionValue = Mathf.Abs(hit.sidewaysSlip);
            if (skidAt <= currentFrictionValue && soundWait <= 0)
            {
                GameObject childObject = Instantiate(skidSound, hit.point, Quaternion.identity) as GameObject;
                childObject.transform.SetParent(soundsParent.transform);
                soundWait = 1f;
            }
            soundWait -= Time.deltaTime * soundEmission;
            if (skidAt <= currentFrictionValue)
            {
                SkidMesh();
            }
            else
            {
                skidding = 0;
            }
        }
        /// <summary>
        /// Implementation of generating the mesh for the skidding
        /// </summary>
        void SkidMesh()
        {
            WheelHit hit;
            transform.GetComponent<WheelCollider>().GetGroundHit(out hit);
            GameObject mark = new GameObject("Mark");
            mark.transform.SetParent(marksParent.transform);
            MeshFilter filter = mark.AddComponent<MeshFilter>();
            mark.AddComponent<MeshRenderer>();

            Mesh markMesh = new Mesh();
            Vector3[] vertices = new Vector3[4];

            int[] triangles = { 0, 1, 2, 0, 2, 3, 0, 3, 2, 0, 2, 1 };
            if (skidding == 0)
            {
                vertices[0] = hit.point + Quaternion.Euler(
                    transform.eulerAngles.x,
                    transform.eulerAngles.y,
                    transform.eulerAngles.z) * new Vector3(markWidth, 0.01f, 0f);
                vertices[1] = hit.point + Quaternion.Euler(
                    transform.eulerAngles.x,
                    transform.eulerAngles.y,
                    transform.eulerAngles.z) * new Vector3(-markWidth, 0.01f, 0f);
                vertices[2] = hit.point + Quaternion.Euler(
                    transform.eulerAngles.x,
                    transform.eulerAngles.y,
                    transform.eulerAngles.z) * new Vector3(-markWidth, 0.01f, 0f);
                vertices[3] = hit.point + Quaternion.Euler(
                    transform.eulerAngles.x,
                    transform.eulerAngles.y,
                    transform.eulerAngles.z) * new Vector3(markWidth, 0.01f, 0f);
                lastPos[0] = vertices[2];
                lastPos[1] = vertices[3];
                skidding = 1;
            }
            else
            {
                vertices[1] = lastPos[0];
                vertices[0] = lastPos[1];
                vertices[2] = hit.point + Quaternion.Euler(
                    transform.eulerAngles.x,
                    transform.eulerAngles.y,
                    transform.eulerAngles.z) * new Vector3(-markWidth, 0.01f, 0f);
                vertices[3] = hit.point + Quaternion.Euler(
                    transform.eulerAngles.x,
                    transform.eulerAngles.y,
                    transform.eulerAngles.z) * new Vector3(markWidth, 0.01f, 0f);
                lastPos[0] = vertices[2];
                lastPos[1] = vertices[3];
            }
            markMesh.vertices = vertices;
            markMesh.triangles = triangles;
            Vector2[] uvm = new Vector2[markMesh.vertices.Length];

            for (int i = 0; i < uvm.Length; i++)
            {
                uvm[i] = new Vector2(markMesh.vertices[i].x, markMesh.vertices[i].z);
            }
            mark.GetComponent<MeshRenderer>().material = newMat;
            mark.transform.position = new Vector3(mark.transform.position.x, mark.transform.position.y, mark.transform.position.z);
            markMesh.uv = uvm;
            filter.mesh = markMesh;
        }
    }
}
