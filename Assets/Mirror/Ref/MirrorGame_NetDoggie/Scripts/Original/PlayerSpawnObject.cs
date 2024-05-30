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
    [SyncVar(hook = nameof(OnSpeedChanged))] private float syncSpeed;

    public void Update()
    {
        string netTypeStr = isClient ? "클라" : "클라아님";

        TextMesh_NetType.text = this.isLocalPlayer ? $"[로컬/{netTypeStr}] {this.netId}" : 
            $"[로컬아님{netTypeStr}] {this.netId}";

        SetHealthBarOnUpdate(_health);

        if (CheckIsFocusedOnUpdate() == false)
            return;

        // 애플리케이션이 포커스 되어 있다면 호출 
        CheckIsLocalPlayerOnUpdate();
    }

    private void SetHealthBarOnUpdate(int health)
    {
        TextMesh_HealthBar.text = new string('-', health);
    }

    private bool CheckIsFocusedOnUpdate()
    {
        // 게임 창이 활성화 되어 있는지
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

        //NavAgent_Player.velocity = forward * Mathf.Max(vertical, 0) * NavAgent_Player.speed;
        //Animator_Player.SetBool("Moving", NavAgent_Player.velocity != Vector3.zero);

        float playerSpeed = NavAgent_Player.speed;

       

        if(Input.GetKey(KeyCode.LeftShift))
        {
            playerSpeed *= 2;
        }

        NavAgent_Player.velocity = forward * Mathf.Max(vertical, 0) * playerSpeed;

        //Animator_Player.SetBool("Moving", NavAgent_Player.velocity.sqrMagnitude > 0.1f);
        //Animator_Player.SetBool("Dashing", NavAgent_Player.velocity.sqrMagnitude > 9.0f);

        //Animator_Player.SetFloat("Speed", NavAgent_Player.velocity.magnitude);

        CmdSetSpeed(NavAgent_Player.velocity.magnitude);

        // 공격
        if (Input.GetKeyDown(_attKey))
        {
            CommandAtk();
        }


        RotateLocalPlayer();
    }

    [Command]
    private void CmdSetSpeed(float speed)
    {
        syncSpeed = speed;
    }

    private void OnSpeedChanged(float oldSpeed, float newSpeed)
    {
        Animator_Player.SetFloat("Speed", newSpeed);
    }

    // 마우스 위치를 기반으로 레이캐스트를 수행 -> 플레이어 회전 
    private void RotateLocalPlayer()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray,out RaycastHit hit, 100))
        {
            Debug.DrawLine(ray.origin, hit.point);
            Vector3 lookRotate = new Vector3(hit.point.x, Transform_Player.position.y, hit.point.z);
            Transform_Player.LookAt(lookRotate);    

        }
    }

    // 클라에서 서버로 호출은 하지만 로직의 동작은 서버사이드 온리
    [Command]
    // 중단점 걸었을때 서버사이드에서만 
    private void CommandAtk()
    {
        GameObject atkObjectForSpawn = Instantiate(Prefab_AtkObject, Transform_AtkSpawnPos.transform.position, Transform_AtkSpawnPos.transform.rotation);
        NetworkServer.Spawn(atkObjectForSpawn);

        RpcOnAtk();
    }

    // 서버에서 호출되지만 클라이언트에서 실행되는 RPC 메서드
    [ClientRpc]
    private void RpcOnAtk()
    {
        Debug.LogWarning($"{this.netId}가 RPC 옴");
        Animator_Player.SetTrigger("Atk");
    }

    // 클라에서 다음 함수가 실행되지 않도록 ServerCallback을 달아줌
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
       var atkGenObject = other.GetComponent<AtkSpawnObject>();
        if (atkGenObject == null)
            return;

        _health--;

        if(_health ==0)
        {
            NetworkServer.Destroy(this.gameObject);
        }

    }
}
