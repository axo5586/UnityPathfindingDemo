using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResistanceField : MonoBehaviour
{
    public float width; // x
    public float length; // z
    public float turnangle;

    public Vector3 directionVector;
    public Vector3[,] field;

    //toggling debug lines boolean
    public bool debugBool;
    public Material resistFieldLine;


    // Use this for initialization
    void Start ()
    {
        width = gameObject.GetComponent<TerrainCollider>().terrainData.size.x / 5;
        length = gameObject.GetComponent<TerrainCollider>().terrainData.size.z / 5;
        turnangle = Random.Range(0f, 360f);
        field = new Vector3[201, 201];
        debugBool = false;


    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.D) == true)
        {
            debugBool = !debugBool;
        }
    }

    public Vector3 GetResistDirection(Vector3 aheadPos)
    {
        Vector3 relativeLoc = Vector3.zero;

        //resistance field generator 
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                Debug.Log(field[x, z]);
                if (Vector3.Magnitude(field[x, z] - aheadPos) <= .1f)
                {
                    relativeLoc = field[x, z];
                }          
                else if (Vector3.Magnitude(field[x, z] - aheadPos) > .1f)
                {
                    return Vector3.zero;
                }
            }
        }
        return relativeLoc;
    }

    //debug lines
    private void OnRenderObject()
    {
        if (debugBool == true)
        {
            resistFieldLine.SetPass(0);

            //Draws resistance field
            GL.Begin(GL.LINES);
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < length; z++)
                {
                    GL.Vertex(new Vector3(x, 5, z));
                    GL.Vertex(Vector3.ClampMagnitude(directionVector, .001f));
                }
            }
            GL.End();
        }
    }
}
