using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI winProcentText;
    [SerializeField] private TextMeshProUGUI loseProcentText;

    [Header("Fx")]
    [SerializeField] private ParticleSystem confettiFx;

    [Header("Script")]
    [SerializeField] private AddInBlender blenderScript;

    [Header("Sounds")]
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    private AudioSource audioSource;
    private bool isLose;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        //Restart
        if (Input.GetKey(KeyCode.R) && isLose)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void CheckWin()
    {
        if (blenderScript.diffColor >= 90)
        {
            isLose = false;
            winPanel.SetActive(true);
            audioSource.PlayOneShot(winSound, 0.3f);
            if (confettiFx != null)
                confettiFx.Play();
            
            winProcentText.text = blenderScript.diffColor.ToString() + "%";
        }

        if (blenderScript.diffColor < 90)
        {
            {
                isLose = true;
                losePanel.SetActive(true);
                audioSource.PlayOneShot(loseSound, 0.3f);
                loseProcentText.text = blenderScript.diffColor.ToString() + "%";
            }
        }
    }
}
