using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Color _flashColor = Color.white;
    [SerializeField] private Color _DissolveColor;
    [SerializeField] private float _flashTime = 0.25f;
    [SerializeField] private float _DissolveTime = 1f;


    private SpriteRenderer[] _spriteRenderers;
    private Material[] _materials;

    private Coroutine damageflashCoroutine;
    private Coroutine dissolveEffectCoroutine;
    private void Awake()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        Init();
    }

    private void Init()
    {
        _materials = new Material[_spriteRenderers.Length];

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            _materials[i] = _spriteRenderers[i].material;
        }
    }

    public void CallDamageFlash()
    {
        damageflashCoroutine = StartCoroutine(DamageFlasher());
    }

    public void CallDissolveEffect()
    {
        dissolveEffectCoroutine = StartCoroutine(DissolveEffect());
    }

    private IEnumerator DamageFlasher()
    {
        SetFlashColor();

        float currentFlashAmount = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < _flashTime)
        {
            elapsedTime += Time.deltaTime;

            currentFlashAmount = Mathf.Lerp(1f, 0f, (elapsedTime / _flashTime));
            SetFlashAmount(currentFlashAmount);

            yield return null;
        }
    }

    private IEnumerator DissolveEffect()
    {
        SetDissolverColor();

        float currentDissolveAmount = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < _DissolveTime)
        {
            elapsedTime += Time.deltaTime;

            currentDissolveAmount = Mathf.Lerp(1f, 0f, (elapsedTime / _DissolveTime));
            SetDissolveAmount(currentDissolveAmount);

            yield return null;
        }

        Destroy(this.gameObject);
    }

    private void SetFlashColor()
    {
        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i].SetColor("_Flash", _flashColor);
        }
    }

    private void SetDissolverColor()
    {
        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i].SetColor("_OutlineColor", _DissolveColor);
        }
    }

    private void SetFlashAmount(float amount)
    {
        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i].SetFloat("_FlashAmount", amount);
        }
    }

    private void SetDissolveAmount(float amount)
    {
        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i].SetFloat("_DissolveAmount", amount);
        }
    }
}
