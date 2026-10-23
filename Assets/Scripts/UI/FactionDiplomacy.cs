using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class FactionDiplomacy : MonoBehaviour
{
    [SerializeField] string relationVariableName;
    [SerializeField] TMP_Text relationDisplay;
    [SerializeField] float minRelation;
    [Space(10)]
    public SupportTypeTag supportTypeTag;
    [SerializeField] SupportType supportType;       public void ClearSupportType() => supportType = null;   public void SetSupportType(SupportType newSupportType) => supportType = newSupportType;
    [SerializeField] TMP_Text supportTypeDisplay;
    [SerializeField] Button CallSupportButton;
    [Space(10)]
    [SerializeField] BarInput travelTime;
    int TravelDaysLeft = 0;

    public enum SupportTypeTag
    {
        nonSupportive,
        militarySupport,
        economicSupport,
        balancedSupport,
        specialSupport
    }

    SupportTypeTag lastTag;
    public bool CheckoutLastTag() 
    {
        if (supportTypeTag == 0) return false;

        bool changed = lastTag != supportTypeTag;
        if(changed)
        {
            lastTag = supportTypeTag;
        }
        return changed;
    }

    private void Awake()
    {
        lastTag = supportTypeTag;
    }

    private void Start()
    {
        supportTypeDisplay.text = supportTypeTag.ToString();

        if (supportType != null) CallSupportButton.onClick.AddListener(supportType.LendHelp);
    }

    private void OnEnable()
    {
        relationDisplay.text = VariableSingleton.GetFloatVariable(relationVariableName).ToString();

        travelTime.Value = TravelDaysLeft;
    }
}
