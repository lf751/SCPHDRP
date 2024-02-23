using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_Intro_extra : MonoBehaviour
{
    public AudioClip[] SCI_1, SCI_2, SCI_3, CRE_1, CRE_2, CRE_3, SEC_1, SEC_2, SEC_3, DIA1, DIA2, NUMBERS, SCRIPTED, ONOFF;
    private AudioClip[] FinalMessage;
    public AudioClip Scientistdiag;
    public Transform[] path1, path2, path3;
    public GameObject Guard1, Guard2, D1, Sci1, Sci2, Jan, check1, check2;
    EV_Puppet_Controller Guard1_, Guard2_, D1_, Sci1_, Sci2_, Jan_;
    int RandOrScript;
    bool checking1 = true, checking2;


    // Start is called before the first frame update
    void Start()
    {
        Guard1_ = Guard1.GetComponent<EV_Puppet_Controller>();
        Guard2_ = Guard2.GetComponent<EV_Puppet_Controller>();
        D1_ = D1.GetComponent<EV_Puppet_Controller>();
        Sci1_ = Sci1.GetComponent<EV_Puppet_Controller>();
        Sci2_ = Sci2.GetComponent<EV_Puppet_Controller>();
        Jan_ = Jan.GetComponent<EV_Puppet_Controller>();

        int Group = Random.Range(0, 3);
        int LineOrGo = Random.Range(0, 2);
        RandOrScript = Random.Range(0, 2);
        if (LineOrGo == 0)
            FinalMessage = new AudioClip[6];
        else
            FinalMessage = new AudioClip[7];

        FinalMessage[1] = DIA1[Random.Range(0, DIA1.Length)];

        if (Group == 0)
            FinalMessage[2] = SCI_1[Random.Range(0, SCI_1.Length)];
        if (Group == 1)
            FinalMessage[2] = CRE_1[Random.Range(0, CRE_1.Length)];
        if (Group == 2)
            FinalMessage[2] = SEC_1[Random.Range(0, SEC_1.Length)];

        if (LineOrGo == 0)
        {
            FinalMessage[3] = NUMBERS[9];
            FinalMessage[4] = NUMBERS[Random.Range(0, NUMBERS.Length-1)];
            FinalMessage[5] = ONOFF[1];
        }
        else
        {
            FinalMessage[6] = ONOFF[1];

            FinalMessage[3] = DIA2[Random.Range(0, DIA2.Length)];

            if (Group == 0)
                FinalMessage[4] = SCI_2[Random.Range(0, SCI_2.Length)];
            if (Group == 1)
                FinalMessage[4] = CRE_2[Random.Range(0, CRE_2.Length)];
            if (Group == 2)
                FinalMessage[4] = SEC_2[Random.Range(0, SEC_2.Length)];

            if (Group == 0)
                FinalMessage[5] = SCI_3[Random.Range(0, SCI_3.Length)];
            if (Group == 1)
                FinalMessage[5] = CRE_3[Random.Range(0, CRE_3.Length)];
            if (Group == 2)
                FinalMessage[5] = SEC_3[Random.Range(0, SEC_3.Length)];


        }

        FinalMessage[0] = ONOFF[0];


    }

    // Update is called once per frame
    void Update()
    {
        if (checking1 == true)
        {
            if (check1.GetComponent<BoxTrigger>().GetState())
            {
                if (RandOrScript == 0)
                    Guard1_.SetSeq(FinalMessage);
                else
                    Guard1_.PlaySound(SCRIPTED[Random.Range(0, SCRIPTED.Length)]);

                Guard2_.SetPath(path1);
                D1_.SetPath(path1);
                Guard2_.AnimTrigger(1, true);
                Sci1_.AnimTrigger(2, true);
                Sci1_.SetPath(path2);
                checking1 = false;
                checking2 = true;
            }
        }

        if (checking2 == true)
        {
            if (check2.GetComponent<BoxTrigger>().GetState())
            {
                Jan_.AnimTrigger(2, true);
                Jan_.SetPath(path3);
                Sci2_.PlaySound(Scientistdiag);
                checking2 = false;
            }
        }
    }

}
