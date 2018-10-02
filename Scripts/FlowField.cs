using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowField : MonoBehaviour
{
    //attributes for generating flow field
    public float width; // x
    public float length; // z
    public Vector3 centroid;

    //toggling debug lines boolean
    public bool debugBool;
    public Material flowfieldLine;

    public Vector3 directionVector;
    public Vector3[,] field;

    // Use this for initialization
    void Start ()
    {
        width = gameObject.GetComponent<TerrainCollider>().terrainData.size.x;
        length = gameObject.GetComponent<TerrainCollider>().terrainData.size.z;
        field = new Vector3[400, 400];
        debugBool = false;

        //centroid is in the middle of the field
        centroid = new Vector3(75, 10, 75);

        // generates centralized flow field, butterflies go towards the trees in the middle of the valley
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                directionVector = new Vector3(x, 0, z);
                directionVector = centroid - directionVector;
                field[x, z] = directionVector.normalized;
            }
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.D) == true)
        {
            debugBool = !debugBool;
        }
    }

    //gets the flow direction
    public Vector3 GetFlowDirection(Vector3 aheadPos)
    {
        Vector3 relativeLoc = Vector3.zero;

        int wid = Mathf.RoundToInt(aheadPos.x);
        int leng = Mathf.RoundToInt(aheadPos.z);


        //finds the location relative to flow field
        Debug.Log("Width " + wid);
        Debug.Log("Length " + leng);
        Debug.Log(field[wid, leng]); 

        if (Vector3.Magnitude(aheadPos - field[wid, leng]) <= 2f)
        {
            relativeLoc = field[wid, leng];
        }
        else if (Vector3.Magnitude(aheadPos - field[wid, leng]) > 2f)
        {
            return Vector3.zero;
        }
        
        return relativeLoc;
    }

    //debug lines
    private void OnRenderObject()
    {
        if (debugBool == true)
        {
            flowfieldLine.SetPass(0);

            //Draws flowfield
            GL.Begin(GL.LINES);
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < length; z++)
                {
                    GL.Vertex(centroid * 2);
                    GL.Vertex(Vector3.ClampMagnitude(directionVector, .001f));
                }
            }              
            GL.End();
        }
    }
    

}
