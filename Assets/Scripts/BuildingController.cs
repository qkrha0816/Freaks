using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingController : MonoBehaviour
{
    private enum Building
    {
        Alter,
        Tower,
        Workshop
    }

    public Material[] materials;
    public GameObject buttens;

    [SerializeField] private GameObject[] Buildings;
    [SerializeField] private ParticleSystem fx_Smoke01; //build FX
    [SerializeField] private GameObject AlterRange;

    private AlterController alterController;
    private GameObject building;
    private BuildingPreview buildingPreview;
    private WorkshopController workshopController;
    private WhiteFreaksController[] whiteFreaksController;
    private FreaksController[] freaksControllers;
    private GameObject belowObejct;

    private bool isClicked = false;
    private bool cantbuild;
    private bool isPreviewActivate = false;
    private int buildnum;

    private Vector3 hittedPoint;

    private void Start()
    {
        alterController = GameObject.Find("Alter").GetComponent<AlterController>();
    }


    void Update()
    {
        // 알터가 클릭이 됐을 시 버튼 UI 생성
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject.name == "Alter")
                {
                    buttens.SetActive(true);
                }
            }
        }

        // 건설 모드 진입 시
        if(isPreviewActivate)
        {
            ViewPreview();

            // 건설 가능할 때 클릭이 인풋되면 건설
            if (building.GetComponent<BuildingPreview>().IsBuildable() && Input.GetMouseButtonDown(0))
            {
                // FX Start
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                Physics.Raycast(ray, out hit);
                hittedPoint = hit.point;

                if (buildnum == 0)      // Alter 건설 
                {
                    Destroy(GameObject.Find("Alter"));
                    AlterRange.SetActive(false);
                }
                else if (buildnum == 2) // WorkShop 건설
                {
                    workshopController = building.GetComponentInParent<WorkshopController>();
                    belowObejct = building.GetComponent<BuildingPreview>().GetBelowObject();
                    workshopController.Init(belowObejct);

                    if(belowObejct.transform.parent.name != "SwitchController")
                    {
                        belowObejct.GetComponent<MeshRenderer>().enabled = false;
                    }
                }

                Build();

                if (Physics.Raycast(ray, out hit))  // smoke FX 
                {
                    Vector3 sPos = hit.point; sPos.y = 0.3f;
                    fx_Smoke01.transform.position = sPos;
                    fx_Smoke01.Play(true);
                }
            }
        }

        // Build Cancel
        if (Input.GetKeyDown(KeyCode.Escape) && isClicked)
        {
            buttens.SetActive(false);
            isPreviewActivate = false;
            Destroy(building);
            isClicked = false;
        }


        // FX End
        if (fx_Smoke01.isStopped)
        {
            fx_Smoke01.Stop();
        }
    }


    public void BtnClick()
    {
        isClicked = true;
    }


    private void ViewPreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Vector3 buildPos = building.transform.position;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 _location = hit.point; _location.y = 0;
            building.transform.position = _location;
        }

        if (buildnum == 0)
        {
            //AlterRange.SetActive(true);
        }
    }

    /// <summary>
    /// 건물 건설
    /// </summary>
    private void Build()
    {
        isPreviewActivate = false;
        buttens.SetActive(false);

        building.GetComponent<Collider>().isTrigger = false;
        building.GetComponent<BuildingPreview>().Destroy();

        alterController.GoBuild(building);
        Destroy(building.GetComponent<BuildingPreview>());

        if(buildnum == 0)
        {
            whiteFreaksController = FindObjectsOfType<WhiteFreaksController>();
            freaksControllers = FindObjectsOfType<FreaksController>();

            for (int i = 0; i < whiteFreaksController.Length; i++)
            {
                whiteFreaksController[i].ChangeAlterPosition(hittedPoint);
            }    
            
            for (int i = 0; i < freaksControllers.Length; i++)
            {
                freaksControllers[i].ChangeAlterPosition(hittedPoint);
            } 
        }
    }

    public void ConstructBuilding(int buildingNum)
    {
        buildnum = buildingNum;

        building = Instantiate(Buildings[buildingNum]);
        building.AddComponent<BuildingPreview>();
        building.GetComponent<BuildingPreview>().Init(buildingNum);

        isPreviewActivate = true;
    }
}