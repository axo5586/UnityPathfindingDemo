using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour {

    //Game Objects
    public GameObject pathObject; //path follower prefab
    public GameObject pathFollower;
    public GameObject pathPointObject; // object to be placed on paths vector3
    public GameObject pathBush; // bush along the path points
    public GameObject flockObject;
    public GameObject flowObject;
    public GameObject[] pathBushes; //array of path bush objects
    public GameObject[] flockObjects;
    public GameObject[] flowObjects;
    public CircleCollider2D[] flockCircles;
    public Vehicle[] flockBehaviors;

    //floor
    public GameObject terrain;
    public TerrainData floor;

    //toggling debug line boolean
    public bool debugBool;

    //positions with huge significance for vehicle calculation
    public Vector3 centroid; // average flock position   
    public Vector3 flockDirection; // average flock direction

    //path points
    public List<Vector3> path;

    //materials for debug lines
    public Material pathLines;
    public Material flowfieldDirection;
    public Material resistArea;

    //attributes for spawn timer for bushes
    public float timerUntilDestroy;   //timer until destroying bush
    public bool timerActive;    //bool for destroying bush
    public int numberBushToRespawn;

    //attribute for scene cameras
    public string[] scenes;
    public int sceneNumber;
    

    // Use this for initialization
    void Start ()
    {
        //instantiating arrays
        flockObjects = new GameObject[15];
        flockCircles = new CircleCollider2D[flockObjects.Length];
        flockBehaviors = new Vehicle[flockObjects.Length];
        flowObjects = new GameObject[10];
        pathBushes = new GameObject[path.Count];

        //timer to destroy bush because sheep eats it after 4 seconds
        timerUntilDestroy = 4f;
        timerActive = false;

        //scene number for camera changing description
        sceneNumber = 0;

        //listing strings for camera changing description
        scenes = new string[5];
        scenes[0] = "Overview of Valley";
        scenes[1] = "Following Sheep through Path";
        scenes[2] = "Viewpoint of Flocking Bird";
        scenes[3] = "Closeup of Butterflies(Flowfield)";
        scenes[4] = "Closeup of Waterfall(Resist)";


        //getting terrain data that's linked in inspector
        floor = terrain.GetComponent<TerrainCollider>().terrainData;

        //instantiation of game objects
        //path follower spawns at path[0]
        pathFollower = Instantiate(pathObject, path[0], Quaternion.identity);

        //flocking objects (ducks)
        for (int i = 0; i < flockObjects.Length; i++)
        {
            flockObjects[i] = Instantiate(flockObject, new Vector3(Random.Range(0, floor.size.x), Random.Range(20f, 25f), Random.Range(0, floor.size.z)), Quaternion.identity);
            flockCircles[i] = flockObjects[i].GetComponent<CircleCollider2D>();
            flockBehaviors[i] = flockObjects[i].GetComponent<Vehicle>();
        }

        //flow objects (butterflies)
        for (int i = 0; i < flowObjects.Length; i++)
        {
            flowObjects[i] = Instantiate(flowObject, new Vector3(Random.Range(0, floor.size.x), Random.Range(5f, 10f), Random.Range(0, floor.size.z)), Quaternion.Euler(-90f,0f,0f));
        }

        //path bushes
        for (int i = 0; i < path.Count; i++)
        {
            pathBushes[i] = Instantiate(pathBush, path[i], Quaternion.identity);
        }


    }
	
	// Update is called once per frame
	void Update ()
    {
        //calculating centroid
        calculateCentroid();

        //calculating flock direction
        calculateFlock();

        //timer usage for path following
        if (timerActive == true)
        {
            timerUntilDestroy-= Time.deltaTime;
            if (timerUntilDestroy <= 0)
            {
                timerActive = false;
            }
        }

        //if sheep is on the bush; remove it after 4 seconds to simulate eating
        for (int i = 0; i < pathBushes.Length; i++)
        {
            if (pathBushes[i] != null)
            {
                if (Vector3.Magnitude(pathBushes[i].transform.position - pathFollower.transform.position) >= .06f && Vector3.Magnitude(pathBushes[i].transform.position - pathFollower.transform.position) < .1f)
                {
                       timerActive = true;
                }

                if (Vector3.Magnitude(pathBushes[i].transform.position - pathFollower.transform.position) <= .01f && timerActive == false)
                {
                       numberBushToRespawn = i;
                       timerUntilDestroy = 4f;
                       Destroy(pathBushes[i]);
                }

                
            }

            //spawn the bush back when sheep is 10 units away from it
            if (pathBushes[i] == null && Vector3.Magnitude(path[i] - pathFollower.transform.position) >= 10)
            {
                pathBushes[i] = Instantiate(pathBush, path[i], Quaternion.identity);
            }
        }

        //toggling debug lines
        if (Input.GetKeyDown(KeyCode.D) == true)
        {
            debugBool = !debugBool;
        }

        //toggling scene number
        if (Input.GetKeyDown(KeyCode.C) == true)
        {
            sceneNumber += 1;

            if (sceneNumber == 5)
            {
                sceneNumber = 0;
            }
        }


    }

    //calculates centroid (center of the flock)
    public Vector3 calculateCentroid()
    {
        //readying vector for calculation by setting to zero
        centroid = Vector3.zero;

        //for every object, add their positions together and divide by number of objects to find average position
        for (int i = 0; i < flockObjects.Length; i++)
        {
            centroid += flockObjects[i].transform.position;
        }

        //centroid =  sum of positions / number of objects
        centroid /= flockObjects.Length;

        return centroid;
    }

    //calculates flock direction
    public Vector3 calculateFlock()
    {
        //readying vector for calculation by setting to zero
        flockDirection = Vector3.zero;

        //for every bird, find its ahead position based on its current velocity and add them all together
        for (int i = 0; i < flockObjects.Length; i++)
        {
            flockDirection += flockBehaviors[i].velocity;
        }

        flockDirection /= flockObjects.Length;
        flockDirection = flockDirection.normalized * .2f;

        return flockDirection;
    }


    //debug lines
    private void OnRenderObject()
    {
        if (debugBool == true)
        {
            pathLines.SetPass(0);

            //Draws path lines direction
            for (int i = 0; i < path.Count - 1; i++)
            {
                GL.Begin(GL.LINES);
                GL.Vertex(path[i]);
                if (i + 1 != path.Count + 1)
                {
                    if (path[i + 1] != null)
                    {
                        GL.Vertex(path[i + 1]);
                        GL.End();
                    }
                   
                }

                else if (i == path.Count)
                {
                    GL.Vertex(path[0]);
                    GL.End();
                }
            }
        }
    }

    //debug boxes
    private void OnGUI()
    {
        //debug line explanations
        GUI.Box(new Rect(10, 10, 250, 23), "Press D to show debug lines");
        GUI.Box(new Rect(570, 10, 300, 23), "Click C to change camera views");

        GUI.Box(new Rect(570, 31, 300, 23), "Now showing: " + scenes[sceneNumber]);
        GUI.Box(new Rect(10, 31, 250, 23), "Green: Centroid of Flock");
        GUI.Box(new Rect(10, 52, 250, 23), "Black: Path of the Sheep");
        GUI.Box(new Rect(10, 73, 250, 23), "Blue: Line of flowfield direction");
        GUI.Box(new Rect(10, 94, 250, 23), "Red: Resistance Field");
    }
}
