using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public AffineTransformer affineTransformer;
    public ScanLineFill scanLineFill;

    // input matriks transformasi
    public InputField Mat_A;
    public InputField Mat_B;
    public InputField Mat_C;
    public InputField Mat_D;

    public InputField Color_R;
    public InputField Color_G;
    public InputField Color_B;
    float ColorValue = 0;
    [SerializeField] Button TransformButton;
    [SerializeField] Button ChangeColorButton;
    [SerializeField] Button ResetColorButton;
    [SerializeField] InputField DegreeInput;
    [SerializeField] InputField ColorR_Input;
    [SerializeField] InputField ColorG_Input;
    [SerializeField] InputField ColorB_Input;

    // Start is called before the first frame update
    void Start()
    {
        TransformButton.onClick.AddListener(TransformButton_OnClick);
        ChangeColorButton.onClick.AddListener(ChangeColorButton_OnClick);
        ResetColorButton.onClick.AddListener(ResetColorButton_OnClick);
        
        DegreeInput.onValueChanged.AddListener(DegreeInput_OnEndEdit);
    }

    public void TransformButton_OnClick()
    {
        affineTransformer.ExecuteAffineTransformation(float.Parse(Mat_A.text), float.Parse(Mat_B.text), float.Parse(Mat_C.text), float.Parse(Mat_D.text));
    }

    public void ChangeColorButton_OnClick()
    {
        checkColor();
        scanLineFill.addColor(float.Parse(Color_R.text), float.Parse(Color_R.text), float.Parse(Color_R.text));
    }

    public void ResetColorButton_OnClick()
    {
        scanLineFill.addColor(0,0,0);
        float a = 0.0f;
        Color_R.text = a.ToString();
        Color_B.text = a.ToString();
        Color_G.text = a.ToString();
    }

    void DegreeInput_OnEndEdit(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            float degree = float.Parse(value);

            float a = Mathf.Cos(degree * Mathf.Deg2Rad);
            float b = Mathf.Sin(degree * Mathf.Deg2Rad);
            float c = -Mathf.Sin(degree * Mathf.Deg2Rad);
            float d = Mathf.Cos(degree * Mathf.Deg2Rad);

            Mat_A.text = a.ToString();
            Mat_B.text = b.ToString();
            Mat_C.text = c.ToString();
            Mat_D.text = d.ToString();
        }
    }

    void checkColor()
    {
        float a = 0.0f;
        if(!string.IsNullOrEmpty(Color_R.text))
        {
            ColorValue = float.Parse(Color_R.text);
            ColorValue = Mathf.Clamp(ColorValue, 0.0f, 1.0f);
            Color_R.text = ColorValue.ToString();
        }
        else
            Color_R.text = a.ToString();

        if (!string.IsNullOrEmpty(Color_G.text))
        {
            ColorValue = float.Parse(Color_G.text);
            ColorValue = Mathf.Clamp(ColorValue, 0.0f, 1.0f);
            Color_G.text = ColorValue.ToString();
        }
        else
            Color_G.text = a.ToString();

        if (!string.IsNullOrEmpty(Color_B.text))
        {
            ColorValue = float.Parse(Color_B.text);
            ColorValue = Mathf.Clamp(ColorValue, 0.0f, 1.0f);
            Color_B.text = ColorValue.ToString();
        }
        else
            Color_B.text = a.ToString();
        

    }

}
