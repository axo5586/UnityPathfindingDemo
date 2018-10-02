using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowObject : Vehicle
{
    //terrain
    GameObject terrain;

    public override void CalcSteeringForces()
    {
        //zero ultimate force vector
        Vector3 ultimateForce = Vector3.zero;

        if (!(ahead.x <= 0) && !(ahead.y <= 0) && !(ahead.z <= 0))
        {
            //follow the vector field
            ultimateForce += FollowField(terrain.GetComponent<FlowField>(), ahead);
        }

        //adding bounding force
        ultimateForce += BoundingForce();


        //clamp
        Vector3.ClampMagnitude(ultimateForce, maxForce);

        //apply
        ApplyForce(ultimateForce);
    }

    // Use this for initialization
    public override void Start ()
    {
        terrain = GameObject.Find("Terrain");
        base.Start();
	}
	
	// Update is called once per frame
	public void Update ()
    {
        //Vehicle movement
        CalcSteeringForces();
        UpdatePosition();
        SetTransform();

        //calculating forward vector
        ahead = transform.position + Vector3.Normalize(velocity) * 1f;

    }
}
