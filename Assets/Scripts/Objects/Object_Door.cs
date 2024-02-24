using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Object_Door : MonoBehaviour
{
    public static int openDoorFlag = 0, closedDoorFlag = 8;
    [SerializeField]
    private GameObject Door01, Door02;
    [SerializeField]
    private float OpenSpeed, DoorEndPos, DoorTime;
    private int id;

    [SerializeField]
    private AudioClip[] Open_AUD;
    [SerializeField]
    private AudioClip[] SCP_AUD;
    [SerializeField]
    private AudioClip[] Close_AUD;
    [SerializeField]
    private AudioSource AUD;

    [SerializeField]
    private NavMeshObstacle carveMesh;
    [SerializeField]
    private NavMeshLink doorLink;
    [SerializeField]
    private bool UseParticle = false;

    Vector3 Pos1, Pos2;
    float moveAmmount = 0;
    public bool switchOpen = false, scp173 = true, ignoreSave = false, isDisabled = false;
    public bool IsOpen = false;
    public bool isMoving = false;
    bool isForcing = false;
    bool lastStateOpen;

    // Start is called before the first frame update

    private void Awake()
    {
        openDoorFlag = NavMesh.GetAreaFromName("Walkable");
        closedDoorFlag = NavMesh.GetAreaFromName("ClosedDoor");
        AUD = GetComponent<AudioSource>();
        Pos1 = Door01.transform.position;
        Pos2 = Door02.transform.position;
    }

    void Start()
    {
        if (!ignoreSave)
        {
            id = GameController.ins.GetDoorID();
            transform.parent = GameController.ins.doorParent.transform;
            ResetState();
        }
    }

    public void ResetState()
    {
        int doorState = GameController.ins.GetDoorState(id);
        if (doorState != -1)
        {
            if (doorState == 0)
            {
                IsOpen = true;
                switchOpen = false;
                moveAmmount = DoorEndPos;
            }
            if (doorState == 1)
            {
                IsOpen = false;
                switchOpen = true;
                moveAmmount = 0;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOpen == false && switchOpen == true)
            DoorOpen();

        if (switchOpen == false && IsOpen == true)
            DoorClose();

        if (isForcing == true)
            Hodor();
    }

    void DoorOpen()
    {
        if (moveAmmount < DoorEndPos)
        {
            moveAmmount += OpenSpeed * Time.deltaTime;
            if (moveAmmount > DoorEndPos)
                moveAmmount = DoorEndPos;

            Door01.transform.position = Pos1 - (Door01.transform.right * moveAmmount);
            Door02.transform.position = Pos2 - (Door02.transform.right * moveAmmount);
        }
        else
        {
            IsOpen = true;
            doorLink.area = openDoorFlag;
            if (!ignoreSave)
                GameController.ins.SetDoorState(true, id);
        }


    }

    void DoorClose()
    {
        if (moveAmmount > 0)
        {
            moveAmmount -= OpenSpeed * Time.deltaTime;
            if (moveAmmount < 0)
                moveAmmount = 0;

            Door01.transform.position = Pos1 - (Door01.transform.right * moveAmmount);
            Door02.transform.position = Pos2 - (Door02.transform.right * moveAmmount);
        }
        else
        {
            if (UseParticle)
            {
                GameController.ins.particleController.StartParticle(1, transform.position, transform.rotation);
            }
            doorLink.area = closedDoorFlag;
            IsOpen = false;
            if (!ignoreSave)
                GameController.ins.SetDoorState(false, id);
        }
    }

    public void DoorSwitch()
    {

        if (!isDisabled)
        {
            if (switchOpen == true && IsOpen == true)
            {
                switchOpen = false;
                PlayClose();
                moveAmmount = DoorEndPos;
            }
            if (switchOpen == false && IsOpen == false)
            {
                switchOpen = true;
                PlayOpen();
                moveAmmount = 0;
            }
        }
    }

    public void DoorSwitch(bool val)
    {
        if (switchOpen != val)
        {
            IsOpen = !val;
            if (val)
                PlayOpen();
            else
                PlayClose();
        }
        switchOpen = val;
    }

    public void InstantSet(bool open)
    {
        if (!isDisabled)
        {
            if (open == false)
            {
                switchOpen = false;
                IsOpen = false;
                Door01.transform.position = Pos1;
                Door02.transform.position = Pos2;
                doorLink.area = closedDoorFlag;
                carveMesh.enabled = false;
                if (!ignoreSave)
                    GameController.ins.SetDoorState(false, id);
            }
            else
            {
                IsOpen = true;
                switchOpen = true;
                Door01.transform.position = Pos1 - (Door01.transform.right * DoorEndPos);
                Door02.transform.position = Pos2 - (Door02.transform.right * DoorEndPos);
                doorLink.area = openDoorFlag;
                carveMesh.enabled = true;
                if (!ignoreSave)
                    GameController.ins.SetDoorState(true, id);
            }
        }
    }

    public void ForceOpen(float time)
    {
        if (!isDisabled)
        {
            if (!isForcing)
            {
                //Debug.Log("ForceDoor start!, door was " + switchOpen);
                lastStateOpen = switchOpen;
            }
            isForcing = true;
            DoorTime = time;
        }
    }

    void Hodor()
    {
        if (switchOpen != true)
            DoorSwitch();
        DoorTime -= (Time.deltaTime);
        if (DoorTime <= 0.0f)
        {
            isForcing = false;
            if (lastStateOpen != switchOpen)
                DoorSwitch();

            //Debug.Log("ForceDoor end!");
        }
    }



    public bool Door173()
    {
        if (switchOpen == false && IsOpen == false && scp173 == true && !isDisabled)
        {
            switchOpen = true;
            PlaySCP(0);
            moveAmmount = 0;
            return (true);
        }
        else
        {
            return (false);
        }
    }

    void PlayOpen()
    {
        AUD.clip = Open_AUD[Random.Range(0, Open_AUD.Length)];
        AUD.Play();
    }
    void PlaySCP(int i)
    {
        AUD.clip = SCP_AUD[i];
        AUD.Play();
    }
    void PlayClose()
    {
        AUD.clip = Close_AUD[Random.Range(0, Close_AUD.Length)];
        AUD.Play();
    }
    /// <summary>
    /// GetState() Obtiene el estado de la puerta, True si Abierta
    /// </summary>
    /// <returns>true Si la puerta esta abierta</returns>
    public bool GetState()
    {
        return (IsOpen && switchOpen);
    }

}
