using System.Collections.Generic;
using UnityEngine;


    /// <summary>
    /// Implementation of a testbed for transforming 2D coordinates into a 3D
    /// body
    /// </summary>
public class TesterCube3D : MonoBehaviour
{

    private static readonly float HEIGHT = 100.0f;
    
    void Start()
    {
        Vector3[] vertices3DWallsInputWC = new Vector3[] {
            new Vector3(0, 0, 100),
            new Vector3(100, 0, 100),
            new Vector3(100, 0, 0),
            new Vector3(0, 0, 0)
        };

        //Debug.Log("area of polygon = " + this.calculatePolygonArea(new List<Vector3>(vertices3DWallsInputWC)));
        bool orientation = this.calculatePolygonOrientation(new List<Vector3>(vertices3DWallsInputWC));
        Debug.Log("orientation of polygon = " + orientation);

        //
        //List<Vector3> listPolygon = new List<Vector3>();
        List<Vector3> listPolygon = new List<Vector3>(vertices3DWallsInputWC);
        //Debug.Log("polygon count = " + listPolygon.Count);
        listPolygon.Add(vertices3DWallsInputWC[0]); // only needed for calculation of bary centre

        // bary centre of base area of cube
        Vector3 baryCentre = this.calculateCentrePosition(listPolygon);

        foreach (Vector3 v3 in vertices3DWallsInputWC)
        {
            Debug.Log(v3);
        }

        // reversing order of polygon coordinates (SUMO delivers data in the wrong direction for vertices and trinangles
        if (!orientation)
        {

            List<Vector3> listPolygonrev = new List<Vector3>();
            //foreach (Vector3 v3 in listVertices3D)
            for (int i = vertices3DWallsInputWC.Length; i > 0; i--)
            {
                //listVertices3Drev.Add(v3);
                //listPolygonrev.AddRange(listPolygon.GetRange(i - 1, 1));
                listPolygonrev.Add(vertices3DWallsInputWC[i-1]);
            }

            //listPolygon = listPolygonrev;
            vertices3DWallsInputWC = listPolygonrev.ToArray();
        }

        foreach (Vector3 v3 in vertices3DWallsInputWC)
        {
            Debug.Log(v3);
        }

        /*
         * 
         */

        Vector3[] vertices3DWallOutputWC;

        vertices3DWallOutputWC = new Vector3[vertices3DWallsInputWC.Length * 2 + 1]; // <-- walls with ceiling

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < vertices3DWallsInputWC.Length; j++)
            {

                if (i == 0)
                {
                    vertices3DWallOutputWC[j] = vertices3DWallsInputWC[j];
                }
                else
                {
                    vertices3DWallOutputWC[j + vertices3DWallsInputWC.Length] = vertices3DWallsInputWC[j];

                    vertices3DWallOutputWC[j + vertices3DWallsInputWC.Length] += new Vector3(0, HEIGHT, 0); // given the wall some height
                    //vertices3DWallOutputWC[j + vertices3DWallsInputWC.Length] += new Vector3(0, Random.Range(5.0f, 15.0f), 0); // given the wall some height
                }
            }
        }

        // adding bary centre
        vertices3DWallOutputWC[vertices3DWallOutputWC.Length - 1] = baryCentre;

        // setting trianlges
        int[] trianglesWallWC;

        //
        List<int> listTriangles = new List<int>();

        // adding walls
        int one = 0;
        int two = vertices3DWallsInputWC.Length;
        int three = two + 1;
        int four = 0;
        int five = three;
        int six = one + 1;
        
        //
        for (int polygonLenght = 0; polygonLenght < vertices3DWallsInputWC.Length; polygonLenght++)
        {
            if(polygonLenght == vertices3DWallsInputWC.Length - 1)
            {
                three = vertices3DWallsInputWC.Length;
                five = vertices3DWallsInputWC.Length;
                six = 0;
            }

            listTriangles.Add(one);
            listTriangles.Add(two);
            listTriangles.Add(three);
            listTriangles.Add(four);
            listTriangles.Add(five);
            listTriangles.Add(six);
            
            one++;
            two++;
            three++;
            four++;
            five++;
            six++;

        }

        // adding ceiling
        int temp = vertices3DWallsInputWC.Length;
        int BARYCENTRE = vertices3DWallOutputWC.Length - 1;
        //
        //for (int polygonLenght = 0; polygonLenght < vertices3DWallsInputWC.Length; polygonLenght++)
        for (int polygonLenght = 0; polygonLenght < vertices3DWallsInputWC.Length - 1; polygonLenght++)
        {

            listTriangles.Add(temp);
            listTriangles.Add(BARYCENTRE);
            if (polygonLenght == vertices3DWallsInputWC.Length - 1)
            {
                listTriangles.Add(vertices3DWallsInputWC.Length);
            }
            else
            {
                listTriangles.Add(temp + 1);
            }

            temp++;

        }

        // adding last indices for ceiling of body
        listTriangles.Add(temp);
        listTriangles.Add(BARYCENTRE);
        listTriangles.Add(vertices3DWallsInputWC.Length);
        
        trianglesWallWC = listTriangles.ToArray();

        // Create the mesh
        Mesh msh = new Mesh();

        msh.vertices = vertices3DWallOutputWC;
        msh.triangles = trianglesWallWC;

        msh.RecalculateNormals();
        msh.RecalculateBounds();

        //msh.colors32 = colors;

        // Set up game object with mesh;
        gameObject.AddComponent(typeof(MeshRenderer));

        Material material = new Material(Shader.Find("Standard"))
        {
            name = "Automatic Material",
            //color = new Color(polygon.Color.r / 255, polygon.Color.g / 255, polygon.Color.b / 255, 1)
            //color = Color.grey
        };
        Renderer renderer = gameObject.GetComponent<Renderer>();
        renderer.material = material;

        MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
    }


    /*
     * calculating centre of gravity of any given polygon (provided it is 3D)
     * (works ONLY with non-self-intersecting closed polygon defined by n vertices)
     * 
     * according: https://en.wikipedia.org/wiki/Centroid#Centroid_of_a_polygon
     */
    private Vector3 calculateCentrePosition(List<Vector3> listPolygon)
    {

        Vector3 centrePosition = new Vector3(0, 0, 0);
        float xCentre = 0.0f;
        //float yCentre = 0.0f;
        float zCentre = 0.0f;

        if (listPolygon == null || listPolygon.Count < 3)
        {
            return centrePosition;
        }

        float area = calculatePolygonArea(listPolygon);

        for (int i = 0; i + 1 < listPolygon.Count; i++)
        {
            Vector3 vector = listPolygon[i];
            Vector3 vectorNext = listPolygon[i + 1];

            //xCentre += (vector.x + vectorNext.x) * (vector.x * vectorNext.y - vectorNext.x * vector.y);
            //yCentre += (vector.y + vectorNext.y) * (vector.x * vectorNext.y - vectorNext.x * vector.y);
            xCentre += (vector.x + vectorNext.x) * (vector.x * vectorNext.z - vectorNext.x * vector.z);
            zCentre += (vector.z + vectorNext.z) * (vector.x * vectorNext.z - vectorNext.x * vector.z);
        }

        //
        xCentre = xCentre / (6.0f * area);
        //yCentre = yCentre / (6.0f * area);
        zCentre = zCentre / (6.0f * area);
        //Debug.Break();
        //
        centrePosition.x = Mathf.Abs(xCentre); // Mathf.Abs securing that all positions have absolute (positive) value and buildings are correctly positioned
        //centrePosition.y = Mathf.Abs(yCentre);
        //centrePosition.z = Mathf.Abs(HEIGHT);
        centrePosition.z = Mathf.Abs(zCentre);
        centrePosition.y = Mathf.Abs(HEIGHT);

        //
        return centrePosition;
    }

    /*
     * calculating centre of gravity of any given polygon
     * (works ONLY with non-self-intersecting closed polygon defined by n vertices)
     * 
     * according: https://en.wikipedia.org/wiki/Centroid#Centroid_of_a_polygon
     */
    private float calculatePolygonArea(List<Vector3> listPolygonPoints)
    {
        if (listPolygonPoints == null || listPolygonPoints.Count < 3)
        {
            return 0.0f;
        }

        float area = 0.0f;

        int count = listPolygonPoints.Count;

        for (int i = 0; i + 1 < listPolygonPoints.Count; i++)
        {
            Vector3 vector = listPolygonPoints[i];
            Vector3 vectorNext = listPolygonPoints[i + 1];

            //area += vector.x * vectorNext.y - vectorNext.x * vector.y;
            area += (vector.x * vectorNext.z - vectorNext.x * vector.z);
        }
        Debug.Log("area of polygon = " + area);
        return Mathf.Abs(area / 2.0f);
    }

    /*
     * calculating orientation of polygon coordinates
     * 
     * false = clockwise, area bigger than 0.0
     *         polygon order has to be reversed
     * true  = counter clockwise (needed for creating building bodies of polygons)
     *         area smaller than 0.0
     *         no changes to polygon order
     * 
     * according: https://de.wikipedia.org/wiki/Gau%C3%9Fsche_Trapezformel#Beispiel
     *        or: https://math.stackexchange.com/a/340860
     */
    private bool calculatePolygonOrientation(List<Vector3> listPolygonPoints)
    {

        float area = 0.0f;
        
        int count = listPolygonPoints.Count;

        for (int i = 0; i + 1 < listPolygonPoints.Count; i++)
        {
            Vector3 vector = listPolygonPoints[i];
            Vector3 vectorNext = listPolygonPoints[i + 1];

            area += vector.x * vectorNext.z - vectorNext.x * vector.z;
        }

        return ((area < 0.0f) ? false : true);
        //return ((area < 0.0f) ? true : false);
        //return ((area > 0.0f) ? false : true);
    }

}
