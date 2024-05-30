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
        string netTypeStr = isClient ? "Ŭ��" : "Ŭ��ƴ�";

        TextMesh_NetType.text = this.isLocalPlayer ? $"[����/{netTypeStr}] {this.netId}" : 
            $"[���þƴ�{netTypeStr}] {this.netId}";

        SetHealthBarOnUpdate(_health);

        if (CheckIsFocusedOnUpdate() == false)
            return;

        // ���ø����̼��� ��Ŀ�� �Ǿ� �ִٸ� ȣ�� 
        CheckIsLocalPlayerOnUpdate();
    }

    private void SetHealthBarOnUpdate(int health)
    {
        TextMesh_HealthBar.text = new string('-', health);
    }

    private bool CheckIsFocusedOnUpdate()
    {
        // ���� â�� Ȱ��ȭ �Ǿ� �ִ���
        return Application.isFocused;
    }

    private void CheckIsLocalPlayerOnUpdate()
    {
        if (this.isLocalPlayer == false)
            return;

        // �����÷��̾��� ȸ��
        float horizontal = Input.GetAxis("Horizontal");
        transform.Rotate(0, horizontal * _rotationSpeed * Time.deltaTime, 0);

        // ���� �÷��̾��� �̵�
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

        // ����
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

    // ���콺 ��ġ�� ������� ����ĳ��Ʈ�� ���� -> �÷��̾� ȸ�� 
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

    // Ŭ�󿡼� ������ ȣ���� ������ ������ ������ �������̵� �¸�
    [Command]
    // �ߴ��� �ɾ����� �������̵忡���� 
    private void CommandAtk()
    {
        GameObject atkObjectForSpawn = Instantiate(Prefab_AtkObject, Transform_AtkSpawnPos.transform.position, Transform_AtkSpawnPos.transform.rotation);
        NetworkServer.Spawn(atkObjectForSpawn);

        RpcOnAtk();
    }

    // �������� ȣ������� Ŭ���̾�Ʈ���� ����Ǵ� RPC �޼���
    [ClientRpc]
    private void RpcOnAtk()
    {
        Debug.LogWarning($"{this.netId}�� RPC ��");
        Animator_Player.SetTrigger("Atk");
    }

    // Ŭ�󿡼� ���� �Լ��� ������� �ʵ��� ServerCallback�� �޾���
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
