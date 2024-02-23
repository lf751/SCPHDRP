using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.HighDefinition;
using Unity.Netcode;

public enum bodyPartMP { Head, Body, Hand, Any };
public enum AilmentMP { Eyes, Sprint, Health, Speed, Zombie, ZombieCure };

public class EfectsDataMP
{
    public bool permanent;
    public float time = -1;
    public float max = -1;
    public float min = -1;
    public float value = -1;
    public float multiplier = -1;

    public EfectsDataMP(bool _perm, float _time, float _max, float _min, float _value, float _mul)
    {
        permanent = _perm;
        time = _time;
        max = _max;
        min = _min;
        value = _value;
        multiplier = _mul;
    }
}

public class EfectTableMP
{
    public Ailment affected;
    public EfectsData effect;

    public EfectTable CopyData()
    {
        EfectTable copied = new()
        {
            affected = this.affected,
            effect = new EfectsData(this.effect.permanent, this.effect.time, this.effect.max, this.effect.min, this.effect.value, this.effect.multiplier)
        };
        return copied;
    }
}



public class Player_ControlMP : NetworkBehaviour
{
    float InputX, InputY, BlinkingTimer, BlinkMult = 1, currentBlinkMult = 1, RunMult = 1, CloseTimer, AsfixTimer, speed, lastBob = 0, headBob, RunningTimer, OpenTimer = 1, lookingForce = 3f, InternalTimer, InternalTimerPain;

    private GameObject hand, CinemaLoaded;
    private Transform _groundChecker;

    RaycastHit WallCheck;
    Vector3 holdCam, fallSpeed, movement, HoldPos, OriPos, totalmove, headPos, forceLook;
    Quaternion toAngle;
    private CharacterController _controller;
    bool Grounded = true, isSmoke = false, objectLock = false, fakeBlink, isRunning, isTired = false, isLooking = false, cognitoEffect, onBlink, cameraNextFrame, isCaptive = false;
    Camera PlayerCam;
    Image eyes, batbar, overlay, handEquip, eyeIcon;
    SegmentedSlider blinkbar, runbar;
    RectTransform hand_rect, hud_rect;
    public bool Freeze = false, isGameplay = false, Crouch = false, onCam = false, godmode = false, checkObjects = true, IsPuttingOn = false, hasZombie = true, allowZombie = true, allowMove = true;
    public bool noMasterController = false;
    bool playedCough = false;
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
    public AudioReverbZone Reverb;
    public AudioClip[] Conch, CurrentStep, Deaths, Breath, Concrete, Metal, PD, Forest, zombieVoice, coughs;
    public AudioSource sfx, va;


    Collider[] Interact;
    Collider InterHold;

    //Iteeemssss
    [System.NonSerialized]
    public gameItem[] equipment = new gameItem[4];
    [System.NonSerialized]
    public List<EfectTable> tempEffects = new();

    public List<EfectTable> headEffects = new();
    public List<EfectTable> bodyEffects = new();
    public List<EfectTable> anyEffects = new();
    public List<EfectTable> handEffects = new();

    int headSlot = 0;
    int bodySlot = 0;
    int anySlot = 0;
    int handSlot = 0;

    int headInv = 0;
    int bodyInv = 0;
    int anyInv = 0;
    int handInv = 0;


    float eyesMin, sprintMin;

    bool protectSmoke;

    //ForcePath Values
    Transform[] Path;
    bool isPath, walkAnim;
    int currentNode;
    Quaternion PathAngle;
    public float NodeDistance;

    private Object_Captive currCaptive;

    public float AsfixiaRead { get { return AsfixTimer; } }

    //Debug Values
    bool IsNoClip = false;
    bool movementMode = false;


    // Start is called before the first frame update

    void Awake()
    {
        if (noMasterController)
            isGameplay = true;


        CameraObj = FindFirstObjectByType<Camera>().gameObject;
        CameraObj.transform.position = CameraContainer.transform.position;

        //handPos.transform.parent = CameraObj.transform;

        CameraObj.GetComponent<Player_MouseLook>().enabled = true;
        _controller = GetComponent<CharacterController>();
        Reverb = GetComponent<AudioReverbZone>();
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
        if (!noMasterController && SaveSystem.instance.playData.worldsCreateds[(int)GameController.instance.worldName])
        {
            CameraObj.GetComponent<Player_MouseLook>().rotation = new Vector3(0, SaveSystem.instance.playData.worlds[(int)GameController.instance.worldName].angle, 0);
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
                ACT_Effects();
                ACT_HUD();
                ACT_CaptiveObject();
                if (!noMasterController)
                    ACT_Blinking();
                ACT_Zombie();
                if (checkObjects)
                    ACT_Buttons();
                    ACT_Move();
                    ACT_Gravity();
                    ACT_Running();
                    ACT_Walk();

                    CollisionDetection();
                    if (isPath)
                        ACT_ForceWalk();
                    _controller.Move(movement * Time.deltaTime);
                ACT_Camera();
                if (Health <= 0)
                    Death(0);
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


                if (isTired && !va.isPlaying)
                {
                    va.clip = Breath[Random.Range(0, Breath.Length)];
                    va.Play();
                }

                if (AsfixTimer < 0 && !playedCough && !va.isPlaying)
                {
                    va.clip = coughs[Random.Range(0, Breath.Length)];
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
        CameraObj.GetComponent<Player_MouseLook>().inputActive = false;

        currCaptive = newCaptive;
        isCaptive = true;
    }

    void StopCapture()
    {
        if (isCaptive)
        {
            Freeze = false;
            StopLook();
            checkObjects = true;

            CameraObj.GetComponent<Player_MouseLook>().inputActive = true;
            currCaptive.EndCaptive();
            isCaptive = false;
            currCaptive = null;
        }
    }

    void ACT_CaptiveObject()
    {
        if (isCaptive && SCPInput.instance.playerInput.Gameplay.InteractNo.triggered)
        {
            StopCapture();
        }
    }


    void ACT_Move()
    {
        Grounded = _controller.isGrounded;//Physics.CheckSphere(_groundChecker.position, GroundDistance, Ground, QueryTriggerInteraction.Ignore);

        InputX = SCPInput.instance.playerInput.Gameplay.Move.ReadValue<Vector2>().x;
        InputY = SCPInput.instance.playerInput.Gameplay.Move.ReadValue<Vector2>().y;

        if (allowMove)
        {
            movement = ((transform.right * InputX) + (transform.forward * InputY));
            Vector3.Normalize(movement);
        }
        else
            movement = Vector3.zero;

        if (SCPInput.instance.playerInput.Gameplay.CrouchTrigger.triggered && !isRunning)
            Crouch = !Crouch;

        if (Health <= 30)
            Crouch = true;

        //Debug.Log("Held Value " + SCPInput.instance.playerInput.Gameplay.RunHold.ReadValue<float>());
        isRunning = (SCPInput.instance.playerInput.Gameplay.RunHold.ReadValue<float>() > 0 && !Crouch && RunningTimer > 0.2f && (GameController.instance.worldName != Worlds.pocket));

        speed = Basespeed;
        if (Crouch)
            speed = crouchspeed;

        if (isRunning)
            speed = runSpeed;

        if (GameController.instance.worldName == Worlds.pocket)
        {
            speed = crouchspeed + 0.5f;
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
            speed = runSpeed + 2;

        movement *= speed;

    }

    void ACT_Walk()
    {
        if (lastBob < stepMiddle && InternalTimer > stepMiddle)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2, Ground, QueryTriggerInteraction.Ignore))
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
                int blinkPercent = (int)Mathf.Ceil(BlinkingTimer / (BlinkingTimerBase / 100));

                blinkbar.UpdateSlider(blinkPercent);

                int runPercent = (int)Mathf.Floor(RunningTimer / (RunningTimerBase / 100));

                runbar.UpdateSlider(runPercent);

                if (InterHold != null)
                {
                    hand.SetActive(true);
                    Vector3 screen = PlayerCam.WorldToScreenPoint(InterHold.transform.position);

                    Vector3 heading = InterHold.transform.position - CameraObj.transform.position;
                    if (Vector3.Dot(CameraObj.transform.forward, heading) < 0)
                    {
                        screen.y = 0f;
                    }

                    hand.transform.position = screen;
                }
                else
                    hand.SetActive(false);

                Vector3 pos = hand_rect.localPosition;

                Vector3 minPosition = hud_rect.rect.min - hand_rect.rect.min;
                Vector3 maxPosition = hud_rect.rect.max - hand_rect.rect.max;

                pos.x = Mathf.Clamp(hand_rect.localPosition.x, minPosition.x, maxPosition.x);
                pos.y = Mathf.Clamp(hand_rect.localPosition.y, minPosition.y, maxPosition.y);

                hand_rect.localPosition = pos;

                if (SCPInput.instance.playerInput.Gameplay.InteractNo.triggered && currCaptive == null)
                {
                    if (equipment[(int)bodyPart.Hand] != null)
                    {
                        ACT_UnEquip(bodyPart.Hand);
                        return;
                    }
                    if (equipment[(int)bodyPart.Head] != null)
                    {
                        ACT_UnEquip(bodyPart.Head);
                        return;
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
            PlayerCam.fieldOfView = 60;
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

            if (cognitoEffect)
            {
                PlayerCam.fieldOfView = 60 + (Camplitude * Mathf.Sin(Cspeed * Time.time));
            }

            if ((((InputX != 0 || InputY != 0)) || walkAnim) && !Freeze)
            {
                InternalTimer += Time.deltaTime * (speed * bobSpeed);
                if (GameController.instance.worldName != Worlds.pocket)
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
        CameraObj.GetComponent<Player_MouseLook>().addedRota = Quaternion.identity;
    }

    void ACT_ForceLook()
    {
        CameraObj.GetComponent<Player_MouseLook>().addedRota = Quaternion.Slerp(CameraObj.transform.rotation, toAngle, lookingForce * Time.deltaTime);
        Vector3 Point = new Vector3(forceLook.x, forceLook.y, forceLook.z) - CameraObj.transform.position;

        toAngle = Quaternion.LookRotation(Point);
    }



    void ACT_ForceWalk()
    {

        if (currentNode > 1 && Vector3.Distance(new Vector3(Path[currentNode - 2].position.x, transform.position.y, Path[currentNode - 2].position.z), transform.position) < NodeDistance)
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

    public void ForceWalk(Transform[] newPath)
    {
        currentNode = 0;
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
        handPos.transform.SetPositionAndRotation(CameraObj.transform.position + (CameraObj.transform.forward * handLength), CameraObj.transform.rotation);

        if (!objectLock)
        {
            float lastdistance = float.PositiveInfinity;
            Interact = Physics.OverlapCapsule(CameraObj.transform.position, handPos.transform.position, handSize * (Freeze ? 1.5f : 1f), InteractiveLayer);
            //Interact = Physics.OverlapSphere(handPos.transform.position, handSize, InteractiveLayer);
            if (Interact.Length != 0)
            {
                InterHold = null;
                float currdistance;
                for (int i = 0; i < Interact.Length; i++)
                {
                    //
                    Vector3 closePoint = Interact[i].ClosestPoint(handPos.transform.position);
                    //currdistance = Vector3.Distance(headPos - new Vector3(0.0f, 0.2f, 0.0f), closePoint);
                    currdistance = Vector3.Distance(handPos.transform.position, closePoint);
                    Debug.DrawRay(closePoint, Vector3.up, Color.black);
                    Debug.DrawRay(headPos, closePoint - headPos, new Color(255, 255, 255, 1.0f));
                    if (currdistance < lastdistance)
                    {
                        /*if (!Physics.Raycast(handPos.transform.position, (closePoint - handPos.transform.position).normalized, currdistance, Ground, QueryTriggerInteraction.Ignore))
                        {
                            lastdistance = currdistance;
                            InterHold = Interact[i];
                        }
                        else */
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
            if (SCPInput.instance.playerInput.Gameplay.InteractYes.triggered)
            {
                InterHold.GetComponent<Object_Interact>().Pressed();
            }
            if ((InterHold != null) && SCPInput.instance.playerInput.Gameplay.InteractHold.ReadValue<float>() > 0)
            {
                InterHold.GetComponent<Object_Interact>().Hold();
                objectLock = true;
            }
            else
            {
                objectLock = false;
            }

            if ((distanceBody > handLength) && (distance > handLength))
            {
                objectLock = false;
                InterHold = null;
            }
        }


    }

    void ACT_Running()
    {
        if (SCPInput.instance.playerInput.Gameplay.RunHold.ReadValue<float>() < 0.1 && RunningTimer < RunningTimerBase)
            RunningTimer += (Time.deltaTime) * RunMult;


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
            eyes.color = new Color(255, 255, 255, Mathf.Clamp(-((BlinkingTimer - 0.064f) * 16), 0.0f, 1.0f));
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
            AsfixTimer += Time.deltaTime * 2;
            if (AsfixTimer > AsfixiaTimer)
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
            GameController.instance.PlayerDeath();
            _controller.enabled = false;
            DeathCol.SetActive(true);
            CameraObj.transform.rotation = Quaternion.Euler(CameraObj.GetComponent<Player_MouseLook>().rotation);
            CameraObj.transform.parent = DeathCol.transform;
            CameraObj.GetComponent<Player_MouseLook>().enabled = false;
            isGameplay = false;
            eyes.color = Color.clear;
            Destroy(handPos);

            overlay.sprite = null;
            handEquip.color = Color.clear;
            SCP_UI.instance.SNav.SetActive(false);
            SCP_UI.instance.radio.StopRadio();



            switch (cause)
            {
                case 3:
                    {
                        sfx.PlayOneShot(Deaths[cause]);
                        GameController.instance.deathmsg = Localization.GetString("deathStrings", "death_106_stone");
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
            GameController.instance.FakeDeath();
            _controller.enabled = false;
            DeathCol.SetActive(true);
            CameraObj.transform.parent = DeathCol.transform;
            CameraObj.GetComponent<Player_MouseLook>().enabled = false;
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
        Collider[] collisions = Physics.OverlapCapsule(transform.position + (_controller.center - Vector3.up * ((_controller.height / 2))), transform.position + (_controller.center + Vector3.up * ((_controller.height / 2))), _controller.radius + 0.2f, Collisionables, QueryTriggerInteraction.Collide);

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


    public void ACT_Equip(gameItem item)
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
        if (item is Equipable_Wear wear)
        {
            ref List<EfectTable> currEffect = ref headEffects;
            switch (wear.part)
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
            Debug.Log("Zombie state: Timer: " + zombieTimer + " off " + zombieVirusFull + " " + zombieVirusMaxTime + " perc " + (Mathf.Min(zombieTimer, (zombieVirusFull)) / zombieVirusFull));
            if (zombieVoiceTimer < 0)
            {
                int choice = Random.Range(0, zombieVoice.Length - 1);

                zombieVoiceTimer = zombieVoice[choice].length + Random.Range(10, 30);

                va.PlayOneShot(zombieVoice[choice], perc);
            }
        }
        speedMul *= (1f - (perc * 0.5f));
        SCP_UI.instance.infectiongraphic.color = new Color(perc, perc, perc, perc);

        if (zombieTimer > zombieVirusMaxTime && GlobalValues.LoadType != LoadType.mapless)
        {
            GameController.instance.Death = DeathEvent.zombie008;
            FakeDeath(0);
            eyes.color = Color.black;
        }
    }

    void ACT_Effects()
    {
        currentBlinkMult = BaseBlinkMult;
        sprintMin = 0;
        speedMul = 1;


        Health -= (bloodloss * 0.125f) * Time.deltaTime;
        if (Health >= 100)
            Health = 100;

        processEffects(ref headEffects);
        processEffects(ref anyEffects);
        processEffects(ref handEffects);
        processEffects(ref bodyEffects);

        if (equipment[(int)bodyPart.Hand] != null && ItemController.instance.items[equipment[(int)bodyPart.Hand].itemFileName] is Equipable_Elec elec && equipment[(int)bodyPart.Hand].valFloat >= 0)
        {
            (equipment[(int)bodyPart.Hand]).valFloat -= elec.SpendFactor * Time.deltaTime;
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

        SCP_UI.instance.ItemSFX(itemData.SFX);

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
            if (itemData.isUnique)
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
                    ItemController.instance.equip[headInv][headSlot] = false;
                    if (itemData.autoEquip && headInv == 0)
                    {
                        ItemController.instance.slots[headSlot].UnEquip(true);
                    }

                    break;
                }
            case bodyPart.Body:
                {
                    ItemController.instance.equip[bodyInv][bodySlot] = false;
                    if (itemData.autoEquip && bodyInv == 0)
                    {
                        ItemController.instance.slots[bodySlot].UnEquip(true);
                    }
                    break;
                }
            case bodyPart.Hand:
                {
                    ItemController.instance.equip[handInv][handSlot] = false;
                    if (itemData.autoEquip && handInv == 0)
                    {
                        ItemController.instance.slots[handSlot].UnEquip(true);
                    }
                    break;
                }
            case bodyPart.Any:
                {
                    ItemController.instance.equip[anyInv][anySlot] = false;
                    if (itemData.autoEquip && anyInv == 0)
                    {
                        ItemController.instance.slots[anySlot].UnEquip(true);
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
            Reverb.enabled = ((Equipable_Wear)ItemController.instance.items[equipment[(int)bodyPart.Head].itemFileName]).protectGas;
            overlay.sprite = ((Equipable_Wear)ItemController.instance.items[equipment[(int)bodyPart.Head].itemFileName]).Overlay;
            overlay.color = new Color(255, 255, 255, 0.75f);
        }
        else
        {
            Reverb.enabled = false;
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

    public void DropItem(gameItem item)
    {
        GameObject newObject;
        newObject = Instantiate(GameController.instance.itemSpawner, handPos.transform.position, Quaternion.identity, GameController.instance.itemParent.transform);
        newObject.GetComponent<Object_Item>().item = item;
        newObject.GetComponent<Object_Item>().id = GameController.instance.AddItem(handPos.transform.position, item);
        Debug.Log("dROPPED " + newObject.GetComponent<Object_Item>().id);
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
        cameraNextFrame = true;
        Vector3 rota = CameraObj.GetComponent<Player_MouseLook>().rotation;
        CameraObj.GetComponent<Player_MouseLook>().rotation = new Vector3(rota.x, rota.y + rotation, 0);
        _controller.enabled = true;
    }


    public void SwitchNoClip()
    {
        if (IsNoClip == false)
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
            DestroyImmediate(CinemaLoaded);
        }
    }


}


