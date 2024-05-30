using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class PlayerSpawnObjectRe : NetworkBehaviour
{
    [Header("Components")]
    public NavMeshAgent NavAgent_Player;
    public Animator Animator_Player;
    public TextMesh TextMesh_HealthBar;
    public TextMesh TextMesh_NetType;
    public Transform Transform_Player;

    [Header("Movement")]
    public float _rotationSPEED = 100.0f;

    [Header("Attack")]
    public KeyCode _attKey = KeyCode.Space;
    public GameObject prefab_AtkObject;
    public Transform Transform_AtkSpawnPos;

    [Header("Stats Server")]
    [SyncVar] public int _health = 4;

    public void Update()
    {
        string netTypeStr = isClient ? "Å¬¶ó" : "Å¬¶ó¾Æ´Ô";

        TextMesh_NetType.text = this.isLocalPlayer ? $"[·ÎÄÃ/{netTypeStr}] {this.netId}" :
            $"[·ÎÄÃ¾Æ´Ô{netTypeStr}] {this.netId}";

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

        float horizontal = Input.GetAxis("Horizontal");
        transform.Rotate(0, horizontal * _rotationSPEED * Time.deltaTime, 0);

        float vertical = Input.GetAxis("Vertical");
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        NavAgent_Player.velocity = forward * Mathf.Max(vertical, 0) * NavAgent_Player.speed;
        Animator_Player.SetBool("Moving", NavAgent_Player.velocity != Vector3.zero);

        if(Input.GetKeyDown(_attKey))
        {
            CommandAtk();
        }
        RotateLocalPlayer();
    }

    private void RotateLocalPlayer()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit, 100))
        {
            Debug.DrawLine(ray.origin, hit.point);
            Vector3 lookRotate = new Vector3(hit.point.x, Transform_Player.position.y, hit.point.z);
            Transform_Player.LookAt(lookRotate);
        }
    }

    [Command]
    private void CommandAtk()
    {
        GameObject atkObjectForSpawn = Instantiate(prefab_AtkObject, Transform_AtkSpawnPos.transform.position, Transform_AtkSpawnPos.transform.rotation);
        NetworkServer.Spawn(atkObjectForSpawn);

        RpcOnAtk();          
    }

    private void RpcOnAtk()
    {
        Debug.LogWarning($"{this.netId}°¡ RPC¿È");
        Animator_Player.SetTrigger("Atk");
    }


    [ServerCallback]


    private void OnTriggerEnter(Collider other)
    {
        var atkGenObject = other.GetComponent<AtkSpawnObject>();
        if (atkGenObject == null)
            return;

        _health--;

        if(_health == 0)
        {
            NetworkServer.Destroy(this.gameObject);
        }
    }

}
