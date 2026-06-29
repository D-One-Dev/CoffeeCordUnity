using TMPro;
using UnityEngine;

public class MessageView : MonoBehaviour
{
    [SerializeField] private TMP_Text _senderText;
    [SerializeField] private TMP_Text _messageText;

    public void Bind(MessageData data, string senderName)
    {
        _senderText.text = senderName;
        _messageText.text = data.text;
    }
}
