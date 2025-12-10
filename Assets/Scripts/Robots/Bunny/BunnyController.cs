using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnyController : MonoBehaviour
{
    // MVC
    private BunnyModel model;
    private BunnyView view;

    // FSM y estados
    private FSM<BunnyStateEnum> fsm;
    private BrokenState<BunnyStateEnum> brokenState;
    private FollowState<BunnyStateEnum> followState;
    private WaitForJumpState<BunnyStateEnum> waitForJumpState;

    // Árbol de decisión
    private ITreeNode decisionTreeRoot;

    // Player
    [SerializeField] Rigidbody playerRB;

    // Steering
    [SerializeField] float angle = 60f;
    [SerializeField] float radius = 3f;
    [SerializeField] float timePrediction = 0.5f;
    [SerializeField] LayerMask obstacleMask;

    private ISteering steering;
    private ObstacleAvoidanceV2 obstacleAvoidance;

    private void Awake()
    {
        // Obtener referencias
        model = GetComponent<BunnyModel>();
        view = GetComponent<BunnyView>();

        // Inicializar steering y avoidance
        steering = new Pursuit(transform, playerRB, timePrediction);
        obstacleAvoidance = new ObstacleAvoidanceV2(transform, angle, radius, obstacleMask);

        // FSM
        fsm = new FSM<BunnyStateEnum>();

        brokenState = new BrokenState<BunnyStateEnum>(model, view, fsm);
        followState = new FollowState<BunnyStateEnum>(model, view, steering, obstacleAvoidance, playerRB);
        waitForJumpState = new WaitForJumpState<BunnyStateEnum>(model, view);

        // Transiciones FSM
        brokenState.AddTransition(BunnyStateEnum.Follow, followState);
        brokenState.AddTransition(BunnyStateEnum.Platform, waitForJumpState);
        followState.AddTransition(BunnyStateEnum.Platform, waitForJumpState);
        waitForJumpState.AddTransition(BunnyStateEnum.Follow, followState);

        fsm.SetInit(brokenState);

        // Árbol de decisión
        InitializeDecisionTree();
    }

    private void Update()
    {
        if (TimeTravelManager.Instance.CurrentTimeState == TimeState.L1)
        {
            view.DesactiveVisuals();
            model.DesactiveCollider();
            return;
        }
        
        if(TimeTravelManager.Instance.CurrentTimeState == TimeState.Origin)
        {
            view.ActiveVisuals();
            model.ActiveCollider();
            if (Input.GetKeyDown(KeyCode.F) && !model.IsWaitingForJump())
            {
                model.SetPlayerWantsJump(true);
            }
        }
        fsm.OnUpdate();
        decisionTreeRoot.Execute();

    }

    private void InitializeDecisionTree()
    {
        var toFollow = new ActionNode(() => fsm.Transition(BunnyStateEnum.Follow));
        var toWait = new ActionNode(() => fsm.Transition(BunnyStateEnum.Platform));
        var toBroken = new ActionNode(() => fsm.Transition(BunnyStateEnum.Broken));

        var isPlayerInJumpZone = new QuestionNode(IsPlayerJumping, toWait, toFollow);
        var isBrokenNode = new QuestionNode(IsRepaired, isPlayerInJumpZone, toBroken);

        decisionTreeRoot = isBrokenNode;
    }

    private bool IsPlayerJumping()
    {
        // cambiar por un input del player
        return model.PlayerWantsJump();
    }

    private bool IsRepaired()
    {
        return model.HasFinishedRepair(); 
    }

    // Gizmos opcionales para el avoidance
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, angle / 2, 0) * transform.forward * radius);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, -angle / 2, 0) * transform.forward * radius);
    }
}

