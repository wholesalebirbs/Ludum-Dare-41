﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerNumber
{
    One,
    Two,
    Three,
    Four,
}

public class Player : MonoBehaviour, IHealthInterface, ITarget {

    private bool playerOffRoad;
    public float ACCELERATION;
	public float MAXSPEED;
	public float ROTATION;
    public float originalSpeed;
    public float lapCompletePlayerHealthIncrease;

    [SerializeField]
    public float _totalHealth = 100;
    public float TotalHealth
    {
        get
        {
            return _totalHealth;
        }

        set
        {
            _totalHealth = value;
        }
    }
    public float _currentHealth;
    public float CurrentHealth
    {
        get
        {
            return _currentHealth;
        }

        set
        {
            _currentHealth = value;
        }
    }

    public Transform Position
    {
        get
        {
            return transform;
        }

    }

    public PlayerNumber PlayerNUmber
    {
        get
        {
            return _id;
        }

    }
    public SpriteRenderer carImage;
	public PlayerNumber _id;

	public float pickupDragDistance;

    //private bool isHitBullet;
    //private bool isHitMissile;


    [SerializeField]
    private bool playerOffMap;

    private bool passedCheckpoint1 = false;
    private bool passedCheckpoint2 = false;
    private bool passedCheckpoint3 = false;

	private float speed = 0;
    private float offTrackSpeed;
	private Rigidbody2D rb;
	private GameObject pickup;

    // Use this for initialization
    void Start ()
    {
        _currentHealth = _totalHealth;
		rb = GetComponent<Rigidbody2D>();
		var spriteRenderer = GetComponent<SpriteRenderer>();
	}

    //Checks to see if the player is on the road
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "offRoad")
        {
            playerOffRoad = true;
        }
        if (col.tag == "pickup")
        {
            if (pickup != null) return;
            pickup = col.gameObject;

            pickup.GetComponent<Collider2D>().enabled = false;
        }

        if (col.tag == "checkpoint0" && passedCheckpoint3){
           passedCheckpoint1 = false;
           passedCheckpoint2 = false;
           passedCheckpoint3 = false;
           //GameObject.Find("Tower " + _id).GetComponent<Tower>().AddHealthLapComplete();
           GameEventHandler.CallOnLapComplete(this);
        }
        if (col.tag == "checkpoint1" && !passedCheckpoint1){
            passedCheckpoint1 = true;
        }
        if (col.tag == "checkpoint2" && !passedCheckpoint2 && passedCheckpoint1){
            passedCheckpoint2 = true;
        }
        if (col.tag == "checkpoint3" && !passedCheckpoint3 && passedCheckpoint2){
            passedCheckpoint3 = true;
        }
    }
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.tag == "offRoad")
        {
            playerOffRoad = false;
        }
    }
    //checks to see if the player is in the playable area
    private void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.tag == "offMap")
        {
           playerOffMap = true;
        }
    }

    void FixedUpdate()
    {
        //Store the current horizontal input in the float moveHorizontal.
        float vertical = Mathf.Round(-Input.GetAxis (_id.ToString() + " Left Y Axis"));
		if (vertical != 0) speed += vertical * ACCELERATION;
		else {
			if (speed > 0 && speed < ACCELERATION) speed = 0;
			else if (speed < 0 && speed > -ACCELERATION) speed = 0;
			else if (speed < 0) speed += ACCELERATION;
			else speed -= ACCELERATION;
		}

		if (speed > MAXSPEED) speed = MAXSPEED;
		if (speed < -MAXSPEED) speed = -MAXSPEED;

        //Store the current vertical input in the float moveVertical.
        float horizontal = Mathf.Round(-Input.GetAxis (_id.ToString() + " Right X Axis"));
        //Debug.Log(horizontal * ROTATION);
		rb.rotation += horizontal * ROTATION;

		rb.velocity = new Vector2 (transform.up.x, transform.up.y).normalized * speed;

		if (pickup != null)
        {
			pickup.transform.position = transform.position + transform.up * pickupDragDistance;
			pickup.transform.rotation = Quaternion.Euler(0, 0, rb.rotation);
		}

		if (Input.GetAxis(_id + " Right Trigger") > 0.5 && pickup != null)
        {
            Debug.Log("Player " + _id + "Spawned a turret");
            GameObject t = ObjectPooler.Instance.GetPooledGameObject(PooledObjectType.Turret);
            if (t != null)
            {
                t.GetComponent<Turret>().Initialize(pickup.transform.position, _id);
                //Instantiate(turret, pickup.transform.position, Quaternion.identity);
                //Destroy(pickup);

                if (pickup != null)
                {
                    pickup.GetComponent<Pickup>().Destroy();
                    pickup = null;
                }
                
            }
            else
            {
                Debug.Log("Something went wrong, Player " + _id.ToString() + "could not spawn turret");
            }

		}
        if (playerOffMap == true)
        {
            //rb.velocity = new Vector2(0, 0);
            //speed = speed / -2;
            //rb.angularVelocity = 0;
            playerOffMap = false;
        }
        else rb.velocity = new Vector2(transform.up.x, transform.up.y).normalized * (playerOffRoad == true ? speed * 0.5f : speed);

    }


    public void TakeDamage(int damage, PlayerNumber number)
    {
        if (_id == number)
        {
            return;
        }

        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        GameEventHandler.CallOnPlayerDeath(this);
    }

    public void Initialize(Vector3 position, Sprite vehicleSprite)
    {
        _currentHealth = _totalHealth;
        transform.position = position;
        carImage.sprite = vehicleSprite;
    }
}
