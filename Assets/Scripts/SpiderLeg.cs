using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderLeg : MonoBehaviour
{
    public int FABRIK_ITT = 3; // Accuracy of inverse kinematics
    public float transitionSpeed = 10f;
    public float stepHeight = 1f;
    public Transform spiderBody;
    List<GameObject> joints = new List<GameObject>();
    public Vector3 targetPos;
    Vector3 targetPosUp;
    Vector3 prevTargetPos = Vector3.zero;
    public bool executingStep = true;  // For alternate stepping

    // Start is called before the first frame update
    void Awake()
    {
        // Putting child joints into list
        for (int i=0; i<this.transform.childCount; i++)
        {
            joints.Add(this.transform.GetChild(i).gameObject);
        }

        // Getting reference to parent transform
        spiderBody = this.transform.parent;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Step cycle (naive approach, can be made better)
        if (Vector3.Distance(this.prevTargetPos, targetPosUp) > 0.1f)
            prevTargetPos = Vector3.Slerp(prevTargetPos, targetPosUp, transitionSpeed * Time.deltaTime);
        else 
        {
            executingStep = false;
            targetPosUp = targetPos;
        }

        // Joint constraint hack (equivalent to applying upward forces)
        joints[1].transform.position += spiderBody.up * spiderBody.localScale.y;

        // FABRIK iterations
        for (int i=0; i<FABRIK_ITT; i++)
            FABRIK(prevTargetPos);
    }

    public void SetTargetPos(Vector3 pos)
    {
        executingStep = true;
        targetPos = pos;
        targetPosUp = pos + spiderBody.up * stepHeight;
    }

    void FABRIK(Vector3 targetPos, bool backwardsOnly = false)
    {
        Vector3 tempPos = targetPos;
        Vector3 basePos = joints[0].transform.position; // For forward propagation

        for(int i=(joints.Count-1); i>=0; i--)
        {
            Vector3 diff = joints[i].transform.position - tempPos;
            float diffMag = Vector3.Magnitude(diff);
            Vector3 heading = diff / diffMag;

            float linkLength = joints[i].transform.GetChild(0).transform.localScale.z;
            joints[i].transform.position = tempPos + heading * linkLength;

            // Look at the targetPos
            joints[i].transform.LookAt(tempPos);

            // position of current joint in the targetPos for previous join
            tempPos = joints[i].transform.position;
        }

        if (backwardsOnly)
            return;

        // Else do the forward prop
        joints[0].transform.position = basePos;
        for(int i=0; i<joints.Count; i++)
        {
            if (i == (joints.Count - 1))
                joints[i].transform.LookAt(targetPos); // Look at targetPos
            else
            {
                // Look at next joint
                joints[i].transform.LookAt(joints[i+1].transform.position);

                // Find the vector between joints
                Vector3 diff = joints[i+1].transform.position - joints[i].transform.position;
                float diffMag = Vector3.Magnitude(diff);
                Vector3 heading = diff / diffMag;

                // Place the joint i+1 on the vector with respective linkLength
                float linkLength = joints[i].transform.GetChild(0).transform.localScale.z;
                joints[i+1].transform.position = joints[i].transform.position + heading * linkLength;
            }
        }
    }
}
