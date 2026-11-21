using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

// Compiled only into the isolated test player. Interact replaces E; collisions run in PhysX.
public sealed class AtmosphereCheck : MonoBehaviour
{
    public static bool Interact;
    static bool started;
    int errors;
    int legacyClothDiagnostics;
    const bool VisualReview = false;
    float deadline;
    public static void Boot() {
        if (started) return;
        started=true;
        var go=new GameObject("Atmosphere verification"); DontDestroyOnLoad(go);
        go.AddComponent<AtmosphereCheck>();
    }
    void Awake() { Application.logMessageReceived += Log; deadline=Time.realtimeSinceStartup+140f; }
    void Log(string text,string stack,LogType type) {
        if (text.StartsWith("CollisionMeshData couldn't be created") || text.StartsWith("Cannot initialize Cloth")) { legacyClothDiagnostics++; return; }
        if(type==LogType.Exception || type==LogType.Error) errors++;
    }
    void Update() { if(Time.realtimeSinceStartup>deadline) { Debug.LogError("QA TIMEOUT"); Application.Quit(3); } }
    static T One<T>() where T:Component {
        foreach(var item in Resources.FindObjectsOfTypeAll<T>()) if(item.gameObject.scene==SceneManager.GetActiveScene()) return item;
        throw new Exception("Missing "+typeof(T).Name);
    }
    static object Get(object obj,string name) { return obj.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(obj); }
    static object Call(object obj,string name,params object[] args) { return obj.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(obj,args); }
    static void Set(object obj,string name,object value) { obj.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(obj,value); }
    static void Check(bool ok,string text) { if(!ok) { Debug.LogError("FAIL: "+text); Application.Quit(2); throw new Exception(text); } Debug.Log("PASS: "+text); }
    static IEnumerator PhysicsFrames() { for(int i=0;i<5;i++) yield return new WaitForFixedUpdate(); }
    static void Position(CharacterController body,Vector3 position) {
        var rb=body.GetComponent<Rigidbody>();
        if(rb) rb.isKinematic=true;
        body.enabled=false; body.transform.position=position; body.enabled=true;
        if(rb) rb.position=position;
        Physics.SyncTransforms();
    }
    static void AtTrigger(CharacterController body,Component target) {
        Collider trigger=Array.Find(target.GetComponents<Collider>(),c=>c.isTrigger);
        Check(trigger!=null,"trigger on "+target.name);
        Position(body,trigger.bounds.center-(body.bounds.center-body.transform.position));
    }
    static void ButtonTo(string destination) {
        foreach(var button in FindObjectsOfType<SimpleSceneTransition>()) if(button.sceneToLoad==destination) { button.transitionButton.onClick.Invoke(); return; }
        throw new Exception("No button to "+destination);
    }
    static IEnumerator Scene(string name) { while(SceneManager.GetActiveScene().name!=name) yield return null; yield return null; yield return null; yield return new WaitForSecondsRealtime(.2f); }
    IEnumerator Start() {
        Application.runInBackground=true;
        yield return new WaitForSecondsRealtime(.5f);
        Check(SceneManager.GetActiveScene().name=="menu","original title scene");
        ButtonTo("SampleScene"); yield return Scene("SampleScene");
        var player=One<PlayerController>(); var body=player.GetComponent<CharacterController>(); var origin=player.transform.position;
        var dread=One<HouseDread>();
        Check(FindObjectsOfType<HouseDread>().Length==1,"one atmosphere controller");
        foreach(var volume in FindObjectsOfType<PostProcessVolume>()) {
            if(volume.profile.TryGetSettings<MotionBlur>(out var blur)) Check(!blur.enabled.value,"motion blur disabled");
            if(volume.profile.TryGetSettings<Grain>(out var grain)) Check(!grain.enabled.value,"grain disabled");
        }
        var pause=One<Pause>(); Call(pause,"PauseGame");
        Check(Time.timeScale==0 && !player.gameObject.activeInHierarchy,"pause freezes player");
        yield return new WaitForSecondsRealtime(.25f); pause.resumeButton.onClick.Invoke(); yield return null;
        Check(Time.timeScale==1 && player.gameObject.activeInHierarchy,"resume restores player");
        player.enabled=false;
        var hunter=One<EnemyAI>();
        var entry=One<triggerEvent>(); AtTrigger(body,entry); yield return PhysicsFrames();
        Check(entry.object1.activeSelf && entry.object2.activeSelf,"front door reveals original objective");
        hunter.gameObject.SetActive(false);
        var crank=One<Crank>(); AtTrigger(body,crank); yield return PhysicsFrames();
        Check((bool)Get(crank,"isPlayerInZone"),"physics enters crank zone");
        var sphere=player.GetComponent<SphereCollider>(); var sphereCenter=sphere.center;
        sphere.center+=Vector3.right*100f; Physics.SyncTransforms(); yield return PhysicsFrames();
        Check((bool)Get(crank,"isPlayerInZone"),"crank survives one player collider leaving");
        sphere.center=sphereCenter; Physics.SyncTransforms(); yield return PhysicsFrames();
        Interact=true; yield return new WaitForSeconds(crank.crankDuration*.4f); Interact=false; yield return null;
        Check(!crank.key.activeSelf,"interrupted crank does not unlock");
        Interact=true; yield return new WaitForSeconds(crank.crankDuration+.2f); Interact=false;
        Check(crank.key.activeSelf && crank.objectToActivate.activeSelf,"held interaction completes crank");
        var key=One<KeyPickup>(); AtTrigger(body,key); yield return PhysicsFrames();
        Check((bool)Get(key,"canPickup"),"physics enters key zone");
        sphere.center+=Vector3.right*100f; Physics.SyncTransforms(); yield return PhysicsFrames();
        Check((bool)Get(key,"canPickup"),"key survives one player collider leaving");
        sphere.center=sphereCenter; Physics.SyncTransforms(); yield return PhysicsFrames();
        Interact=true; yield return null; yield return null; Interact=false;
        Check(key.transform.parent==key.keyHolder,"key attaches to player");
        var door=One<DoorOpener>(); AtTrigger(body,door); yield return PhysicsFrames();
        Check(!door.door.activeSelf && door.door1.activeSelf,"carried key unlocks exit through physics");
        var exit=One<SceneTransition>(); AtTrigger(body,exit); yield return Scene("win");
        Check(SceneManager.GetActiveScene().name=="win","escape loads original win scene");
        foreach(var b in FindObjectsOfType<SimpleSceneTransition>()) Debug.Log("WIN BUTTON "+b.sceneToLoad);
        // Scene resets are exercised independently from the original win menu's chosen destination.
        SceneManager.LoadScene("SampleScene"); yield return Scene("SampleScene");
        Check(!One<Crank>().key.activeSelf,"fresh run resets puzzle");
        Check(FindObjectsOfType<HouseDread>().Length==1,"restart does not duplicate atmosphere");
        player=One<PlayerController>(); body=player.GetComponent<CharacterController>(); player.enabled=false;
        hunter=One<EnemyAI>(); hunter.gameObject.SetActive(true); yield return null; yield return null;
        var agent=hunter.GetComponent<NavMeshAgent>(); Check(agent.isOnNavMesh,"hunter is on baked navigation");
        // Check a real patrol, hearing, and recovery after leaving attack state.
        Position(body,origin); hunter.detectionRadius=.01f; hunter.attackRadius=.001f;
        var before=hunter.transform.position; yield return new WaitForSeconds(1f);
        Check((hunter.transform.position-before).sqrMagnitude>.05f,"hunter patrol moves");
        hunter.currentState=EnemyAI.EnemyState.Attacking; yield return null; yield return null;
        hunter.HearSound(hunter.transform.position+Vector3.right*2f); yield return null; yield return null;
        Check(!agent.isStopped,"hunter recovers after attack state");
        hunter.gameObject.SetActive(false);
        dread=One<HouseDread>();
        var fixtures=FindObjectsOfType<Light>(); var intensities=new float[fixtures.Length];
        for(int i=0;i<fixtures.Length;i++) intensities[i]=fixtures[i].intensity;
        dread.StartCoroutine((IEnumerator)Call(dread,"Encounter",1));
        yield return new WaitForSeconds(3.5f);
        for(int i=0;i<fixtures.Length;i++) Check(Mathf.Approximately(fixtures[i].intensity,intensities[i]),"lamp intensity restored "+i);
        var figure=(GameObject)Get(dread,"apparition"); Check(figure!=null && figure.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length>0,"apparition has original rigged figure");
        Check(figure.GetComponentsInChildren<Collider>(true).Length==0 && figure.GetComponentsInChildren<EnemyAI>(true).Length==0 && figure.GetComponentsInChildren<NavMeshAgent>(true).Length==0,"apparition cannot collide, navigate or capture");
        bool found=false;
        var camera=player.GetComponentInChildren<Camera>();
        foreach(var location in hunter.waypoints) {
            if(location==null) continue;
            foreach(var target in hunter.waypoints) {
                if(target==null || Vector3.Distance(location.position,target.position)<6f)continue;
                Position(body,location.position+Vector3.up);
                camera.transform.LookAt(target.position+Vector3.up*1.5f);
                if((bool)Call(dread,"ShowFigure")) { found=true; break; }
            }
            if(found)break;
        }
        Check(found,"apparition can appear in a visible authored location");
        camera.transform.LookAt(figure.transform.position+Vector3.up*1.5f);
        Debug.Log("FIGURE AT "+figure.transform.position+" CAMERA "+camera.transform.position+" LAYER "+player.gameObject.layer);
        foreach(var renderer in figure.GetComponentsInChildren<SkinnedMeshRenderer>(true)) Debug.Log("FIGURE RENDERER "+renderer.name+" active="+renderer.gameObject.activeInHierarchy+" enabled="+renderer.enabled+" bounds="+renderer.bounds);
        yield return new WaitForSeconds(.5f);
        if(VisualReview) {
            Debug.Log("VISUAL REVIEW READY");
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(Application.dataPath,"../visual-review.png"));
            yield return new WaitForSecondsRealtime(35f);
        }
        figure.SetActive(false);
        var times=new List<float>();
        for(int i=0;i<600;i++) { yield return null; times.Add(Time.unscaledDeltaTime*1000f); }
        times.Sort(); float sum=0; foreach(float t in times)sum+=t;
        Debug.Log("TIMING "+Screen.width+"x"+Screen.height+" GPU="+SystemInfo.graphicsDeviceName+" mean_ms="+(sum/times.Count)+" p95_ms="+times[(int)(times.Count*.95f)]+" p99_ms="+times[(int)(times.Count*.99f)]);
        hunter.gameObject.SetActive(true); yield return null; hunter.enabled=false; agent.isStopped=true;
        Position(body,hunter.transform.position); yield return PhysicsFrames(); yield return Scene("dead");
        Check(SceneManager.GetActiveScene().name=="dead","real collision captures and loads death");
        foreach(var b in FindObjectsOfType<SimpleSceneTransition>()) Debug.Log("DEAD BUTTON "+b.sceneToLoad);
        Check(errors==0,"no new runtime errors or script exceptions");
        Debug.Log("LEGACY CLOTH DIAGNOSTICS "+legacyClothDiagnostics);
        Debug.Log("ATMOSPHERE CHECK PASSED"); Application.Quit(0);
    }
}
