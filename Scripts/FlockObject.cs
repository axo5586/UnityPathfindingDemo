using UnityEngine;

public class FlockObject : Vehicle {

    public Material centroidLine;

    //terrain
    GameObject terrain;

    public override void CalcSteeringForces()
    {
        Vector3 ultimateForce = Vector3.zero;

        //adding flock forces
        ultimateForce += Alignment();
        ultimateForce += Seperation();
        ultimateForce += Cohesion();

        //if bird is close to waterfall, resist
        if (transform.position.x <= (terrain.GetComponent<TerrainCollider>().terrainData.size.x / 5) + 2 && transform.position.z <= (terrain.GetComponent<TerrainCollider>().terrainData.size.z / 5) + 2)
        {
            ultimateForce -= ResistingField(terrain.GetComponent<ResistanceField>(), ahead) * 2f;
        }
        
        //adding bounding forces
        ultimateForce += BoundingForce();

        //clamps magnitude
        Vector3.ClampMagnitude(ultimateForce, maxForce);

        //applies the ultimate force
        ApplyForce(ultimateForce);

    }

    // Use this for initialization
    public override void Start()
    {
        terrain = GameObject.Find("Terrain");
        base.Start();
	}

    public void Update()
    {
        //Vehicle movement
        CalcSteeringForces();
        UpdatePosition();
        SetTransform();
        transform.LookAt(sceneManager.flockDirection);

        //calculating forward vector
        ahead = transform.position + Vector3.Normalize(velocity) * 1.5f;

        if (Input.GetKeyDown(KeyCode.D) == true)
        {
            debugBool = !debugBool;
        } 
    }

    //debug lines
    private void OnRenderObject()
    {
        if (debugBool == true)
        {
            centroidLine.SetPass(0);
            //Draws flock direction
            GL.Begin(GL.LINES);
            GL.Vertex(transform.position);
            GL.Vertex(sceneManager.centroid);
            GL.End();
        }
    }


}
