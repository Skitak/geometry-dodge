using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public GameObject m_playerPrefab;

    void Start()
    {
        if (m_playerPrefab)
            Instantiate(m_playerPrefab, RandomPosition(), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private Vector3 RandomPosition()
    {
        return new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0);
    }
}
