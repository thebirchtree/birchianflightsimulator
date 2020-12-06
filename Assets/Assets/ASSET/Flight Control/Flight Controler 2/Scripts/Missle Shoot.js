 #pragma strict
 var Spawn : Transform;
var Missle : Rigidbody;
var waitTime : int;
var speed : int;
private var ray : Ray;

private var hit : RaycastHit;

function Update () {
    if(Input.GetKeyDown(KeyCode.Mouse1))
    {
    var instance : Rigidbody = Instantiate(Missle, Spawn.position, Spawn.rotation);
        instance.velocity = Spawn.forward * speed;
        }
ray = Camera.main.ScreenPointToRay(Input.mousePosition);

    if(Physics.Raycast(ray, hit)){
    var point: Vector3 = hit.point;

        Spawn.transform.LookAt(point);
    }
}