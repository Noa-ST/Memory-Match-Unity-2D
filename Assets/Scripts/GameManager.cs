using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public int timeLimit;
    public MatchItem[] matchItems;
    public MatchItemUi itemUiPb;
    public Transform gridRoot;
    public GameState state;
    private List<MatchItem> m_matchItemsCopy;
    private List<MatchItemUi> m_matchItemsUis;
    private List<MatchItemUi> m_answers;
    private float m_timeCounting;
    private int m_totalMatchItem;
    private int m_totalMoving; // số lần đoán của ng chs
    private int m_rightMoving; // số lần đoán đúng
    private bool m_isAnswerChecking;

    public int TotalMoving { get => m_totalMoving; }
    public int RightMoving { get => m_rightMoving; }

    public override void Awake()
    {
        MakeSingleton(false);
        m_matchItemsCopy = new List<MatchItem>();
        m_matchItemsUis = new List<MatchItemUi>();
        m_answers = new List<MatchItemUi>();
        m_timeCounting = timeLimit;
        state = GameState.Starting;

    }

    public override void Start()
    {
        
        base.Start();
        if (AudioController.Ins)
            AudioController.Ins.PlayBackgroundMusic();

    }

    private void Update()
    {
        if (state != GameState.Playing) return;

        m_timeCounting -= Time.deltaTime;

        if(m_timeCounting <= 0 && state != GameState.Timeout)
        {
            state = GameState.Timeout;
            m_timeCounting = 0;
            if (GUIManager.Ins)
                GUIManager.Ins.timeoutDialog.Show(true);
            if(AudioController.Ins)
                AudioController.Ins.PlaySound(AudioController.Ins.timeOut);
        }

        if (GUIManager.Ins)
            GUIManager.Ins.UpdateTimeBar((float)m_timeCounting, (float)timeLimit);

    }

    public void PlayGame()
    {
        state = GameState.Playing;
        GenerateMatchItems();
        if (GUIManager.Ins)
            GUIManager.Ins.ShowGamePlay(true);
    }

    private void GenerateMatchItems()
    {

        if (matchItems == null || matchItems.Length <= 0 || itemUiPb == null || gridRoot == null) return;

        int totalItem = matchItems.Length;
        int divItem = totalItem % 2;
        m_totalMatchItem = totalItem - divItem;

        for (int i = 0; i < m_totalMatchItem; i++)
        {
            var matchItem = matchItems[i];
            if (matchItem != null)
                matchItem.Id = i;
        }

        m_matchItemsCopy.AddRange(matchItems); //1/2 tổng số thẻ cần dùng trong game
        m_matchItemsCopy.AddRange(matchItems); //tổng số thẻ cần dùng trong game

        ShuffleMatchItems();
        ClearGrid();

        for (int i = 0; i < m_matchItemsCopy.Count; i++)
        {
            var matchItem = m_matchItemsCopy[i];

            var matchItemUIClone = Instantiate(itemUiPb, Vector3.zero, Quaternion.identity);
            matchItemUIClone.transform.SetParent(gridRoot);
            matchItemUIClone.transform.localPosition = Vector3.zero;
            matchItemUIClone.transform.localScale = Vector3.one;
            matchItemUIClone.UpdateFirstState(matchItem.icon);
            matchItemUIClone.Id = matchItem.Id;
            m_matchItemsUis.Add(matchItemUIClone);

            if (matchItemUIClone.btnComp)
            {
                matchItemUIClone.btnComp.onClick.RemoveAllListeners();
                matchItemUIClone.btnComp.onClick.AddListener(() =>
                {
                    if (m_isAnswerChecking) return;

                    m_answers.Add(matchItemUIClone);
                    matchItemUIClone.OpenAminTrigger();
                    if(m_answers.Count == 2)
                    {
                        m_totalMoving++;
                        m_isAnswerChecking = true;
                        StartCoroutine(CheckAnsweCo());
                    }

                    matchItemUIClone.btnComp.enabled = false;
                });
            }
        }
    }

    private IEnumerator CheckAnsweCo()
    {
        bool isRight = m_answers[0]!= null && m_answers[1] != null
            && m_answers[0].Id == m_answers[1].Id;

        yield return new WaitForSeconds(1f);

        if (m_answers != null && m_answers.Count == 2)
        {
            if (isRight)
            {
                m_rightMoving++;
                for (int i=0; i < m_answers.Count; i++)
                {
                    var answer = m_answers[i];
                    if (answer)
                        answer.ExplodeAminTrigger();

                    if (AudioController.Ins)
                        AudioController.Ins.PlaySound(AudioController.Ins.right);
                }
            } else
            {
                for (int i = 0; i < m_answers.Count; i++)
                {
                    var answer = m_answers[i];
                    if (answer)
                        answer.OpenAminTrigger();

                    if (AudioController.Ins)
                        AudioController.Ins.PlaySound(AudioController.Ins.wrong);
                }
            }
        }

        m_answers.Clear();
        m_isAnswerChecking = false;

        if(m_rightMoving ==  m_totalMatchItem)
        {
            Pref.bestMove = m_totalMoving;
            if (GUIManager.Ins)
                GUIManager.Ins.gameOverDialog.Show(true);

            if (AudioController.Ins)
                AudioController.Ins.PlaySound(AudioController.Ins.gameover);
        }
    }

    private void ShuffleMatchItems()
    {
        if (m_matchItemsCopy == null || m_matchItemsCopy.Count <= 0) return;

        for (int i = 0; i < m_matchItemsCopy.Count; i++)
        {
            var temp = m_matchItemsCopy[i];
                if(temp != null)
            {
                int randIdx = Random.Range(0, m_matchItemsCopy.Count);
                m_matchItemsCopy[i] = m_matchItemsCopy[randIdx];
                m_matchItemsCopy[randIdx] = temp;
            }
        }
    }

    private void ClearGrid()
    {
        if (gridRoot == null) return;

        for (int i = 0; i < gridRoot.childCount; i++)
        {
            var child = gridRoot.GetChild(i);
            if (child)
                Destroy(child.gameObject);
        }
    }

}
