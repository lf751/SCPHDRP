using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ev_ZombieDeath : MonoBehaviour
{
    public Transform[] Path;
    public Transform viewTarget;
    public AudioSource scientistSound;
    public AudioClip part1, part2;
    public Animator scientist;

    float Timer;
    int state = 0;

    private void OnEnable()
    {
        GameController.ins.player.GetComponent<PlayerControl>().ForceWalk(Path);
        GameController.ins.currPly.ForceLook(viewTarget.position, 5f);
        GameController.ins.currPly.allowMove = false;
        GameController.ins.currPly.forceWalkSpeed = 0.5f;

        Timer = 1f;
    }

    private void Update()
    {
        Timer -= Time.deltaTime;

        if (Timer < 0)
        {
            switch (state)
            {
                case 0:
                    {
                        scientistSound.clip = part1;
                        scientistSound.Play();
                        Timer = 7;
                        state = 1;
                        break;
                    }
                case 1:
                    {
                        state = 2;
                        GameController.ins.currPly.FakeBlink(3);
                        Timer = 8f;
                        break;
                    }
                case 2:
                    {
                        scientistSound.clip = part2;
                        scientistSound.Play();
                        Timer = 1f;
                        state = 3;
                        break;
                    }
                case 3:
                    {
                        scientist.SetTrigger("die");
                        GameController.ins.currPly.FakeBlink(5);
                        Timer = 6f;
                        state = 4;
                        break;
                    }
                case 4:
                    {
                        state = 5;
                        GameController.ins.deathmsg = Localization.GetString("deathStrings", "death_008");
                        GameController.ins.currPly.Death(0);
                        break;
                    }

            }
        }
    }

    /* Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }*/
}
