using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardIKManager : IKManager
{
    public Transform handPos;

    public override void DoLook()
    {
        base.DoLook();
        Puppet_Anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
        Puppet_Anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
        Puppet_Anim.SetIKPosition(AvatarIKGoal.LeftHand, handPos.position);
        Puppet_Anim.SetIKRotation(AvatarIKGoal.LeftHand, handPos.rotation);
    }
}
