using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    enum PlayerStatus
    {
        Running,
        Chasing,
        Following,
        Fighting
    }
    // Start is called before the first frame update
    void Start()
    {
        var fsm = new FiniteStateMachine<PlayerStatus>();

        var run = new State<PlayerStatus>(PlayerStatus.Running, "running");
        var chase= new State<PlayerStatus>(PlayerStatus.Chasing);
        var follow = new State<PlayerStatus>(PlayerStatus.Following);
        var fight = new State<PlayerStatus>(PlayerStatus.Fighting);

        fsm.Add(run);
        fsm.Add(chase);
        fsm.Add(follow);
        fsm.Add(fight);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
