using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuTutorialScript : MonoBehaviour
{
    [SerializeField] TMP_Text tutorialText;
    [SerializeField] Image image1, image2;

    [SerializeField] TMP_Text pageNumText;

    [SerializeField] TutorialPageInfo[] tutorialPages;

    int currPage = 0;

    // Start is called before the first frame update
    void Start()
    {
        ResetTutorial();
    }

    public void NextButton()
    {
        if (currPage < tutorialPages.Length)
        {
            currPage++;
            UpdatePage();
        }
    }

    public void BackButton()
    {
        if (currPage > 0)
        {
            currPage--;
            UpdatePage();
        }
    }

    public void ResetTutorial()
    {
        currPage = 0;
        UpdatePage();
    }

    void UpdatePage()
    {
        tutorialText.text = tutorialPages[currPage].pageText;
        image1.sprite = tutorialPages[currPage].firstImage;
        image2.sprite = tutorialPages[currPage].secondImage;
        pageNumText.text = "Pg. " + (currPage + 1);
    }
}

[System.Serializable]
public class TutorialPageInfo
{
    [TextArea(5, 12)]
    public string pageText;
    public Sprite firstImage, secondImage;
}

