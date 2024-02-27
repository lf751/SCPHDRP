using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class npc_zombieGuard : Map_NPC
{
    enum zombieState { wakeup, idle, patrol, chase, soundChase, attack, attackCool}

    //public AudioClip idle, panic, horror, chaseClip, scream;
    Camera mainCamera;
    Plane[] frustum;
    Collider[] closeSounds;
    NavMeshAgent agent;
    AudioSource voiceSource;
    public AudioClip Hit;
    public Animator animator;
    public float viewLimit, listeningRange, closeRange, timeToHit, timeToIdle, dampSpeed = 0.1f;
    public LayerMask ground, playerMask, soundLayer;
    public bool debugIsTargeting;
    zombieState state, currAnim;
    float Timer, framerate = 15, distanceFromPlayer = Mathf.Infinity, intMoveSpeed=0, refMoveSpeed=0;
    int currNode, currSoundLevel;
    bool onPath, seePlayer, foundSound, hasPath;
    Vector3 currTarget;

    // Start is called before the first frame update
    void Awake()
    {
        state = zombieState.idle;
        currAnim = zombieState.idle;
        mainCamera = Camera.main;
        agent = GetComponent<NavMeshAgent>();
        voiceSource = GetComponent<AudioSource>();
        agent.updatePosition = false;
    }

    public override void setData(NPC_Data state)
    {
        agent.Warp(state.Pos.toVector3());
        base.setData(state);
    }

    // Update is called once per frame
    void Update()
    {
        if (data.isActive)
        {
            NPCUpdate();
        }
        else
        {
            state = zombieState.wakeup;
            Timer = Random.Range(7f, 10f);
        }
        voiceSource.volume = data.isActive ? 1 : 0;

    }

    private void LateUpdate()
    {
        intMoveSpeed = Mathf.SmoothDamp(intMoveSpeed, agent.isOnOffMeshLink ? 1 : agent.desiredVelocity.magnitude, ref refMoveSpeed, Time.deltaTime * dampSpeed);
        animator.SetFloat("moveSpeed", intMoveSpeed);

        animator.SetBool("isActive", data.isActive);

        if (currAnim != state)
        {
            switch (state)
            {
                case zombieState.attack:
                    {
                        animator.SetTrigger("attackAnim");
                        break;
                    }
            }
            currAnim = state;
        }
    }

    private void OnDrawGizmos()
    {
        if (currTarget != null)
            Gizmos.DrawSphere(currTarget, 0.3f);
    }

    void NPCUpdate()
    {
        //agent.velocity=animator.velocity;
        Vector3 worldDeltaPosition = agent.nextPosition - transform.position;
        if (worldDeltaPosition.magnitude > agent.radius && !agent.isOnOffMeshLink)
            agent.nextPosition = transform.position + 0.9f * worldDeltaPosition;

        seePlayer = false;
        if (debugIsTargeting && Time.frameCount % framerate == 0)
        {
            seePlayer = CanSee();
            if (!seePlayer)
            {
                distanceFromPlayer = Mathf.Infinity;
            }
            else
            {
                currTarget = GameController.ins.currPly.transform.position;
            }

            if (state != zombieState.chase && state != zombieState.attack && state != zombieState.attackCool && state != zombieState.wakeup && seePlayer)
            {
                state = zombieState.chase;
            }
        }

        if (Time.frameCount % framerate == 0 && !foundSound)
        {
            CheckSounds();
        }


        //Current State
        switch (state)
        {
            case zombieState.wakeup:
            case zombieState.attackCool:
            case zombieState.attack:
                {
                    onPath = false;
                    agent.ResetPath();
                    break;
                }
            case zombieState.idle:
                {
                    onPath = false;
                    agent.ResetPath();
                    if (foundSound)
                    {
                        if (currSoundLevel > 0)
                        {
                            state = zombieState.soundChase;
                            onPath = false;
                        }
                        foundSound = false;
                    }
                    break;
                }
            case zombieState.patrol:
                {
                    onPath = false;
                    if (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid)
                        agent.SetDestination(getRandomPoint());

                    if (foundSound)
                    {
                        if (currSoundLevel > 0)
                        {
                            state = zombieState.soundChase;
                            onPath = false;
                        }
                        foundSound = false;
                    }

                    break;
                }
            case zombieState.soundChase:
                {
                    if (!onPath)
                    {
                        agent.SetDestination(currTarget);
                        onPath = true;
                    }


                    if (agent.hasPath && agent.remainingDistance < 0.5f)
                    {
                        state = zombieState.idle;
                        onPath = false;
                        currSoundLevel = 0;
                        Timer = Random.Range(5, 10);
                    }
                    break;
                }
            case zombieState.chase:
                {
                    currSoundLevel = 0;
                    if (Time.frameCount % framerate == 0)
                    {
                        if (seePlayer)
                            agent.SetDestination(currTarget);
                    }

                    if (agent.hasPath && agent.remainingDistance < 0.5f && distanceFromPlayer > 1f)
                    {
                        state = zombieState.idle;
                        Timer = Random.Range(5, 10);
                    }

                    if (Physics.OverlapSphere(transform.position + transform.forward, 0.75f, playerMask).Length > 0)
                    {
                        //GameController.instance.playercache.Death(4);
                        Debug.Log("Kill");
                        state = zombieState.attack;
                        Timer = timeToHit;
                    }

                    break;
                }
        }

        Timer -= Time.deltaTime;

        //Next State
        if (Timer < 0)
        {
            switch (state)
            {
                case zombieState.attack:
                    {
                        state = zombieState.attackCool;
                        Timer = timeToIdle;
                        voiceSource.PlayOneShot(Hit);

                        if (GameController.ins.isAlive && Physics.OverlapSphere(transform.position + transform.forward, 0.8f, playerMask).Length > 0)
                        {
                            GameController.ins.currPly.Health -= 25;
                            if(GameController.ins.currPly.Health <= 0)
                            {
                                GameController.ins.deathmsg = Localization.GetString("deathStrings", "death_049_1");
                            }
                        }

                        break;
                    }
                case zombieState.attackCool:
                case zombieState.wakeup:
                case zombieState.idle:
                    {
                        state = zombieState.patrol;
                        Timer = Random.Range(10f, 15f);

                        break;
                    }
                case zombieState.patrol:
                    {

                        state = zombieState.idle;
                        Timer = Random.Range(5f, 10f);
                        break;
                    }
            }
        }
    }

    void CheckSounds()
    {
        float lastdistance = 100f;
        float currdistance;
        int tempSound;
        foundSound = false;
        WorldSound currentSound;
        closeSounds = Physics.OverlapSphere(transform.position, listeningRange, soundLayer);
        if (closeSounds.Length != 0)
        {
            for (int i = 0; i < closeSounds.Length; i++)
            {
                currentSound = closeSounds[i].gameObject.GetComponent<WorldSound>();
                tempSound = currentSound.SoundLevel;
                currdistance = Vector3.Distance(transform.position, closeSounds[i].transform.position);
                if (currdistance > closeRange)
                    tempSound -= 1;

                if (currSoundLevel < tempSound)
                {
                    if (currdistance < lastdistance)
                    {
                        lastdistance = currdistance;
                        currTarget = closeSounds[i].gameObject.transform.position;
                        currSoundLevel = tempSound;
                        //soundlevel = "sonido " + data.npcvalue[valSoundLevel] + " distancia " + currdistance + " ";
                        foundSound = true;
                    }

                }
            }
        }
    }

    Vector3 getRandomPoint()
    {
        currTarget = transform.position + Random.insideUnitSphere * 5;
        currTarget.y = transform.position.y;
        return currTarget;
    }


    /// <summary>
    /// Check if SCP 049 face is on the camera view
    /// </summary>
    /// <returns></returns>
    bool CanSee()
    {
        Vector3 playerDir = GameController.ins.currPly.transform.position - transform.position;
        distanceFromPlayer = Vector3.Distance(transform.position, GameController.ins.currPly.transform.position);
        return (Vector3.Dot(playerDir.normalized, transform.forward) > viewLimit) && !Physics.Raycast(transform.position + Vector3.up * 1.5f, playerDir.normalized, distanceFromPlayer, ground) && distanceFromPlayer < 20f;
    }

    private void OnAnimatorMove()
    {
        if (!agent.isOnOffMeshLink)
        {
            Vector3 position = animator.rootPosition;
            position.y = agent.nextPosition.y;
            transform.position = position;
        }

        //agent.nextPosition= animator.rootPosition;
        //transform.position = agent.nextPosition;
    }
}
