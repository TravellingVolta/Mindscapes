using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using GameDataEditor;
using UnityEngine.UI;
using System;


class FlashCard : MonoBehaviour
{
    [SerializeField] Transform transQuestionCard;
    [SerializeField] Button buttonFlipCard;
    [SerializeField] Button buttonNextQuestion;

    [SerializeField] TextMeshProUGUI textCard;
    [SerializeField] TextMeshProUGUI textTip;

    float flippingTime = 0.5f;
    int fadeSide = 0; // 0=front, 1=back
    int isShrinking = -1;
    bool isFlipping = false;
    float distancePerTime;
    float timeCount = 0;


    GDEFlashCardData cardData;

    private void OnEnable()
    {
        buttonFlipCard.onClick.AddListener(FlipCard);
        buttonNextQuestion.onClick.AddListener(NewQuestion);
    }

    private void OnDisable()
    {
        buttonFlipCard.onClick.RemoveListener(FlipCard);
        buttonNextQuestion.onClick.RemoveListener(NewQuestion);
    }

    void Start()
    {
        NewQuestion();
    }

    void Update()
    {
        if (isFlipping)
        {
            Vector3 v = transQuestionCard.localScale;
            v.x += isShrinking * distancePerTime * Time.deltaTime;
            transQuestionCard.localScale = v;

            timeCount += Time.deltaTime;
            if (timeCount >= flippingTime && isShrinking < 0)
            {
                isShrinking = 1;
                timeCount = 0;
                if (fadeSide == 0)
                {
                    fadeSide = 1;

                    textCard.text = cardData.Answer;
                    textTip.text = cardData.AnswerTip;

                }
                else if (fadeSide == 1)
                {
                    fadeSide = 0;

                    textCard.text = cardData.Question;
                    textTip.text = cardData.QuestionTip;
                }
            }
            else if (timeCount >= flippingTime && isShrinking == 1)
            {
                isFlipping = false;
            }
        }
    }

    public void FlipCard()
    {
        buttonFlipCard.interactable = false;

        timeCount = 0;
        isFlipping = true;
        isShrinking = -1;
    }

    public void NewQuestion()
    {
        fadeSide = 0;

        buttonFlipCard.interactable = true;

        var key = GameDataManager.Instance.GetRandomQuestionKey();
        cardData = GameDataManager.Instance.GetFlashCardsData(key);

        textCard.text = cardData.Question;
        textTip.text = cardData.QuestionTip;

        distancePerTime = transQuestionCard.localScale.x / flippingTime;
    }
}
