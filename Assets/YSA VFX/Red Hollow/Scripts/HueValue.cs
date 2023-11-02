using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HueValue: MonoBehaviour
{
    public enum ColorType
    {
        Material,
        TrailMaterial,
        Light,
        ParticleColor
    }

    public ColorType type;
    public int colorNumber = 1;

    public float hue = 0;

    void Update()
    {
        if (type == ColorType.Material)
        {
            for (int i = 0; i < colorNumber; i++)
            {
                Color color = transform.GetComponent<Renderer>().material.GetColor("_Color"+(i+1));
                transform.GetComponent<Renderer>().material.SetColor("_Color"+(i+1), Hue(color, hue));
            }
        }

        if (type == ColorType.TrailMaterial)
        {
            for (int i = 0; i < colorNumber; i++)
            {
                Color color = transform.GetComponent<ParticleSystemRenderer>().trailMaterial.GetColor("_Color" + (i + 1));
                transform.GetComponent<ParticleSystemRenderer>().trailMaterial.SetColor("_Color" + (i + 1), Hue(color, hue));
            }
        }

        if (type == ColorType.Light)
        {
            Color color = transform.GetComponent<Light>().color;
            transform.GetComponent<Light>().color = Hue(color, hue);
        }
        
        if (type == ColorType.ParticleColor)
        {
            Color color = transform.GetComponent<ParticleSystem>().main.startColor.color;

            ParticleSystem.MainModule m = transform.GetComponent<ParticleSystem>().main;
            m.startColor = Hue(color, hue);
        }
    }

    Color Hue(Color main, float hue)
    {
        float h;
        float s;
        float v;

        float sh = hue;

        if (hue > 1)
        {
            sh = 1;
        }
        if (hue < 0)
        {
            sh = 0;
        }
        Color.RGBToHSV(main, out h, out s, out v);
        return Color.HSVToRGB(sh, s, v);
    }
}
