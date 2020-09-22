using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{

    public void Next()
    {
        SceneManager.LoadScene(1);
    }
   
}
