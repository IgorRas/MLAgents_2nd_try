using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackCheckpoints : MonoBehaviour
{
    public event EventHandler<CarCheckpointEventArgs> OnCarCorrectCheckpoint;
    public event EventHandler<CarCheckpointEventArgs> OnCarWrongCheckpoint;


    [SerializeField] private List<Transform> carTransformList;

    private List<CheckpointSingle> checkpointSingleList;
    private List<int> nextCheckpointSingleIndexList;

    private void Awake()
    {
        Transform checkpointsTransform = transform.Find("Checkpoints");

        checkpointSingleList = new List<CheckpointSingle>();
        foreach (Transform checkpointSingleTransform in checkpointsTransform)
        {
            CheckpointSingle checkpointSingle = checkpointSingleTransform.GetComponent<CheckpointSingle>();

            checkpointSingle.SetTrackCheckpoints(this);

            checkpointSingleList.Add(checkpointSingle);
        }

        nextCheckpointSingleIndexList = new List<int>();
        foreach (Transform carTransform in carTransformList)
        {
            CarDriverAgent carDriverAgent = carTransform.GetComponent<CarDriverAgent>();
            carDriverAgent.SetTrackCheckpoints(this);
            nextCheckpointSingleIndexList.Add(0);

        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 600;
    }

    public void ResetCheckpoint(Transform carTransform)
    {
        nextCheckpointSingleIndexList[carTransformList.IndexOf(carTransform)] = 0;

    }

    public CheckpointSingle GetNextCheckpoint(Transform carTransform)
    {
        return checkpointSingleList[nextCheckpointSingleIndexList[carTransformList.IndexOf(carTransform)]];
    }


    public void CarThroughCheckpoint(CheckpointSingle checkpointSingle, Transform carTransform)
    {
        int nextCheckpointSingleIndex = nextCheckpointSingleIndexList[carTransformList.IndexOf(carTransform)];

        if (checkpointSingleList.IndexOf(checkpointSingle) == nextCheckpointSingleIndex)
        {
            // Correct checkpoint
            //Debug.Log("Correct");
            CheckpointSingle correctCheckpointSingle = checkpointSingleList[nextCheckpointSingleIndex];
            //correctCheckpointSingle.Hide();

            nextCheckpointSingleIndexList[carTransformList.IndexOf(carTransform)]
                = (nextCheckpointSingleIndex + 1) % checkpointSingleList.Count;
            OnCarCorrectCheckpoint?.Invoke(this, new CarCheckpointEventArgs(carTransform));
        }
        else
        {
            // Wrong checkpoint
            Debug.Log("Wrong");
            OnCarWrongCheckpoint?.Invoke(this, new CarCheckpointEventArgs(carTransform));

            CheckpointSingle correctCheckpointSingle = checkpointSingleList[nextCheckpointSingleIndex];
            //correctCheckpointSingle.Show();
        }
    }

    public class CarCheckpointEventArgs : EventArgs
    {
        public Transform carTransform { get; }
        //public static readonly CarCheckpointEventArgs Empty = new EventArgs();

        public CarCheckpointEventArgs(Transform carTransformer)
        {
            carTransform = carTransformer;
        }


        public CarCheckpointEventArgs() { }

    }
}


