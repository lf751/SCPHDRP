using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKManager : MonoBehaviour
{
    public float lookSpeed = 0.7f, dampSpeed = 4;
    float toValue, currValue=0;
    Transform lookAt;
    public Transform defLook;
    Vector3 velocity = Vector3.zero, currLook;
    bool isLook;
    [HideInInspector]
    public Animator Puppet_Anim;

    // Update is called once per frame
    private void Start()
    {
        Puppet_Anim = GetComponent<Animator>();
    }

    public void Update()
    {
        currValue = Mathf.Lerp(currValue, toValue,dampSpeed * Time.deltaTime);
        if (currValue <= Mathf.Epsilon)
            currValue = 0;
        if (Mathf.Approximately(currValue, 1))
            currValue = 1;

        if (isLook)
        {
            currLook=Vector3.SmoothDamp(currLook, lookAt.transform.position, ref velocity, lookSpeed);
        }
        else
        {
            currLook = Vector3.SmoothDamp(currLook, defLook.transform.position, ref velocity, lookSpeed);
        }
    }

    private void OnAnimatorIK()
    {
        DoLook();
    }

    public void StartLook(Transform _transform)
    {
        lookAt = _transform;
        toValue = 1;
        isLook = true;
    }

    public void StopLook()
    {
        isLook = false;
        toValue = 0;
    }

    public virtual void DoLook()
    {
        Puppet_Anim.SetLookAtWeight(currValue);
        Puppet_Anim.SetLookAtPosition(currLook);

    }
}
