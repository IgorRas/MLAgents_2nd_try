using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CarDriverAgent : Agent
{
    /*
        Do zrobienia:
        -dopracowanie rewardów
            -może zmiany w wielkości nagród
        -może więcej obserwacji
    */

    private CarDriver carDriver;
    private float timer = 0f;

    private TrackCheckpoints trackCheckpoints;

    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform spawnPosition;
    public GameObject objectToSpawn;

    private void Awake()
    {
        carDriver = GetComponent<CarDriver>();

    }

    private void Start()
    {
        trackCheckpoints.OnCarCorrectCheckpoint += TrackCheckpoints_OnCarCorrectCheckpoint;
        trackCheckpoints.OnCarWrongCheckpoint += TrackCheckpoints_OnCarWrongCheckpoint;
        //Instantiate(objectToSpawn, transform.position, transform.rotation);
    }
    /*
    private void FixedUpdate()
    {
        AddReward(-0.01f);
        //Debug.Log(trackCheckpoints.GetNextCheckpoint(transform).transform.forward);
    }
    */
    private void TrackCheckpoints_OnCarWrongCheckpoint(object sender, TrackCheckpoints.CarCheckpointEventArgs e)
    {
        if (e.carTransform == transform)
        {
            AddReward(-1f);
        }
    }

    private void TrackCheckpoints_OnCarCorrectCheckpoint(object sender, TrackCheckpoints.CarCheckpointEventArgs e)
    {
        if (e.carTransform == transform)
        {
            AddReward(1f);
            //Debug.Log("hitcorrectcheckpoint");
        }
    }


    public override void OnEpisodeBegin()
    {
        transform.position = spawnPosition.position;
        transform.forward = spawnPosition.forward;
        trackCheckpoints.ResetCheckpoint(transform);
        carDriver.StopCompletely();
    }

    public void DestroyAll()
    {
        string nameofobject = objectToSpawn.name;
        string name = "spheres/" + nameofobject;
        foreach (var item in GameObject.FindGameObjectsWithTag(nameofobject))
        {
            for (var i = item.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(item.transform.GetChild(i).gameObject);
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        /*
        Vector3 checkpointForward = trackCheckpoints.GetNextCheckpoint(transform).transform.forward;
        float directionDot = Vector3.Dot(transform.forward, checkpointForward);
        sensor.AddObservation(directionDot);
        */
        Vector3 nextCheckpoint = trackCheckpoints.GetNextCheckpoint(transform).transform.position;
        float checkpointx = nextCheckpoint.x;
        float checkpointz = nextCheckpoint.z;
        float x = transform.position.x;
        float z = transform.position.z;
        float distanceToNextCheckpoint = (float)Math.Sqrt(Math.Pow(x - checkpointx, 2) + Math.Pow(z - checkpointz, 2));
        //Debug.Log(distanceToNextCheckpoint / 20f);
        sensor.AddObservation(distanceToNextCheckpoint / 20f);
        //Debug.Log(carDriver.GetSpeed() / 70f);
        sensor.AddObservation(carDriver.GetSpeed() / 70f);
        //Debug.Log(Math.Round(transform.localEulerAngles.y, MidpointRounding.ToEven) / 360);
        sensor.AddObservation((float)Math.Round(transform.localEulerAngles.y, MidpointRounding.ToEven) / 360);

        string nameofobject = objectToSpawn.name;
        string name = "spheres/" + nameofobject;
        GameObject newObject = Instantiate(objectToSpawn, transform.position, transform.rotation, GameObject.Find(name).transform);

        float rmin = 0f;
        float rmax = 70f;
        float tmin = 0.5f;
        float tmax = 3f;
        float m = carDriver.GetSpeed();
        float scaledm = ((m - rmin) / (rmax - rmin)) * (tmax - tmin) + tmin;
        newObject.transform.localScale = newObject.transform.localScale * scaledm;


    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float forwardAmount = 0f;
        float turnAmount = 0f;


        switch (actions.DiscreteActions[0])
        {
            case 0: forwardAmount = 0f; break;
            case 1: forwardAmount = +1f; break;
            case 2: forwardAmount = -1f; break;
        }

        switch (actions.DiscreteActions[1])
        {
            case 0: turnAmount = 0f; break;
            case 1: turnAmount = +1f; break;
            case 2: turnAmount = -1f; break;
        }

        carDriver.SetInputs(forwardAmount, turnAmount);
        //AddReward(-0.01f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-1f);
            EndEpisode();
            DestroyAll();
        }
    }

    private void OnCollisionStay(Collision collision)
    {

        if (collision.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            // Hit a Wall
            AddReward(-0.1f);
            //EndEpisode();

            timer += Time.fixedDeltaTime;
            if (timer > 1f)
            {
                timer = timer - 1f;
                //AddReward(-0.5f);
                EndEpisode();

            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Wall>(out Wall wall))
        {
            AddReward(-0.5f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int forwardAction = 0;
        if (Input.GetKey(KeyCode.UpArrow)) forwardAction = 1;
        if (Input.GetKey(KeyCode.DownArrow)) forwardAction = 2;

        int turnAction = 0;
        if (Input.GetKey(KeyCode.RightArrow)) turnAction = 1;
        if (Input.GetKey(KeyCode.LeftArrow)) turnAction = 2;

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = forwardAction;
        discreteActions[1] = turnAction;
    }

    public void SetTrackCheckpoints(TrackCheckpoints trackCheckpoints)
    {
        this.trackCheckpoints = trackCheckpoints;
    }

}
