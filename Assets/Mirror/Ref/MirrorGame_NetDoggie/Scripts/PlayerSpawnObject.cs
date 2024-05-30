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

    }

    // Ŭ�󿡼� ������ ȣ���� ������ ������ ������ �������̵� �¸�
    [Command]
    // �ߴ��� �ɾ����� �������̵忡���� 
    private void CommandAtk()
    {

    }

    [ClientRpc]
    private void RpcOnAtk()
    {

    }

    // Ŭ�󿡼� ���� �Լ��� ������� �ʵ��� ServerCallback�� �޾���
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        
    }



}
