using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

public enum State {
    IDLE,
    CHASE,
    DEAD
}

public class MonsterController : MonoBehaviour {
    [SerializeField] private bool canAttack = true;
    [SerializeField] private LayerMask lm = 0;
    [SerializeField] private GameObject bloodPrefab = null;

    private State state = State.IDLE;
    private Animator anim = null;
    private NavMeshAgent agent = null;
    private Transform player = null;
    private int health = 100;
    private bool dieOnceGuard = false;
    
    private void Start() {
        NullChecker();
        
        GetComponentInChildren<Collider>().enabled = false;
        agent.enabled = false;
        StartCoroutine(FinishSpawnAnimation());
    }

    private void NullChecker() {
        player = FindObjectOfType<MovementController>().transform;
        if (player == null)
            throw new Exception("Cant find player on monster");
        anim = GetComponentInChildren<Animator>();
        if (anim == null)
            throw new Exception("Cant find Animator");
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            throw new Exception("Cant find NavMeshAgent");
    }

    // private bool IsObjectInCameraView() {
    //     Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
    //     bool viewCheck = GeometryUtility.TestPlanesAABB(planes, spawnChecker.GetComponentInChildren<Collider>().bounds);
    //     Physics.Linecast(spawnChecker.transform.position, camera.transform.position, out RaycastHit hit, lm);
    //     bool rayCheck = hit.collider == null;
    //     return viewCheck && rayCheck;
    // }

    private void Update() {
        Chase();
    }

    private IEnumerator FinishSpawnAnimation() {
        yield return new WaitForSeconds(2.5f);
        GetComponentInChildren<Collider>().enabled = true;
        agent.enabled = true;

        UpdateState(State.CHASE);
    }
    
    private void Chase() {
        if (state != State.CHASE)
            return;
        agent.SetDestination(player.position);

        Vector3 dir = transform.position - player.transform.position;
        Ray forwardRay = new Ray(transform.position + new Vector3(0, 1, 0), -dir.normalized * 2f);
        Debug.DrawRay(forwardRay.origin, forwardRay.direction, Color.green);
        if (Physics.Raycast(forwardRay, out RaycastHit hit, 2f, lm)) {
            if (!canAttack)
                return;
            Attack();
        }
    }

    public void UpdateHealth(int amount) {
        health += amount;
        if(health <= 0)
            Die(true);
    }

    private void Attack() {
        GameController.Instance.UpdateHealth(-10);
        Die(false);
    }

    private void Die(bool fromPlayer) {
        if(dieOnceGuard == true)
            return;
        if(fromPlayer)
            GameController.Instance.UpdateScore(10);
        dieOnceGuard = true;
        UpdateState(State.DEAD);
        GameObject blood = Instantiate(bloodPrefab);
        blood.transform.SetPositionAndRotation(transform.position + new Vector3(0, 1, 0), Quaternion.identity);
        PickupSpawnController.Instance.CheckSpawnChance(transform.position);
        FindObjectOfType<MonsterSpawnerController>().RemoveListedMonster(gameObject);
        Destroy(gameObject);
    }

    private void UpdateState(State newState) {
        state = newState;
        anim.SetBool("Chasing", state == State.CHASE);
        agent.SetDestination(state == State.CHASE ? player.position : transform.position);
    }
}