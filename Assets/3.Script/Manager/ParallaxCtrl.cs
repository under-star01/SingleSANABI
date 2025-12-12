using UnityEngine;

public class ParallaxCtrl : MonoBehaviour
{
    [SerializeField] private float parallaxMultiple = 0f; // 카메라를 따라가는 정도 (0 = 움직임x / 1 = 카메라와 동일한 속도)

    private Transform cam;
    private Vector3 lastCamPos;

    private void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;
    }

    private void LateUpdate()
    {
        Vector3 change = cam.position - lastCamPos;

        // 일부 카메라 변화량을 따라감
        transform.position += new Vector3(change.x * parallaxMultiple, change.y * parallaxMultiple, 0f);

        // 변경된 카메라 위치 갱신
        lastCamPos = cam.position;
    }
}
