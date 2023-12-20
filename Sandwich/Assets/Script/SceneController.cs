using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] string sceneName = "Sandwich";

    public void ReloadScene() => SceneManager.LoadScene(sceneName);
}
