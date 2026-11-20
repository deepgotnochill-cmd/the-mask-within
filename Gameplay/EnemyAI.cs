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

	public GameObject Pausemenu;

	private const float NearSight = 1.8f;
	private const float ViewConeDot = 0.42f;
	private Transform player;
	private CharacterController playerBody;
	private NavMeshAgent agent;
	private Animator animator;
	private AudioSource footsteps;
	private AudioSource music;
	private Vector3 lastKnownPosition;
	private Vector3 lastPosition;
	private float distanceWalked;
	private float memoryLeft;
	private float searchLeft;
	private float contactTime;
	private float lastContactTick = -10f;
	private float nextRouteUpdate;
	private int waypoint;
	private bool caught;
	public bool CanWitnessHiding => !caught && isActiveAndEnabled && player != null && CanSeePlayer();

	private void Start()
	{
		var target = GameObject.FindGameObjectWithTag("Player");
		if (target == null) { enabled = false; return; }
		player = target.transform;
		playerBody = target.GetComponent<CharacterController>();
		agent = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();
		footsteps = GetComponent<AudioSource>();
		music = musicGameObject != null ? musicGameObject.GetComponent<AudioSource>() : null;
		if (footsteps != null)
		{
			footsteps.spatialBlend = 1f;
			footsteps.dopplerLevel = 0f;
		}
		currentState = EnemyState.Patrolling;
		lastPosition = transform.position;
		if (gameObject.GetComponent<FearDirector>() == null)
			gameObject.AddComponent<FearDirector>();
	}

	private void Update()
	{
		if (caught || Time.timeScale == 0f || agent == null || !agent.isOnNavMesh || !player.gameObject.activeInHierarchy) return;

		bool seesPlayer = CanSeePlayer();
		if (seesPlayer)
		{
			lastKnownPosition = player.position;
			memoryLeft = Mathf.Clamp(memoryDuration, 1f, 4f);
			SetState(EnemyState.Chasing);
		}
		else if (currentState == EnemyState.Chasing)
		{
			memoryLeft -= Time.deltaTime;
			if (memoryLeft <= 0f || Arrived(lastKnownPosition))
				SetState(EnemyState.Searching);
		}

		if (currentState == EnemyState.Searching || currentState == EnemyState.Investigating)
		{
			searchLeft -= Time.deltaTime;
			if (searchLeft <= 0f) SetState(EnemyState.Patrolling);
		}

		bool chasing = currentState == EnemyState.Chasing;
		agent.speed = chasing ? chaseSpeed : patrolSpeed;
		if (currentState == EnemyState.Patrolling)
		{
			if (waypoints != null && waypoints.Length > 0)
			{
				var target = waypoints[waypoint];
				if (target == null || Arrived(target.position)) waypoint = (waypoint + 1) % waypoints.Length;
				else Follow(target.position);
			}
		}
		else if (Arrived(lastKnownPosition) && !seesPlayer)
		{
			if (agent.hasPath) agent.ResetPath();
			transform.Rotate(0f, Time.deltaTime * 60f, 0f);
		}
		else Follow(lastKnownPosition);

		PlaySteps(chasing);
		if (animator != null) animator.SetFloat("Speed", agent.velocity.magnitude);
	}

	private void Follow(Vector3 position)
	{
		if (Time.time < nextRouteUpdate) return;
		nextRouteUpdate = Time.time + 0.18f;
		if (NavMesh.SamplePosition(position, out var hit, 2f, agent.areaMask))
			agent.SetDestination(hit.position);
	}

	private bool Arrived(Vector3 target)
	{
		// The player's pivot sits above the floor, while the agent pivot is at its feet.
		Vector3 delta = target - transform.position;
		return Mathf.Abs(delta.y) < 1.75f && delta.x * delta.x + delta.z * delta.z < 0.6f;
	}

	private void SetState(EnemyState state)
	{
		if (currentState == state) return;
		currentState = state;
		nextRouteUpdate = 0f;
		if (state == EnemyState.Searching || state == EnemyState.Investigating)
			searchLeft = Mathf.Max(searchDuration, 4f);
		if (music != null)
		{
			if (state == EnemyState.Chasing) music.Play();
			else if (music.isPlaying) music.Stop();
		}
	}

	private bool CanSeePlayer()
	{
		if (playerBody != null && !playerBody.enabled) return false;
		Vector3 origin = transform.position + Vector3.up * 1.45f;
		Vector3 target = playerBody != null ? playerBody.bounds.center : player.position;
		Vector3 offset = target - origin;
		if (offset.sqrMagnitude > detectionRadius * detectionRadius) return false;
		Vector3 horizontal = Vector3.ProjectOnPlane(offset, Vector3.up);
		if (horizontal.sqrMagnitude > NearSight * NearSight
			&& Vector3.Dot(transform.forward, horizontal.normalized) < ViewConeDot) return false;
		int visibilityMask = Physics.DefaultRaycastLayers & ~(1 << gameObject.layer);
		if (!Physics.Linecast(origin, target, out var hit, visibilityMask, QueryTriggerInteraction.Ignore))
			return true;
		return hit.transform == player || hit.transform.IsChildOf(player);
	}

	public void HearSound(Vector3 position)
	{
		if (caught || currentState == EnemyState.Chasing || Time.timeScale == 0f) return;
		if ((position - transform.position).sqrMagnitude > hearingRadius * hearingRadius) return;
		lastKnownPosition = position;
		SetState(EnemyState.Investigating);
		searchLeft = Mathf.Max(searchDuration, 4f);
	}

	private void PlaySteps(bool chasing)
	{
		distanceWalked += Vector3.Distance(lastPosition, transform.position);
		lastPosition = transform.position;
		float stride = chasing ? footstepDistanceRunning : footstepDistanceWalking;
		if (distanceWalked < Mathf.Max(0.25f, stride)) return;
		distanceWalked = 0f;
		var clip = chasing ? runningFootstep : walkingFootstep;
		if (footsteps != null && clip != null) footsteps.PlayOneShot(clip);
	}

	private void OnTriggerStay(Collider other)
	{
		if (caught || player == null || other.transform != player || !other.isTrigger || Time.timeScale <= 0f) return;
		if (lastContactTick == Time.fixedTime) return;
		if (Time.fixedTime - lastContactTick > Time.fixedDeltaTime * 1.5f) contactTime = 0f;
		lastContactTick = Time.fixedTime;
		if (!CanSeePlayer()) { contactTime = 0f; return; }
		contactTime += Time.fixedDeltaTime;
		if (contactTime < 0.35f) return;
		caught = true;
		SetState(EnemyState.Attacking);
		if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
		if (Pausemenu != null) Pausemenu.SetActive(false);
		if (objectToActivate1 != null) objectToActivate1.SetActive(true);
		if (objectToActivate2 != null) objectToActivate2.SetActive(true);
		if (objectToActivate3 != null) objectToActivate3.SetActive(false);
		player.gameObject.SetActive(false);
		Invoke(nameof(ChangeScene), 1.7f);
	}
	private void ChangeScene() => SceneManager.LoadScene(sceneToLoad);
}

