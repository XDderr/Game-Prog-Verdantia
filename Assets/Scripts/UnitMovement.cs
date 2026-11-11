using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : MonoBehaviour
{
    Camera cam;
    NavMeshAgent agent;
    public LayerMask ground;

    public bool isCommandedtoMove;

    Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();
    }
    
    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, ground)){
                isCommandedtoMove = true;
                agent.SetDestination(hit.point);
                animator.SetBool("isMoving", true);
            }
        }

        if(agent.hasPath == false || agent.remainingDistance <= agent.stoppingDistance){
            isCommandedtoMove = false;
            animator.SetBool("isMoving", false);
        }
        else{
            animator.SetBool("isMoving", true);
        }

        if(Input.GetMouseButtonDown(1)){
            unitHalt(true);
        }
    }

    public void unitHalt(bool haltInput){
        if(haltInput){
            agent.velocity = Vector3.zero;
        }
        else{
            return;
        }
        
    }
}
