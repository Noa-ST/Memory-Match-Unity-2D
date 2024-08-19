using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchItemUi : MonoBehaviour
{
    private int m_id;
    public Sprite bg;
    public Sprite backBg;
    public Image itemBG;
    public Image itemIcon;
    public Button btnComp;
    private bool m_isOpened;
    private Animator amin;

    public int Id { get => m_id; set => m_id = value; }

    private void Awake()
    {
        amin = GetComponent<Animator>();
    }

    public void UpdateFirstState(Sprite icon)
    {
        if (itemBG)
            itemBG.sprite = backBg;

        if (itemIcon)
        {
            itemIcon.sprite = icon;
            itemIcon.gameObject.SetActive(false);
        }
    }

    public void ChangeState()
    {
        m_isOpened = !m_isOpened;

        if (itemBG)
            itemBG.sprite = m_isOpened ? bg : backBg;

        if (itemIcon)
            itemIcon.gameObject.SetActive(m_isOpened);
    }

    public void OpenAminTrigger()
    {
        if (amin)
            amin.SetBool(AminState.Flip.ToString(), true);
    }

    public void ExplodeAminTrigger()
    {
        if (amin)
            amin.SetBool(AminState.Explode.ToString(), true);
    }

    public void BackToIdle()
    {
        if (amin)
            amin.SetBool(AminState.Flip.ToString(), false);

        if (btnComp)
            btnComp.enabled = !m_isOpened;
    }


}
