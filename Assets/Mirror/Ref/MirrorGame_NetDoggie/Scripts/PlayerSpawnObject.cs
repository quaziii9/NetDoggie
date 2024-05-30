using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class PlayerSpawnObject : NetworkBehaviour
{
    [Header("Components")]
    public NavMeshAgent NavAgent_Player;
    public Animator Animator_Player;
    public TextMesh TextMesh_HealthBar;
    public TextMesh TextMesh_NetType;
    public Transform Transform_Player;

    [Header("Movement")]
    public float _rotationSpeed = 100.0f;

    [Header("Attack")]
    public KeyCode _attKey = KeyCode.Space;
    public GameObject Prefab_AtkObject;
    public Transform Transform_AtkSpawnPos;

    [Header("Stats Server")]
    [SyncVar] public int _health = 4;

    public void Update()
    {
        SetHealthBarOnUpdate(_health);

        if (CheckIsFocusedOnUpdate() == false)
            return;


        CheckIsLocalPlayerOnUpdate();
    }

    private void SetHealthBarOnUpdate(int health)
    {
        TextMesh_HealthBar.text = new string('-', health);
    }

    private bool CheckIsFocusedOnUpdate()
    {
        return Application.isFocused;
    }

    private void CheckIsLocalPlayerOnUpdate()
    {
        if (this.isLocalPlayer == false)
            return;

        // 로컬플레이어의 회전
        float horizontal = Input.GetAxis("Horizontal");
        transform.Rotate(0, horizontal * _rotationSpeed * Time.deltaTime, 0);

        // 로컬 플레이어의 이동
        float vertical = Input.GetAxis("Vertical");
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        NavAgent_Player.velocity = forward * Mathf.Max(vertical, 0) * NavAgent_Player.speed;
        Animator_Player.SetBool("Moving", NavAgent_Player.velocity != Vector3.zero);
        
        // 공격
        if(Input.GetKeyDown(_attKey))
        {
            CommandAtk();
        }
    }

    // 클라에서 서버로 호출은 하지만 로직의 동작은 서버사이드 온리
    [Command]
    // 중단점 걸었을때 서버사이드에서만 
    private void CommandAtk()
    {

    }

    [ClientRpc]
    private void RpcOnAtk()
    {

    }

    // 클라에서 다음 함수가 실행되지 않도록 ServerCallback을 달아줌
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        
    }



}
