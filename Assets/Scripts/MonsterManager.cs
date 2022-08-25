using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;


public class MonsterManager : MonoBehaviour
{
    public GameObject MonsterPrefab;

    public int MonstersLeft { get { return _Monsters.Count; } }
    public int WaveNumber { get; private set; }


    private GameObject _MonstersParent;
    private float _LastWaveTime;

    private List<GameObject> _Monsters;


    // Start is called before the first frame update
    void Start()
    {
        _Monsters = new List<GameObject>();

        _MonstersParent = GameObject.Find("Monsters");
        _LastWaveTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BeginNextWave()
    {
        WaveNumber++;
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        GameObject playerBase = GameObject.Find("Player Base");

        for (int i = 0; i < 5; i++)
        {
            GameObject monster = Instantiate(MonsterPrefab, transform.position, Quaternion.identity, _MonstersParent.transform);
            monster.GetComponent<TestEnemy>().SetTarget(playerBase);
            monster.GetComponent<Health>().OnDeath += OnDeath;
            _Monsters.Add(monster);

            yield return new WaitForSeconds(2.0f);
        }

    }

    private void OnDeath(GameObject sender)
    {
        _Monsters.Remove(sender);        
    }
}
