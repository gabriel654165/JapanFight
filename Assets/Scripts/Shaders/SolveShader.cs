using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolveShader : MonoBehaviour
{
    [SerializeField] private float m_duration = 0.75f;

    private Renderer[] m_spriteRenderers;
    private Material[] m_materials;

    private int m_solveAmount = Shader.PropertyToID("_SolveAmount");
    private int m_verticalSolveAmount = Shader.PropertyToID("_VerticalSolve"); 

    void Start()
    {
        m_spriteRenderers = GetComponentsInChildren<Renderer>();

        int nbMaterials = 0;
        for (int i = 0; i < m_spriteRenderers.Length; i++)
        {
            foreach (var material in m_spriteRenderers[i].materials) {
                nbMaterials++;
            }
        }

        m_materials = new Material[nbMaterials];
        for (int i = 0, j = 0; j < nbMaterials; i++)
        {
            foreach (var material in m_spriteRenderers[i].materials) {
                m_materials[j] = material;
                j++;
            }
        }

        StartCoroutine(Appear(true, true));
    }

    private IEnumerator Appear(bool useSolve, bool useVertical)
    {
        float elapsedTime = 0f;

        while (elapsedTime <= m_duration) {
            float lerpedSolve = Mathf.Lerp(0f, 1.1f, (elapsedTime / m_duration));
            float lerpedVerticalSolve = Mathf.Lerp(1.1f, 0f, (elapsedTime / m_duration));

            elapsedTime += Time.deltaTime;
            for (int i = 0; i < m_materials.Length; i++)
            {
                if (useSolve)
                    m_materials[i].SetFloat(m_solveAmount, lerpedSolve);
                
                if (useVertical)
                    m_materials[i].SetFloat(m_verticalSolveAmount, lerpedVerticalSolve);
            }

            yield return null;
        }
        for (int i = 0; i < m_materials.Length; i++)
        {
            if (useSolve)
                m_materials[i].SetFloat(m_solveAmount, 1.1f);
            if (useVertical)
                m_materials[i].SetFloat(m_verticalSolveAmount, 0f);
        }
    }

    private IEnumerator Disappear(bool useSolve, bool useVertical)
    {
        float elapsedTime = 0f;

        while (elapsedTime <= m_duration) {
            float lerpedSolve = Mathf.Lerp(1.1f, 0f, (elapsedTime / m_duration));
            float lerpedVerticalSolve = Mathf.Lerp(0f, 1.1f, (elapsedTime / m_duration));

            elapsedTime += Time.deltaTime;
            for (int i = 0; i < m_materials.Length; i++)
            {
                if (useSolve)
                    m_materials[i].SetFloat(m_solveAmount, lerpedSolve);
                
                if (useVertical)
                    m_materials[i].SetFloat(m_verticalSolveAmount, lerpedVerticalSolve);
            }

            yield return null;
        }
        for (int i = 0; i < m_materials.Length; i++)
        {
            if (useSolve)
                m_materials[i].SetFloat(m_solveAmount, 0f);
            if (useVertical)
                m_materials[i].SetFloat(m_verticalSolveAmount, 1.1f);
        }
    }
}
