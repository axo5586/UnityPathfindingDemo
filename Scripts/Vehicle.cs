using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Vehicle : MonoBehaviour
{
    // Vehicle Fields
    public Vector3 vehiclePosition;
    public Vector3 direction;
    public Vector3 velocity;
    public Vector3 acceleration;

    //floats
    public float maxSpeed;
    public float maxForce;
    public float mass;
    public float radius;

    //scene manager
    public SceneManager sceneManager;
    public GameObject scene;

    //toggling debug lines boolean
    public bool debugBool;

    //vectors for future position
    public Vector3 ahead;

    //weights for flocking
    public float alignmentWeight;
    public float cohesionWeight;
    public float seperationWeight;
    public float boundingWeight;
    public float avoidanceWeight;

    //all forces for flocking, used for debugging
    public Vector3 seperationForce;
    public Vector3 cohesionForce;
    public Vector3 alignmentForce;
    public Vector3 boundingForce;

    


    // Use this for initialization
    public virtual void Start ()
    {
        vehiclePosition = transform.position;
        scene = GameObject.Find("SceneManager");
        sceneManager = scene.GetComponent<SceneManager>();
        
    }
	
	// Update is called once per frame
	void Update ()
    {
        //Vehicle movement
        CalcSteeringForces();
        UpdatePosition();
        SetTransform();

        //calculating forward vector
        ahead = transform.position + Vector3.Normalize(velocity) * 1.5f;
    }
    //update position
    public void UpdatePosition()
    {
        velocity += acceleration;
        Vector3.ClampMagnitude(velocity, maxSpeed);
        vehiclePosition += velocity;
        direction = velocity.normalized;
        acceleration = Vector3.zero;
    }

    //sets transform
    public void SetTransform()
    {
        transform.position += transform.forward;
        transform.position = vehiclePosition;
    }

    //applies force given
    public void ApplyForce(Vector3 force)
    {
        // if F= MA, A = F/M
        acceleration += force / mass;
    }

    //calc forces method to be passed on and overriden
    public abstract void CalcSteeringForces();

    //seek a point
    public Vector3 Seek(Vector3 targetPosition)
    {
        //1. get desired velocity by target pos - autonomous agent pos 
        Vector3 desiredVelocity = targetPosition - vehiclePosition;

        //2. normalize desired velocity and then multiply it by maxSpeed
        desiredVelocity = desiredVelocity.normalized * maxSpeed;

        //3. steering force = scaled desired velocity - current velocity 
        Vector3 seekForce = desiredVelocity - velocity;

        //4. return steering force 
        return seekForce;
    }

    //flee from a point
    public Vector3 Flee(Vector3 targetPosition)
    {
        //1. get desired velocity by target pos - autonomous agent pos 
        Vector3 desiredVelocity = targetPosition - vehiclePosition;

        //2. negate it and flip the direction of desired velocity
        Vector3 negatedDesired = -desiredVelocity;

        //3. normalize it and scale it by maxSpeed
        negatedDesired = negatedDesired.normalized * maxSpeed;

        //4. calculate the fleeing force
        Vector3 fleeForce = negatedDesired - velocity;

        //5. return fleeing force
        return fleeForce;
    }

    //arriving at a point, slow down
    public Vector3 Arrival(Vector3 targetPosition)
    {
        //1. get desired velocity by target pos - autonomous agent pos 
        Vector3 desiredVelocity = targetPosition - vehiclePosition;

        //if distance of object to target is <= 2, slow it down based on distance; otherwise proceed normally and
        //clamp it by normalizing and then multiplying by max speed
        if (Vector3.Magnitude(targetPosition - vehiclePosition) <= 2) 
        {
            desiredVelocity = desiredVelocity.normalized * maxSpeed * (Vector3.Magnitude(targetPosition - vehiclePosition) / 2);
        }
        else if (Vector3.Magnitude(targetPosition - vehiclePosition) > 2)
        {
            desiredVelocity = desiredVelocity.normalized * maxSpeed;
        }

        //calculate arrival force
        Vector3 arrivalForce = desiredVelocity - velocity;

        //returns arrival force
        return arrivalForce;
    }

    //****************************************** METHODS FOR FLOCKING ***********************************************//

    //aligning force
    public Vector3 Alignment()
    {
        //aligning force assigned to 0
        alignmentForce = Vector3.zero;

        //alignment force to the flock is equal to the flock force minus this bird's current velocity
        alignmentForce = sceneManager.flockDirection;
        alignmentForce = alignmentForce.normalized * maxSpeed;

        //weights and returns alignment force
        alignmentForce *= alignmentWeight;
        return alignmentForce;
    }

    //cohesive force for flock
    public Vector3 Cohesion()
    {
        //cohesion force assigned to 0
        cohesionForce = Vector3.zero;

        //cohesion force is the vector that's equal to the average bird position minus vehicle's own position
        cohesionForce = sceneManager.centroid - transform.position;
        cohesionForce = cohesionForce.normalized * maxSpeed;

        //weights and returns cohesion force
        cohesionForce *= cohesionWeight;
        return cohesionForce;
    }

    //seperation force
    public Vector3 Seperation()
    {
        seperationForce = Vector3.zero;

        for (int i = 0; i < sceneManager.flockObjects.Length; i++)
        {
            if (Vector3.Magnitude(sceneManager.flockObjects[i].transform.position - transform.position) <= 2f)
            {
                seperationForce -= sceneManager.flockObjects[i].transform.position - transform.position;
            }
        }

        seperationForce = seperationForce.normalized * maxSpeed * .2f;

        //weights and returns seperation force
        seperationForce *= seperationWeight;
        return seperationForce;
    }

    //********************************************Resisted by the area of resistance***********************************************//

    public Vector3 ResistingField(ResistanceField resistField, Vector3 aheadPos)
    {
        Vector3 resistDir = Vector3.zero;
        

        if (resistField.GetResistDirection(aheadPos) != Vector3.zero)
        {
            resistDir = Vector3.ClampMagnitude(resistField.GetResistDirection(aheadPos), maxSpeed);
        }

        //calculate the desired velocity
        resistDir = resistDir - transform.position;

        //normalize and then max speed it
        resistDir = resistDir.normalized * maxSpeed;

        //desired velocity calculated
        Debug.Log(resistDir);
        return resistDir;
    }

    //********************************************Follow the vector field vectors***********************************************//

    public Vector3 FollowField(FlowField flowField, Vector3 aheadPos)
    {
        Vector3 flowDirection = Vector3.zero;

        flowDirection = Vector3.ClampMagnitude(flowField.GetFlowDirection(aheadPos), maxSpeed);

        //calculate the desired velocity
        flowDirection = flowDirection - transform.position;

        //normalize and then max speed it
        flowDirection = flowDirection.normalized * maxSpeed;

        //desired velocity calculated
        Debug.Log(flowDirection);
        return flowDirection;
    }


    //******************************************* BOUNDING FORCES ***********************************************//
    //force to make flock objects stay in bounds
    public Vector3 BoundingForce()
    {
        boundingForce = Vector3.zero;

        //checking if birds are out of bounds
        for (int i = 0; i < sceneManager.flockObjects.Length; i++)
        {
            if (transform.position.x <= 0 || transform.position.x >= sceneManager.floor.size.x)
            {
                boundingForce = Seek(new Vector3(sceneManager.floor.size.x / 2, 22, sceneManager.floor.size.z / 2));
            }

            else if (transform.position.y <= 20 || transform.position.y >= 25)
            {
                boundingForce = Seek(new Vector3(sceneManager.floor.size.x / 2, 22, sceneManager.floor.size.z / 2));
            }

            else if (transform.position.z <= 0 || transform.position.z >= sceneManager.floor.size.z)
            {
                boundingForce = Seek(new Vector3(sceneManager.floor.size.x / 2, 22, sceneManager.floor.size.z / 2));
            }
        }

        boundingForce *= boundingWeight;
        return boundingForce;
    }
}
