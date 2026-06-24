using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetBoolBehavior : StateMachineBehaviour
{
    [Header("Tên Bool Parameter")]
    [Tooltip("Tên của bool parameter trong Animator Controller")]
    public string boolName;


    [Header("Update giá trị Bool")]
    [Tooltip("Nếu true, sẽ cập nhật giá trị khi vào/thoát State Machine")]
    public bool updateOnState;


    [Tooltip("Giá trị bool khi vào state/state machine")]
    public bool updateOnStateMachine;



    [Header("Giá trị Bool")]
    [Tooltip("Nếu true, sẽ cập nhật giá trị khi vào/thoát State")]
    public bool valueOnEnter;


    [Tooltip("Giá trị bool khi thoát state/state machine")]
    public bool valueOnExit;



    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!updateOnState)
        {
            animator.SetBool(boolName, valueOnEnter);
        }
    }



    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!updateOnState)
        {
            animator.SetBool(boolName, valueOnExit);
        }
    }



    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        if (updateOnStateMachine)
        {
            animator.SetBool(boolName, valueOnEnter);
        }
    }




    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        if (updateOnStateMachine)
        {
            animator.SetBool(boolName, valueOnExit);
        }
    }
}
