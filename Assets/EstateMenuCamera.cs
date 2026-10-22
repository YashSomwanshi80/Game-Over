using UnityEngine;

// Presentation-only view. Runs after the first-person camera's local pose update.
[DefaultExecutionOrder(1000)]
public sealed class EstateMenuCamera : MonoBehaviour
{
    Camera view;MansionLighting lighting;
    public void Configure(Camera camera){view=camera;lighting=FindFirstObjectByType<MansionLighting>();if(lighting!=null)lighting.SetShowcase(true);enabled=true;LateUpdate();}
    public void EnterGame()
    {
        enabled=false;if(lighting!=null)lighting.SetShowcase(false);if(view==null)return;
        view.transform.localPosition=new Vector3(0,1.55f,0);view.transform.localRotation=Quaternion.identity;view.fieldOfView=72;
    }
    void LateUpdate()
    {
        if(view==null)return;
        float t=.5f+.5f*Mathf.Sin(Time.unscaledTime*.045f);
        view.transform.position=Vector3.Lerp(new Vector3(-18,5.6f,-23),new Vector3(-10,4.6f,-25),t);
        view.transform.LookAt(new Vector3(1,2.9f,-2));view.fieldOfView=56;
    }
}
