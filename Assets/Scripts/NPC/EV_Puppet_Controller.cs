using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class EV_Puppet_Controller : MonoBehaviour
{
    public float Speed, accel, Distance, Gravity, maxfallspeed, animOffset, stopDistance, pushoverrange, pushSpeed = 0.125f, rotationSpeed=3F, lerpTime, doorDis = 1.2f, animDampSpeed=0.1f;
    Vector3 movement, currDirection, lastDirection, animMov=Vector3.zero, currPoint;
    Quaternion fromAngle, toAngle, currAngle, movAngle, toMovAngle;
    float fallSpeed, currentLerpTime = 1f, perc, intMoveX=0, intMoveY=0, refMoveSpeedX=0, refMoveSpeedY=0;
    int currentNode = 0, currSeq = 0;
    bool isPath, hasSubs, isRotate, isLook, isSequence = false, isPursuit = false, hasDoor = false, isPushing = false, active = true, isMoving = false, stopRota=false;
    Transform[] ActualPath;
    Transform rotaAt, lookAt, Location;
    int pathNodes, lastSeq;
    public CharacterController _controller;
    public GameObject Puppet_Mesh;
    Animator Puppet_Anim;
    public LayerMask DoorLay, PlayerLay;
    public bool PushOver=false, isDebuging = false, canDoor=true;
    public IKManager ikManager;

    /// <summary>
    /// Audio Values
    /// </summary>

    AudioClip currAudio;
    AudioClip[] audioSeq;
    public AudioSource Audio;

    // Start is called before the first frame update
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        Audio = GetComponent<AudioSource>();

        Puppet_Anim = Puppet_Mesh.GetComponent<Animator>();
        Puppet_Anim.SetFloat("AnimOffset", animOffset);


    }

    void Start()
    {
        currAngle = transform.rotation;
    }

    public void DeactivateCollision()
    {
        _controller.enabled = false;
        active = false;
    }
    public void EnableCollision()
    {
        _controller.enabled = true;
        active = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            if(canDoor)
            CheckDoor();

            if (PushOver && !isPursuit && !isPath)
                PlayerPush();
            
            if (isRotate)
                ACT_Rotation();
            if (isPath && ((!isRotate)||(isRotate&&perc>=1)))
                ACT_Path();
            if (isPursuit)
                FindPath();

            /*if (!_controller.isGrounded)*/
            ACT_Gravity();

            if (Time.deltaTime != 0)
                _controller.Move((movAngle * movement) * Time.deltaTime);

            transform.rotation = currAngle;
            animMov = ((movAngle * Quaternion.Inverse(currAngle)) * movement);

            
                //Debug.Log("Movement = " + movement + " magnitude = " + movement.magnitude + " AniMovement = " + animMov + " currAngle " + currAngle.eulerAngles + " movAngle = " + movAngle.eulerAngles);

            if (!isMoving)
            movement = Vector3.Lerp(movement, Vector3.zero, 4f * Time.deltaTime);

            isMoving = false;

            /*if (isPath)
                transform.rotation = Quaternion.Lerp(transform.rotation, toAngle, 8f * Time.deltaTime);
            else
                transform.rotation = Quaternion.Lerp(transform.rotation, toAngle, 3f * Time.deltaTime);*/

            /*if (!isPursuit)
            {*/

            //}
        }
    }

    void LateUpdate()
    {
        ACT_Anim();
        if (isSequence)
            Sequence();
    }

    void ACT_Path()
    {
        if (Vector3.Distance(new Vector3(ActualPath[currentNode].position.x, transform.position.y, ActualPath[currentNode].position.z), transform.position) < Distance)
        {
            if (currentNode != pathNodes)
                currentNode += 1;
        }
        toMovAngle = Quaternion.LookRotation(new Vector3(ActualPath[currentNode].position.x, transform.position.y, ActualPath[currentNode].position.z) - transform.position);

        if (movement.magnitude < Speed)
        movement += (Vector3.forward * accel) * Time.deltaTime;
        isMoving = true;

        if (!isRotate)
        {
            movAngle = Quaternion.LookRotation(transform.forward);
        }
        movAngle = Quaternion.Lerp(movAngle, toMovAngle, rotationSpeed * Time.deltaTime);
        if(!isRotate)
        {
            currAngle = movAngle;
        }

        if ((Vector3.Distance(new Vector3(ActualPath[currentNode].position.x, transform.position.y, ActualPath[currentNode].position.z), transform.position) < stopDistance) && currentNode == pathNodes)
        {
            isPath = false;
        }

        /*if (isDebuging)
            Debug.Log("I'm moving, im moving!");*/
    }

    void ACT_Anim()
    {
        intMoveX = Mathf.SmoothDamp(intMoveX, animMov.x, ref refMoveSpeedX, Time.deltaTime * animDampSpeed);
        intMoveY = Mathf.SmoothDamp(intMoveY, animMov.z, ref refMoveSpeedY, Time.deltaTime * animDampSpeed);
        Puppet_Anim.SetFloat("moveX", intMoveX);
        Puppet_Anim.SetFloat("moveY", intMoveY);

    }


    void ACT_Gravity()
    {
        if (_controller.isGrounded)
            fallSpeed = 0;
        else
        {
            fallSpeed -= Gravity;
            if (fallSpeed < maxfallspeed)
                fallSpeed = maxfallspeed;
        }

        movement.y = fallSpeed;
    }


    public void SetPath(Transform[] NewPath, bool stopRotation = true)
    {
        ActualPath = NewPath;
        pathNodes = NewPath.Length-1;
        isPath = true;
        isPursuit = false;
        currentNode = 0;
        if (stopRotation)
            isRotate = false;

        movAngle = Quaternion.LookRotation(new Vector3(ActualPath[currentNode].position.x, transform.position.y, ActualPath[currentNode].position.z) - transform.position);

    }

    void ACT_Rotation()
    {
        //Debug.Log("I'm rotating");
        toAngle = Quaternion.LookRotation((new Vector3(rotaAt.position.x, transform.position.y, rotaAt.position.z) - transform.position));
        currentLerpTime += Time.deltaTime;
        if (currentLerpTime > lerpTime)
        {
            currentLerpTime = lerpTime;
            if (stopRota)
                isRotate = false;
        }

        //lerp!
        perc = currentLerpTime / lerpTime;
        currAngle = Quaternion.Lerp(fromAngle, toAngle, perc);
    }

    public void SetRota(Transform LookAt, bool stopDone = false)
    {
        stopRota = stopDone;
        rotaAt = LookAt;
        fromAngle = transform.rotation;
        if (Vector3.Dot(transform.right, (new Vector3(rotaAt.position.x, transform.position.y, rotaAt.position.z) - transform.position)) > 0)
            Puppet_Anim.SetTrigger("turnRight");
        else
            Puppet_Anim.SetTrigger("turnLeft");

        isRotate = true;
        isPath = false;
        isPursuit = false;
        currentLerpTime = 0f;
        perc = 0f;
    }
    public void StopRota()
    {
        isRotate = false;
    }

    public void SetLookAt(Transform LookAt)
    {
        ikManager.StartLook(LookAt);
    }
    public void StopLookAt()
    {
        ikManager.StopLook();
    }

    public void PausePath()
    {
        isPath = false;
    }
    public void ResumePath()
    {
        isPath = true;
    }

    public void SetPursuit(Transform Here)
    {
        isPath = false;
        isRotate = false;
        isPursuit = true;
        Location = Here;
    }

    public void StopPursuit()
    {
        isPursuit = false;
    }

    void FindPath()
    {
        Vector3 targetVector = Location.transform.position;
    }



    public void PlaySound(AudioClip toPlay, bool _playSubs = false, bool Force = false)
    {
        if (toPlay == null)
            Debug.Log("Puppet " + gameObject.name + " received null clip");
        if (Audio == null)
            Debug.Log("Puppet " + gameObject.name + " received null clip");
        currAudio = toPlay;
        Audio.clip = currAudio;
        Audio.Play();
        if (_playSubs)
            SubtitleEngine.instance.playVoice(currAudio.name, Force);
    }
    public void StopSound()
    {
        Audio.Stop();
    }

    public void PlaySFX(AudioClip toPlay)
    {
        Audio.PlayOneShot(toPlay);
    }

    public void SetSeq(AudioClip[] newSeq, bool _hasSubs = false)
    {
        audioSeq = newSeq;
        lastSeq = newSeq.Length - 1;
        isSequence = true;
        currSeq = 0;
        PlaySound(audioSeq[0], _hasSubs);
        hasSubs = _hasSubs;
    }

    public void AnimTrigger(int Number, bool value = false)
    {
        switch(Number)
        {
            case 3:
                {
                    Puppet_Anim.SetBool("param3", value);
                    break;
                }
            case 2:
                {
                    Puppet_Anim.SetBool("param2", value);
                    break;
                }
            case 1:
                {
                    Puppet_Anim.SetBool("param1", value);
                    break;
                }
            case -1:
                {
                    Puppet_Anim.SetTrigger("param-1");
                    break;
                }
            case -2:
                {
                    Puppet_Anim.SetTrigger("param-2");
                    break;
                }
            case -3:
                {
                    Puppet_Anim.SetTrigger("param-3");
                    break;
                }
            case -4:
                {
                    Puppet_Anim.SetTrigger("param-4");
                    break;
                }
            case -5:
                {
                    Puppet_Anim.SetTrigger("param-5");
                    break;
                }
            case -6:
                {
                    Puppet_Anim.SetTrigger("param-6");
                    break;
                }
            case -7:
                {
                    Puppet_Anim.SetTrigger("param-7");
                    break;
                }
            case -8:
                {
                    Puppet_Anim.SetTrigger("param-8");
                    break;
                }
        }

    }


    void Sequence()
    {
        if (Mathf.Approximately(Audio.time, Audio.clip.length)||Audio.time>Audio.clip.length)
        {
            if (currSeq < lastSeq)
            {
                currSeq += 1;
                Debug.Log("PLaying audio in sequence " + audioSeq[currSeq].name + ", at idx " + currSeq);
                PlaySound(audioSeq[currSeq], hasSubs);
            }
            else
                isSequence = false;
        }

    }

    void CheckDoor()
    {
            Collider[] Interact;
            Interact = Physics.OverlapSphere(transform.position + (movAngle * (Vector3.forward * doorDis)), 0.5f, DoorLay);
            if (Interact.Length != 0)
            {
                Debug.DrawRay(transform.position+(transform.forward* doorDis), Interact[0].transform.position - transform.position);
                Interact[0].transform.gameObject.GetComponent<Object_Door>().ForceOpen(1.5f);
            }
    }

    void PlayerPush()
    {
        Collider[] Interact;
        Interact = Interact = Physics.OverlapCapsule(transform.position+Vector3.up * 4, transform.position, pushoverrange, PlayerLay);

        if (Interact.Length != 0)
        {
            //Debug.DrawRay(transform.position + (transform.forward * 1.5f), Interact[0].transform.position - transform.position);
            movAngle = Quaternion.Inverse(Quaternion.LookRotation(new Vector3(Interact[0].transform.position.x, transform.position.y, Interact[0].transform.position.z) - transform.position).normalized);
            movement += (Vector3.forward * (pushSpeed/2));
            isPushing = true;
            isMoving = true;
        }
        else
            isPushing = false;

        
    }

    public void puppetWarp(Vector3 here, float rotation)
    {
        transform.position = here;
        Vector3 rota = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(rota.x, rota.y + rotation, rota.z);
    }

    [ExecuteInEditMode]
    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.position + (transform.forward * doorDis), Color.blue);
    }




}
