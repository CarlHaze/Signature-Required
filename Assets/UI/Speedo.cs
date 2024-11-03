using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class Speedo : VisualElement
{

    static CustomStyleProperty<Color> s_FillColor= new CustomStyleProperty<Color>("--fill-color");
    static CustomStyleProperty<Color> s_BackgroundColor = new CustomStyleProperty<Color>("--background-color");

    Color m_FillColor;
    Color m_BackgroundColor;


    // This is the number that the label displays as a percentage
    [SerializeField, DontCreateProperty]
    float m_Progress;

    // A value between 0 and 100 
    [UxmlAttribute, CreateProperty]
    public float progress
    {
        // The progress proerty is exposed in C#.
        get => m_Progress;
        set
        {
            // Whenever the progress proerty changes, MarkDirtyRepain() is named. This causes a call to the generateVisualContent callback.
            m_Progress = Mathf.Clamp(value, 0.01f, 100f);
            MarkDirtyRepaint();
        }
    }


    public Speedo()
    {
        // Register a callback after custom style resolution.
        RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

        generateVisualContent += GenerateVisualContent;

    }

    private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
    {
       if (evt.currentTarget == this)
        {
            Speedo element = (Speedo)evt.currentTarget;
            element.UpdateCustomStyles();
        }
    }

    private void UpdateCustomStyles()
    {
        bool repaint = false;
        if (customStyle.TryGetValue(s_FillColor, out m_FillColor))
            repaint = true;
        if (customStyle.TryGetValue(s_BackgroundColor, out m_BackgroundColor))
            repaint = true;
        if (repaint)
            MarkDirtyRepaint();

    }

    private void GenerateVisualContent(MeshGenerationContext context)
    {
        
        float width = contentRect.width;
        float height = contentRect.height;

        //draw a semi circle
        var painter = context.painter2D; 
        painter.BeginPath();
        painter.lineWidth = 10f;
        painter.Arc(new Vector2(width * 0.5f, height), width * 0.5f, 180f, 0f);
        painter.ClosePath();
        painter.fillColor = m_BackgroundColor;
        painter.Fill(FillRule.NonZero);
        painter.Stroke();

        //Fill
        painter.BeginPath();
        painter.LineTo(new Vector2(width * 0.5f, height));
        painter.lineWidth = 10f;

        float amount = 180f * ((100f-progress) / 100f);

        painter.Arc(new Vector2(width * 0.5f, height), width * 0.5f, 180f, 0f - amount);
        painter.ClosePath();
        painter.fillColor = m_FillColor;
        painter.Fill(FillRule.NonZero);
        painter.Stroke();

    }
}
