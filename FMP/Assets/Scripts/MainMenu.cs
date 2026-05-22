using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public bool button1Hover = false;
    public bool button2Hover = false;
    public bool button3Hover = false;
    public Transform sword;
    public Animator swordAnims;
    public bool selected = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sword.SetLocalPositionAndRotation(new Vector3(0, 274.58f, 0), transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        if(selected == false)
        {
            if (button1Hover == true)
            {
                sword.SetLocalPositionAndRotation(new Vector3(0, 274.58f, 0), transform.rotation);
            }

            if (button2Hover == true)
            {
                sword.SetLocalPositionAndRotation(new Vector3(72.17f, 129.8f, 0), transform.rotation);
            }

            if (button3Hover == true)
            {
                sword.SetLocalPositionAndRotation(new Vector3(0, 0, 0), transform.rotation);
            }
        }
    }



    public void Button1enter()
    {
        button1Hover = true;
    }

    public void Button1Exit()
    {
        button1Hover = false;
    }

    public void Button2enter()
    {
        button2Hover = true;
    }

    public void Button2Exit()
    {
        button2Hover = false;
    }

    public void Button3enter()
    {
        button3Hover = true;
    }

    public void Button3Exit()
    {
        button3Hover = false;
    }


    public void ButtonSelect()
    {
        swordAnims.SetTrigger("Stab");
        selected = true;
        if(button1Hover == true)
        {
            SceneManager.LoadScene("Level_1");
        }
    }

    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}


