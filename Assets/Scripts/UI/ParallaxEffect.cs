using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralax : MonoBehaviour
{
    public Camera cam;
    public Transform followTarget;

    //Vị trí bắt đầu cho hiệu ứng parallax trong trò chơi
    Vector2 startingPosition;

    //Vị trí điểm Z bắt đầu cho hiệu ứng parallax trong trò chơi
    float startingZ;

    //Khoảng cách camera đã di chuyển so với vị trí ban đầu của parallax object
    Vector2 camMoveSinceStart => (Vector2)cam.transform.position - startingPosition;
    
    float zDistanceFromTarget => transform.position.z - followTarget.position.z;

    //Nếu đối tượng nằm phía trước mục tiêu, sử dụng nearClipPlane. Nếu nó nằm phía sau mục tiêu, sử dụng farClipPlane.
    float clipingPlane => (cam.transform.position.z + (zDistanceFromTarget > 0 ? cam.farClipPlane : cam.nearClipPlane));

    //Vật thể càng ở xa người chơi, đối tượng ParallaxEffect sẽ di chuyển càng nhanh. Kéo giá trị Z của nó lại gần mục tiêu để làm cho nó di chuyển chậm hơn.
    float parallaxFactor => Mathf.Abs(zDistanceFromTarget) / clipingPlane;

    // Hàm Start được gọi trước khi cập nhật khung hình đầu tiên
    void Start()
    {
        startingPosition = transform.position;
        startingZ = transform.position.z;
    }

    // Hàm Update được gọi mỗi frame
    void Update()
    {
        //Khi mục tiêu di chuyển, di chuyển parallax object một khoảng cách bằng số lần hệ số nhân.
        Vector2 newPosition = startingPosition + camMoveSinceStart * parallaxFactor;

        //Vị trí X/Y thay đổi dựa trên tốc độ di chuyển của mục tiêu nhân với hệ số parallax, nhưng vị trí Z vẫn không đổi.
        transform.position = new Vector3(newPosition.x, newPosition.y, startingZ);
    }
}
