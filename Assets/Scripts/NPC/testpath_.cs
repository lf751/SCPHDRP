using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class testpath_ : MonoBehaviour
{

    private enum state_fakeagent { idle, patrol, hearing, hearing2, walk, run, attack , dopath, ignoreall};

    public NavMeshAgent Agent;
    public AudioSource Audio;
    GameObject Player;
    Vector3 currentTarget;
    Collider[] CloseSounds;
    public Transform[] patrol;
    public AudioClip[] RandomChatter;
    public AudioClip[] Detecting;
    public AudioClip[] Found;
    public AudioClip[] Stop;
    public AudioClip Hit;
    Vector3 lookAt;

    bool Talk;


    public float ListeningRange, closeRange, walkSpeed, runSpeed, HearingTimer, Hearing2Timer, defIdle, defWalk, foundPlayer, foundPlayerRun, AttackCool = 1, AttackDistance;
    float Timer = 0, AttackTimer = 0, playerDistance, playerCheck;
    public int frame;
    public Animator Animator;
    public LayerMask SoundLayer, groundlayer;
    int SoundLevel = 0;
    int currentNode = 0, currentPatrol;
    public bool isDebuggin, debugSpeed, debugPlayerPos;
    public bool WorldSearch = false;


    bool foundTarget;
    bool destSet;
    bool stateSet, debugGameLoaded = false;
    bool checkPlayer = true;
    string soundlevel = "No sounds ";
    state_fakeagent state = state_fakeagent.idle;





    // Start is called before the first frame update
    void Start()
    {
        Agent.Warp(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (!debugGameLoaded)
        {
            if (GameController.instance.doGameplay)
            {
                Player = GameController.instance.player;
                debugGameLoaded = true;
                checkPlayer = true;
            }

        }
        else
        {
            switch (state)
            {
                case state_fakeagent.idle:
                    {
                        if (!stateSet)
                        {
                            playerCheck = foundPlayer;
                            Animator.SetBool("move", false);
                            Agent.isStopped = true;
                            destSet = false;
                            stateSet = true;
                            Timer = Random.Range(defIdle, defIdle+3);
                            if (isDebuggin)
                                Debug.Log("Volviendo a Idle");
                        }
                        break;
                    }
                case state_fakeagent.dopath:
                    {
                        if (!stateSet)
                        {
                            playerCheck = foundPlayer;
                            Animator.SetBool("move", true);
                            Agent.isStopped = false;
                            Agent.speed = walkSpeed;
                            Agent.SetDestination(currentTarget);
                            stateSet = true;
                            if (isDebuggin)
                                Debug.Log("Starting path");

                        }
                        else if (!Agent.pathPending && Agent.hasPath && Agent.remainingDistance < 1)
                        {
                            state = state_fakeagent.ignoreall;
                            Animator.SetBool("move", false);
                            Timer = 45;
                            stateSet = true;
                            if (isDebuggin)
                                Debug.Log("Ending path");
                        }
                        Timer = 10;
                        break;
                    }
                case state_fakeagent.attack:
                    {
                        Animator.SetBool("move", true);
                        Animator.SetTrigger("reset");

                        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookAt), 5 * Time.deltaTime);
                        playerCheck = foundPlayerRun;
                        if (playerDistance < AttackDistance && GameController.instance.isAlive)
                        {
                            Audio.PlayOneShot(Hit);
                            GameController.instance.playercache.Death(0);
                            GameController.instance.deathmsg = Localization.GetString("deathStrings", "death_mtf");
                            Agent.isStopped = true;
                            Animator.SetTrigger("attack" + Random.Range(1, 3));

                        }
                        else
                        {
                            Agent.isStopped = false;
                            Agent.speed = runSpeed;
                            if (CheckPlayer())
                                Agent.SetDestination(Player.transform.position);
                        }
                        break;
                    }
                case state_fakeagent.patrol:
                    {
                        if (!stateSet)
                        {
                            //PlayVoice(1);

                            playerCheck = foundPlayer;
                            Animator.SetBool("move", true);
                            Agent.isStopped = false;
                            Agent.speed = walkSpeed;
                            if (!WorldSearch)
                                Agent.SetDestination(patrol[currentNode].position);
                            else
                                Agent.SetDestination(GameController.instance.GetPatrol(transform.position, 6, 0));
                            Timer = Random.Range(defWalk, defWalk+3);
                            if (!WorldSearch)
                                Timer = Random.Range(1, defWalk - 2);

                            stateSet = true;
                            if (isDebuggin)
                                Debug.Log("Caminata");

                        }
                        else if (Agent.remainingDistance < 1)
                        {
                            if (!WorldSearch)
                            {
                                /*currentNode += 1;
                                if (currentNode >= patrol.Length)
                                    currentNode = 0;*/
                                currentNode = Random.Range(0, patrol.Length);
                                Agent.SetDestination(patrol[currentNode].position);
                                //stateSet = false;
                            }
                            else
                                Agent.SetDestination(GameController.instance.GetPatrol(transform.position, 6, 0));
                        }
                        break;
                    }
                case state_fakeagent.run:
                    {
                        if (destSet == false)
                        {
                            playerCheck = foundPlayerRun;
                            Animator.SetBool("move", true);
                            Agent.isStopped = false;
                            Agent.speed = runSpeed;
                            Agent.SetDestination(currentTarget);
                            destSet = true;
                            if (isDebuggin)
                                Debug.Log(soundlevel + "Corro hacia el!");
                            PlayVoice(4);
                        }
                        else if (Agent.remainingDistance < 1)
                        {
                            destSet = false;
                            foundTarget = false;
                            state = state_fakeagent.idle;
                        }
                        break;
                    }
                case state_fakeagent.walk:
                    {
                        if (destSet == false)
                        {
                            playerCheck = foundPlayer;
                            PlayVoice(3);
                            Animator.SetBool("move", true);
                            Agent.isStopped = false;
                            Agent.speed = walkSpeed;
                            Agent.SetDestination(currentTarget);
                            destSet = true;
                            if (isDebuggin)
                                Debug.Log(soundlevel + "Camino hacia el!");
                        }
                        else if (Agent.remainingDistance < 1)
                        {
                            destSet = false;
                            foundTarget = false;
                            state = state_fakeagent.idle;
                        }
                        break;
                    }
                case state_fakeagent.hearing:
                    {
                        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookAt), 5 * Time.deltaTime);

                        if (!stateSet)
                        {
                            Agent.isStopped = true;
                            Animator.SetBool("move", false);
                            Animator.SetTrigger("look");
                            destSet = false;
                            Timer = HearingTimer;
                            if (isDebuggin)
                                Debug.Log(soundlevel + "Escuche algo?");
                            stateSet = true;
                        }
                        break;
                    }
                case state_fakeagent.hearing2:
                    {
                        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookAt), 5 * Time.deltaTime);


                        if (!stateSet)
                        {
                            Agent.isStopped = true;
                            Animator.SetBool("move", false);
                            Animator.SetTrigger("vocal");
                            destSet = false;
                            Timer = Hearing2Timer;
                            if (isDebuggin)
                                Debug.Log(soundlevel + "Si escuche!");
                            stateSet = true;
                            PlayVoice(2);
                        }
                        break;
                    }
            }

            Timer -= Time.deltaTime;
            AttackTimer -= Time.deltaTime;

            if ((state != state_fakeagent.run && state != state_fakeagent.walk && state != state_fakeagent.attack && state != state_fakeagent.dopath ) && Timer <= 0)
            {
                foundTarget = false;
                stateSet = false;

                switch (state)
                {
                    case state_fakeagent.idle:
                        {
                            state = state_fakeagent.patrol;
                            break;
                        }
                    case state_fakeagent.patrol:
                        {
                            state = state_fakeagent.idle;
                            break;
                        }
                    case state_fakeagent.ignoreall:
                        {
                            state = state_fakeagent.patrol;
                            break;
                        }
                }
            }
            if (state == state_fakeagent.run || state == state_fakeagent.walk || state == state_fakeagent.patrol || state == state_fakeagent.attack || state == state_fakeagent.dopath)
            {
                if (Agent.isOnOffMeshLink)
                    Animator.SetFloat("speed", 2);
                else
                    Animator.SetFloat("speed", Agent.velocity.magnitude);

                if (debugSpeed)
                    Debug.Log(Agent.velocity.magnitude);
            }





            if (foundTarget == false && state != state_fakeagent.attack && state != state_fakeagent.dopath && state != state_fakeagent.ignoreall)
            {
                CheckSounds();
                if (foundTarget)
                {
                    stateSet = false;

                    if (SoundLevel == 0)
                    {
                        switch (state)
                        {
                            case state_fakeagent.hearing2:
                                {
                                    state = state_fakeagent.walk;
                                    break;
                                }
                            case state_fakeagent.run:
                                {
                                    state = state_fakeagent.hearing2;
                                    break;
                                }
                            case state_fakeagent.hearing:
                                {
                                    state = state_fakeagent.hearing2;
                                    break;
                                }
                            default:
                                {
                                    Debug.Log("Cambiando a hearing!");
                                    state = state_fakeagent.hearing;
                                    break;
                                }
                        }
                    }
                    if (SoundLevel == 1)
                    {
                        switch (state)
                        {
                            case state_fakeagent.hearing2:
                                {
                                    state = state_fakeagent.walk;
                                    break;
                                }
                            case state_fakeagent.hearing:
                                {
                                    state = state_fakeagent.walk;
                                    break;
                                }
                            case state_fakeagent.run:
                                {
                                    state = state_fakeagent.walk;
                                    break;
                                }
                            default:
                                {
                                    state = state_fakeagent.hearing2;
                                    break;
                                }
                        }
                    }
                    if (SoundLevel > 1)
                    {
                        switch (state)
                        {
                            case state_fakeagent.hearing2:
                                {
                                    state = state_fakeagent.run;
                                    break;
                                }
                            case state_fakeagent.run:
                                {
                                    state = state_fakeagent.run;
                                    break;
                                }
                            default:
                                {
                                    state = state_fakeagent.walk;
                                    break;
                                }
                        }
                    }
                }
                else
                {
                    if (state != state_fakeagent.idle && state != state_fakeagent.patrol)
                        state = state_fakeagent.idle;
                }
            }

            if (checkPlayer)
                playerDistance = Vector3.Distance(Player.transform.position, transform.position);
            lookAt = new Vector3(Player.transform.position.x, transform.position.y, Player.transform.position.z) - transform.position;
            if (debugPlayerPos)
                Debug.Log("Producto dot " + Vector3.Dot(transform.forward, lookAt.normalized));

            if (playerDistance < playerCheck)
            {
                if (state != state_fakeagent.attack && state != state_fakeagent.dopath && Vector3.Dot(transform.forward, lookAt.normalized) > 0.4f && CheckPlayer())
                {
                    foundTarget = false;
                    stateSet = false;
                    PlayVoice(4);

                    if (state == state_fakeagent.run)
                        Animator.SetTrigger("leap");
                    state = state_fakeagent.attack;
                }
            }
            else if (state == state_fakeagent.attack)
                state = state_fakeagent.idle;
        }
    }

    bool CheckPlayer()
    {
        Debug.DrawRay(Player.transform.position, (transform.position + new Vector3(0, 0.4f, 0)) - Player.transform.position);

        if (Time.frameCount % frame == 0)
        {
            if (!Physics.Raycast(Player.transform.position, (transform.position + new Vector3(0, 0.4f, 0)) - Player.transform.position, playerDistance, groundlayer))
            {
                return true;
            }
        }
        return false;
    }

    void PlayVoice(int library)
    {
        if (!Audio.isPlaying)
        {
            Audio.Stop();
            float delay = 0;
            if (library == 1)
            {
                Audio.clip = RandomChatter[Random.Range(0, RandomChatter.Length)];
                delay = 0.5f;
            }
            if (library == 2)
            {
                Audio.clip = Detecting[Random.Range(0, Detecting.Length)];
                delay = 2;
            }
            if (library == 3)
            {
                Audio.clip = Found[Random.Range(0, Found.Length)];
                delay = 0.5f;
            }
            if (library == 4)
            {
                Audio.clip = Stop[Random.Range(0, Stop.Length)];
            }
            Audio.PlayDelayed(delay);
        }
    }

    public void GoHere(Vector3 here)
    {
        Debug.Log("Doing Path", this);
        foundTarget = false;
        currentTarget = here;
        Timer = 100;
        state = state_fakeagent.dopath;
        stateSet = false;
    }

    public void StopThis()
    {
        Debug.Log("Stopping Path", this);
        currentTarget = transform.position;
        foundTarget = false;
        stateSet = false;
        state = state_fakeagent.idle;
    }





    void CheckSounds()
    {
        float lastdistance = 100f;
        SoundLevel = -1;
        float currdistance;
        int currentSoundLevel;
        foundTarget = false;
        WorldSound currentSound;
        CloseSounds = Physics.OverlapSphere(transform.position, ListeningRange, SoundLayer);
        if (CloseSounds.Length != 0)
        {
            for (int i = 0; i < CloseSounds.Length; i++)
            {
                currentSound = CloseSounds[i].gameObject.GetComponent<WorldSound>();
                currentSoundLevel = currentSound.SoundLevel;
                currdistance = Vector3.Distance(transform.position, CloseSounds[i].transform.position);
                if (currdistance > closeRange)
                    currentSoundLevel -= 1;

                if (SoundLevel < currentSoundLevel)
                {
                    if (currdistance < lastdistance)
                    {
                        currentTarget = CloseSounds[i].gameObject.transform.position;
                        SoundLevel = currentSoundLevel;
                        soundlevel = "sonido " + SoundLevel + " distancia " + currdistance + " ";
                        foundTarget = true;
                    }

                }
            }
        }
    }


    /*private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && AttackTimer <= 0)
        {
            if (GameController.instance.isAlive)
            {
                other.gameObject.GetComponent<Player_Control>().Health -= 25;
                AttackTimer = AttackCool;
                Audio.PlayOneShot(Hit);
            }
            else
            {
                checkPlayer = false;
                playerDistance = 100;
                debugGameLoaded = false;
            }
        }
    }*/


}
