using Mirror;
using UnityEngine;

public class AtkSpawnObjectRe : NetworkBehaviour
{
    public float _destroyAfter = 10.0f;
    public float _force = 1000;

    public Rigidbody RigidBody_AtkObject;

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), _destroyAfter);
    }

    private void Start()
    {
        RigidBody_AtkObject.AddForce(transform.forward * _force);
    }

    private void DestroySelf()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    [ServerCallback]

    private void OnTriggerEnter(Collider other)
    {
        DestroySelf();
    }
}
