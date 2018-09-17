using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour {

    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 40f;
    [SerializeField] float levelLoadDelay = 1.5f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip death;
    [SerializeField] AudioClip success;

    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem successParticles;
    [SerializeField] ParticleSystem deathParticles;

    Rigidbody rigidBody;
    AudioSource audioSource;

    enum State { Alive, Dying, Transcending }
    State state = State.Alive;
    bool collisionsDisbled = false;

	// Use this for initialization
	void Start ()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
        if (state == State.Alive)
        {
            RespondToThrustInput();
            Rotate();
        }
        if (Debug.isDebugBuild)
        {
            RespondToDebugInput();
        }
    }

    // Process rocket collisions with objects
    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive || collisionsDisbled)
        {
            return;
        }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                // Do nothing
                break;

            case "Finish":
                StartSuccessSequence();
                break;

            default:
                StartDeathSequence();
                break;
        }
    }

    // Makes rocket unable to collide or move
    // plays the death audio and particle effects
    // calls for the first scene to load
    private void StartDeathSequence()
    {
        state = State.Dying;
        audioSource.Stop();
        audioSource.PlayOneShot(death);
        deathParticles.Play();
        Invoke("DeadFirstLevel", levelLoadDelay);
    }

    // Makes rocket unable to collide or move, 
    // plays the success audio and particle effects
    // calls for the next scene to load
    private void StartSuccessSequence()
    {
        state = State.Transcending;
        audioSource.Stop();
        audioSource.PlayOneShot(success);
        successParticles.Play();
        Invoke("LoadNextScene", levelLoadDelay);
    }

    // Loads the first scene
    private void DeadFirstLevel()
    {
        SceneManager.LoadScene(0);
    }

    // Loads next scene onto screen
    private void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings) {
            nextSceneIndex = 0;
        }
        SceneManager.LoadScene(nextSceneIndex); 
    }

    // Processes debug inputs,
    // load levels or change collision settings
    private void RespondToDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextScene();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            collisionsDisbled = !collisionsDisbled;
        }
    }

    // Moves the rocket when the player presses space
    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space)) // can thrust while rotating
        {
            ApplyThrust();
        }

        else
        {
            audioSource.Stop();
            mainEngineParticles.Stop();
        }
    }

    // Translates the rocket along its direction
    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(mainThrust * Vector3.up * Time.deltaTime);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngine);
        }
        mainEngineParticles.Play();
    }

    // Rotates the rocket left or right 
    private void Rotate()
    {
        rigidBody.freezeRotation = true; // manual control of rotation

        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }

        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }

        rigidBody.freezeRotation = false; 
    }
}
