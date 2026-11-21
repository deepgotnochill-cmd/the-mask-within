using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
	public enum EnemyState
	{
		Patrolling = 0,
		Chasing = 1,
		Searching = 2,
		Attacking = 3,
		Investigating = 4
	}

	public EnemyState currentState;

	public Transform[] waypoints;

	public float detectionRadius = 10f;

	public float attackRadius = 2f;

	public float searchDuration = 5f;

	public float hearingRadius = 15f;

	public float patrolSpeed = 2f;

	public float chaseSpeed = 4f;

	public int damage = 10;

	public float memoryDuration = 3f;

	public AudioClip walkingFootstep;

	public AudioClip runningFootstep;

	public float footstepDistanceWalking = 1.5f;

	public float footstepDistanceRunning = 1f;

	public GameObject objectToActivate1;

	public GameObject objectToActivate2;

	public GameObject objectToActivate3;

	public string sceneToLoad;

	public GameObject musicGameObject;

	private int currentWaypointIndex;

	private Transform player;

	private NavMeshAgent agent;

	private Vector3 lastSeenPosition;

	private float searchTimer;

	private float memoryTimer;

	private Animator animator;

	private AudioSource audioSource;

	private AudioSource musicSource;

	private float distanceTraveled;

	private Vector3 lastPosition;
	private bool caughtPlayer;

	public GameObject Pausemenu;

	private void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
		agent = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();
		musicSource = musicGameObject.GetComponent<AudioSource>();
		lastPosition = base.transform.position;
		Patrol();
	}

	private void Update()
	{
		if (caughtPlayer || Time.timeScale <= 0f || player == null || !player.gameObject.activeInHierarchy || !agent.isOnNavMesh) return;
		CheckStateTransitions();
		switch (currentState)
		{
		case EnemyState.Patrolling:
			Patrol();
			break;
		case EnemyState.Chasing:
			Chase();
			break;
		case EnemyState.Searching:
			Search();
			break;
		case EnemyState.Attacking:
			Attack();
			break;
		case EnemyState.Investigating:
			Investigate();
			break;
		}
		HandleFootsteps();
		animator.SetFloat("Speed", agent.velocity.magnitude);
	}

	private void CheckStateTransitions()
	{
		float num = Vector3.Distance(player.position, base.transform.position);
		if (num <= attackRadius)
		{
			currentState = EnemyState.Attacking;
		}
		else if (num <= detectionRadius && CanSeePlayer())
		{
			currentState = EnemyState.Chasing;
			lastSeenPosition = player.position;
			memoryTimer = memoryDuration;
			if (!musicSource.isPlaying)
			{
				musicSource.Play();
			}
		}
		else if (currentState == EnemyState.Chasing)
		{
			memoryTimer -= Time.deltaTime;
			if (memoryTimer <= 0f)
			{
				currentState = EnemyState.Searching;
				searchTimer = searchDuration;
				if (musicSource.isPlaying)
				{
					musicSource.Stop();
				}
			}
		}
		else if (currentState == EnemyState.Attacking)
		{
			currentState = EnemyState.Searching;
			searchTimer = searchDuration;
		}
		else if (currentState == EnemyState.Searching && searchTimer <= 0f)
		{
			currentState = EnemyState.Patrolling;
			if (musicSource.isPlaying)
			{
				musicSource.Stop();
			}
		}
	}

	private void Patrol()
	{
		agent.isStopped = false;
		if (waypoints.Length != 0)
		{
			agent.speed = patrolSpeed;
			agent.destination = waypoints[currentWaypointIndex].position;
			if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
			{
				currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
			}
		}
	}

	private void Chase()
	{
		agent.isStopped = false;
		agent.speed = chaseSpeed;
		agent.destination = lastSeenPosition;
		if (CanSeePlayer())
		{
			lastSeenPosition = player.position;
			memoryTimer = memoryDuration;
		}
	}

	private void Search()
	{
		agent.isStopped = false;
		agent.speed = patrolSpeed;
		agent.destination = lastSeenPosition;
		// Searching must expire even when the last heard point is behind a closed door.
		searchTimer -= Time.deltaTime;
	}

	private void Attack()
	{
		agent.isStopped = true;
		animator.SetFloat("Speed", 0f);
	}

	private void Investigate()
	{
		agent.isStopped = false;
		agent.speed = chaseSpeed;
		agent.destination = lastSeenPosition;
		searchTimer -= Time.deltaTime;
		if (searchTimer <= 0f || (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f))
		{
			currentState = EnemyState.Patrolling;
		}
	}

	private bool CanSeePlayer()
	{
		Vector3 normalized = (player.position - base.transform.position).normalized;
		if (Physics.Raycast(base.transform.position, normalized, out var hitInfo, detectionRadius) && hitInfo.transform == player)
		{
			return true;
		}
		return false;
	}

	public void HearSound(Vector3 soundPosition)
	{
		if (caughtPlayer || agent == null || !agent.isOnNavMesh || currentState == EnemyState.Chasing || currentState == EnemyState.Attacking) return;
		if (Vector3.Distance(soundPosition, base.transform.position) <= hearingRadius)
		{
			lastSeenPosition = soundPosition;
			currentState = EnemyState.Investigating;
			searchTimer = searchDuration;
			agent.speed = chaseSpeed;
		}
	}

	private void HandleFootsteps()
	{
		float magnitude = agent.velocity.magnitude;
		distanceTraveled += Vector3.Distance(base.transform.position, lastPosition);
		lastPosition = base.transform.position;
		if (magnitude > 0f)
		{
			float num = ((agent.speed == patrolSpeed) ? footstepDistanceWalking : footstepDistanceRunning);
			if (distanceTraveled >= num)
			{
				AudioClip clip = ((agent.speed == patrolSpeed) ? walkingFootstep : runningFootstep);
				audioSource.PlayOneShot(clip);
				distanceTraveled = 0f;
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!caughtPlayer && Time.timeScale > 0f && other.CompareTag("Player"))
		{
			caughtPlayer = true;
			Pausemenu.SetActive(value: false);
			objectToActivate1.SetActive(value: true);
			objectToActivate2.SetActive(value: true);
			objectToActivate3.SetActive(value: false);
			other.gameObject.SetActive(value: false);
			Invoke("ChangeScene", 1.7f);
		}
	}

	private void ChangeScene()
	{
		SceneManager.LoadScene(sceneToLoad);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position, detectionRadius);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(base.transform.position, hearingRadius);
	}
}
