using System;
using UnityEngine;
using Zenject;
using PrimeTween;
using System.Collections.Generic;

public class CanvasSwitcher: IDisposable
{
    private EventHandler _eventHandler;
    
    private List<Sequence> _sequences;

    [Inject]
    public void Construct(EventHandler eventHandler)
    {
        _sequences = new List<Sequence>();

        _eventHandler = eventHandler;
        _eventHandler.OnHideCanvas += HideCanvas;
        _eventHandler.OnShowCanvas += ShowCanvas;
    }

    private void HideCanvas(CanvasGroup canvas)
    {
        Sequence sequence = Sequence.Create();
        sequence.ChainCallback(canvas, target => target.interactable = false);
        sequence.ChainCallback(canvas, target => target.blocksRaycasts = false);
        sequence.Chain(Tween.Alpha(canvas, 0f, 0.05f, ease: Ease.InOutSine));
        sequence.ChainCallback(canvas, target => target.gameObject.SetActive(false));
        sequence.OnComplete(this, target => target._sequences.Remove(sequence));

        _sequences.Add(sequence);
    }

    private void ShowCanvas(CanvasGroup canvas)
    {
        Sequence sequence = Sequence.Create();
        sequence.ChainCallback(canvas, target => target.gameObject.SetActive(true));
        sequence.Chain(Tween.Alpha(canvas, 1f, 0.05f, ease: Ease.InOutSine));
        sequence.ChainCallback(canvas, target => target.blocksRaycasts = true);
        sequence.ChainCallback(canvas, target => target.interactable = true);
        sequence.OnComplete(this, target => target._sequences.Remove(sequence));

        _sequences.Add(sequence);
    }

    public void Dispose()
    {
        foreach(Sequence sequence in _sequences) sequence.Complete();
        _eventHandler.OnHideCanvas -= HideCanvas;
        _eventHandler.OnShowCanvas -= ShowCanvas;
    }
}
