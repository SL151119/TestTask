using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using DG.Tweening;

public class AddInBlender : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color appleCol;
    [SerializeField] private Color bananaCol;
    [SerializeField] private Color firstLvlColor;

    [Header("Blender")]
    [SerializeField] private GameObject blenderLid;
    [SerializeField] private GameObject blender;

    [Header("Cubes")]
    [SerializeField] private GameObject[] cubesFill;
    [SerializeField] private MeshRenderer[] cubesRend;

    [Header("Raycast")]
    [SerializeField] private float rayLength;
    [SerializeField] private LayerMask layerMask;

    [Header("Script")]
    [SerializeField] private GameManager gameManagerScript;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI blenderIsFullText;

    private Vector3 _doJump = new Vector3(-0.7519996f, 1.256f, 5.338f);
    private Vector3 _appleScale = new Vector3(0.40f, 0.40f, 0.40f);
    private Vector3 _bananaScale = new Vector3(0.25f, 0.25f, 0.25f);
    private Vector3 _cubesToRotate = new Vector3(0f, -384.0f, 0.0f);
    private Vector3 _shake = new Vector3(0.005f, 0, 0);
    private RaycastHit _hit;

    private int _appleCount;
    private int _bananaCount;
    private int _index;

    private bool _canMix = false;
    private bool _fruitIsClicked = false;
    private bool _resetTime = false;
    private bool _lidIsOpen = false;
    private bool _isMixed = false;

    private float _clickTime;
    private float _scaleCubesY = 0.04506186f;

    public float diffColor { get; private set; }

    void Start()
    {
        //Spawn fruits on the start
        ObjectPooling.SharedInstance.GetPooledObject("Apple");
        ObjectPooling.SharedInstance.GetPooledObject("Banana");

        _index = 0;
        _appleCount = 0;
        _bananaCount = 0;
        for (int i = 0; i < 3; i++)
        {
            cubesRend[i] = cubesFill[i].GetComponent<MeshRenderer>();
        }

    }
    void Update()
    {
        if (blenderLid.transform.localPosition.y == 0.26f)
            _lidIsOpen = false;
        else
            _lidIsOpen = true;

        if (!_fruitIsClicked && !_isMixed)
        {
            if (Input.GetMouseButtonDown(0) &&
            !EventSystem.current.IsPointerOverGameObject())
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out _hit, rayLength, layerMask))
                {
                    _resetTime = true;
                    _clickTime = 0f;
                    if (_index != 4)
                    {
                        StartCoroutine("DropInBlender");
                    }
                    else
                    {
                        StartCoroutine("BlenderFull");
                    }
                }
            }
        }

        //After 5 seconds close blender lid
        if (_resetTime && _clickTime < 5f)
        {
            _clickTime += Time.deltaTime;
            if (_clickTime >= 5)
            {
                blenderLid.transform.DOLocalMoveY(0.26f, 0.5f, snapping: false);
                _resetTime = false;
            }
        }
    }
    IEnumerator DropInBlender()
    {
            _fruitIsClicked = true;
            blenderLid.transform.DOLocalMoveY(0.46f, 0.5f, snapping: false);
            yield return new WaitForSeconds(0.5f);
            _hit.transform.DOJump(_doJump, 0.5f, 1, 0.6f, snapping: false);
            StartCoroutine("ActionWithFruit");
    }
    IEnumerator ActionWithFruit()
    {
        if (_hit.collider.CompareTag("Apple"))
        {
            _hit.transform.DOScale(_appleScale, 0.5f);
            yield return new WaitForSeconds(0.5f);
            ShakeBlender(0.8f);
            DOTween.Kill(_hit.transform);
            _hit.transform.gameObject.SetActive(false);
            _appleCount++;
            cubesRend[_index].material.color = MixColor(appleCol); //cube take color from fruit
            cubesFill[_index].transform.DOScaleY(_scaleCubesY, 0.3f);
            _index++;
            _fruitIsClicked = false;
            ObjectPooling.SharedInstance.GetPooledObject("Apple"); //spawn one more fruit
        }
        if (_hit.collider.CompareTag("Banana"))
        {
            _hit.transform.DOScale(_bananaScale, 0.5f);
            yield return new WaitForSeconds(0.5f);
            ShakeBlender(0.8f);
            DOTween.Kill(_hit.transform);
            _hit.transform.gameObject.SetActive(false);
            _bananaCount++;
            cubesRend[_index].material.color = MixColor(bananaCol); //cube take color from fruit
            cubesFill[_index].transform.DOScaleY(_scaleCubesY, 0.3f);
            _index++;
            _fruitIsClicked = false;
            ObjectPooling.SharedInstance.GetPooledObject("Banana"); //spawn one more fruit
        }
    }

    IEnumerator BlenderFull()
    {
        blenderIsFullText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        blenderIsFullText.gameObject.SetActive(false);
    }

    void ShakeBlender(float _shakeTime)
    {
        blender.transform.DOPunchPosition(_shake, _shakeTime);
    }

    Color MixColor (params Color [] color)
    {
        Color resultColor = new Color(0, 0, 0, 0);
        foreach (Color c in color)
        {
            resultColor += c;
        }
            resultColor /= color.Length;
            return new Color
                (resultColor.r, resultColor.g,
                 resultColor.b, resultColor.a * color.Length);
    }

    public void MixFruits()
    {
        //if blender not empty u can mix
        if (_appleCount >= 1 || _bananaCount >= 1)
        {
            _canMix = true;
            if (_canMix && !_isMixed)
            {
                _canMix = false;
                StartCoroutine("Mix");
            }
        }
    }
    IEnumerator Mix()
    {
        _isMixed = true;
            if (_lidIsOpen)
            {
                blenderLid.transform.DOLocalMoveY(0.26f, 0.5f, snapping: false);
                yield return new WaitForSeconds(0.8f);
            }

            for (int j = 0; j < _index; j++)
            {
                cubesFill[j].transform.DORotate(_cubesToRotate, 2.0f);
                ShakeBlender(2.0f);
            }

            yield return new WaitForSeconds(0.4f);
            FinalColor();
            yield return new WaitForSeconds(1.5f);
            gameManagerScript.CheckWin();
    }


    void FinalColor()
    {
        //all cubes take final color
        for (int o = 0; o < _index; o++)
        {
            if (_appleCount == 1 && _bananaCount == 0)
            {
                cubesRend[o].material.color = MixColor(appleCol);
                DifferenceColor(cubesRend[o].material.color, firstLvlColor);
            }

            if (_appleCount == 2 && _bananaCount == 0)
            {
                cubesRend[o].material.color = MixColor(appleCol, appleCol);
                DifferenceColor(cubesRend[o].material.color, firstLvlColor);
            }

            if (_appleCount == 3 && _bananaCount == 0)
            {
                cubesRend[o].material.color = MixColor(appleCol, appleCol);
                DifferenceColor(cubesRend[o].material.color, firstLvlColor);
            }

            if (_appleCount == 4 && _bananaCount == 0)
            {
                cubesRend[o].material.color = MixColor(appleCol, appleCol);
                DifferenceColor(cubesRend[o].material.color, firstLvlColor);
            }

            if (_appleCount == 0 && _bananaCount == 1)
            {
                cubesRend[o].material.color = MixColor(bananaCol);
                DifferenceColor(cubesRend[o].material.color, firstLvlColor);
            }

            if (_appleCount == 0 && _bananaCount == 2)
            {
                cubesRend[o].material.color = MixColor(bananaCol, bananaCol);
                DifferenceColor(cubesRend[o].material.color, firstLvlColor);
            }

            if (_appleCount == 0 && _bananaCount == 3)
            {
                cubesRend[o].material.color = MixColor(bananaCol, bananaCol);
                DifferenceColor(cubesRend[o].material.color, firstLvlColor);
            }
            if (_appleCount == 0 && _bananaCount == 4)
            {
                cubesRend[o].material.color = MixColor(bananaCol, bananaCol);
                DifferenceColor(cubesRend[o].material.color, firstLvlColor);
            }

            if (_appleCount == 1 && _bananaCount == 1)
            {
                cubesRend[o].material.color = MixColor(appleCol, bananaCol);
                DifferenceColor(cubesRend[o].material.color, firstLvlColor);
            }

            if (_appleCount == 2 && _bananaCount == 2)
            {
                cubesRend[o].material.color = MixColor((appleCol + bananaCol) / 2, (appleCol + bananaCol) / 2);
                DifferenceColor(cubesRend[o].material.color, firstLvlColor);
            }

            if (_appleCount == 2 && _bananaCount == 1)
            {
                cubesRend[o].material.color = MixColor((appleCol + bananaCol) / 2, appleCol);
                DifferenceColor(cubesRend[o].material.color, firstLvlColor);
            }

            if (_appleCount == 3 && _bananaCount == 1)
            {
                cubesRend[o].material.color = MixColor((appleCol + bananaCol + appleCol) / 3, appleCol);
                DifferenceColor(cubesRend[o].material.color, firstLvlColor);
            }

            if (_appleCount == 1 && _bananaCount == 2)
            {
                cubesRend[o].material.color = MixColor((appleCol + bananaCol) / 2, bananaCol);
                DifferenceColor(cubesRend[o].material.color, firstLvlColor);
            }

            if (_appleCount == 1 && _bananaCount == 3)
            {
                cubesRend[o].material.color = MixColor((bananaCol + appleCol + bananaCol) / 3, bananaCol);
                DifferenceColor(cubesRend[o].material.color, firstLvlColor);
            }
        }
    }
    public float DifferenceColor(Color currentColor, Color needColor)
    {
       //Ñalculating color differences
       diffColor = Mathf.Round(100 - (new Vector3(0.59f * (needColor.r - currentColor.r), 0.3f * (needColor.g - currentColor.g), 0.11f * (needColor.b - currentColor.b)).magnitude * 100));
       return diffColor;
    }
}
