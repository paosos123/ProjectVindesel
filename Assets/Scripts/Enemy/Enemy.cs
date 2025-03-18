using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 public class Enemy : MonoBehaviour
{
   [SerializeField] private int maxHp ;
    protected int currentHp;
    // Start is called before the first frame update
    void Start()
    {
        currentHp = maxHp;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
