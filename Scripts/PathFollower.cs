using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : Vehicle
{
    public int pathCount;

    //using time variables to increment path counting to make sure the sheep stays and looks like it's eating a plant
    public float timerUntilCount;   //timer until path count
    public bool timerActive;    //bool for path counting

    public override void CalcSteeringForces()
    {
        //zero ultimate force vector
        Vector3 ultimateForce = Vector3.zero;

        //add seek force towards path
        ultimateForce += PathFollow();

        //clamp
        Vector3.ClampMagnitude(ultimateForce, maxForce);

        //apply
        ApplyForce(ultimateForce);
    }

    // Use this for initialization
    public override void Start ()
    {
        pathCount = 1;
        timerActive = false;
        timerUntilCount = 5f;
        base.Start();      
    }

    public void Update()
    {
        //timer usage for path following
        if (timerActive == true)
        {
            timerUntilCount -= Time.deltaTime;
            if (timerUntilCount <= 0)
            {
                timerActive = false;
            }
        }

        //Vehicle movement
        CalcSteeringForces();
        UpdatePosition();
        SetTransform();
        PathFollow();
        transform.LookAt(sceneManager.path[pathCount]);

        //calculating forward vector
        ahead = transform.position + Vector3.Normalize(velocity) * 1.5f;
    }

    //follow the path generated in the scene manager 
    public Vector3 PathFollow()
    {  
        Vector3 pathVector = Vector3.zero;
        if (pathCount == 14)
        {
            pathCount = 0;
        }
        if (Vector3.Magnitude(sceneManager.path[pathCount] - transform.position) >= .25f)
        {
            pathVector = Seek(sceneManager.path[pathCount]);
        }
        else if (Vector3.Magnitude(sceneManager.path[pathCount] - transform.position) < .25f)
        {
           //add arrival vector if the distance is small
           pathVector += Arrival(sceneManager.path[pathCount]);

           //if the distance is small and the system is not waiting for the time to pass for sheep to stay, move on with the path following
           if ((Vector3.Magnitude(sceneManager.path[pathCount] - transform.position) < .06f) && (Vector3.Magnitude(sceneManager.path[pathCount] - transform.position) > .01f))
           {
                timerActive = true;
           }
           if ((Vector3.Magnitude(sceneManager.path[pathCount] - transform.position) < .01f) && timerActive == false)
           {
                pathCount = pathCount + 1;
                pathVector = Seek(sceneManager.path[pathCount]);
                timerUntilCount = 5f;
            }
        }
        return pathVector;
    }

}
