using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class testscript : MonoBehaviour
{
    [SerializeField]
    Transform _destination;

    NavMeshAgent _navMeshagent;

    // Use this for initialization
    public void Start()
    {
        _navMeshagent = this.GetComponent<NavMeshAgent>();

        if (_navMeshagent == null)
        {
            Debug.LogError("Nav Mesh Agent component not found attached to " + gameObject.name);
        }
        else
        {
            SetDestination();
        }
    }

    public void LateUpdate()
    {
        if (_navMeshagent == null)
        {
            Debug.LogError("Nav Mesh Agent component not found attached to " + gameObject.name);
        }
        else
        {
                SetDestination();
        }


        if (Input.GetButton("Jump"))
        {
            _navMeshagent.speed = 100;
        }
        else
        {
            _navMeshagent.speed = 0;
        }



    }

    private void SetDestination()
    {
        if (_destination != null)
        {
            Vector3 targetVector = _destination.transform.position;
            _navMeshagent.SetDestination(targetVector);
        }
    }


}