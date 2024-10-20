using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;
using GameDataEditor;


class FlippingCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] TextMeshProUGUI textQuestionFront;
    [SerializeField] TextMeshProUGUI textExplanationBack;

    [SerializeField] TextMeshProUGUI textTipFront;
    [SerializeField] TextMeshProUGUI textTipBack;

    GDEFlashCardData cardData;

    void Start()
    {
        var key = GameDataManager.Instance.GetRandomQuestionKey();
        cardData = GameDataManager.Instance.GetFlashCardsData(key);

        textTipFront.text = cardData.Concept;
        textQuestionFront.text = cardData.Front;

        textTipBack.text = cardData.Word;
        textExplanationBack.text = cardData.Back;

        textTipFront.gameObject.SetActive(true);
        textQuestionFront.gameObject.SetActive(true);

        textTipBack.gameObject.SetActive(false);
        textExplanationBack.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        textTipFront.gameObject.SetActive(false);
        textQuestionFront.gameObject.SetActive(false);

        textTipBack.gameObject.SetActive(true);
        textExplanationBack.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        textTipFront.gameObject.SetActive(true);
        textQuestionFront.gameObject.SetActive(true);

        textTipBack.gameObject.SetActive(false);
        textExplanationBack.gameObject.SetActive(false);

    }
}
