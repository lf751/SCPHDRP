using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Audio;

public enum bodyPart { Head, Body, Hand, Any };
public enum Ailment { Eyes, Sprint, Health, Speed, Zombie, ZombieCure};

[System.Serializable]
public class EfectsData
{
    public bool permanent;
    public float time = -1;
    public float max = -1;
    public float min = -1;
    public float value = -1;
    public float multiplier = -1;

    public EfectsData() {
        time = -1;
        max = -1;
        min = -1;
        value = -1;
        multiplier = -1;
    }



    public EfectsData(bool _perm, float _time, float _max, float _min, float _value, float _mul)
    {
        permanent = _perm;
        time = _time;
        max = _max;
        min = _min;
        value = _value;
        multiplier = _mul;
    }
}

[System.Serializable]
public class EfectTable
{
    public Ailment affected;
    public EfectsData effect;

    public EfectTable CopyData()
    {
        EfectTable copied = new EfectTable();
        copied.affected = this.affected;
        copied.effect = new EfectsData(this.effect.permanent, this.effect.time, this.effect.max, this.effect.min, this.effect.value, this.effect.multiplier);
        return copied;
    }
}



public class PlayerControl : MonoBehaviour
{
    float InputX, InputY, BlinkMult = 1, currentBlinkMult = 1, RunMult = 1, AsfixTimer, speed, lastBob = 0, headBob, RunningTimer, OpenTimer = 1, lookingForce = 3f, InternalTimer, InternalTimerPain;

    [System.NonSerialized]
    public float BlinkingTimer, CloseTimer;

    private GameObject hand, CinemaLoaded;
    private Transform _groundChecker;

    RaycastHit WallCheck;
    Vector3 holdCam, fallSpeed, movement, HoldPos, OriPos, totalmove, headPos, forceLook;
    Quaternion toAngle;
    private CharacterController _controller;
    bool Grounded = true, isSmoke = false, objectLock = false, fakeBlink, isRunning, isTired = false, isLooking = false, cognitoEffect, onBlink, cameraNextFrame, isCaptive = false;
    [System.NonSerialized]
    public Camera PlayerCam;
    Image eyes, blinkbar, runbar, batbar, overlay, handEquip, eyeIcon;
    RectTransform hand_rect, hud_rect;
    public bool Freeze = false, isGameplay = false, Crouch = false, isEquiping = false, onCam = false, godmode = false, checkObjects = true, IsPuttingOn = false, hasZombie = true, allowZombie = true, allowMove = true;
    public bool noMasterController = false;
    bool playedCough = false, playerCouch = false;
    const float stepMiddle = (Mathf.PI / 2) * 3;

    [Header("Movement")]
    public float GroundDistance = 0.2f;
    public float Gravity = -9.81f, maxfallspeed, Basespeed = 3, crouchspeed = 2, runSpeed = 4, speedMul = 1, forceWalkSpeed = 1.6f;
    [Header("Camera")]
    public float HurtDivisor = 3;
    public float baseAmplitude, bobSpeed, headBobmult = 20, Camplitude, Cspeed, hamplitude, crouchMoveSpeed = 30f;
    [Header("Gimmicks")]
    public float BlinkingTimerBase;
    public float ClosedEyes, BaseBlinkMult = 0.75f, AsfixiaTimer, RunningTimerBase, OpenMulti, CollisionSphere, Health = 100, bloodloss = 0, handLength, handSize, zombieVirusMaxTime, zombieVirusFull;
    [System.NonSerialized]
    public float zombieTimer, zombieVoiceTimer;
    public LayerMask Ground, InteractiveLayer, Collisionables;
    [Header("Object References")]
    public Transform DefHead;
    public Transform CrouchHead;
    public GameObject CameraObj, DeathCol, handPos, CameraContainer, CinemaEffect, SoundPrefab;

    [Header("Audio")]
    public AudioClip[] Conch, CurrentStep, Deaths, Breath, Concrete, Metal, PD, Forest, zombieVoice, coughs;
    public AudioSource sfx, va;
    public AudioMixerSnapshot audNormal, audMask;


    Collider[] Interact;
    Collider InterHold;

    //Iteeemssss
    [System.NonSerialized]
    public GameItem[] equipment = new GameItem[4];
    [System.NonSerialized]
    public List<EfectTable> tempEffects = new List<EfectTable>();

    public List<EfectTable> headEffects = new List<EfectTable>();
    public List<EfectTable> bodyEffects = new List<EfectTable>();
    public List<EfectTable> anyEffects = new List<EfectTable>();
    public List<EfectTable> handEffects = new List<EfectTable>();

    [System.NonSerialized]
    public int headSlot = 0;
    [System.NonSerialized]
    public int bodySlot = 0;
    [System.NonSerialized]
    public int anySlot = 0;
    [System.NonSerialized]
    public int handSlot = 0;

    [System.NonSerialized]
    public int headInv = 0;
    [System.NonSerialized]
    public int bodyInv = 0;
    [System.NonSerialized]
    public int anyInv = 0;
    [System.NonSerialized]
    public int handInv = 0;


    float eyesMin, sprintMin, sprintMax;

    bool protectSmoke;

    //ForcePath Values
    Transform[] Path;
    bool isPath, walkAnim;
    int currentNode;
    Quaternion PathAngle;
    public float NodeDistance;
    //public float fovAdjustSpeed = 1f;
    //public float defFov = 75f;

    private Object_Captive currCaptive;

    public float AsfixiaRead { get { return AsfixTimer; } }

    //Debug Values
    [System.NonSerialized]
    public bool IsNoClip = false;
    bool movementMode = false;

    //[System.NonSerialized]
    //public float AddedFov;
    //float currentAddedFov, targetFovSet, targetFovVel, targetFovCur;


    // Start is called before the first frame update

    void Awake()
    {
        headInv = -1;
        anyInv= -1;
        handInv= -1;
        bodyInv = -1;

        headSlot = -1;
        anyInv = -1;
        handInv = -1;
        bodyInv = -1;

        if (noMasterController)
            isGameplay = true;


        CameraObj = Camera.main.gameObject;
        CameraObj.transform.position = CameraContainer.transform.position;

        //handPos.transform.parent = CameraObj.transform;

        CameraObj.GetComponent<PlayerMouseLook>().enabled = true;
        _controller = GetComponent<CharacterController>();
        _groundChecker = transform.GetChild(0);
        PlayerCam = CameraObj.GetComponent<Camera>();
        speed = Basespeed;
        RunningTimer = RunningTimerBase;
        AsfixTimer = AsfixiaTimer;
        if (!noMasterController)
        {
            eyes = SCP_UI.instance.eyes;
            blinkbar = SCP_UI.instance.blinkBar;
            runbar = SCP_UI.instance.runBar;
            batbar = SCP_UI.instance.navBar;
            hand = SCP_UI.instance.hand;

            overlay = SCP_UI.instance.Overlay;
            handEquip = SCP_UI.instance.handEquip;
            eyeIcon = SCP_UI.instance.eyegraphics;

            hand_rect = hand.GetComponent<RectTransform>();
            hud_rect = SCP_UI.instance.HUD.GetComponent<RectTransform>();
        }

        hand_rect = hand.GetComponent<RectTransform>();
        hud_rect = SCP_UI.instance.HUD.GetComponent<RectTransform>();

        InternalTimer = Mathf.PI / 2;
        InternalTimerPain = Mathf.PI / 2;
    }

    private void Start()
    {
        CameraObj.transform.rotation = Quaternion.identity;
        if (!noMasterController && GameController.ins.worldName != Worlds.dontCare && SaveSystem.instance.playData.worldsCreateds[(int)GameController.ins.worldName])
        {
            CameraObj.GetComponent<PlayerMouseLook>().rotation = new Vector3(0, SaveSystem.instance.playData.worlds[(int)GameController.ins.worldName].angle, 0);
        }
        //handPos.transform.position = CameraObj.transform.position + (CameraObj.transform.forward * 0.5f);

        currentBlinkMult = BaseBlinkMult;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameplay && Time.timeScale == 1f)
        {
            if (IsNoClip == false)
            {
                //AddedFov = 0;
                ACT_Effects();
                ACT_HUD();
                ACT_CaptiveObject();
                if (!noMasterController)
                    ACT_Blinking();
                ACT_Zombie();
                if (checkObjects)
                    ACT_Buttons();
                if (!Freeze)
                {
                    ACT_Move();
                    ACT_Gravity();
                    ACT_Running();
                    ACT_Walk();

                    CollisionDetection();
                    if (isPath)
                        ACT_ForceWalk();
                    _controller.Move(movement * Time.deltaTime);
                }
                ACT_Camera();

                if (Health <= 0)
                    Death(0);
            }
            else
            {
                ACT_SimpleMove();
                ACT_NoClipCamera();
                transform.position += movement * Time.deltaTime;
                if (SCPInput.instance.playerInput.Gameplay.Blink.triggered)
                    movementMode = !movementMode;
            }
        }
    }

    private void LateUpdate()
    {
        if (isGameplay && Time.timeScale == 1f)
        {
            if (!IsNoClip)
            {
                if (isLooking)
                    ACT_ForceLook();

                //Debug.Log("Movement: " + movement.magnitude + ", " + movement.magnitude + ", " + speed + ", " + (movement.magnitude*Time.deltaTime));
                if (isTired && !va.isPlaying && (InputX!=0f || InputY!=0f))
                {
                    va.clip = Breath[Random.Range(0, Breath.Length)];
                    va.Play();
                }

                if(AsfixTimer<0 && !playedCough && !va.isPlaying)
                {
                    va.clip = coughs[Random.Range(0, coughs.Length)];
                    va.Play();
                    playedCough = true;
                }
            }

        }
    }

    public void CaptureObject(Object_Captive newCaptive)
    {
        Freeze = true;
        ForceLook(newCaptive.transform.position, 4f);
        checkObjects = false;
        CameraObj.GetComponent<PlayerMouseLook>().inputActive = false;

        currCaptive = newCaptive;
        isCaptive = true;
    }

    void StopCapture()
    {
        if(isCaptive)
        {
            Freeze = false;
            StopLook();
            checkObjects = true;

            CameraObj.GetComponent<PlayerMouseLook>().inputActive = true;
            currCaptive.EndCaptive();
            isCaptive = false;
            currCaptive = null;
        }
    }

    void ACT_CaptiveObject()
    {
        if(isCaptive && SCPInput.instance.playerInput.Gameplay.InteractNo.triggered)
        {
            StopCapture();
        }
    }


    void ACT_Move()
    {
        Grounded = _controller.isGrounded;//Physics.CheckSphere(_groundChecker.position, GroundDistance, Ground, QueryTriggerInteraction.Ignore);

        InputX = SCPInput.instance.playerInput.Gameplay.Move.ReadValue<Vector2>().x;
        InputY = SCPInput.instance.playerInput.Gameplay.Move.ReadValue<Vector2>().y;
        //Debug.Log("Input x and y: " + InputX + "," + InputY);

        if (allowMove)
        {
            movement = ((transform.right * InputX) + (transform.forward * InputY));
            //Debug.Log("Movement: " + movement + ", " + movement.magnitude);
            Vector3.Normalize(movement);
            //Debug.Log("Movement norm: " + movement + ", " + movement.magnitude);
        }
        else
            movement = Vector3.zero;

        if (SCPInput.instance.playerInput.Gameplay.CrouchTrigger.triggered && !isRunning)
            Crouch = !Crouch;

        if (Health <= 30)
            Crouch = true;

        if (isEquiping)
            Crouch = true;

        //Debug.Log("Held Value " + SCPInput.instance.playerInput.Gameplay.RunHold.ReadValue<float>());
        isRunning = (SCPInput.instance.playerInput.Gameplay.RunHold.ReadValue<float>() > 0 && !Crouch && RunningTimer > 0.2f && (GameController.ins.worldName != Worlds.pocket));

        speed = Basespeed;
        if (Crouch)
            speed = crouchspeed;

        if (isRunning)
            speed = runSpeed;

        if (GameController.ins.worldName==Worlds.pocket)
        {
            speed = crouchspeed+0.5f;
        }

        speed *= speedMul;

        movement *= speed;

    }

    void ACT_SimpleMove()
    {
        InputX = SCPInput.instance.playerInput.Gameplay.Move.ReadValue<Vector2>().x;
        InputY = SCPInput.instance.playerInput.Gameplay.Move.ReadValue<Vector2>().y;

        movement = ((transform.right * InputX) + (transform.forward * InputY));
        Vector3.Normalize(movement);

        if (SCPInput.instance.playerInput.Gameplay.CrouchTrigger.triggered && !isRunning)
            Crouch = !Crouch;

        isRunning = (SCPInput.instance.playerInput.Gameplay.RunHold.ReadValue<float>() > 0 && !Crouch && RunningTimer > 0.2f);

        speed = Basespeed;
        if (Crouch)
            speed = crouchspeed;

        if (isRunning)
            speed = runSpeed+2;

        movement *= speed;

    }

    void ACT_Walk()
    {
        if(lastBob < stepMiddle && InternalTimer > stepMiddle)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 2, Ground, QueryTriggerInteraction.Ignore))
            {
                switch (hit.collider.gameObject.tag)
                {
                    case "Metal":
                        {
                            CurrentStep = Metal;
                            break;
                        }
                    case "PD":
                        {
                            CurrentStep = PD;
                            break;
                        }
                    case "Forest":
                        {
                            CurrentStep = Forest;
                            break;
                        }
                    default:
                        {
                            CurrentStep = Concrete;
                            break;
                        }
                }
                sfx.PlayOneShot(CurrentStep[Random.Range(0, CurrentStep.Length)]);

                GameObject soundSpawn = Instantiate(SoundPrefab, transform.position, Quaternion.identity);
                int sound = 1;
                int dur = 2;
                if (Crouch)
                {
                    sound = 0;
                    dur = 4;
                }
                if (isRunning)
                    sound = 2;

                soundSpawn.GetComponent<WorldSound>().SoundLevel = sound;
                soundSpawn.GetComponent<WorldSound>().Timer = dur;


            }
        }
        lastBob = InternalTimer;
    }

    void ACT_HUD()
    {
        if (!cameraNextFrame)
        {
            if (!noMasterController)
            {
                int blinkPercent = ((int)Mathf.Ceil((BlinkingTimer / (BlinkingTimerBase / 100)) / 5));

                blinkbar.rectTransform.sizeDelta = new Vector2(blinkPercent * 8, 14);

                int runPercent = ((int)Mathf.Floor((RunningTimer / (RunningTimerBase / 100)) / 5));

                runbar.rectTransform.sizeDelta = new Vector2(runPercent * 8, 14);

                if (InterHold != null)
                {
                    //hand.SetActive(true);
                    Vector3 screen = PlayerCam.WorldToScreenPoint(InterHold.transform.position);

                    Vector3 heading = InterHold.transform.position - CameraObj.transform.position;
                    if (Vector3.Dot(CameraObj.transform.forward, heading) < 0)
                    {
                        screen.y = 0f;
                    }

                    hand.transform.position = screen;

                    Vector3 pos = hand_rect.localPosition;

                    Vector3 minPosition = hud_rect.rect.min - hand_rect.rect.min;
                    Vector3 maxPosition = hud_rect.rect.max - hand_rect.rect.max;

                    pos.x = Mathf.Clamp(hand_rect.localPosition.x, minPosition.x, maxPosition.x);
                    pos.y = Mathf.Clamp(hand_rect.localPosition.y, minPosition.y, maxPosition.y);

                    hand_rect.localPosition = pos;
                }
                else
                    hand_rect.localPosition = new Vector3(-1280,-1280);



                if (SCPInput.instance.playerInput.Gameplay.InteractNo.triggered && currCaptive == null)
                {
                    if (!isEquiping)
                    {
                        if (equipment[(int)bodyPart.Hand] != null)
                        {
                            //ACT_UnEquip(bodyPart.Hand);
                            ItemController.instance.Use(handSlot, handInv, false, true);
                            return;
                        }
                        if (equipment[(int)bodyPart.Head] != null)
                        {
                            //ACT_UnEquip(bodyPart.Head);
                            ItemController.instance.Use(headSlot, headInv, false, true);
                            return;
                        }
                    }
                }
            }
        }

    }

    public void CognitoHazard(bool state)
    {
        cognitoEffect = state;
        if (cognitoEffect == false)
        {
            //PlayerCam.fieldOfView = 60;
            //AddedFov = 0;
        }
    }

    void ACT_Camera()
    {
        holdCam = CameraObj.transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0.0f, holdCam.y, 0.0f);

        if (!cameraNextFrame)
        {
            if (Crouch)
            {
                headPos.x = CrouchHead.transform.position.x;
                headPos.z = CrouchHead.transform.position.z;

                //if (Vector3.Distance(headPos, CrouchHead.transform.position) > 0.005f)
                headPos.y = Mathf.Lerp(headPos.y, CrouchHead.transform.position.y, crouchMoveSpeed * Time.deltaTime);
                if (Vector3.Distance(headPos, CrouchHead.transform.position) > 2)
                    headPos.y = CrouchHead.transform.position.y;
            }
            else
            {
                headPos.x = DefHead.transform.position.x;
                headPos.z = DefHead.transform.position.z;

                //if (Vector3.Distance(headPos, DefHead.transform.position) > 0.005f)
                headPos.y = Mathf.Lerp(headPos.y, DefHead.transform.position.y, crouchMoveSpeed * Time.deltaTime);
                /*else
                    headPos.y = DefHead.transform.position.y;*/

                if (Vector3.Distance(headPos, DefHead.transform.position) > 2)
                    headPos.y = DefHead.transform.position.y;
            }

            //Debug.Log("Expected pos = " + headPos + " Internal Timer " + InternalTimer + " headBob " + headBob + " ((101 - Health) / HurtDivisor) = " + ((101 - Health) / HurtDivisor));
            Debug.DrawRay(headPos, transform.forward);

            //if (cognitoEffect)
            //{
            //    AddedFov = (Camplitude * Mathf.Sin(Cspeed * Time.time));
            //    //PlayerCam.fieldOfView = 60 + (Camplitude * Mathf.Sin(Cspeed * Time.time));
            //}
            //else
            //{
            //    AddedFov = 0f;
            //}

            //if (AddedFov != 0f)
            //{
            //    currentAddedFov = AddedFov;
            //    targetFovSet = 1f;
            //}
            //else
            //    targetFovSet = 0f;

            //targetFovCur = Mathf.SmoothDamp(targetFovCur, targetFovSet, ref targetFovVel, fovAdjustSpeed, Mathf.Infinity, Time.deltaTime);

            //PlayerCam.fieldOfView = (Camera.HorizontalToVerticalFieldOfView(defFov + (targetFovCur * currentAddedFov), PlayerCam.aspect));


            if ((((InputX != 0 || InputY != 0)) || walkAnim) && !Freeze)
            {
                InternalTimer += Time.deltaTime * (speed * bobSpeed);
                if (GameController.ins.worldName != Worlds.pocket)
                    InternalTimerPain += (((101 - Health) / HurtDivisor) * (speed / 2)) * Time.deltaTime;
                else
                    InternalTimerPain += (((101 - 50) / HurtDivisor) * (speed / 2)) * Time.deltaTime;

                headBob = baseAmplitude * Mathf.Sin(InternalTimer);
                HoldPos = headPos;

                HoldPos.y = Mathf.MoveTowards(HoldPos.y, headPos.y + headBob, headBobmult * Time.deltaTime);

                /*if (Vector3.Distance(CameraObj.transform.position, headPos) > 2)
                    CameraObj.transform.position = headPos;
                else*/
                CameraObj.transform.position = HoldPos;
                float z = CameraObj.transform.eulerAngles.z;

                z = (z > 180) ? z - 360 : z;

                if (Health < 80)
                {
                   // if (!GameController.instance.isPocket)
                    CameraObj.transform.rotation = Quaternion.Euler(CameraObj.transform.eulerAngles.x, CameraObj.transform.eulerAngles.y, Mathf.MoveTowards(z, 0 + ((hamplitude * ((101 - Health) / HurtDivisor)) * Mathf.Sin(InternalTimerPain)), 10 * Time.deltaTime));
                }
                else if (CameraObj.transform.eulerAngles.z > 0.0001f)
                    CameraObj.transform.rotation = Quaternion.Euler(CameraObj.transform.eulerAngles.x, CameraObj.transform.eulerAngles.y, Mathf.Lerp(z, 0, 10 * Time.deltaTime));

                if (InternalTimer > Mathf.PI * 2)
                {
                    InternalTimer -= Mathf.PI * 2;
                }

                if (InternalTimerPain > Mathf.PI * 2)
                {
                    InternalTimerPain -= Mathf.PI * 2;
                }

            }
            else
            {
                InternalTimer = Mathf.PI / 2;
                
                HoldPos = headPos;
                headBob = Mathf.Lerp(headBob, baseAmplitude, headBobmult * Time.deltaTime);

                HoldPos.y = headPos.y + headBob;
                CameraObj.transform.position = HoldPos;
                //CameraObj.transform.position = new Vector3(headPos.x, Mathf.MoveTowards(CameraObj.transform.position.y, headPos.y+ baseAmplitude, (headBobmult/3) * Time.deltaTime), headPos.z);
                /*if (Vector3.Distance(CameraObj.transform.position, headPos) > 0.005f)
                    
                else
                    CameraObj.transform.position = headPos;*/

                if (Vector3.Distance(CameraObj.transform.position, headPos) > 2)
                    CameraObj.transform.position = headPos;

            }
        }
        else
        {
            cameraNextFrame = false;
            /*if (Vector3.Distance(CameraObj.transform.position, headPos) > 2)
                CameraObj.transform.position = headPos;*/
        }

    }

    void ACT_NoClipCamera()
    {
        holdCam = CameraObj.transform.rotation.eulerAngles;
        if (!movementMode)
            transform.rotation = Quaternion.Euler(holdCam.x, holdCam.y, 0.0f);
        else
            transform.rotation = Quaternion.Euler(0, holdCam.y, 0.0f);
        CameraObj.transform.position = transform.position;
    }


    void ACT_Gravity()
    {
        fallSpeed.y -= Gravity * Time.deltaTime;
        if (fallSpeed.y < maxfallspeed)
            fallSpeed.y = maxfallspeed;

        if (Grounded)
        {
            fallSpeed.y = -_controller.stepOffset / Time.deltaTime;
        }

        movement.y = fallSpeed.y;
    }


    public void ForceLook(Vector3 point, float Force)
    {
        forceLook = point;
        lookingForce = Force;
        isLooking = true;

    }

    public void StopLook()
    {
        isLooking = false;
        CameraObj.GetComponent<PlayerMouseLook>().addedRota = Quaternion.identity;
    }

    void ACT_ForceLook()
    {
        Vector3 Point = (forceLook - CameraObj.transform.position).normalized;
        if(Point!=CameraObj.transform.forward)
            CameraObj.GetComponent<PlayerMouseLook>().addedRota = Quaternion.Slerp(CameraObj.transform.rotation, toAngle, lookingForce * Time.deltaTime);
        toAngle = Quaternion.LookRotation(Point);
    }

   

    void ACT_ForceWalk()
    {
        
        if (currentNode > 1 && Vector3.Distance(new Vector3(Path[currentNode-2].position.x, transform.position.y, Path[currentNode-2].position.z), transform.position) < NodeDistance)
        {
                currentNode -= 1;
        }

        Vector3 Point = new Vector3(Path[currentNode].position.x, transform.position.y, Path[currentNode].position.z) - transform.position;
        movement += (Point.normalized * forceWalkSpeed);

        if (Vector3.Distance(new Vector3(Path[currentNode].position.x, transform.position.y, Path[currentNode].position.z), transform.position) < NodeDistance)
        {
            if (currentNode != Path.Length - 1)
            {
                currentNode += 1;
                walkAnim = true;
            }
            else
            {
                walkAnim = false;
                movement -= (Point.normalized * forceWalkSpeed);
            }

        }
    }

    public void StopWalk()
    {
        isPath = false;
        walkAnim = false;
    }

    public void ForceWalk(Transform [] newPath)
    {
        float lastSqrDis = Mathf.Infinity;
        int chosedIdx=0;
        for(int i = 0; i < newPath.Length; i++)
        {
            if (transform.position.SqrDistance(newPath[i].position) < lastSqrDis)
            {
                lastSqrDis = transform.position.SqrDistance(newPath[i].position);
                chosedIdx = i;
            }
        }
        if(chosedIdx != newPath.Length - 1)
        {
            Vector3 dirToNode = newPath[chosedIdx].position.DirectionTo(newPath[chosedIdx + 1].position);
            Vector3 dirToPlayer = newPath[chosedIdx].position.DirectionTo(transform.position);
            if (Vector3.Dot(dirToNode, dirToPlayer) > 0)
                chosedIdx++;
        }

        currentNode = chosedIdx;
        Path = newPath;
        isPath = true;
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(handPos.transform.position, handSize);
        //Gizmos.DrawRay(CameraObj.transform.position, CameraObj.transform.position + (CameraObj.transform.forward * handLength));
    }

    void ACT_Buttons()
    {
        handPos.transform.position = CameraObj.transform.position + (CameraObj.transform.forward * handLength);
        handPos.transform.rotation = CameraObj.transform.rotation;

        if (!objectLock)
        {
            float lastdistance = float.PositiveInfinity;
            Interact = Physics.OverlapCapsule(CameraObj.transform.position, handPos.transform.position, handSize * (Freeze ? 1.5f:1f), InteractiveLayer);
            if (Interact.Length != 0)
            {
                InterHold = null;
                float currdistance;
                for (int i = 0; i < Interact.Length; i++)
                {
                    //
                    Vector3 closePoint = Interact[i].ClosestPoint(handPos.transform.position);
                    currdistance = Vector3.Distance(handPos.transform.position, closePoint);
                    Debug.DrawRay(closePoint, Vector3.up, Color.black);
                    Debug.DrawRay(headPos, closePoint - headPos, new Color(255, 255, 255, 1.0f));
                    if (currdistance < lastdistance)
                    {
                        if (!Physics.Raycast((headPos - new Vector3(0.0f, 0.2f, 0.0f)), (closePoint - (headPos - new Vector3(0.0f, 0.2f, 0.0f))).normalized, Vector3.Distance(headPos - new Vector3(0.0f, 0.2f, 0.0f), closePoint), Ground, QueryTriggerInteraction.Ignore))
                        {
                            lastdistance = currdistance;
                            InterHold = Interact[i];
                        }
                    }
                }
            }
        }



        if (InterHold != null)
        {
            float distance = Vector3.Distance(handPos.transform.position, InterHold.transform.position);
            float distanceBody = Vector3.Distance(transform.position, InterHold.transform.position);
            if ((InterHold != null) && SCPInput.instance.playerInput.Gameplay.InteractHold.ReadValue<float>() > 0)
            {
                InterHold.GetComponent<Object_Interact>().Hold();
                objectLock = true;
            }
            else
            {
                objectLock = false;
            }

            if (SCPInput.instance.playerInput.Gameplay.InteractYes.triggered)
            {
                InterHold.GetComponent<Object_Interact>().Pressed();
            }

            if ((distanceBody > handLength) && (distance > handLength))
            {
                objectLock = false;
                InterHold = null;
            }
        }
        else
        {
            objectLock = false;
            InterHold = null;
        }


    }

    void ACT_Running()
    {
        if (SCPInput.instance.playerInput.Gameplay.RunHold.ReadValue<float>() < 0.1 && RunningTimer < RunningTimerBase && RunningTimer < sprintMax)
            RunningTimer += (Time.deltaTime) * RunMult;
        if (RunningTimer > sprintMax)
            RunningTimer = sprintMax;

        if (isRunning && (InputX != 0 || InputY != 0) && RunningTimer > sprintMin)
        {
            RunningTimer -= (Time.deltaTime) * RunMult;
        }

        if ((RunningTimer < ((RunningTimerBase / 100) * 20)))
        {
            isTired = true;
        }
        if ((RunningTimer > ((RunningTimerBase / 100) * 35)))
        {
            isTired = false;
        }
    }   

    void ACT_Blinking()
    {
        BlinkingTimer = Mathf.Min(BlinkingTimer, BlinkingTimerBase);

        if (onBlink == false)
            eyes.color = new Color(255, 255, 255, Mathf.Clamp(-((BlinkingTimer-0.064f)*16), 0.0f, 1.0f));
        else
        {
            OpenTimer -= Time.deltaTime * OpenMulti;
            if (OpenTimer < 0)
                onBlink = false;
            eyes.color = new Color(255, 255, 255, OpenTimer);
        }

        if (SCPInput.instance.playerInput.Gameplay.Blink.ReadValue<float>() > 0 && !fakeBlink)
        {
            CloseTimer = ClosedEyes;
            if (BlinkingTimer > 0.125)
                BlinkingTimer = 0.125f;

        }

        if (isSmoke && !protectSmoke)
        {
            eyeIcon.color = Color.red;
            BlinkMult = 4;
            AsfixTimer -= (Time.deltaTime);
            {
                if (AsfixTimer <= 0.0f)
                {
                    Health -= (Time.deltaTime) * 2.5f;
                }
            }

        }
        else
        {
            if (AsfixTimer < 0)
                AsfixTimer = 0;
            AsfixTimer += Time.deltaTime*2;
            if (AsfixTimer>AsfixiaTimer)
            {
                AsfixTimer = AsfixiaTimer;
                playedCough = false;
                eyeIcon.color = Color.white;
                BlinkMult = currentBlinkMult;
            }
            
        }


        BlinkingTimer -= (Time.deltaTime) * BlinkMult;
        if (BlinkingTimer <= 0.0f)
        {
            CloseTimer -= Time.deltaTime;
            if (CloseTimer <= 0.0f)
            {
                BlinkingTimer = BlinkingTimerBase;
                CloseTimer = ClosedEyes;
                fakeBlink = false;
                OpenTimer = 1;
                onBlink = true;
            }
        }
    }

    public void Death(int cause)
    {
        if (isGameplay && !godmode)
        {
            StopCapture();
            GameController.ins.PlayerDeath();
            _controller.enabled = false;
            DeathCol.SetActive(true);
            CameraObj.transform.rotation = Quaternion.Euler(CameraObj.GetComponent<PlayerMouseLook>().rotation);
            CameraObj.transform.parent = DeathCol.transform;
            CameraObj.GetComponent<PlayerMouseLook>().enabled = false;
            isGameplay = false;
            eyes.color = Color.clear;
            Destroy(handPos);

            overlay.sprite = null;
            handEquip.color = Color.clear;
            SCP_UI.instance.SNav.SetActive(false);
            //SCP_UI.instance.radio.StopRadio();



            switch (cause)
            {
                case 3:
                    {
                        sfx.PlayOneShot(Deaths[cause]);
                        GameController.ins.deathmsg = Localization.GetString("deathStrings", "death_106_stone");
                        break;
                    }
                case 1:
                    {
                        sfx.PlayOneShot(Conch[Random.Range(0, Conch.Length)]);
                        break;
                    }
                default:
                    {
                        sfx.PlayOneShot(Deaths[cause]);
                        break;
                    }
            }
        }
    }

    public void FakeDeath(int cause)
    {
        if (isGameplay && !godmode && !noMasterController)
        {
            StopCapture();
            GameController.ins.FakeDeath();
            _controller.enabled = false;
            DeathCol.SetActive(true);
            CameraObj.transform.parent = DeathCol.transform;
            CameraObj.GetComponent<PlayerMouseLook>().enabled = false;
            isGameplay = false;
            eyes.color = Color.clear;
            Destroy(handPos);

            overlay.sprite = null;
            handEquip.color = Color.clear;
            SCP_UI.instance.SNav.SetActive(false);

            switch (cause)
            {
                case 0:
                    {
                        sfx.PlayOneShot(Deaths[0]);
                        break;
                    }

                case 1:
                    {
                        sfx.PlayOneShot(Conch[Random.Range(0, Conch.Length)]);
                        break;
                    }
                case 2:
                    {
                        sfx.PlayOneShot(Deaths[1]);
                        break;
                    }
            }
        }
    }

    public void FakeBlink(float time)
    {
        BlinkingTimer = -1f;
        CloseTimer = time;
        fakeBlink = true;
    }

    public bool IsBlinking()
    {
        if (CloseTimer < ClosedEyes && fakeBlink != true)
            return (true);
        else
            return (false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Smoke"))
            isSmoke = true;
        if (other.gameObject.CompareTag("Camera"))
            onCam = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Smoke"))
            isSmoke = false;
        if (other.gameObject.CompareTag("Camera"))
            onCam = false;
    }

    /*void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (gameObject.CompareTag("Physics"))
        {
            Debug.Log("Pushing");
            Rigidbody body = hit.collider.attachedRigidbody;
            body.AddForce(movement * 70, ForceMode.Force);
        }

    }*/

    void CollisionDetection()
    {
        Collider[] collisions = Physics.OverlapCapsule(transform.position+(_controller.center-Vector3.up*((_controller.height/2))), transform.position + (_controller.center + Vector3.up * ((_controller.height / 2))), _controller.radius+0.2f, Collisionables, QueryTriggerInteraction.Collide);

        if (collisions.Length != 0)
        {
            for (int i = 0; i < collisions.Length; i++)
            {
                if (collisions[i].gameObject.CompareTag("Death"))
                    Death(0);
                if (collisions[i].gameObject.CompareTag("DeathFall"))
                    Death(3);

                if (collisions[i].gameObject.CompareTag("Physics"))
                {
                    Debug.Log("Pushing");
                    Rigidbody body = collisions[i].attachedRigidbody;
                    body.AddForce(movement, ForceMode.Impulse);
                }
            }
        }
    }


    public void ACT_Equip(GameItem item)
    {
        Equipable_Wear itemData = (Equipable_Wear)ItemController.instance.items[item.itemFileName];

        if (equipment[(int)itemData.part] != null)
            ACT_UnEquip(itemData.part);

        switch (itemData.part)
        {
            case bodyPart.Head:
                {
                    equipment[(int)itemData.part] = item;
                    if (itemData.isUnique)
                        SubtitleEngine.instance.playFormatted("playStrings", "play_equip_uni", "itemStrings", itemData.getName());
                    else if (itemData.isFem)
                        SubtitleEngine.instance.playFormatted("playStrings", "play_equip_fem", "itemStrings", itemData.getName());
                    else
                        SubtitleEngine.instance.playFormatted("playStrings", "play_equip_male", "itemStrings", itemData.getName());

                    if(headInv!=-1&&headSlot!=-1)
                        ItemController.instance.equip[headInv][headSlot] = false;
                    headSlot = ItemController.instance.currhover;
                    headInv = ItemController.instance.currInv;
                    ItemController.instance.equip[headInv][headSlot] = true;
                    break;
                }
            case bodyPart.Body:
                {
                    equipment[(int)itemData.part] = item;
                    if (itemData.isUnique)
                        SubtitleEngine.instance.playFormatted("playStrings", "play_equip_uni", "itemStrings", itemData.getName());
                    else if (itemData.isFem)
                        SubtitleEngine.instance.playFormatted("playStrings", "play_equip_fem", "itemStrings", itemData.getName());
                    else
                        SubtitleEngine.instance.playFormatted("playStrings", "play_equip_male", "itemStrings", itemData.getName());

                    if (bodyInv != -1 && bodySlot != -1)
                        ItemController.instance.equip[bodyInv][bodySlot] = false;
                    bodySlot = ItemController.instance.currhover;
                    bodyInv = ItemController.instance.currInv;
                    ItemController.instance.equip[bodyInv][bodySlot] = true;



                    break;
                }
            case bodyPart.Any:
                {
                    equipment[(int)itemData.part] = item;
                    if (itemData.isUnique)
                        SubtitleEngine.instance.playFormatted("playStrings", "play_equip_uni", "itemStrings", itemData.getName());
                    else if (itemData.isFem)
                        SubtitleEngine.instance.playFormatted("playStrings", "play_equip_fem", "itemStrings", itemData.getName());
                    else
                        SubtitleEngine.instance.playFormatted("playStrings", "play_equip_male", "itemStrings", itemData.getName());

                    if (anyInv != -1 && anySlot != -1)
                        ItemController.instance.equip[anyInv][anySlot] = false;
                    anySlot = ItemController.instance.currhover;
                    anyInv = ItemController.instance.currInv;
                    ItemController.instance.equip[anyInv][anySlot] = true;
                    break;
                }
            case bodyPart.Hand:
                {
                    equipment[(int)itemData.part] = item;
                    if (itemData.isUnique)
                        SubtitleEngine.instance.playFormatted("playStrings", "play_equip_uni", "itemStrings", itemData.getName());
                    else if (itemData.isFem)
                        SubtitleEngine.instance.playFormatted("playStrings", "play_equip_fem", "itemStrings", itemData.getName());
                    else
                        SubtitleEngine.instance.playFormatted("playStrings", "play_equip_male", "itemStrings", itemData.getName());

                    if (handInv != -1 && handSlot != -1)
                        ItemController.instance.equip[handInv][handSlot] = false;
                    handSlot = ItemController.instance.currhover;
                    handInv = ItemController.instance.currInv;
                    ItemController.instance.equip[handInv][handSlot] = true;

                    break;
                }
        }

        itemData.OnEquip(ref item);

        if (itemData.hasEffect)
            SetEffect(itemData);
        ReloadEquipment();
    }

    public void SetEffect(Item item)
    {
        if (item is Equipable_Wear)
        {
            ref List<EfectTable> currEffect = ref headEffects;
            switch (((Equipable_Wear)item).part)
            {
                case bodyPart.Any:
                    {
                        currEffect = ref anyEffects;
                        break;
                    }
                case bodyPart.Body:
                    {
                        currEffect = ref bodyEffects;
                        break;
                    }
                case bodyPart.Hand:
                    {
                        currEffect = ref handEffects;
                        break;
                    }
            }

            for (int i = 0; i < currEffect.Count; i++)
            {
                StopEffects(currEffect[i]);
            }

            currEffect.Clear();

            for (int i = 0; i < item.Effects.Count; i++)
            {
                currEffect.Add(item.Effects[i].CopyData());
            }

        }
        else
        {
            //Debug.Log("Copying effects");
            for (int i = 0; i < item.Effects.Count; i++)
            {
                tempEffects.Add(item.Effects[i].CopyData());
            }
        }
    }

    void ACT_Zombie()
    {
        if (hasZombie)
            zombieTimer += Time.deltaTime;
        else
            zombieTimer -= Time.deltaTime * 4;

        zombieTimer = Mathf.Max(zombieTimer, 0);

        float perc = (Mathf.Min(zombieTimer, (zombieVirusFull)) / zombieVirusFull);
        if (hasZombie)
        {
            zombieVoiceTimer -= Time.deltaTime;
            //Debug.Log("Zombie state: Timer: " + zombieTimer + " off " + zombieVirusFull + " " + zombieVirusMaxTime + " perc " + (Mathf.Min(zombieTimer, (zombieVirusFull)) / zombieVirusFull));
            if (zombieVoiceTimer < 0)
            {
                int choice = Random.Range(0, zombieVoice.Length - 1);

                zombieVoiceTimer = zombieVoice[choice].length + Random.Range(10, 30);

                va.PlayOneShot(zombieVoice[choice], perc);
            }
        }
        speedMul *= (1f-(perc*0.5f));
        SCP_UI.instance.infectiongraphic.color = new Color(perc, perc, perc, perc);

        if (zombieTimer > zombieVirusMaxTime)
        {
            if (GlobalValues.LoadType != LoadType.mapless)
            {
                if (GameController.ins.worldName == Worlds.facility)
                {
                    GameController.ins.Death = DeathEvent.zombie008;
                    FakeDeath(0);
                    eyes.color = Color.black;
                }
                else
                {
                    Death(0);
                }
            }
        }
    }

    void ACT_Effects()
    {
        currentBlinkMult = BaseBlinkMult;
        sprintMin = 0;
        sprintMax = RunningTimerBase;
        speedMul = 1;


        Health -= (bloodloss * 0.125f) * Time.deltaTime;
        if (Health >= 100)
            Health = 100;

        processEffects(ref headEffects);
        processEffects(ref anyEffects);
        processEffects(ref handEffects);
        processEffects(ref bodyEffects);

        for (int i = 0; i < 4; i++)
        {
            if (equipment[i] != null && ItemController.instance.items[equipment[i].itemFileName] is Equipable_Elec && equipment[i].valFloat >= 0)
            {
                (equipment[i]).valFloat -= ((Equipable_Elec)ItemController.instance.items[equipment[i].itemFileName]).SpendFactor * Time.deltaTime;
            }
        }

        for (int i = 0; i < tempEffects.Count; i++)
        {
            DoEffects(tempEffects[i]);
            if (!tempEffects[i].effect.permanent)
            {
                tempEffects[i].effect.time -= Time.deltaTime;
            }
            if (!tempEffects[i].effect.permanent && tempEffects[i].effect.time < 0)
            {
                StopEffects(tempEffects[i]);
                tempEffects.RemoveAt(i);
                i--;
            }
        }
        
    }

    void processEffects(ref List<EfectTable> effects)
    {
        for (int i = 0; i < effects.Count; i++)
        {

            DoEffects(effects[i]);
        }
    }

    void DoEffects(EfectTable ail)
    {
        switch (ail.affected)
        {
            case Ailment.Eyes:
                {
                    if (ail.effect.multiplier != -1)
                        currentBlinkMult *= ail.effect.multiplier;
                    if (ail.effect.value != -1)
                    {
                        BlinkingTimer = ail.effect.value;
                        ail.effect.value = -1;
                    }
                    break;
                }
            case Ailment.Speed:
                {
                    if (ail.effect.multiplier != -1)
                        speedMul *= ail.effect.multiplier;
                    break;
                }
            case Ailment.Sprint:
                {
                    if (ail.effect.min != -1)
                        sprintMin = Mathf.Max(sprintMin, ail.effect.min);
                    if (ail.effect.max != -1)
                        sprintMax = Mathf.Min(sprintMax, ail.effect.max);
                    break;
                }
            case Ailment.Health:
                {
                    if (ail.effect.value != -1)
                    {
                        Health += ail.effect.value;
                        ail.effect.value = -1;
                    }
                    if (ail.effect.multiplier != -1)
                    {
                        if (bloodloss == 1)
                            SubtitleEngine.instance.playSub("playStrings", "play_cureblood");
                        if (bloodloss > 1)
                            SubtitleEngine.instance.playSub("playStrings", "play_cureblood2");
                        bloodloss -= ail.effect.multiplier;
                        ail.effect.multiplier = -1;
                    }
                    else
                        SubtitleEngine.instance.playSub("playStrings", "play_cure");
                    break;
                }
            case Ailment.Zombie:
                {
                    Debug.Log("You are dumb. You are sad. Your brain, run by Zombie");
                    hasZombie = true;
                    break;
                }
            case Ailment.ZombieCure:
                {
                    hasZombie = false;
                    break;
                }
        }
    }

    void StopEffects(EfectTable what)
    {
        switch (what.affected)
        {
            case Ailment.Eyes:
                {
                    //currentBlinkMult = BaseBlinkMult;
                    break;
                }
            case Ailment.Sprint:
                {
                    //sprintMin = 0;
                    break;
                }
        }
    }

    public void ACT_UnEquip(bodyPart where)
    {
        Equipable_Wear itemData = (Equipable_Wear)ItemController.instance.items[equipment[(int)where].itemFileName];

        itemData.OnDequip(ref equipment[(int)where]);

        /*if(GameController.ins.isAlive)
            SCP_UI.instance.ItemSFX(itemData.SFX);*/

        if (itemData.hasEffect)
        {
            ref List<EfectTable> currEffect = ref headEffects;

            switch (where)
            {
                case bodyPart.Any:
                    {
                        currEffect = ref anyEffects;
                        break;
                    }
                case bodyPart.Body:
                    {
                        currEffect = ref bodyEffects;
                        break;
                    }
                case bodyPart.Hand:
                    {
                        currEffect = ref handEffects;
                        break;
                    }
            }

            for (int i = 0; i < currEffect.Count; i++)
            {
                StopEffects(currEffect[i]);
            }
            currEffect.Clear();
        }

        /*if (itemData is Document_Equipable)
        {
            Resources.UnloadAsset(itemData.Overlay);
        }*/


        if (where != bodyPart.Hand)
        {
            if(itemData.isUnique)
                SubtitleEngine.instance.playFormatted("playStrings", "play_dequip_uni", "itemStrings", itemData.getName());
            else if (itemData.isFem)
                SubtitleEngine.instance.playFormatted("playStrings", "play_dequip_fem", "itemStrings", itemData.getName());
            else
                SubtitleEngine.instance.playFormatted("playStrings", "play_dequip_male", "itemStrings", itemData.getName());
        }
        switch (where)
        {
            case bodyPart.Head:
                {
                    if (headInv != -1 && headSlot != -1)
                    {
                        ItemController.instance.equip[headInv][headSlot] = false;
                        if (itemData.autoEquip && headInv == 0)
                        {
                            ItemController.instance.slots[headSlot].UnEquip(true);
                        }
                        headInv = -1;
                        headSlot = -1;
                    }
                    break;
                }
            case bodyPart.Body:
                {
                    if (bodyInv != -1 && bodySlot != -1)
                    {
                        ItemController.instance.equip[bodyInv][bodySlot] = false;
                        if (itemData.autoEquip && bodyInv == 0)
                        {
                            ItemController.instance.slots[bodySlot].UnEquip(true);
                        }
                        bodyInv = -1;
                        bodySlot = -1;
                    }
                    break;
                }
            case bodyPart.Hand:
                {
                    if (handInv != -1 && handSlot != -1)
                    {
                        ItemController.instance.equip[handInv][handSlot] = false;
                        if (itemData.autoEquip && handInv == 0)
                        {
                            ItemController.instance.slots[handSlot].UnEquip(true);
                        }
                        handInv = -1;
                        handSlot = -1;
                    }
                    break;
                }
            case bodyPart.Any:
                {
                    if (anyInv != -1 && anySlot != -1)
                    {
                        ItemController.instance.equip[anyInv][anySlot] = false;
                        if (itemData.autoEquip && anyInv == 0)
                        {
                            ItemController.instance.slots[anySlot].UnEquip(true);
                        }
                        anyInv = -1;
                        anySlot = -1;
                    }
                    break;
                }
        }
        equipment[(int)where] = null;
        ReloadEquipment();

    }
        

    void ReloadEquipment()
    {
        if (equipment[(int)bodyPart.Head] != null)
        {
            protectSmoke = ((Equipable_Wear)ItemController.instance.items[equipment[(int)bodyPart.Head].itemFileName]).protectGas;
            if (((Equipable_Wear)ItemController.instance.items[equipment[(int)bodyPart.Head].itemFileName]).protectGas){
                audMask.TransitionTo(0.2f);
            }
            else
            {
                audNormal.TransitionTo(0.2f);
            }
            overlay.sprite = ((Equipable_Wear)ItemController.instance.items[equipment[(int)bodyPart.Head].itemFileName]).Overlay;
            overlay.color = new Color(255, 255, 255, 0.75f);
        }
        else
        {
            audNormal.TransitionTo(0.2f);
            protectSmoke = false;
            overlay.sprite = null;
        }

        if (equipment[(int)bodyPart.Hand] != null && !((ItemController.instance.items[equipment[(int)bodyPart.Hand].itemFileName] is Equipable_Radio) || (ItemController.instance.items[equipment[(int)bodyPart.Hand].itemFileName] is Equipable_Nav)))
        {
            handEquip.sprite = ((Equipable_Wear)ItemController.instance.items[equipment[(int)bodyPart.Hand].itemFileName]).Overlay;
            handEquip.color = Color.white;
            handEquip.SetNativeSize();
        }
        else
        {
            //SCP_UI.instance.bottomScrible.text = "";
            handEquip.sprite = null;
            handEquip.SetNativeSize();
            handEquip.color = Color.clear;
        }
    }

    private void OnDestroy()
    {
        for(int i = 0; i < 4; i++)
        {
            if(equipment[i]!=null)
                ACT_UnEquip((bodyPart)i);
        }
    }
    public void DropItem(GameItem item)
    {
        GameObject newObject;
        newObject = Instantiate(GameController.ins.itemSpawner, handPos.transform.position, Quaternion.identity, GameController.ins.itemParent.transform);
        newObject.GetComponent<Object_Item>().item = item;
        newObject.GetComponent<Object_Item>().id = GameController.ins.AddItem(handPos.transform.position, item);
        Debug.Log("Dropped " + newObject.GetComponent<Object_Item>().id);
        newObject.GetComponent<Object_Item>().Spawn();
        newObject.GetComponent<Rigidbody>().AddForce(transform.forward * 5f);
    }

    public void playerWarp(Vector3 here, float rotation)
    {
        _controller.enabled = false;
        transform.position = here;
        //Debug.Log("body pos " + transform.position + " head pos " + CrouchHead.transform.position);
        if (Crouch)
            CameraObj.transform.position = CrouchHead.transform.position;
        else
            CameraObj.transform.position = DefHead.transform.position;
        //cameraNextFrame = true;
        Vector3 rota = CameraObj.GetComponent<PlayerMouseLook>().rotation;
        CameraObj.GetComponent<PlayerMouseLook>().rotation = new Vector3(rota.x, rota.y + rotation, 0);
        _controller.enabled = true;
    }


    public void SwitchNoClip(bool toValue)
    {
        if (toValue == true)
        {
            IsNoClip = true;
            _controller.enabled = false;
            SCP_UI.instance.HUD.enabled = false;
            CinemaLoaded = Instantiate(CinemaEffect);
        }
        else
        {
            IsNoClip = false;
            _controller.enabled = true;
            SCP_UI.instance.HUD.enabled = true;
            Destroy(CinemaLoaded);
        }
    }


}


