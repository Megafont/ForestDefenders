using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    public float WalkSpeed = 5f;
    public float RunSpeed = 10f;
    public float TurnSpeed = 5f; // This is in degrees.

    public float AttackPower = 10f;
    public float AttackCooldown = 0.5f;

    public float RaycastLength = 1f;


    private Animator _Animator;
    private Rigidbody _Rigidbody;

    private Vector3 _Velocity;
    private float _TurnRate;
    private float _AttackCooldownRemainingTime;

    private bool _IsBuildModeActive;
    private GameObject _BuildObjectGhostPrefab;
    private GameObject _BuildObjectGhost;
    private GameObject _BarricadePrefab;
    private GameObject _BarricadesParent;
    private Vector3 _BuildGhostOffset = Vector3.forward * 2f + Vector3.up * 0.5f;
    private float _LastBuildTime;

    private int _WoodCount;
    private const int _AverageWoodPerHit = 5;

    private RadialMenu _RadialMenu;


    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Health>().OnDeath += OnDeath;

        _Animator = GetComponent<Animator>();    
        _Rigidbody = GetComponent<Rigidbody>();


        _BarricadePrefab = Resources.Load<GameObject>("Test Objects/Barricade");        
        _BuildObjectGhostPrefab = Resources.Load<GameObject>("Test Objects/Barricade Ghost");
        _BuildObjectGhost = Instantiate(_BuildObjectGhostPrefab,
                                        transform.position + _Rigidbody.transform.forward,
                                        Quaternion.identity,
                                        transform);
        _BuildObjectGhost.SetActive(false);
        _BarricadesParent = GameObject.Find("Barricades");


        GameObject obj = GameObject.Find("Radial Menu");
        if (!obj)
            throw new System.Exception("The radial menu GameObject was not found!");
        else
        {
            _RadialMenu = obj.GetComponent<RadialMenu>();
            if (_RadialMenu == null)
                throw new System.Exception("The radial menu GameObject does not have a RadialMenu component!");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (_AttackCooldownRemainingTime > 0)
        {
            _AttackCooldownRemainingTime -= Time.deltaTime;
        }


        if (_IsBuildModeActive)
        {
            _BuildGhostOffset = _Rigidbody.position + _Rigidbody.transform.forward * 2f;
            _BuildGhostOffset = new Vector3(_BuildGhostOffset.x, 0.5f, _BuildGhostOffset.z);
            _BuildObjectGhost.transform.position = _BuildGhostOffset;
        }
    }

    private void FixedUpdate()
    {
        _Rigidbody.MovePosition(_Rigidbody.position + _Velocity * Time.fixedDeltaTime);


        if (_TurnRate != 0)
        {
            Quaternion q = _Rigidbody.rotation;
            q.eulerAngles = new Vector3(q.eulerAngles.x, q.eulerAngles.y + _TurnRate, q.eulerAngles.z);
            //Debug.Log($"Angles: {q.eulerAngles}");            
            _Rigidbody.MoveRotation(q);

            _TurnRate = 0;
        }
    }
    /*
    public void OnMove(InputAction.CallbackContext value)
    {
        Vector2 moveValue = value.ReadValue<Vector2>();

        //Debug.Log($"Player move input: {movement}   Magnitude: {movement.magnitude}");

        _Animator.SetFloat("X", moveValue.x);
        _Animator.SetFloat("Y", moveValue.y);

        float speed;
        if (moveValue.magnitude <= 0.75f)
            speed = WalkSpeed;
        else
            speed = RunSpeed * moveValue.magnitude;

        //Debug.Log($"Walk: {WalkSpeed}    Run: {RunSpeed}    Speed: {speed}");

        _Velocity = new Vector3(moveValue.x, 0, moveValue.y) * speed;
        _Velocity = _Rigidbody.rotation * _Velocity;

        //Debug.Log($"Velocity: {_Velocity}");
    }

    public void OnTurn(InputAction.CallbackContext value)
    {
        Vector2 lookValue = value.ReadValue<Vector2>();

        _TurnRate = TurnSpeed * lookValue.x;
        //Debug.Log($"Turn rate {_TurnRate}");
    }

    public void OnAttack(InputAction.CallbackContext value)
    {
        if (!_IsBuildModeActive)
            DoAttack();
        else
            DoBuildAction();

    }

    public void OnToggleBuild(InputAction.CallbackContext value)
    {
        if (value.started)
            _IsBuildModeActive = true;

        if (value.canceled)
            _IsBuildModeActive = false;

        if (_IsBuildModeActive)
        {
            _RadialMenu.ShowRadialMenu();
        }

        _BuildObjectGhost.SetActive(_IsBuildModeActive);

        //Debug.Log($"Build: " + _IsBuildModeActive);
    }
    */
    private void DoAttack()
    {
        if (_AttackCooldownRemainingTime > 0)
            return;

        _AttackCooldownRemainingTime = AttackCooldown;

        _Animator.ResetTrigger("Attack 1");
        _Animator.ResetTrigger("Attack 2");
        _Animator.ResetTrigger("Attack 3");

        int n = Random.Range(1, 3);
        _Animator.SetTrigger("Attack " + n);

        RaycastHit[] raycastHits = Physics.SphereCastAll(_Rigidbody.transform.position + _Rigidbody.transform.forward * 0.75f, 1.0f, _Rigidbody.transform.forward, 0.1f);
        {
            foreach (RaycastHit hit in raycastHits)
            {
                Health health = hit.collider.GetComponent<Health>();
                if (health)
                {
                    if (hit.collider.tag == "Tree")
                    {
                        _WoodCount += _AverageWoodPerHit + Random.Range(-2, 2);
                        Debug.Log("Hit tree!");
                    }
                    else if (hit.collider.tag == "Enemy")
                    {
                        health.TakeDamage(AttackPower);
                    }
                }
                else
                {
                    if (hit.collider.tag == "Building")
                    {
                        DoDestroyAction(hit.collider.gameObject);
                    }

                }
            } // end foreach hit

        }

    }

    private void DoBuildAction()
    {
        if (_BuildObjectGhost.GetComponent<BuildingConstructionGhost>().CanBuild &&
            Time.time - _LastBuildTime >= 0.1f)
        {
            Instantiate(_BarricadePrefab, _BuildGhostOffset, _Rigidbody.rotation, _BarricadesParent.transform);
            _LastBuildTime = Time.time;
        }
        else
        {
            //Debug.LogError("Can't build. Something's in the way!");
        }
    }

    private void DoDestroyAction(GameObject objToDestroy)
    {
        Destroy(objToDestroy);
    }

    private void OnDeath(GameObject sender)
    {
        Destroy(gameObject);
    }
}
